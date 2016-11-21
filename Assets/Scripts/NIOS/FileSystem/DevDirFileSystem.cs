using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// todo:
// http://unix.stackexchange.com/a/281377/153172
// /dev/disk/by-id
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
		// find unique name, keep adding index until name is unique
		var nameIndex = 0;
		var name = string.Empty;
		do
		{
			name = GetName(device, nameIndex);
			nameIndex++;
		} while (devices.ContainsKey(name));

		devices.Add(name, device);
	}

	string GetName(IDevice device, int index)
	{
		var name = device.DeviceType.NamePrefix;
		if (device.DeviceType.NumberingType == DeviceNumberingType.Alphabet)
			name += ToBase26Alphabet(index);
		else
			name += (index + 1).ToString();
		return name;
	}

	/// <summary>
	/// decimal to something like 27 alphabet base
	/// non-first or sole letter's index starts at 0
	/// first letter's index starts at 1
	/// abcdefghijklmnopqrstuvwxyz
	/// 01234567890123456789012345
	/// 00000000001111111111222222
	/// 0 returns a
	/// 1 returns b
	/// 25 returnz z
	/// 26 return aa
	/// 27 returns ab
	/// 50 returns az
	/// 51 returns ba
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	static string ToBase26Alphabet(int currentNumber)
	{
		var result = string.Empty;
		var baseCurrent = 26;

		while (true)
		{
			var nextNumber = (int)(currentNumber / baseCurrent);
			var a = nextNumber * baseCurrent;
			var remainder = currentNumber - a;

			currentNumber = nextNumber;

			if (currentNumber == 0 && result.Length > 0) remainder--; // for beauty, first character starts at 'a'==0 even thought it is should start at 'b'==1
			result = ((char)('a' + remainder)).ToString() + result;

			if (currentNumber == 0) return result;
		}
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