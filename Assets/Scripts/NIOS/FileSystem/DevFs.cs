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
	Dictionary<string, IDevice> devices = new Dictionary<string, IDevice>();

	public DevFs(DirEntry mountPoint)
	{
		this.mountPoint = mountPoint;
	}

	public void AddDevice(IDevice device)
	{
		// find unique name, keep adding index until name is unique
		var nameIndex = 0;
		var name = string.Empty;
		do
		{
			name =  device.DeviceType.GetName(nameIndex);
			nameIndex++;
		} while (devices.ContainsKey(name));

		devices.Add(name, device);
	}



	public void UpdateDirectoryInfo(DirEntry.UpdateHandle dir)
	{
		foreach (var d in devices.Keys)
			dir.AddFile(d);
	}


	public Stream Open(FileEntry file, FileMode mode, FileAccess access, FileShare share)
	{
		var p = mountPoint.GetRelativePathTo(file);
		if (!devices.ContainsKey(p)) throw new FileNotFoundException("device " + p + " not found");

		if (access == FileAccess.Read) return devices[p].OpenRead();
		if (access == FileAccess.Write) return devices[p].OpenWrite();

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