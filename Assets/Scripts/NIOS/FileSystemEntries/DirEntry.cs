using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class DirectorySecurity
{

}

/// <summary>
/// for help see linux system directory entry structure http://www.makelinux.net/books/lkd2/ch12lev1sec7
/// </summary>
public class DirEntry : FileSystemEntry // DirectoryInfo
{
	public override string FullName
	{
		get
		{
			if (Parent == null) return "/";
			return Parent.FullName + Name + "/";
		}
	}

	/// <summary>
	/// If true, then this directory will not by touched by file systems.
	/// </summary>
	public bool isMountPoint = false;
	bool initialized = false;
	bool exists = false;
	DirEntry parent;

	List<DirEntry> directories = new List<DirEntry>();
	List<DirEntry> Directories
	{
		get
		{
			TryInitialize();
			return directories;
		}
	}
	List<FileEntry> files = new List<FileEntry>();
	List<FileEntry> Files
	{
		get
		{
			TryInitialize();
			return files;
		}
	}
	IEnumerable<FileSystemEntry> fileSystemEntries { get { return Directories.Cast<FileSystemEntry>().Concat(Files.Cast<FileSystemEntry>()); } }


	public static DirEntry MakeRoot()
	{
		return new DirEntry("root", null);
	}

	public DirEntry(string name, DirEntry parent, bool exists = false) : base(name, parent)
	{
		this.parent = parent;
		this.exists = exists;
	}


	public string GetRelativePathTo(FileSystemEntry other)
	{
		if (!Exists) throw new Error(FullName + " doesnt exist anymore");
		var me = this.FullName;
		var ot = other.FullName;

		if (me == ot) return ".";

		if (!ot.StartsWith(me))
			throw new Exception(this.FullName + " cannot get relative path to " + other.FullName);

		var p = ot.Substring(me.Length);
		if (p.StartsWith("/")) p = p.Substring(1);
		return p;
	}


	void ThrowIfEntryAlreadyExists(string name)
	{
		if (Directories.Any(d => d.Name == name))
			throw new System.IO.IOException(name + " already exists, its a directory in " + this.FullName);
		if (Files.Any(f => f.Name == name))
			throw new System.IO.IOException(name + " already exists, its a file in " + this.FullName);
	}


	public class UpdateHandle
	{
		public DirEntry DirEntry { get; private set; }
		public List<DirEntry> Directories { get { return DirEntry.directories; } }
		public List<FileEntry> Files { get { return DirEntry.files; } }
		DirEntry[] mountPoints;
		public UpdateHandle(DirEntry dirEntry)
		{
			DirEntry = dirEntry;
			mountPoints = Directories.Where(d => d.isMountPoint).ToArray();
			Directories.Clear();
			Files.Clear();
		}
		public void AddDirectory(string name)
		{
			var d = new DirEntry(name, DirEntry);
			d.exists = true;
			DirEntry.directories.Add(d);
		}
		public void AddFile(string name)
		{
			var f = new FileEntry(name, DirEntry, true);
			DirEntry.files.Add(f);
		}
		public void Finished()
		{
			Directories.RemoveAll(d => mountPoints.Any(m => m.Name == d.Name));
			Directories.AddRange(mountPoints);
		}
	}

	void TryInitialize()
	{
		if (initialized) return;
		initialized = true;
		if (fileSystem != null)
		{
			var u = new UpdateHandle(this);
			fileSystem.UpdateDirectoryInfo(u);
			u.Finished();
		}
	}

	public FileEntry GetFileEntry(string name)
	{
		var file = Files.FirstOrDefault(f => f.Name == name);
		if (file == null)
			file = new FileEntry(name, this);

		return file;
	}
	public DirEntry GetDirEntry(string name)
	{
		var dir = Directories.FirstOrDefault(d => d.Name == name);
		if (dir == null)
			dir = new DirEntry(name, this);

		return dir;
	}




	//
	// Summary:
	//     Gets a value indicating whether the directory exists.
	//
	// Returns:
	//     true if the directory exists; otherwise, false.
	public override bool Exists { get { return exists; } }

	//
	// Summary:
	//     Gets the name of this System.IO.DirectoryInfo instance.
	//
	// Returns:
	//     The directory name.
	public override string Name { get { return base.Name; } }

