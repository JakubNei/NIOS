using System;
using System.IO;




public enum DeviceNumbering
{
	Alphabet,
	Numbers,
}

public class DeviceType
{
	public readonly static DeviceType Keyboard = new DeviceType("keyboard", DeviceNumbering.Numbers);
	public readonly static DeviceType Display = new DeviceType("display", DeviceNumbering.Numbers);
	public readonly static DeviceType SCSIDevice = new DeviceType("sd", DeviceNumbering.Alphabet);

	public string NamePrefix { get; private set; }
	public DeviceNumbering Numbering { get; private set; }

	public DeviceType(string namePrefix, DeviceNumbering numbering)
	{
		this.NamePrefix = namePrefix;
		this.Numbering = numbering;
	}

	public string GetName(int index = 0)
	{
		var name = NamePrefix;
		if (Numbering == DeviceNumbering.Alphabet)
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


}
public interface IDevice
{
	Guid Guid { get; }
	DeviceType DeviceType { get; }
	Stream OpenRead();
	Stream OpenWrite();
}
