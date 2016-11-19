using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public enum SpecialFolder
{
	Desktop = 0,
	Programs = 2,
	MyDocuments = 5,
	Personal = 5,
	Favorites = 6,
	Startup = 7,
	Recent = 8,
	SendTo = 9,
	StartMenu = 11,
	MyMusic = 13,
	DesktopDirectory = 16,
	MyComputer = 17,
	Templates = 21,
	ApplicationData = 26,
	LocalApplicationData = 28,
	InternetCache = 32,
	Cookies = 33,
	History = 34,
	CommonApplicationData = 35,
	System = 37,
	ProgramFiles = 38,
	MyPictures = 39,
	CommonProgramFiles = 43
}

public class Process
{
	public class ThreadClass : Shortcuts
	{
		public ThreadClass(Process s) : base(s)
		{
		}

		public Thread NewThread(ThreadStart start)
		{
			return OperatingSystem.NewThread(start);
		}
	}
	public ThreadClass Thread { get; private set; }

	public class ConsoleClass : Shortcuts
	{
		TextReader stdIn { get { return Config.stdIn; } set { Config.stdIn = value; } }
		TextWriter stdOut { get { return Config.stdOut; } set { Config.stdOut = value; } }
		TextWriter stdErr { get { return Config.stdErr; } set { Config.stdErr = value; } }

		public ConsoleClass(Process session) : base(session)
		{
		}

		#region fields copied from System.Console

		public TextWriter Error { get { return stdErr; } }
		public TextReader In { get { return stdIn; } }