	//
	// Summary:
	//     Gets the parent directory of a specified subdirectory.
	//
	// Returns:
	//     The parent directory, or null if the path is null or if the file path denotes
	//     a root (such as "\", "C:", or * "\\server\share").
	//
	// Exceptions:
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public DirEntry Parent { get { return parent; } }

	/*
	//
	// Summary:
	//     Gets the root portion of the directory.
	//
	// Returns:
	//     An object that represents the root of the directory.
	//
	// Exceptions:
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public DirEntry Root
	{
		get 
		{
			if (Parent == null) return this;
			else return Parent.Root;
		}
	}
	*/

	//
	// Summary:
	//     Creates a directory.
	//
	// Exceptions:
	//   T:System.IO.IOException:
	//     The directory cannot be created.
	public void Create()
	{
		Create(null);
	}

	//
	// Summary:
	//     Creates a directory using a System.Security.AccessControl.DirectorySecurity object.
	//
	// Parameters:
	//   directorySecurity:
	//     The access control to apply to the directory.
	//
	// Exceptions:
	//   T:System.IO.IOException:
	//     The directory specified by path is read-only or is not empty.
	//
	//   T:System.IO.IOException:
	//     The directory specified by path is read-only or is not empty.
	//
	//   T:System.UnauthorizedAccessException:
	//     The caller does not have the required permission.
	//
	//   T:System.ArgumentException:
	//     path is a zero-length string, contains only white space, or contains one or more
	//     invalid characters as defined by System.IO.Path.InvalidPathChars.
	//
	//   T:System.ArgumentNullException:
	//     path is null.
	//
	//   T:System.IO.PathTooLongException:
	//     The specified path, file name, or both exceed the system-defined maximum length.
	//     For example, on Windows-based platforms, paths must be less than 248 characters,
	//     and file names must be less than 260 characters.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The specified path is invalid, such as being on an unmapped drive.
	//
	//   T:System.NotSupportedException:
	//     Creating a directory with only the colon (:) character was attempted.
	public void Create(DirectorySecurity directorySecurity)
	{
		if (Exists)
			throw new System.IO.IOException("attempting to create already existing directory");

		if (Parent != null)
		{
			if (!Parent.Exists)
				Parent.Create(directorySecurity);

			Parent.Directories.Add(this);
			Parent.Refresh();
		}
		exists = true;
		if (fileSystem != null)
			fileSystem.CreateDirectory(this, directorySecurity);
	}

	//
	// Summary:
	//     Creates a subdirectory or subdirectories on the specified path. The specified
	//     path can be relative to this instance of the System.IO.DirectoryInfo class.
	//
	// Parameters:
	//   path:
	//     The specified path. This cannot be a different disk volume or Universal Naming
	//     Convention (UNC) name.
	//
	// Returns:
	//     The last directory specified in path.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     path does not specify a valid file path or contains invalid DirectoryInfo characters.
	//
	//   T:System.ArgumentNullException:
	//     path is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The specified path is invalid, such as being on an unmapped drive.
	//
	//   T:System.IO.IOException:
	//     The subdirectory cannot be created.-or- A file or directory already has the name
	//     specified by path.
	//
	//   T:System.IO.PathTooLongException:
	//     The specified path, file name, or both exceed the system-defined maximum length.
	//     For example, on Windows-based platforms, paths must be less than 248 characters,
	//     and file names must be less than 260 characters. The specified path, file name,
	//     or both are too long.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have code access permission to create the directory.-or-The
	//     caller does not have code access permission to read the directory described by
	//     the returned System.IO.DirectoryInfo object. This can occur when the path parameter
	//     describes an existing directory.
	//
	//   T:System.NotSupportedException:
	//     path contains a colon character (:) that is not part of a drive label ("C:\").
	public DirEntry CreateSubdirectory(string path)
	{
		return CreateSubdirectory(path, null);
	}
	//
	// Summary:
	//     Creates a subdirectory or subdirectories on the specified path with the specified
	//     security. The specified path can be relative to this instance of the System.IO.DirectoryInfo
	//     class.
	//
	// Parameters:
	//   path:
	//     The specified path. This cannot be a different disk volume or Universal Naming
	//     Convention (UNC) name.
	//
	//   directorySecurity:
	//     The security to apply.
	//
	// Returns:
	//     The last directory specified in path.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     path does not specify a valid file path or contains invalid DirectoryInfo characters.
	//
	//   T:System.ArgumentNullException:
	//     path is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The specified path is invalid, such as being on an unmapped drive.
	//
	//   T:System.IO.IOException:
	//     The subdirectory cannot be created.-or- A file or directory already has the name
	//     specified by path.
	//
	//   T:System.IO.PathTooLongException:
	//     The specified path, file name, or both exceed the system-defined maximum length.
	//     For example, on Windows-based platforms, paths must be less than 248 characters,
	//     and file names must be less than 260 characters. The specified path, file name,
	//     or both are too long.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have code access permission to create the directory.-or-The
	//     caller does not have code access permission to read the directory described by
	//     the returned System.IO.DirectoryInfo object. This can occur when the path parameter
	//     describes an existing directory.
	//
	//   T:System.NotSupportedException:
	//     path contains a colon character (:) that is not part of a drive label ("C:\").
	public DirEntry CreateSubdirectory(string path, DirectorySecurity directorySecurity)
	{
		var d = new DirEntry(path, this);
		d.Create(directorySecurity);
		return d;
	}

