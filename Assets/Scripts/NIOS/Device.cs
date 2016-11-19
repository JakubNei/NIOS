using System.IO;

public enum DeviceType
{
	Block, // storage, hard discs
	Character, // no storage, write/read text to terminal or printer
}

public interface IDevice
{
	DeviceType Type { get; }

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

	public DeviceType Type { get { return DeviceType.Block; } }

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