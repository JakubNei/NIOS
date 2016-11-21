using StdLib;
using System;

public class InitializeFileSystem
{


	void IsClass(FileEntry file, Type type)
	{
		file.WriteAllText("make_c_sharp_instance_of:" + type.FullName);
	}

	public void Install(Session s, string dirPath)
	{
		var root = s.Directory.GetDirEntry(dirPath);

		s.Console.WriteLine("installing nios to " + root.FullName);

		// http://www.thegeekstuff.com/2010/09/linux-file-system-structure/?utm_source=tuicool
		var bin = root.CreateSubdirectory("bin");
		{
			IsClass(bin.GetFileEntry("sh"), typeof(Shell));
			IsClass(bin.GetFileEntry("echo"), typeof(EchoProgram));
			IsClass(bin.GetFileEntry("grep"), typeof(GrepProgram));
			IsClass(bin.GetFileEntry("pwd"), typeof(PwdProgram));
			IsClass(bin.GetFileEntry("sleep"), typeof(SleepProgram));
			IsClass(bin.GetFileEntry("date"), typeof(DateProgram));
			IsClass(bin.GetFileEntry("mkdir"), typeof(MkDirProgram));
			IsClass(bin.GetFileEntry("rm"), typeof(RmProgram));
			IsClass(bin.GetFileEntry("rmdir"), typeof(RmDirProgram));
			IsClass(bin.GetFileEntry("cat"), typeof(CatProgram));
			IsClass(bin.GetFileEntry("touch"), typeof(TouchProgram));
			IsClass(bin.GetFileEntry("ls"), typeof(LsProgram));
			IsClass(bin.GetFileEntry("brute"), typeof(BruteForceAttackPasswdProgram));
		}
		var sbin = root.CreateSubdirectory("sbin");
		var etc = root.CreateSubdirectory("etc");
		{
			etc.GetFileEntry("passwd").MakeSureExists();
			etc.GetFileEntry("shadow").MakeSureExists();
		}
		var dev = root.CreateSubdirectory("dev");
		var proc = root.CreateSubdirectory("proc");
		var var = root.CreateSubdirectory("var");
		{
			var var_log = var.GetDirEntry("log");
			var var_lib = var.GetDirEntry("lib");
			var var_tmp = var.GetDirEntry("tmp");
		}
		var tmp = root.CreateSubdirectory("tmp");

		var usr = root.CreateSubdirectory("usr");
		var home = root.CreateSubdirectory("home");
		var boot = root.CreateSubdirectory("boot");

		var nios = boot.GetFileEntry("nios.img-0.0.0.1a");
		nios.WriteAllBytes(OperatingSystem.bootSectorBytes);

		var lib = root.CreateSubdirectory("lib");
		/*foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
		{
			FileEntry f = lib.CreateFile(a.GetName().Name.ToLower());
			f.WriteAllText(a.FullName);
		}*/

		var opt = root.CreateSubdirectory("opt");
		var mnt = root.CreateSubdirectory("mnt");
		var media = root.CreateSubdirectory("media");
		var srv = root.CreateSubdirectory("srv");

		s.Console.WriteLine("installation done");
	}
}