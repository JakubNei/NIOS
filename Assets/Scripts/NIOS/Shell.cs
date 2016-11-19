using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class Shell : Program
{
	string currentCommand;
	Process currentUserSession { get { return this; } }

	public override void Main(string[] arguments)
	{
		while (true)
		{
			BeginCommand();
			var line = Utils.SanitizeInput(Console.ReadLine());


			//UnityEngine.Debug.Log("executing " + line);
			var t = Thread.NewThread(() =>
			{
				ExecuteCommand(currentUserSession, line);
			});
			t.Start();
			var started = DateTime.UtcNow;
			while (t != null && t.IsAlive)
			{
				if (started.AddSeconds(10) < DateTime.UtcNow)
				{
					t.Abort();
					t = null;
					Console.WriteLine("command execution took over 10 second, aborting");
				}
			}
		}
	}
	string[] StandartParseArguments(string t)
	{
		var args = new List<string>();
		var currentArg = string.Empty;
		char isInsideQuota = (char)0;
		foreach (var c in t)
		{
			if (c == '"' || c == '\'')
			{
				if (isInsideQuota != (char)0)
					isInsideQuota = c;
				else if (c == isInsideQuota)
					isInsideQuota = (char)0;
			}
			else if ((c == ' ' || c == '\n') && isInsideQuota == (char)0)
			{
				args.Add(currentArg);
				currentArg = string.Empty;
			}
			else
			{
				currentArg += c;
			}
		}

		if (string.IsNullOrEmpty(currentArg) == false) args.Add(currentArg);

		return args.ToArray();
	}

	public void ExecuteCommand(Process session, string cmd)
	{
		var w = Console.Out;
		w.WriteLine();

		cmd = cmd.Trim();
		if (string.IsNullOrEmpty(cmd)) return;

		try
		{
			if (ExecuteCommand_2(session, cmd) == false)
				w.WriteLine(cmd + " is not recognized as program or internal command");
		}
		catch (Error e)
		{
			w.WriteLine(e.Message);
		}
		catch (Exception e)
		{
			w.WriteLine(cmd + ", failed to execute, exception:");
			w.WriteLine(e.ToString());
		}
	}

	class CmdTokenizer
	{
		public string currentText;
		public TokenType currentType;
		public bool argumentsDoSplitByWhiteChar = false;

		public enum TokenType
		{
			Start = 0,
			ProgramName,
			Argument,
			Pipeline, // |
			RedirectOutputSet, // >
			RedirectOutputAppend, // >>
			RedirectInput, // <
		}

		string cmd;
		int index = 0;

		public CmdTokenizer(string cmd)
		{
			this.cmd = cmd;
		}

		char Peek
		{
			get
			{
				return cmd[index];
			}
		}

		char Eat()
		{
			var c = cmd[index];
			index++;
			return c;
		}

		/// <summary>
		/// Can still continue ?
		/// </summary>
		public bool Can
		{
			get
			{
				return index < cmd.Length;
			}
		}

		static Regex isWhiteChar = new Regex(@"^\s+$");

		bool IsWhiteChar(char c)
		{
			return isWhiteChar.IsMatch(c.ToString());
		}

		bool IsSpecialMeaningChar(char c)
		{
			return c == '|' || c == '>' || c == '<';
		}

		char isInsideQuotes = (char)0;


		//ls|grep F>test
		public void MoveNext()
		{
			currentText = string.Empty;
			isInsideQuotes = (char)0;

			switch (currentType)
			{
				case TokenType.Pipeline:
				case TokenType.RedirectOutputSet:
				case TokenType.RedirectOutputAppend:
				case TokenType.RedirectInput:
				case TokenType.Start:
					currentType = TokenType.ProgramName;
					while (Can && !IsWhiteChar(Peek) && !IsSpecialMeaningChar(Peek))
						currentText += Eat();
					break;

				default:

					if (IsSpecialMeaningChar(Peek))
					{
						if (Peek == '|')
						{
							currentType = TokenType.Pipeline;
							currentText += Eat();
						}
						if (Peek == '>')
						{
							currentType = TokenType.RedirectOutputSet;
							currentText += Eat();
							if (Peek == '>')
							{
								currentType = TokenType.RedirectOutputAppend;
								currentText += Eat();
							}
						}
						if (Peek == '<')
						{
							currentType = TokenType.RedirectInput;
							currentText += Eat();
						}
						break;
					}

					currentType = TokenType.Argument;
					while (Can)
					{
						if (isInsideQuotes == (char)0) // is not inside quotes
						{
							if (Peek == '"' || Peek == '\'')
							{
								// starting quote
								isInsideQuotes = Eat();
								continue;
							}
							if (argumentsDoSplitByWhiteChar && IsWhiteChar(Peek))
								break;

							if (IsSpecialMeaningChar(Peek))
								break;

							currentText += Eat();
						}
						else // is inside quotes
						{
							if (isInsideQuotes == Peek)
							{
								Eat();
								isInsideQuotes = (char)0;
								break; // end quote, end argument
							}
							currentText += Eat();
						}
					}
					break;
			}

			// skip white chars
			while (Can && IsWhiteChar(Peek)) Eat();
		}
	}

	bool ExecuteCommand_2(Process session, string cmd)
	{
		var tokenizer = new CmdTokenizer(cmd);

		var parts = new List<string>();

		var isSimpleExecution = true;

		while (tokenizer.Can)
		{
			tokenizer.MoveNext();
			if (tokenizer.currentType == CmdTokenizer.TokenType.ProgramName)
				tokenizer.argumentsDoSplitByWhiteChar = DoesProgramWantWholeArgument(tokenizer.currentText);

			var c = tokenizer.currentText;
			if (c == "|" || c == ">" || c == "<") isSimpleExecution = false;
			parts.Add(c);
		}

		if (isSimpleExecution)
		{
			return TryExecute(session, parts.First(), parts.Skip(1).ToArray());
		}

		//terminal.GetWriter().WriteLine("parsed: " + string.Join("#", parts.ToArray()));

		MemoryStream pipe = null;

		TextReader stdIn;
		TextWriter stdOut;
		var arguments = new List<string>();
		var programName = string.Empty;

		var pipeliningType = string.Empty;

		foreach (var p in parts)
		{
			if (p == "|" || p == ">" || p == ">>" || p == "<")
			{
				pipeliningType = p;

				if (pipe == null) stdIn = Console.In;
				else stdIn = new StreamReader(pipe);

				pipe = new MemoryStream();
				stdOut = new StreamWriter(pipe);

				if (TryExecute(session, stdIn, stdOut, programName, arguments.ToArray()) == false) return false;
				programName = string.Empty;
				arguments.Clear();

				stdOut.Flush();
				pipe.Position = 0;

				continue;
			}

			if (programName == string.Empty)
				programName = p;
			else
				arguments.Add(p);
		}

		if (pipeliningType == "|")
		{
			stdIn = new StreamReader(pipe);
			stdOut = Console.Out;
			if (TryExecute(session, stdIn, stdOut, programName, arguments.ToArray()) == false) return false;
		}
		if (pipeliningType == ">")
		{
			var file = Path.GetFullPath(programName);
			using (var r = new StreamReader(pipe))
				File.WriteAllText(file, r.ReadToEnd());
		}

		return true;
	}

	bool DoesProgramWantWholeArgument(string name)
	{
		if (name == "echo") return true;

		return false;
	}

	bool TryExecute(Process session, string name, params string[] arguments)
	{
		var stdIn = Console.In;
		var stdOut = Console.Out;
		return TryExecute(session, stdIn, stdOut, name, arguments);
	}

	bool TryExecute(Process session, TextReader stdIn, TextWriter stdOut, string name, params string[] arguments)
	{
		var builtIn = new BuiltInPrograms();
		builtIn.Initialize(currentUserSession.Config.Clone());
		builtIn.Console.SetIn(stdIn);
		builtIn.Console.SetOut(stdOut);

		var executed = builtIn.TryExecuteInbuilt(name, arguments);

		session.Config.currentDirectory = builtIn.Config.currentDirectory; // this could have changed

		if (executed == false)
		{
			var file = "/bin/" + name;
			if (File.Exists(file))
			{
				var typeFulleName = File.ReadAllText(file);
				var type = Assembly.GetExecutingAssembly().GetType(typeFulleName);
				if (type == null) throw new Error("unable to find type " + typeFulleName);
				var instance = Activator.CreateInstance(type);
				if (instance == null) throw new Error("failed to create instance of " + type);
				var program = (Program)instance;
				program.Initialize(currentUserSession.Config.Clone());
				program.Console.SetIn(stdIn);
				program.Console.SetOut(stdOut);

				program.Main(arguments);
				return true;
			}
		}
		return executed;
	}

	class BuiltInPrograms : Process
	{



		public bool TryExecuteInbuilt(string name, params string[] arguments)
		{
			if (name == "clr" || name == "clear" || name == "cls")
			{
				OperatingSystem.terminal.Clear();
			}
			else if (name == "cd")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (!Directory.Exists(p)) throw new Error("directory '" + p + "' doesnt exist");
				var d = Directory.GetDirEntry(p);
				Environment.CurrentDirectory = d.FullName;
			}
			else if (name == "logout")
			{
				OperatingSystem.terminal.Clear();
			}
			else if (name == "who")
			{
				//To see list of logged in user type who or w command:
				Console.WriteLine(this.Environment.UserName);
			}
			else if (name == "instal")
			{
				var p = "/";
				if (arguments.Length == 1) p = Path.GetFullPath(arguments[0]);
				new InitializeFileSystem().Install(this, p);
			}
			else if (name == "dir" || name == "ls")
			{
				var dirPath = Environment.CurrentDirectory;
				if (arguments.Length > 0)
					dirPath = Path.GetFullPath(arguments[0]);

				var dir = Directory.GetDirEntry(dirPath);
				if (!dir.Exists) throw new Error("directory '" + dir.FullName + "' doesnt exist");

				var ecma48 = new StdLib.Ecma48.Client(Console.Out);
				ecma48.ResetAttributes();

				int counter = 0;
				foreach (var d in dir.EnumerateDirectories())
				{
					counter++;
					ecma48.SetForegroundColor(StdLib.Ecma48.Color.Default);
					Console.Write(d.CreationTime + "  " + d.LastWriteTime);
					ecma48.SetForegroundColor(StdLib.Ecma48.Color.Cyan);
					Console.WriteLine("  D  " + d.Name);
				}
				foreach (var f in dir.EnumerateFiles())
				{
					counter++;
					ecma48.SetForegroundColor(StdLib.Ecma48.Color.Default);
					Console.Write(f.CreationTime + "  " + f.LastWriteTime);
					ecma48.SetForegroundColor(StdLib.Ecma48.Color.Green);
					Console.WriteLine("  F  " + f.Name);
				}
				if (counter == 0)
					Console.WriteLine("'" + dir.fileSystem + "' is empty");

				ecma48.ResetAttributes();
			}
			else if (name == "mkdir")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (File.Exists(p)) throw new Error("'" + p + "' already exists as file");
				if (Directory.Exists(p)) throw new Error("directory '" + p + "' already exists");
				Directory.CreateDirectory(p);
			}
			else if (name == "rm")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (Directory.Exists(p)) throw new Error("'" + p + "' is directory, use rmdir instead");
				if (File.Exists(p)) File.Delete(p);
				else throw new Error("file '" + p + "' doesnt exist");
			}
			else if (name == "rmdir")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (File.Exists(p)) throw new Error("'" + p + "' is file, use rm instead");
				if (Directory.Exists(p)) Directory.Delete(p);
				else throw new Error("directory '" + p + "' doesnt exist");
			}
			else if (name == "touch")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (Directory.Exists(p)) throw new Error("'" + p + "' already exists as directory");
				if (File.Exists(p)) throw new Error("file '" + p + "' already exists");
				File.WriteAllText(p, string.Empty);
			}
			else if (name == "cat")
			{
				if (arguments.Length != 1) throw new Error("one argument required");
				var p = arguments[0];
				if (Directory.Exists(p)) throw new Error("'" + p + "' isnt file");
				if (!File.Exists(p)) throw new Error("file '" + p + "' doesnt exist");
				Console.WriteLine(File.ReadAllText(p));
			}
			else
			{
				return false;
			}
			return true;
		}
	}


	void BeginCommand()
	{
		currentCommand = string.Empty;
		var path = currentUserSession.Environment.CurrentDirectory;
		var homePath = Environment.GetFolderPath(SpecialFolder.Personal);
		if (path.StartsWith(homePath)) path = "~" + path.Substring(homePath.Length);
		Console.Write(currentUserSession.Environment.UserName + "@" + Environment.MachineName + ":" + path + "$ ");
	}
}
