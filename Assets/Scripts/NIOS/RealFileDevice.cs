

using System;
using System.IO;

public class RealFileDevice : IDevice
{
	string filePath;
	public DeviceType DeviceType { get { return DeviceType.SCSIDevice; } }

	Guid guid;
	public Guid Guid { get { return guid; } }

	public RealFileDevice(string filePath)
	{
		this.filePath = filePath;
		this.guid = Utils.StringToGuid(filePath);
	}

	public Stream OpenRead()
	{
		var f = new FileInfo(filePath);
		if (!f.Exists)
		{
			if (!f.Directory.Exists) f.Directory.Create();
			File.WriteAllText(f.FullName, string.Empty);
		}
		return f.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
	}

	public Stream OpenWrite()
	{
		var f = new FileInfo(filePath);
		if (!f.Exists)
		{
			if (!f.Directory.Exists) f.Directory.Create();
			File.WriteAllText(f.FullName, string.Empty);
		}
		return f.Open(FileMode.Truncate, FileAccess.Write, FileShare.ReadWrite);
	}
}