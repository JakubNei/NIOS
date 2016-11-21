using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public abstract class ProgramBase : Session
{
	public abstract void Main(string[] arguments);
}

public class GrepProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument expected");
		var find = arguments[0].Trim();
		string line;
		while ((line = Console.ReadLine()) != null)
		{
			if (line.Contains(find))
				Console.WriteLine(line);
		}
	}
}

public class EchoProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument expected");
		Console.WriteLine(arguments[0]);
	}
}

public class PwdProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		Console.WriteLine(Environment.CurrentDirectory);
	}
}

// http://www.computerhope.com/unix/usleep.htm
public class SleepProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one number argument expected");
		double seconds;
		if (!double.TryParse(arguments[0], out seconds)) throw new Error(arguments[0] + " is not number");
		System.Threading.Thread.Sleep((int)(seconds * 1000));
	}
}

// http://www.computerhope.com/unix/udate.htm
public class DateProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Contains("-u") || arguments.Contains("--utc") || arguments.Contains("--univeral")) Console.WriteLine(DateTime.UtcNow);
		Console.WriteLine(DateTime.Now);
	}
}


public class LsProgram : ProgramBase
{
	public override void Main(string[] arguments)
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
			Console.WriteLine("'" + dir.FullName + "' is empty");

		ecma48.ResetAttributes();
	}
}

public class MkDirProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument required");
		var p = arguments[0];
		if (File.Exists(p)) throw new Error("'" + p + "' already exists as file");
		if (Directory.Exists(p)) throw new Error("directory '" + p + "' already exists");
		Directory.CreateDirectory(p);
	}
}

public class RmProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument required");
		var p = arguments[0];
		if (Directory.Exists(p)) throw new Error("'" + p + "' is directory, use rmdir instead");
		if (File.Exists(p)) File.Delete(p);
		else throw new Error("file '" + p + "' doesnt exist");
	}
}

public class RmDirProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument required");
		var p = arguments[0];
		if (File.Exists(p)) throw new Error("'" + p + "' is file, use rm instead");
		if (Directory.Exists(p)) Directory.Delete(p);
		else throw new Error("directory '" + p + "' doesnt exist");
	}
}

public class TouchProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument required");
		var p = arguments[0];
		if (Directory.Exists(p)) throw new Error("'" + p + "' already exists as directory");
		if (File.Exists(p)) throw new Error("file '" + p + "' already exists");
		File.WriteAllText(p, string.Empty);
	}
}

public class CatProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		if (arguments.Length != 1) throw new Error("one argument required");
		var p = arguments[0];
		if (Directory.Exists(p)) throw new Error("'" + p + "' isnt file");
		if (!File.Exists(p)) throw new Error("file '" + p + "' doesnt exist");
		Console.WriteLine(File.ReadAllText(p));
	}
}



public class BruteForceAttackPasswdProgram : ProgramBase
{
	public override void Main(string[] arguments)
	{
		int argMaxLength = -1;
		string argMask = null;
		if (arguments.Length == 1)
		{
			if (arguments[0] == "help")
			{
				WriteHelp();
				return;
			}
			if (!int.TryParse(arguments[0], out argMaxLength))
				argMask = arguments[0];
		}
		else
		{
			WriteHelp();
			return;
		}
		Console.WriteLine("starting brute force attack onto /etc/passwd");

		using (var r = new StreamReader(File.OpenRead("/etc/passwd")))
		{
			string line;
			while ((line = r.ReadLine()) != null)
			{
				var csv = line.Split(',');
				var hashedPassword = csv[1];

				string plainPassword = null;
				if (argMask == null)
				{
					var mask = "?";
					do
					{
						plainPassword = Attack(hashedPassword, mask);
						mask += "?";
					} while (plainPassword == null && mask.Length <= argMaxLength);
				}
				else
				{
					plainPassword = Attack(hashedPassword, argMask);
				}

				Console.Write(line);
				if (plainPassword == null) Console.Write("  not_found");
				else Console.Write("  found=" + plainPassword);
				Console.WriteLine();
			}
		}
	}

	string Attack(string hashedPassword, string mask)
	{
		foreach (var plainPassword in Combinations(mask))
		{
			if (hashedPassword == Utils.GetStringSha256Hash(plainPassword))
				return plainPassword;
		}
		return null;
	}

	void WriteHelp()
	{
		Console.WriteLine("attempts brute force attack on /etc/passwd");
		Console.WriteLine("first argument is password mask or max password length");
		Console.WriteLine("password mask consists of these characters:");
		Console.WriteLine("\t c = lower case characters");
		Console.WriteLine("\t C = upper case characters");
		Console.WriteLine("\t d = digit, number characters");
		Console.WriteLine("\t s = special characters: !$#@-./*\\");
		Console.WriteLine("\t ? = all above");
		Console.WriteLine("example mask matching passwords of 1 number and 3 lower case letters: dccc");
	}

	//Charset call for each position of computed pass
	string CharSet(char charsetMask)
	{
		switch (charsetMask)
		{
			case 'c': return "abcdefghijklmnopqrstuvwxyz";
			case 'C': return "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			case 'd': return "0123456789";
			case 's': return "!$#@-./*\\";
			case '?': return "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!$#@-./*\\";

			default: throw new InvalidOperationException("bad charset mask: " + charsetMask + ", see help for more info");
		}
	}

	//Return computed pass on defined charset, base on given mask
	IEnumerable<string> Combinations(string mask)
	{
		var sets = mask.Select<char, string>(CharSet).Cast<IEnumerable<char>>();
		return Combine(sets).Select(x => new string(x.ToArray()));
	}

	//Compute new combination throw LINQ
	IEnumerable<IEnumerable<T>> Combine<T>(IEnumerable<IEnumerable<T>> sequences)
	{
		IEnumerable<IEnumerable<T>> seq = new[] { Enumerable.Empty<T>() };

		return sequences.Aggregate(
			seq,
			(accumulator, sequence) =>
				from accseq in accumulator
				from item in sequence
				select accseq.Concat(new[] { item }));
	}
}


