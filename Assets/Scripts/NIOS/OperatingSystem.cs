using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using StdLib;

public interface IBootSectorProgram
{
	void StartUp(Computer machine);
}

public partial class OperatingSystem : IBootSectorProgram
{
	public static readonly byte[] bootSectorBytes = Utils.ProgramContents(645, 100);
	public static readonly string bootSectorBase64 = Convert.ToBase64String(bootSectorBytes);

	public string MachineName { get; private set; }

	List<ProgramBase> programs = new List<ProgramBase>();

	DirEntry rootDirectory;

	Session systemSession;

	public Computer Machine { get; private set; }

	void AddProgram(ProgramBase program)
	{
		programs.Add(program);
	}

	public void StartUp(Computer machine)
	{
		this.Machine = machine;

		MachineName = "vm0387";

		NewThread(Main).Start();
	}

	public Thread NewThread(ThreadStart start)
	{
		return Machine.CreateThread(start);
	}

	void Main()
	{
		try
		{
			SetupSystem();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogException(e);
			systemSession.Console.WriteLine();
			systemSession.Console.WriteLine(e);
		}
	}


	public class Process
	{
		public OperatingSystem operatingSystem;
		public Session.Configuration Session { get; set; }
		public void Start(string filePath, params string[] args)
		{
			var data = operatingSystem.systemSession.File.ReadAllText(filePath);

			const string t = "make_c_sharp_instance_of:";
			if (data.StartsWith(t))
			{
				var typeName = data.Substring(t.Length);
				var type = Assembly.GetExecutingAssembly().GetType(typeName);
				if (type == null) throw new Error("unable to find type " + data);
				var instance = Activator.CreateInstance(type);
				if (instance == null) throw new Error("failed to create instance of " + type);
				var program = (ProgramBase)instance;
				program.Initialize(Session);
				program.Main(args);
			}
			else
			{
				throw new Error(filePath + ", has unexpected data, can not start");
			}
		}
	}

	public Process NewProcess()
	{
		return new Process() { operatingSystem = this };
	}


	void SetupSystem()
	{
		rootDirectory = DirEntry.MakeRoot();

		var dev = rootDirectory.CreateSubdirectory("dev");
		var devFs = new DevDirFileSystem(dev);
		dev.fileSystem = devFs;
		dev.isMountPoint = true;
		dev.Refresh();

		foreach (var d in Machine.Devices) devFs.AddDevice(d);

		Mount("/dev/sda", "/", "csv");

		var terminal = GetFileEntry("/dev/tty1");

		var cfg = new Session.Configuration();
		cfg.operatingSystem = this;
		cfg.userName = "system";

		if (terminal.Exists)
		{
			cfg.stdIn = terminal.OpenText();
			cfg.stdErr = cfg.stdOut = new StreamWriter(terminal.OpenWrite()) { AutoFlush = true };
		}
		else
		{
			cfg.stdIn = new StreamReader(new MemoryStream(new byte[] { }));
			var stdOut = GetFileEntry("/tmp/std-out-" + DateTime.UtcNow.Ticks + ".txt");
			cfg.stdOut = new StreamWriter(stdOut.OpenWrite()) { AutoFlush = true };
			var stdErr = GetFileEntry("/tmp/std-err-" + DateTime.UtcNow.Ticks + ".txt");
			cfg.stdErr = new StreamWriter(stdErr.OpenWrite()) { AutoFlush = true };
		}

		cfg.currentDirectory = "/";
		systemSession = new Session();
		systemSession.Initialize(cfg);

		if(!systemSession.File.Exists("/bin/sh"))
		{
			systemSession.Console.WriteLine("/bin/sh not found, re/installing system");
			new InitializeFileSystem().Install(systemSession, "/");
		}

		if (terminal.Exists)
			UserInteraction();
	}