	//
	// Summary:
	//     Deletes this System.IO.DirectoryInfo if it is empty.
	//
	// Exceptions:
	//   T:System.UnauthorizedAccessException:
	//     The directory contains a read-only file.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The directory described by this System.IO.DirectoryInfo object does not exist
	//     or could not be found.
	//
	//   T:System.IO.IOException:
	//     The directory is not empty. -or-The directory is the application's current working
	//     directory.-or-There is an open handle on the directory, and the operating system
	//     is Windows XP or earlier. This open handle can result from enumerating directories.
	//     For more information, see How to: Enumerate Directories and Files.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public override void Delete()
	{
		if (!Exists) throw new System.IO.DirectoryNotFoundException("directory " + FullName + " doesnt exist");

		if (EnumerateFiles().FirstOrDefault() != null || EnumerateDirectories().FirstOrDefault() != null)
			throw new System.IO.IOException("directory is not empty");

		if (fileSystem != null)
			fileSystem.DeleteDirectory(this);
		if (Parent != null) Parent.Refresh();
	}
	//
	// Summary:
	//     Deletes this instance of a System.IO.DirectoryInfo, specifying whether to delete
	//     subdirectories and files.
	//
	// Parameters:
	//   recursive:
	//     true to delete this directory, its subdirectories, and all files; otherwise,
	//     false.
	//
	// Exceptions:
	//   T:System.UnauthorizedAccessException:
	//     The directory contains a read-only file.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The directory described by this System.IO.DirectoryInfo object does not exist
	//     or could not be found.
	//
	//   T:System.IO.IOException:
	//     The directory is read-only.-or- The directory contains one or more files or subdirectories
	//     and recursive is false.-or-The directory is the application's current working
	//     directory. -or-There is an open handle on the directory or on one of its files,
	//     and the operating system is Windows XP or earlier. This open handle can result
	//     from enumerating directories and files. For more information, see How to: Enumerate
	//     Directories and Files.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public void Delete(bool recursive)
	{
		if (!Exists) throw new System.IO.DirectoryNotFoundException("directory " + FullName + " doesnt exist");
		if (recursive)
		{
			foreach (var d in EnumerateDirectories())
				d.Delete(recursive);

			if (fileSystem != null)
				fileSystem.DeleteDirectory(this);
			if (Parent != null) Parent.Refresh();
		}
		else
		{
			Delete();
		}
	}
	//
	// Summary:
	//     Returns an enumerable collection of directory information in the current directory.
	//
	// Returns:
	//     An enumerable collection of directories in the current directory.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<DirEntry> EnumerateDirectories()
	{
		if (!Exists) throw new System.IO.DirectoryNotFoundException();
		return Directories;
	}
	//
	// Summary:
	//     Returns an enumerable collection of directory information that matches a specified
	//     search pattern.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	// Returns:
	//     An enumerable collection of directories that matches searchPattern.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<DirEntry> EnumerateDirectories(string searchPattern)
	{
		var g = new GlobSearch(searchPattern);
		return EnumerateDirectories().Where(d => g.Matches(d.Name));

	}
	//
	// Summary:
	//     Returns an enumerable collection of directory information that matches a specified
	//     search pattern and search subdirectory option.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories. The default value is
	//     System.IO.SearchOption.TopDirectoryOnly.
	//
	// Returns:
	//     An enumerable collection of directories that matches searchPattern and searchOption.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<DirEntry> EnumerateDirectories(string searchPattern, SearchOption searchOption)
	{
		if (searchOption == SearchOption.TopDirectoryOnly) return EnumerateDirectories(searchPattern);
		else throw new NotImplementedException();
	}
	//
	// Summary:
	//     Returns an enumerable collection of file information in the current directory.
	//
	// Returns:
	//     An enumerable collection of the files in the current directory.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileEntry> EnumerateFiles()
	{
		return Files;
	}
	//
	// Summary:
	//     Returns an enumerable collection of file information that matches a search pattern.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of files. This parameter can contain
	//     a combination of valid literal path and wildcard (* and ?) characters (see Remarks),
	//     but doesn't support regular expressions. The default pattern is "*", which returns
	//     all files.
	//
	// Returns:
	//     An enumerable collection of files that matches searchPattern.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid, (for
	//     example, it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileEntry> EnumerateFiles(string searchPattern)
	{
		var g = new GlobSearch(searchPattern);
		return EnumerateFiles().Where(d => g.Matches(d.Name));
	}
	//
	// Summary:
	//     Returns an enumerable collection of file information that matches a specified
	//     search pattern and search subdirectory option.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of files. This parameter can contain
	//     a combination of valid literal path and wildcard (* and ?) characters (see Remarks),
	//     but doesn't support regular expressions. The default pattern is "*", which returns
	//     all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories. The default value is
	//     System.IO.SearchOption.TopDirectoryOnly.
	//
	// Returns:
	//     An enumerable collection of files that matches searchPattern and searchOption.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileEntry> EnumerateFiles(string searchPattern, SearchOption searchOption)
	{
		if (searchOption == SearchOption.TopDirectoryOnly) return EnumerateFiles(searchPattern);
		else throw new NotImplementedException();
	}
	//
	// Summary:
	//     Returns an enumerable collection of file system information in the current directory.
	//
	// Returns:
	//     An enumerable collection of file system information in the current directory.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileSystemEntry> EnumerateFileSystemInfos()
	{
		return fileSystemEntries;
	}
	//
	// Summary:
	//     Returns an enumerable collection of file system information that matches a specified
	//     search pattern.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	// Returns:
	//     An enumerable collection of file system information objects that matches searchPattern.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileSystemEntry> EnumerateFileSystemInfos(string searchPattern)
	{
		var g = new GlobSearch(searchPattern);
		return EnumerateFileSystemInfos().Where(e => g.Matches(e.Name));
	}
	//
	// Summary:
	//     Returns an enumerable collection of file system information that matches a specified
	//     search pattern and search subdirectory option.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories. The default value is
	//     System.IO.SearchOption.TopDirectoryOnly.
	//
	// Returns:
	//     An enumerable collection of file system information objects that matches searchPattern
	//     and searchOption.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid (for example,
	//     it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public IEnumerable<FileSystemEntry> EnumerateFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		if (searchOption == SearchOption.TopDirectoryOnly) return EnumerateFileSystemInfos(searchPattern);
		else throw new NotImplementedException();
	}
	//
	// Summary:
	//     Gets a System.Security.AccessControl.DirectorySecurity object that encapsulates
	//     the access control list (ACL) entries for the directory described by the current
	//     System.IO.DirectoryInfo object.
	//
	// Returns:
	//     A System.Security.AccessControl.DirectorySecurity object that encapsulates the
	//     access control rules for the directory.
	//
	// Exceptions:
	//   T:System.SystemException:
	//     The directory could not be found or modified.
	//
	//   T:System.UnauthorizedAccessException:
	//     The current process does not have access to open the directory.
	//
	//   T:System.UnauthorizedAccessException:
	//     The directory is read-only.-or- This operation is not supported on the current
	//     platform.-or- The caller does not have the required permission.
	//
	//   T:System.IO.IOException:
	//     An I/O error occurred while opening the directory.
	//
	//   T:System.PlatformNotSupportedException:
	//     The current operating system is not Microsoft Windows 2000 or later.
	public DirectorySecurity GetAccessControl()
	{
		throw new NotImplementedException();
	}

	/*
	//
	// Summary:
	//     Gets a System.Security.AccessControl.DirectorySecurity object that encapsulates
	//     the specified type of access control list (ACL) entries for the directory described
	//     by the current System.IO.DirectoryInfo object.
	//
	// Parameters:
	//   includeSections:
	//     One of the System.Security.AccessControl.AccessControlSections values that specifies
	//     the type of access control list (ACL) information to receive.
	//
	// Returns:
	//     A System.Security.AccessControl.DirectorySecurity object that encapsulates the
	//     access control rules for the file described by the path parameter.ExceptionsException
	//     typeConditionSystem.SystemExceptionThe directory could not be found or modified.System.UnauthorizedAccessExceptionThe
	//     current process does not have access to open the directory.System.IO.IOExceptionAn
	//     I/O error occurred while opening the directory.System.PlatformNotSupportedExceptionThe
	//     current operating system is not Microsoft Windows 2000 or later.System.UnauthorizedAccessExceptionThe
	//     directory is read-only.-or- This operation is not supported on the current platform.-or-
	//     The caller does not have the required permission.
	public DirectorySecurity GetAccessControl(AccessControlSections includeSections);
	*/

	//
	// Summary:
	//     Returns the subdirectories of the current directory.
	//
	// Returns:
	//     An array of System.IO.DirectoryInfo objects.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the System.IO.DirectoryInfo object is invalid, such
	//     as being on an unmapped drive.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	//
	//   T:System.UnauthorizedAccessException:
	//     The caller does not have the required permission.
	public DirEntry[] GetDirectories()
	{
		return Directories.ToArray();
	}

	//
	// Summary:
	//     Returns an array of directories in the current System.IO.DirectoryInfo matching
	//     the given search criteria.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	// Returns:
	//     An array of type DirectoryInfo matching searchPattern.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the DirectoryInfo object is invalid (for example, it
	//     is on an unmapped drive).
	//
	//   T:System.UnauthorizedAccessException:
	//     The caller does not have the required permission.
	public DirEntry[] GetDirectories(string searchPattern)
	{
		return EnumerateDirectories(searchPattern).ToArray();
	}

	//
	// Summary:
	//     Returns an array of directories in the current System.IO.DirectoryInfo matching
	//     the given search criteria and using a value to determine whether to search subdirectories.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories. This parameter can
	//     contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories.
	//
	// Returns:
	//     An array of type DirectoryInfo matching searchPattern.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path encapsulated in the DirectoryInfo object is invalid (for example, it
	//     is on an unmapped drive).
	//
	//   T:System.UnauthorizedAccessException:
	//     The caller does not have the required permission.
	public DirEntry[] GetDirectories(string searchPattern, SearchOption searchOption)
	{
		return EnumerateDirectories(searchPattern, searchOption).ToArray();
	}
	//
	// Summary:
	//     Returns a file list from the current directory.
	//
	// Returns:
	//     An array of type System.IO.FileInfo.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path is invalid, such as being on an unmapped drive.
	public FileEntry[] GetFiles()
	{
		return Files.ToArray();
	}

	//
	// Summary:
	//     Returns a file list from the current directory matching the given search pattern.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of files. This parameter can contain
	//     a combination of valid literal path and wildcard (* and ?) characters (see Remarks),
	//     but doesn't support regular expressions. The default pattern is "*", which returns
	//     all files.
	//
	// Returns:
	//     An array of type System.IO.FileInfo.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path is invalid (for example, it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public FileEntry[] GetFiles(string searchPattern)
	{
		return EnumerateFiles(searchPattern).ToArray();
	}

	//
	// Summary:
	//     Returns a file list from the current directory matching the given search pattern
	//     and using a value to determine whether to search subdirectories.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of files. This parameter can contain
	//     a combination of valid literal path and wildcard (* and ?) characters (see Remarks),
	//     but doesn't support regular expressions. The default pattern is "*", which returns
	//     all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories.
	//
	// Returns:
	//     An array of type System.IO.FileInfo.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The path is invalid (for example, it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public FileEntry[] GetFiles(string searchPattern, SearchOption searchOption)
	{
		return EnumerateFiles(searchPattern, searchOption).ToArray();
	}

	//
	// Summary:
	//     Returns an array of strongly typed System.IO.FileSystemInfo entries representing
	//     all the files and subdirectories in a directory.
	//
	// Returns:
	//     An array of strongly typed System.IO.FileSystemInfo entries.
	//
	// Exceptions:
	//   T:System.IO.DirectoryNotFoundException:
	//     The path is invalid (for example, it is on an unmapped drive).
	public FileSystemEntry[] GetFileSystemInfos()
	{
		return fileSystemEntries.ToArray();
	}

	//
	// Summary:
	//     Retrieves an array of strongly typed System.IO.FileSystemInfo objects representing
	//     the files and subdirectories that match the specified search criteria.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories and files. This parameter
	//     can contain a combination of valid literal path and wildcard (* and ?) characters
	//     (see Remarks), but doesn't support regular expressions. The default pattern is
	//     "*", which returns all files.
	//
	// Returns:
	//     An array of strongly typed FileSystemInfo objects matching the search criteria.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The specified path is invalid (for example, it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public FileSystemEntry[] GetFileSystemInfos(string searchPattern)
	{
		var g = new GlobSearch(searchPattern);
		return fileSystemEntries.Where(e => g.Matches(e.Name)).ToArray();
	}

	//
	// Summary:
	//     Retrieves an array of System.IO.FileSystemInfo objects that represent the files
	//     and subdirectories matching the specified search criteria.
	//
	// Parameters:
	//   searchPattern:
	//     The search string to match against the names of directories and filesa. This
	//     parameter can contain a combination of valid literal path and wildcard (* and
	//     ?) characters (see Remarks), but doesn't support regular expressions. The default
	//     pattern is "*", which returns all files.
	//
	//   searchOption:
	//     One of the enumeration values that specifies whether the search operation should
	//     include only the current directory or all subdirectories. The default value is
	//     System.IO.SearchOption.TopDirectoryOnly.
	//
	// Returns:
	//     An array of file system entries that match the search criteria.
	//
	// Exceptions:
	//   T:System.ArgumentException:
	//     searchPattern contains one or more invalid characters defined by the System.IO.Path.GetInvalidPathChars
	//     method.
	//
	//   T:System.ArgumentNullException:
	//     searchPattern is null.
	//
	//   T:System.ArgumentOutOfRangeException:
	//     searchOption is not a valid System.IO.SearchOption value.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The specified path is invalid (for example, it is on an unmapped drive).
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	public FileSystemEntry[] GetFileSystemInfos(string searchPattern, SearchOption searchOption)
	{
		if (searchOption == SearchOption.TopDirectoryOnly) return GetFileSystemInfos(searchPattern);
		else throw new NotImplementedException();
	}

	//
	// Summary:
	//     Moves a System.IO.DirectoryInfo instance and its contents to a new path.
	//
	// Parameters:
	//   destDirName:
	//     The name and path to which to move this directory. The destination cannot be
	//     another disk volume or a directory with the identical name. It can be an existing
	//     directory to which you want to add this directory as a subdirectory.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     destDirName is null.
	//
	//   T:System.ArgumentException:
	//     destDirName is an empty string (''").
	//
	//   T:System.IO.IOException:
	//     An attempt was made to move a directory to a different volume. -or-destDirName
	//     already exists.-or-You are not authorized to access this path.-or- The directory
	//     being moved and the destination directory have the same name.
	//
	//   T:System.Security.SecurityException:
	//     The caller does not have the required permission.
	//
	//   T:System.IO.DirectoryNotFoundException:
	//     The destination directory cannot be found.
	public void MoveTo(string destDirName)
	{
		throw new NotImplementedException();
	}

	//
	// Summary:
	//     Applies access control list (ACL) entries described by a System.Security.AccessControl.DirectorySecurity
	//     object to the directory described by the current System.IO.DirectoryInfo object.
	//
	// Parameters:
	//   directorySecurity:
	//     An object that describes an ACL entry to apply to the directory described by
	//     the path parameter.
	//
	// Exceptions:
	//   T:System.ArgumentNullException:
	//     The directorySecurity parameter is null.
	//
	//   T:System.SystemException:
	//     The file could not be found or modified.
	//
	//   T:System.UnauthorizedAccessException:
	//     The current process does not have access to open the file.
	//
	//   T:System.PlatformNotSupportedException:
	//     The current operating system is not Microsoft Windows 2000 or later.
	public void SetAccessControl(DirectorySecurity directorySecurity)
	{
		throw new NotImplementedException();
	}
	//
	// Summary:
	//     Returns the original path that was passed by the user.
	//
	// Returns:
	//     Returns the original path that was passed by the user.
	public override string ToString()
	{
		return FullName;
	}


	public override void Refresh()
	{
		initialized = false;
		foreach (var d in directories)
			if (d.initialized)
				d.Refresh();
	}
}