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

	public ITerminal terminal;

	public string MachineName { get; private set; }

	Process currentUserSession;

	List<Program> programs = new List<Program>();

	DirEntry rootDirectory;

	Process systemSession;

	public Computer Machine { get; private set; }

	void AddProgram(Program program)
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
			UserInteraction();
		}
		catch (Exception e)
		{
			terminal.GetWriter().Write(e);
			UnityEngine.Debug.LogException(e);
		}
	}




	void SetupSystem()
	{
		terminal = Machine.GetFirstDeviceOfType<ITerminal>();

		rootDirectory = DirEntry.MakeRoot();

		var dev = rootDirectory.CreateSubdirectory("dev");
		var devFs = new DevDirFileSystem(dev);
		dev.fileSystem = devFs;

		var cfg = new Process.Configuration();
		cfg.operatingSystem = this;
		cfg.userName = "system";
		cfg.stdIn = terminal.GetReader();
		cfg.stdErr = cfg.stdOut = terminal.GetWriter();
		cfg.currentDirectory = "/";
		systemSession = new Process();
		systemSession.Initialize(cfg);

		foreach (var d in Machine.Devices) devFs.AddDevice(d);

		Mount("/dev/sda", "/", "csv");

		/*
		var systemShell = new Shell();
		var systemShellInput = new TextReader();
		systemShell.InheritFrom(systemSession);
		systemShell.Initialize(this, systemSession.Config);
		systemShell.Console.SetIn(systemShellInput);
		systemShell.Console.SetOut(new null);
		systemShell.Console.SetError(new null);
		systemShell.Main(new string[0]);
		systemShellInput.WriteLine("mnt /dev/sda /");
		*/

		rootDirectory.Refresh();
	}

	public void Mount(string device, string target, string fileSystem = "csv")
	{
		var deviceFile = GetFileEntry(device);
		var toDir = GetDirEntry(target);

		if (fileSystem == "csv") toDir.fileSystem = new CsvFileSystem(toDir, deviceFile);
		toDir.Refresh();
	}

	void UserInteraction()
	{
		terminal.Clear();

		var w = terminal.GetWriter();

		w.WriteLine("niOS [version 0.0.1.0a]");
		w.WriteLine("(c) 2016 Neitri Industries. All rights reserved.");
		w.WriteLine(DateTime.UtcNow);
		//.WriteLine("This software is protected by following patents US14761 NI4674765 EU41546 US145-7756 US765-577")
		//.WriteLine("Any unauthorized reproduction of this software is strictly prohibited.")
		w.WriteLine();

		while (true)
		{
			SystemSanityCheck();

			if (currentUserSession == null || string.IsNullOrEmpty(currentUserSession.Environment.UserName))
				Authenticate();

			var shell = new Shell();
			shell.Initialize(currentUserSession.Config.Clone());
			shell.Main(new string[] { });
		}
	}


	void SystemSanityCheck()
	{
		if (!systemSession.Directory.Exists("/bin")) systemSession.Console.WriteLine("warning: /bin not found, some programs will not be available");
	}

	// https://www.digitalocean.com/community/tutorials/how-to-use-passwd-and-adduser-to-manage-passwords-on-a-linux-vps
	void Authenticate()
	{
		//FindOrCreateFile("/etc/passwd").WriteAllText("test");
		//var b = FindOrCreateFile("/etc/passwd").ReadAllText();

		var csv = CSV.FromLines(GetFileEntry("/etc/passwd").ReadAllLinesEmptyIfDoesntExist());

		var w = terminal.GetWriter();

		string userName;
		enterUserName:
		w.Write("log in as user: ");
		
		userName = Utils.SanitizeInput(systemSession.Console.ReadLine());
		w.WriteLine();
		if (userName.Length < 3 || !Regex.IsMatch(userName, @"^[\w\d]+$"))
		{
			w.WriteLine("user name must be minimum of 3 characters long and must contain only characters and numbers");
			goto enterUserName;
		}

		var userExists = csv.Any(l => l[0] == userName);

		if (userExists)
		{
			enterPassword:
			w.Write("enter password: ");
			var pass = EnterPassword();
			w.WriteLine();
			if (csv.Any(l => l[1] == pass) == false)
			{
				w.WriteLine("wrong password, try again");
				goto enterPassword;
			}
		}
		else
		{
			w.WriteLine("user does not exist, creating new user");
			enterNewPassword:
			w.Write("enter new password: ");
			var pass = EnterPassword();
			w.WriteLine();
			w.Write("enter new password again: ");
			var passAgain = EnterPassword();
			w.WriteLine();
			if (pass != passAgain)
			{
				w.WriteLine("passwords do not match, try again");
				goto enterNewPassword;
			}

			csv.Add(userName, pass);
			GetFileEntry("/etc/passwd").WriteAllLines(csv.GetLinesToSave());
		}

		var cfg = systemSession.Config.Clone();
		cfg.userName = userName;
		currentUserSession = new Process();
		currentUserSession.Initialize(cfg);

		var p = currentUserSession.Environment.GetFolderPath(SpecialFolder.Personal);
		if (!currentUserSession.Directory.Exists(p)) currentUserSession.Directory.CreateDirectory(p);
		currentUserSession.Environment.CurrentDirectory = p;


		w.WriteLine("logged in as user " + currentUserSession.Environment.UserName);
		w.WriteLine();
	}

	string EnterPassword()
	{
        var password = systemSession.Console.ReadPassword('*');
		return Utils.GetStringSha256Hash(password);
	}
}
