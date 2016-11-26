using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// http://askubuntu.com/questions/341939/why-cant-i-create-a-directory-in-sys
/// https://www.kernel.org/doc/Documentation/filesystems/sysfs.txt
/// </summary>
public class SysFs : IFileSystem
{
	DirEntry mountPoint;
	OperatingSystem os;
	public SysFs(DirEntry mountPoint, OperatingSystem os)
	{
		this.mountPoint = mountPoint;
		this.os = os;
	}

	public class MyDirEntry : DirEntry
	{
		public Guid guid;

		public MyDirEntry(string name, DirEntry parent, bool exists) : base(name, parent, exists)
		{
		}
	}
	public class MyFileEntry : FileEntry
	{
		public Guid guid;

		public MyFileEntry(string name, DirEntry parent, bool exists) : base(name, parent, exists)
		{
		}
	}


	public void UpdateDirectoryInfo(DirEntry.UpdateHandle handle)
	{
		if (handle.DirEntry.FullName == mountPoint.FullName)
		{
			foreach (var device in os.Devices)
			{
				var name = device.name;
				var dir  = new MyDirEntry(name, mountPoint, true);
				dir.guid = device.guid;
				handle.Directories.Add(dir);
			}
		}
		else
		{
				
		}
	}

	public Stream Open(FileEntry file, FileMode mode, FileAccess access, FileShare share)
	{
		var p = mountPoint.GetRelativePathTo(file);
		var myFile = file as MyFileEntry;
		if (myFile == null) throw new FileNotFoundException("device '" + file.FullName + "' was not created by this file system");
		var device = os.Devices.FirstOrDefault(d => d.guid == myFile.guid);
		if (device == null) throw new FileNotFoundException("device '" + file.FullName + "' not found");

		if (access == FileAccess.Read) return device.OpenRead();
		if (access == FileAccess.Write) return device.OpenWrite();

		throw new NotImplementedException(FileAccess.ReadWrite + " is not implemented");
	}

	public void CreateDirectory(DirEntry directory, DirectorySecurity directorySecurity)
	{
		throw new NotImplementedException();
	}

	public void DeleteDirectory(DirEntry directory)
	{
		throw new NotImplementedException();
	}

	public void DeleteFile(FileEntry file)
	{
		throw new NotImplementedException();
	}


}