		public Encoding InputEncoding { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
		public TextWriter Out { get { return stdOut; } }

		public Encoding OutputEncoding { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

		public Stream OpenStandardError()
		{
			throw new NotImplementedException();
		}

		public Stream OpenStandardError(int bufferSize)
		{
			throw new NotImplementedException();
		}

		public Stream OpenStandardInput()
		{
			throw new NotImplementedException();
		}

		public Stream OpenStandardInput(int bufferSize)
		{
			throw new NotImplementedException();
		}

		public Stream OpenStandardOutput()
		{
			throw new NotImplementedException();
		}

		public Stream OpenStandardOutput(int bufferSize)
		{
			throw new NotImplementedException();
		}

		//
		// Summary:
		//     Obtains the next character or function key pressed by the user. The pressed key
		//     is displayed in the console window.
		//
		// Returns:
		//     A System.ConsoleKeyInfo object that describes the System.ConsoleKey constant
		//     and Unicode character, if any, that correspond to the pressed console key. The
		//     System.ConsoleKeyInfo object also describes, in a bitwise combination of System.ConsoleModifiers
		//     values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously
		//     with the console key.
		//
		// Exceptions:
		//   T:System.InvalidOperationException:
		//     The System.Console.In property is redirected from some stream other than the
		//     console.
		public ConsoleKeyInfo ReadKey()
		{
			return ReadKey(false);
		}
		//
		// Summary:
		//     Obtains the next character or function key pressed by the user. The pressed key
		//     is optionally displayed in the console window.
		//
		// Parameters:
		//   intercept:
		//     Determines whether to display the pressed key in the console window. true to
		//     not display the pressed key; otherwise, false.
		//
		// Returns:
		//     A System.ConsoleKeyInfo object that describes the System.ConsoleKey constant
		//     and Unicode character, if any, that correspond to the pressed console key. The
		//     System.ConsoleKeyInfo object also describes, in a bitwise combination of System.ConsoleModifiers
		//     values, whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously
		//     with the console key.
		//
		// Exceptions:
		//   T:System.InvalidOperationException:
		//     The System.Console.In property is redirected from some stream other than the
		//     console.
		public ConsoleKeyInfo ReadKey(bool intercept)
		{
			var c = stdIn.Read();
			if (!intercept)
				Write((char)c);
			return new ConsoleKeyInfo((char)c, (ConsoleKey)c, false, false, false);
		}

		public int Read()
		{
			int first = -1;
			string str = string.Empty;
			while (true)
			{
				var i = ReadKey(true).KeyChar;
				if (first == -1) first = i;
				var c = (char)first;
				if (c == ASCII.BackSpace)
				{
					if (str.Length > 0)
					{
						str = str.Substring(0, str.Length - 1);
						this.Write(ASCII.BackSpace);
					}
				}
				else if (c == ASCII.NewLine || c == ASCII.CarriageReturn) break;
				else
				{
					str += c.ToString();
					this.Write(c);
				}
			}
			return first;
		}

		public string ReadLine()
		{
			var str = string.Empty;
			while (true)
			{
				var c = ReadKey(true).KeyChar;
				if (c == ASCII.BackSpace)
				{
					if (str.Length > 0)
					{
						str = str.Substring(0, str.Length - 1);
						this.Write(ASCII.BackSpace);
					}
				}
				else if (c == ASCII.NewLine || c == ASCII.CarriageReturn) break;
				else
				{
					str += c.ToString();
					this.Write(c);
				}
			}
			return str;
		}

		public void SetError(TextWriter newError)
		{
			stdErr = newError;
		}

		public void SetIn(TextReader newIn)
		{
			stdIn = newIn;
		}

		public void SetOut(TextWriter newOut)
		{
			stdOut = newOut;
		}

		public void Write(int value)
		{
			stdOut.Write(value);
		}

		public void Write(bool value)
		{
			stdOut.Write(value);
		}

		public void Write(char value)
		{
			stdOut.Write(value);
		}

		public void Write(char[] buffer)
		{
			stdOut.Write(buffer);
		}

		public void Write(double value)
		{
			stdOut.Write(value);
		}

		public void Write(uint value)
		{
			stdOut.Write(value);
		}

		public void Write(ulong value)
		{
			stdOut.Write(value);
		}

		public void Write(decimal value)
		{
			stdOut.Write(value);
		}

		public void Write(string value)
		{
			stdOut.Write(value);
		}

		public void Write(float value)
		{
			stdOut.Write(value);
		}

		public void Write(object value)
		{
			stdOut.Write(value);
		}

		public void Write(long value)
		{
			stdOut.Write(value);
		}

		public void Write(string format, params object[] arg)
		{
			stdOut.Write(format, arg);
		}

		public void Write(string format, object arg0)
		{
			stdOut.Write(format, arg0);
		}

		public void Write(string format, object arg0, object arg1)
		{
			stdOut.Write(format, arg0, arg1);
		}

		public void Write(char[] buffer, int index, int count)
		{
			stdOut.Write(buffer, index, count);
		}

		public void Write(string format, object arg0, object arg1, object arg2)
		{
			stdOut.Write(format, arg0, arg1, arg2);
		}

		public void Write(string format, object arg0, object arg1, object arg2, object arg3)
		{
			stdOut.Write(format, arg0, arg1, arg2, arg3);
		}

		public void WriteLine()
		{
			stdOut.WriteLine();
		}

		public void WriteLine(long value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(bool value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(char[] buffer)
		{
			stdOut.WriteLine(buffer);
		}

		public void WriteLine(decimal value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(double value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(int value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(char value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(ulong value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(uint value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(string value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(float value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(object value)
		{
			stdOut.WriteLine(value);
		}

		public void WriteLine(string format, object arg0)
		{
			stdOut.WriteLine(format, arg0);
		}

		public void WriteLine(string format, params object[] arg)
		{
			stdOut.WriteLine(format, arg);
		}

		public void WriteLine(char[] buffer, int index, int count)
		{
			stdOut.WriteLine(buffer, index, count);
		}

		public void WriteLine(string format, object arg0, object arg1)
		{
			stdOut.WriteLine(format, arg0, arg1);
		}

		public void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			stdOut.WriteLine(format, arg0, arg1, arg2);
		}

		public void WriteLine(string format, object arg0, object arg1, object arg2, object arg3)
		{
			stdOut.WriteLine(format, arg0, arg1, arg2, arg3);
		}

		#endregion fields copied from System.Console
	}

	public ConsoleClass Console { get; private set; }

	public class EnvironmentClass : Shortcuts
	{
		public string CommandLine { get { return Config.cmdLineUsedToStart; } }

		public string CurrentDirectory
		{
			get
			{
				return Config.currentDirectory;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					Config.currentDirectory = "/";
					return;
				}
				var path = Path.GetFullPath(value);
				var dir = OperatingSystem.GetDirEntry(path);
				if (!dir.Exists) throw new System.IO.DirectoryNotFoundException("cannot set CurrentDirectory to non existent directory " + dir.FullName);
				Config.currentDirectory = dir.FullName;
			}
		}
		// public string EmbeddingHostName { get; }
		public int ExitCode { get; set; }
		public bool HasShutdownStarted { get { return false; } }
		public string MachineName { get { return OperatingSystem.MachineName; } }
		public string NewLine { get { return "\n"; } }
		// public OperatingSystem OSVersion { get; }
		public int ProcessorCount { get { return 1; } }
		// public bool SocketSecurityEnabled { get { return false; } }
		// public string StackTrace { get; }
		// public int TickCount { get; }
		// public string UserDomainName { get; }
		// public bool UserInteractive { get; }
		public string UserName { get { return Config.userName; } }
		//public Version Version { get; }
		//public long WorkingSet { get; }

		//public void Exit(int exitCode);
		//public string ExpandEnvironmentVariables(string name);
		//public void FailFast(string message);
		public string[] GetCommandLineArgs() { return Config.argsUsedToStart; }
		// public string GetEnvironmentVariable(string variable);
		// public string GetEnvironmentVariable(string variable, EnvironmentVariableTarget target);
		// public IDictionary GetEnvironmentVariables();
		// public IDictionary GetEnvironmentVariables(EnvironmentVariableTarget target);
		public string GetFolderPath(SpecialFolder folder)
		{
			if (folder == SpecialFolder.Personal) return "/home/" + UserName;
			throw new NotImplementedException();
		}
		// public string[] GetLogicalDrives();
		// public void SetEnvironmentVariable(string variable, string value);
		// public void SetEnvironmentVariable(string variable, string value, EnvironmentVariableTarget target);


		public EnvironmentClass(Process session) : base(session)
		{
		}
	}

	public EnvironmentClass Environment { get; private set; }

	public class PathClass : Shortcuts
	{
		public PathClass(Process session) : base(session)
		{
		}

		public readonly char AltDirectorySeparatorChar = '/';
		public readonly char DirectorySeparatorChar = '/';
		public readonly char PathSeparator = '/';
		public readonly char VolumeSeparatorChar = '/';

		readonly char[] invalidPathChars = new char[] {
			// escape characters
			(char)0, (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10, (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20, (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30, (char)31,
		};

		public string ChangeExtension(string path, string extension)
		{
			throw new NotImplementedException();
		}

		public string Combine(params string[] paths)
		{
			var parts = paths.SelectMany(p1 =>
				p1.Split(new[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries).Select(p2 => p2.Trim())
			);
			return "/" + string.Join("/", parts.ToArray());
		}

		public string GetDirectoryName(string path)
		{
			while (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
			var lastSlash = path.LastIndexOf('/');
			if (lastSlash == -1) return path;
			return path.Substring(lastSlash + 1);
		}

		public string GetExtension(string path)
		{
			path = Path.GetFullPath(path);
			var lastSlash = path.LastIndexOf('/');
			if (lastSlash != -1) path = path.Substring(lastSlash + 1);
			var lastDot = path.LastIndexOf('.');
			if (lastDot == -1) return string.Empty;
			return path.Substring(lastDot);
		}

		public string GetFileName(string path)
		{
			while (path.EndsWith("/")) path = path.Substring(0, path.Length - 1);
			var lastSlash = path.LastIndexOf('/');
			if (lastSlash == -1) return path;
			return path.Substring(lastSlash + 1);
		}

		public string GetFileNameWithoutExtension(string path)
		{
			var name = GetFileName(path);
			var extension = GetExtension(path);
			return name.Substring(0, name.Length - extension.Length);
		}

		public string GetFullPath(string path)
		{
			if (path.StartsWith("/")) return path;
			else return Combine(Environment.CurrentDirectory, path);
		}

		public char[] GetInvalidFileNameChars()
		{
			return GetInvalidPathChars();
		}

		public char[] GetInvalidPathChars()
		{
			return invalidPathChars;
		}

		public string GetPathRoot(string path)
		{
			throw new NotImplementedException();
		}

		public string GetRandomFileName()
		{
			throw new NotImplementedException();
		}

		public string GetTempFileName()
		{
			throw new NotImplementedException();
		}

		public string GetTempPath()
		{
			throw new NotImplementedException();
		}

		public bool HasExtension(string path)
		{
			return !string.IsNullOrEmpty(GetExtension(path));
		}

		public bool IsPathRooted(string path)
		{
			return path.StartsWith("/");
		}
	}

	public PathClass Path { get; private set; }

	public class FileClass : Shortcuts
	{
		public FileClass(Process session) : base(session)
		{
		}
		public FileEntry GetFileEntry(string path)
		{
			path = Path.GetFullPath(path);
			return OperatingSystem.GetFileEntry(path);
		}
		public void AppendAllText(string path, string contents)
		{
			throw new NotImplementedException();
		}

		public void AppendAllText(string path, string contents, Encoding encoding)
		{
			throw new NotImplementedException();
		}

		public StreamWriter AppendText(string path)
		{
			return GetFileEntry(path).AppendText();
		}

		public void Copy(string sourceFileName, string destFileName)
		{
			destFileName = Path.GetFullPath(destFileName);
			GetFileEntry(sourceFileName).CopyTo(destFileName);
		}

		public void Copy(string sourceFileName, string destFileName, bool overwrite)
		{
			throw new NotImplementedException();
		}

		public Stream Create(string path)
		{
			return GetFileEntry(path).Create();
		}

		public Stream Create(string path, int bufferSize)
		{
			throw new NotImplementedException();
		}

		public StreamWriter CreateText(string path)
		{
			return GetFileEntry(path).CreateText();
		}

		public void Delete(string path)
		{
			GetFileEntry(path).Delete();
		}

		public bool Exists(string path)
		{
			return GetFileEntry(path).Exists;
		}

		public FileAttributes GetAttributes(string path)
		{
			throw new NotImplementedException();
		}

		public DateTime GetCreationTime(string path)
		{
			return GetFileEntry(path).CreationTime;
		}

		public DateTime GetCreationTimeUtc(string path)
		{
			return GetFileEntry(path).CreationTimeUtc;
		}

		public DateTime GetLastAccessTime(string path)
		{
			return GetFileEntry(path).LastAccessTime;
		}

		public DateTime GetLastAccessTimeUtc(string path)
		{
			return GetFileEntry(path).CreationTimeUtc;
		}

		public DateTime GetLastWriteTime(string path)
		{
			return GetFileEntry(path).LastWriteTime;
		}

		public DateTime GetLastWriteTimeUtc(string path)
		{
			return GetFileEntry(path).LastWriteTimeUtc;
		}

		public void Move(string sourceFileName, string destFileName)
		{
			destFileName = Path.GetFullPath(destFileName);
			GetFileEntry(sourceFileName).MoveTo(destFileName);
		}

		public Stream Open(string path, FileMode mode)
		{
			return GetFileEntry(path).Open(mode);
		}

		public Stream Open(string path, FileMode mode, FileAccess access)
		{
			return GetFileEntry(path).Open(mode, access);
		}

		public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return GetFileEntry(path).Open(mode, access, share);
		}

		public Stream OpenRead(string path)
		{
			return GetFileEntry(path).OpenRead();
		}

		public StreamReader OpenText(string path)
		{
			return GetFileEntry(path).OpenText();
		}

		public Stream OpenWrite(string path)
		{
			return GetFileEntry(path).OpenWrite();
		}

		public byte[] ReadAllBytes(string path)
		{
			return GetFileEntry(path).ReadAllBytes();
		}

		public string[] ReadAllLines(string path)
		{
			return ReadAllLines(path, null);
		}

		public string[] ReadAllLines(string path, Encoding encoding)
		{
			return GetFileEntry(path).ReadAllLines(encoding);
		}

		public string ReadAllText(string path)
		{
			return GetFileEntry(path).ReadAllText();
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			return GetFileEntry(path).ReadAllText(encoding);
		}

		public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
		{
			throw new NotImplementedException();
		}

		public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
		{
			throw new NotImplementedException();
		}

		public void SetAttributes(string path, FileAttributes fileAttributes)
		{
			throw new NotImplementedException();
		}

		public void SetCreationTime(string path, DateTime creationTime)
		{
			throw new NotImplementedException();
		}

		public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			throw new NotImplementedException();
		}

		public void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			throw new NotImplementedException();
		}

		public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			throw new NotImplementedException();
		}

		public void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			throw new NotImplementedException();
		}

		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			throw new NotImplementedException();
		}

		public void WriteAllBytes(string path, byte[] bytes)
		{
			GetFileEntry(path).WriteAllBytes(bytes);
		}

		public void WriteAllLines(string path, string[] contents)
		{
			GetFileEntry(path).WriteAllLines(contents);
		}

		public void WriteAllLines(string path, string[] contents, Encoding encoding)
		{
			GetFileEntry(path).WriteAllLines(contents, encoding);
		}

		public void WriteAllText(string path, string contents)
		{
			GetFileEntry(path).WriteAllText(contents);
		}

		public void WriteAllText(string path, string contents, Encoding encoding)
		{
			GetFileEntry(path).WriteAllText(contents, encoding);
		}
	}

	public FileClass File { get; private set; }

	// https://msdn.microsoft.com/en-us/library/07wt70x2(v=vs.110).aspx
	public class DirectoryClass : Shortcuts
	{
		public DirectoryClass(Process session) : base(session)
		{
		}

		public DirEntry GetDirEntry(string path)
		{
			path = Path.GetFullPath(path);
			return OperatingSystem.GetDirEntry(path);
		}

		public DirEntry CreateDirectory(string path)
		{
			var dir = GetDirEntry(path);
			dir.Create();
			return dir;
		}

		public void Delete(string path)
		{
			GetDirEntry(path).Delete();
		}

		public void Delete(string path, bool recursive)
		{
			if (recursive) Delete(path);
			else throw new NotImplementedException();
		}

		public bool Exists(string path)
		{
			return GetDirEntry(path).Exists;
		}

		public DateTime GetCreationTime(string path)
		{
			return GetDirEntry(path).CreationTime;
		}

		public DateTime GetCreationTimeUtc(string path)
		{
			return GetDirEntry(path).CreationTimeUtc;
		}

		public string GetCurrentDirectory()
		{
			return Environment.CurrentDirectory;
		}

		public string[] GetDirectories(string path)
		{
			return GetDirEntry(path).EnumerateDirectories().Select(f => f.FullName).ToArray();
		}

		public string[] GetDirectories(string path, string searchPattern)
		{
			return GetDirEntry(path).EnumerateDirectories(searchPattern).Select(f => f.FullName).ToArray();
		}

		public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			return GetDirEntry(path).EnumerateDirectories(searchPattern, searchOption).Select(f => f.FullName).ToArray();
		}

		public string GetDirectoryRoot(string path)
		{
			throw new NotImplementedException();
		}

		public string[] GetFiles(string path)
		{
			return GetDirEntry(path).EnumerateFiles().Select(f => f.FullName).ToArray();
		}

		public string[] GetFiles(string path, string searchPattern)
		{
			return GetDirEntry(path).EnumerateFiles(searchPattern).Select(f => f.FullName).ToArray();
		}

		public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return GetDirEntry(path).EnumerateFiles(searchPattern, searchOption).Select(f => f.FullName).ToArray();
		}

		public string[] GetFileSystemEntries(string path)
		{
			return GetDirEntry(path).EnumerateFileSystemInfos().Select(f => f.FullName).ToArray();
		}

		public string[] GetFileSystemEntries(string path, string searchPattern)
		{
			return GetDirEntry(path).EnumerateFileSystemInfos(searchPattern).Select(f => f.FullName).ToArray();
		}

		public DateTime GetLastAccessTime(string path)
		{
			return GetDirEntry(path).LastAccessTime;
		}

		public DateTime GetLastAccessTimeUtc(string path)
		{
			return GetDirEntry(path).LastAccessTimeUtc;
		}

		public DateTime GetLastWriteTime(string path)
		{
			return GetDirEntry(path).LastWriteTime;
		}

		public DateTime GetLastWriteTimeUtc(string path)
		{
			return GetDirEntry(path).LastWriteTimeUtc;
		}

		public string[] GetLogicalDrives()
		{
			throw new NotImplementedException();
		}

		public DirEntry GetParent(string path)
		{
			return GetDirEntry(path).Parent;
		}

		public void Move(string sourceDirName, string destDirName)
		{
			GetDirEntry(sourceDirName).MoveTo(Path.GetFullPath(sourceDirName));
		}

		public void SetCreationTime(string path, DateTime creationTime)
		{
			GetDirEntry(path).CreationTime = creationTime;
		}

		public void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			GetDirEntry(path).CreationTimeUtc = creationTimeUtc;
		}

		public void SetCurrentDirectory(string path)
		{
			path = Path.GetFullPath(path);
			Environment.CurrentDirectory = path;
		}

		public void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			GetDirEntry(path).LastWriteTime = lastAccessTime;
		}

		public void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			GetDirEntry(path).LastAccessTimeUtc = lastAccessTimeUtc;
		}

		public void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			GetDirEntry(path).LastWriteTime = lastWriteTime;
		}

		public void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			GetDirEntry(path).LastWriteTimeUtc = lastWriteTimeUtc;
		}
	}

	public DirectoryClass Directory { get; private set; }

	public class Shortcuts
	{
		protected Process session;

		protected OperatingSystem OperatingSystem { get { return session.OperatingSystem; } }
		protected ConsoleClass Console { get { return session.Console; } }
		protected EnvironmentClass Environment { get { return session.Environment; } }
		protected PathClass Path { get { return session.Path; } }
		protected FileClass File { get { return session.File; } }
		protected DirectoryClass Directory { get { return session.Directory; } }
		protected Configuration Config { get { return session.Config; } }

		public Shortcuts(Process session)
		{
			this.session = session;
		}
	}


	public OperatingSystem OperatingSystem { get { return Config.operatingSystem; } }

	public Configuration Config { get; private set; }

	public class Configuration
	{
		public OperatingSystem operatingSystem;
		public string userName;
		public string cmdLineUsedToStart;
		public string[] argsUsedToStart;
		public string currentDirectory;
		public TextReader stdIn;
		public TextWriter stdOut;
		public TextWriter stdErr;

		public Configuration Clone()
		{
			return (Configuration)MemberwiseClone();
		}
	}

	public void Initialize(Configuration config)
	{
		Config = config;
		Thread = new ThreadClass(this);
		Console = new ConsoleClass(this);
		Environment = new EnvironmentClass(this);
		Path = new PathClass(this);
		File = new FileClass(this);
		Directory = new DirectoryClass(this);
	}
}