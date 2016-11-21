using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StdLib
{
	public static class Extensions
	{
		public static string[] ReadAllLinesEmptyIfDoesntExist(this FileEntry file)
		{
			if (file.Exists) return file.ReadAllLines();
			return new string[] { };
		}

		public static FileEntry MakeSureExists(this FileEntry file)
		{
			if (!file.Exists)
				file.Create();
			return file;
		}
		public static string ReadPassword(this Session.ConsoleClass console, char replacingChar = '*')
		{
			var str = string.Empty;
			while (true)
			{
				var c = console.ReadKey(true).KeyChar;
				if (c == ASCII.BackSpace)
				{
					if (str.Length > 0)
					{
						str = str.Substring(0, str.Length - 1);
						console.Write(ASCII.BackSpace);
					}
				}
				else if (c == ASCII.NewLine || c == ASCII.CarriageReturn) break;
				else
				{
					str += c.ToString();
					console.Write(replacingChar);
				}
			}
			return str;
		}

	}
}
