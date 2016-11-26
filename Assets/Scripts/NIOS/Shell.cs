using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class Shell : ProgramBase
{
	string currentCommand;
	bool shouldContinue = true;

	const string version = "1.0a";

	public override void Main(string[] arguments)
	{

		var p = Environment.GetFolderPath(SpecialFolder.Personal);
		if (!Directory.Exists(p)) Directory.CreateDirectory(p);
		Environment.CurrentDirectory = p;

		Console.WriteLine("Welcome to Bourne-like shell version " + version);

		while (shouldContinue)
		{
			BeginCommand();
			var line = Utils.SanitizeInput(Console.ReadLine());


			//UnityEngine.Debug.Log("executing " + line);
			var t = Thread.NewThread(() =>
			{
				ExecuteCommand(Session, line);
			});
			t.Start();
			var started = World.UtcNow;
			while (t != null && t.IsAlive)
			{
				if (started.AddSeconds(10) < World.UtcNow)
				{
					t.Abort();
					t = null;
					Console.WriteLine("command execution took over 10 second, aborting");
				}
			}
		}
	}


	public void ExecuteCommand(Session session, string cmd)
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
					while (Can && !Peek.IsWhiteSpace() && !IsSpecialMeaningChar(Peek))
						currentText += Eat();
					break;

				case TokenType.ProgramName:
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
							if (argumentsDoSplitByWhiteChar && Peek.IsWhiteSpace())
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
			while (Can && Peek.IsWhiteSpace()) Eat();
		}
	}

	bool ExecuteCommand_2(Session session, string cmd)
	{
		var tokenizer = new CmdTokenizer(cmd);

		var parts = new List<string>();

		var isSimpleExecution = true;

		while (tokenizer.Can)
		{
			tokenizer.MoveNext();

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

	bool TryExecute(Session session, string name, params string[] arguments)
	{
		var stdIn = Console.In;
		var stdOut = Console.Out;
		return TryExecute(session, stdIn, stdOut, name, arguments);
	}

	bool TryExecute(Session session, TextReader stdIn, TextWriter stdOut, string name, params string[] arguments)
	{
		var executed = TryExecuteInbuilt(name, arguments);

		if (executed == false)
		{
			var file = "/bin/" + name;
			if (File.Exists(file))
			{
				var p = this.Process.NewProcess();
				p.Session.stdIn = stdIn;
				p.Session.stdOut = stdOut;
				p.Start(file, arguments);

				return true;
			}
		}
		return executed;
	}


	bool TryExecuteInbuilt(string name, params string[] arguments)
	{
		if (name == "clr" || name == "clear" || name == "cls")
		{
			var client = new StdLib.Ecma48.Client(Console.Out);
			client.EraseDisplay();
		}
		else if (name == "help" || name == "man" || name == "?")
		{
			var d = Directory.GetDirEntry("/bin/");
			Console.WriteLine("list of programs or command you can run:");
			Console.WriteLine("\tshell commands:");
			foreach (var i in new string[] { "clr", "help", "cd", "logout", "who", "instal" }) Console.WriteLine("\t\t" + i);
			Console.WriteLine("\tprograms from " + d.FullName + ":");
			foreach (var f in d.EnumerateFiles()) Console.WriteLine("\t\t" + f.Name);
		}
		else if (name == "cd")
		{
			if (arguments.Length != 1) throw new Error("one argument required");
			var p = arguments[0];
			var d = Directory.GetDirEntry(p);
			if (!d.Exists) throw new Error("directory '" + p + "' ('" + d.FullName + "') doesnt exist");
			Environment.CurrentDirectory = d.FullName;
		}
		else if (name == "logout")
		{
			TryExecuteInbuilt("clear", null);
			shouldContinue = false;
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
			new InitializeFileSystem().Install(Session, p);
		}
		else
		{
			return false;
		}
		return true;
	}

	void BeginCommand()
	{
		currentCommand = string.Empty;
		var path = Environment.CurrentDirectory;
		var homePath = Environment.GetFolderPath(SpecialFolder.Personal);
		if (path.StartsWith(homePath)) path = "~" + path.Substring(homePath.Length);
		Console.Write(Environment.UserName + "@" + Environment.MachineName + ":" + path + "$ ");
	}
}
