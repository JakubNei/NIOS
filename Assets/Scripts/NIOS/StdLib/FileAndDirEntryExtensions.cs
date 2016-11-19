using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StdLib
{
	public static class FileAndDirEntryExtensions
	{
		public static string[] ReadAllLinesEmptyIfDoesntExist(this FileEntry file)
		{
			if (file.Exists) return file.ReadAllLines();
			return new string[] { };
		}

		public static FileEntry  MakeSureExists(this FileEntry file)
		{
			if (!file.Exists)
				file.Create();
			return file;
		}
	}
}
