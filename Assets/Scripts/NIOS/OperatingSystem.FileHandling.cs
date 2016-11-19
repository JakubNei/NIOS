using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

public partial class OperatingSystem : IBootSectorProgram
{
	/*
	public bool IsValidPathPart(string path)
	{
		return Regex.IsMatch(path, @"^[\w\d _-=+#!")
	}
	*/


	//
	// Summary:
	//     Initializes a new instance of the System.IO.DirectoryInfo class on the specified
	//     path.
	//
	// Parameters:
	//   path:
	//     A string specifying the path on which to create the DirectoryInfo.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     path is null.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	//
	//   T:System.ArgumentException:
	//     path contains invalid characters such as ", <, >, or |.
	//
	//   T:System.IO.PathTooLongException:
	//     The specified path, file name, or both exceed the system-defined maximum length.
	//     For example, on Windows-based platforms, paths must be less than 248 characters,
	//     and file names must be less than 260 characters. The specified path, file name,
	//     or both are too long.

	public DirEntry GetDirEntry(string absolutePath)
	{
		var dir = rootDirectory;
		var parts = absolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < parts.Length; i++)
		{
			var part = parts[i];
			if (part == "..")
			{
				if (dir.Parent != null) dir = dir.Parent;
			}
			else if (part == ".") continue;
			else dir = dir.GetDirEntry(part);
		}
		return dir;
	}

	public FileEntry GetFileEntry(string absolutePath)
	{
		var fileName = absolutePath;
		var lastSlash = absolutePath.LastIndexOf('/');

		var dirPath = absolutePath.Substring(0, lastSlash);
		fileName = absolutePath.Substring(lastSlash + 1);

		var dir = GetDirEntry(dirPath);
		var file = dir.GetFileEntry(fileName);
		return file;
	}







	string GetAbsolutePathFrom(DirEntry workingDirectory, string relativePath)
	{
		if (relativePath.StartsWith("/")) return rootDirectory.FullName + relativePath.Substring(1);
		if (relativePath.StartsWith("~")) return currentUserSession.Environment.GetFolderPath(SpecialFolder.Personal) + relativePath.Substring(1);
		return workingDirectory.FullName + relativePath;
	}
}