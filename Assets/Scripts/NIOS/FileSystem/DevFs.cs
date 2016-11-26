using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// todo:
// http://unix.stackexchange.com/a/281377/153172
// /dev/disk/by-id
public class DevFs : IFileSystem
{
	DirEntry mountPoint;
	OperatingSystem os;
	public DevFs(DirEntry mountPoint, OperatingSystem os)
	{
		this.mountPoint = mountPoint;
		this.os = os;
	}

	public class MyFileEntry : FileEntry
	{
		public Guid guid;

		public MyFileEntry(string name, DirEntry parent, bool exists) : base(name, parent, exists)
		{
		}
	}

	public void UpdateDirectoryInfo(DirEntry.UpdateHandle dir)
	{
		foreach (var device in os.Devices)			
		{
			var name = device.name;
			var file = new MyFileEntry(name, mountPoint, true);
			file.guid = device.guid;
			dir.Files.Add(file);
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