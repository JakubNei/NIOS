using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class DevDirFileSystem : IFileSystem
{
	DirEntry mountPoint;
	Dictionary<string, IDevice> devices = new Dictionary<string, IDevice>();

	public DevDirFileSystem(DirEntry mountPoint)
	{
		this.mountPoint = mountPoint;
	}

	public void AddDevice(IDevice device)
	{
		var name = "";
		if (device.Type == DeviceType.Block)
			name = "sd" + (char)((int)'a' + devices.Count(d => d.Value.Type == DeviceType.Block));
		if (device.Type == DeviceType.Character)
			name = "tt" + (char)((int)'1' + devices.Count(d => d.Value.Type == DeviceType.Character));

		if (devices.ContainsKey(name)) throw new InvalidOperationException("device " + name + " already exists");
		devices.Add(name, device);
	}


	public void GatherDirectoryInfo(DirEntry dir)
	{
		foreach (var d in devices.Keys)
			dir.FileSystemAddsFile(d);
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