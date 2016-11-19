using StdLib;

public class InitializeFileSystem
{


	public void Install(Process s, string dirPath)
	{
		var root = s.Directory.GetDirEntry(dirPath);

		s.Console.WriteLine("installing os to " + root.FullName);

		// http://www.thegeekstuff.com/2010/09/linux-file-system-structure/?utm_source=tuicool
		var bin = root.CreateSubdirectory("bin");
		{
			bin.GetFileEntry("echo").WriteAllText(typeof(EchoProgram).FullName);
			bin.GetFileEntry("grep").WriteAllText(typeof(GrepProgram).FullName);
			bin.GetFileEntry("pwd").WriteAllText(typeof(PwdProgram).FullName);
			bin.GetFileEntry("sleep").WriteAllText(typeof(SleepProgram).FullName);
			bin.GetFileEntry("date").WriteAllText(typeof(DateProgram).FullName);
			bin.GetFileEntry("brute").WriteAllText(typeof(BruteForceAttackPasswdProgram).FullName);
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