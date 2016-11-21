using System;
using System.IO;



public enum DeviceIOType
{
	Block, // storage, hard discs
	Character, // no storage, write/read text to terminal or printer
}
public enum DeviceNumberingType
{
	Alphabet,
	Numbers,
}

public class DeviceType
{
	public readonly static DeviceType Terminal = new DeviceType(DeviceIOType.Character, "tty", DeviceNumberingType.Numbers);
	public readonly static DeviceType SCSIDevice = new DeviceType(DeviceIOType.Block, "sd", DeviceNumberingType.Alphabet);

	public DeviceIOType IOType { get; private set; }
	public string NamePrefix { get; private set; }
	public DeviceNumberingType NumberingType { get; private set; }

	public DeviceType(DeviceIOType ioType, string namePrefix, DeviceNumberingType numberingType)
	{
		this.IOType = ioType;
		this.NamePrefix = namePrefix;
		this.NumberingType = numberingType;
	}

}
public interface IDevice
{
	DeviceType DeviceType { get; }
	Stream OpenRead();
	Stream OpenWrite();
}

public class StorageDevice : IDevice
{
	string filePath;

	public StorageDevice(string filePath)
	{
		this.filePath = filePath;
	}

	public DeviceType DeviceType { get { return DeviceType.SCSIDevice; } }

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