	public void Mount(string device, string target, string fileSystem = "csv")
	{
		var deviceFile = GetFileEntry(device);
		var toDir = GetDirEntry(target);

		if (fileSystem == "csv") toDir.fileSystem = new CsvFileSystem(toDir, deviceFile);
		toDir.isMountPoint = true;
		toDir.Refresh();
	}

	void UserInteraction()
	{
		systemSession.Console.WriteLine("niOS [version 0.0.1.0a]");
		systemSession.Console.WriteLine("(c) 2016 Neitri Industries. All rights reserved.");
		systemSession.Console.WriteLine(DateTime.UtcNow);
		//.WriteLine("This software is protected by following patents US14761 NI4674765 EU41546 US145-7756 US765-577")
		//.WriteLine("Any unauthorized reproduction of this software is strictly prohibited.")
		systemSession.Console.WriteLine();

		while (true)
		{
			SystemSanityCheck();

			var currentUserSession = AuthenticateNewSession();

			var shell = NewProcess();
			shell.Session = currentUserSession.Config.Clone();
			shell.Start("/bin/sh");
		}
	}


	void SystemSanityCheck()
	{
		if (!systemSession.Directory.Exists("/bin")) systemSession.Console.WriteLine("warning: /bin not found, some programs will not be available");
	}

	// https://www.digitalocean.com/community/tutorials/how-to-use-passwd-and-adduser-to-manage-passwords-on-a-linux-vps
	Session AuthenticateNewSession()
	{
		//FindOrCreateFile("/etc/passwd").WriteAllText("test");
		//var b = FindOrCreateFile("/etc/passwd").ReadAllText();

		var csv = CSV.FromLines(GetFileEntry("/etc/passwd").ReadAllLinesEmptyIfDoesntExist());

		string userName;
		enterUserName:
		systemSession.Console.Write("log in as user: ");

		userName = Utils.SanitizeInput(systemSession.Console.ReadLine());
		systemSession.Console.WriteLine();
		if (userName.Length < 3 || !Regex.IsMatch(userName, @"^[\w\d]+$"))
		{
			systemSession.Console.WriteLine("user name must be minimum of 3 characters long and must contain only characters and numbers");
			goto enterUserName;
		}

		var userExists = csv.Any(l => l[0] == userName);

		if (userExists)
		{
			enterPassword:
			systemSession.Console.Write("enter password: ");
			var pass = EnterPassword();
			systemSession.Console.WriteLine();
			if (csv.Any(l => l[1] == pass) == false)
			{
				systemSession.Console.WriteLine("wrong password, try again");
				goto enterPassword;
			}
		}
		else
		{
			systemSession.Console.WriteLine("user does not exist, creating new user");
			enterNewPassword:
			systemSession.Console.Write("enter new password: ");
			var pass = EnterPassword();
			systemSession.Console.WriteLine();
			systemSession.Console.Write("enter new password again: ");
			var passAgain = EnterPassword();
			systemSession.Console.WriteLine();
			if (pass != passAgain)
			{
				systemSession.Console.WriteLine("passwords do not match, try again");
				goto enterNewPassword;
			}

			csv.Add(userName, pass);
			GetFileEntry("/etc/passwd").WriteAllLines(csv.GetLinesToSave());
		}

		var cfg = systemSession.Config.Clone();
		cfg.userName = userName;
		var currentUserSession = new Session();
		currentUserSession.Initialize(cfg);

		var p = currentUserSession.Environment.GetFolderPath(SpecialFolder.Personal);
		if (!currentUserSession.Directory.Exists(p)) currentUserSession.Directory.CreateDirectory(p);
		currentUserSession.Environment.CurrentDirectory = p;

		systemSession.Console.WriteLine("logged in as user " + currentUserSession.Environment.UserName);
		systemSession.Console.WriteLine();

		return currentUserSession;
	}

	string EnterPassword()
	{
		var password = systemSession.Console.ReadPassword('*');
		return Utils.GetStringSha256Hash(password);
	}
}
