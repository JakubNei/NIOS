using System;
using System.Globalization;
using System.IO;
using System.Threading;

public static class Utils
{
	public static string GetStringSha256Hash(string text)
	{
		if (String.IsNullOrEmpty(text))
			return String.Empty;

		using (var sha = new System.Security.Cryptography.SHA256Managed())
		{
			byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
			byte[] hash = sha.ComputeHash(textData);
			return Convert.ToBase64String(hash);
		}
	}

	public static byte[] ProgramContents(int seed, int length)
	{
		var buf = new byte[length];
		var r = new Random(seed);
		r.NextBytes(buf);
		return buf;
	}

	public static string SanitizeInput(string input)
	{
		var text = string.Empty;
		foreach (var c in input)
		{
			if (c == '\b')
			{
				if (text.Length > 1)
					text = text.Substring(text.Length - 1);
			}
			else if (c == ' ' || ((int)c >= 0x20 && (int)c <= 0xFF))
			{
				text += c;
			}
		}
		return text;
	}

	public static void SetDefaultCultureInfo(Thread thrad)
	{
		thrad.CurrentCulture = thrad.CurrentUICulture = GetDefaultCultureInfo();
	}

	public static CultureInfo GetDefaultCultureInfo()
	{
		var cultureInfo = new CultureInfo("", false);
		cultureInfo.DateTimeFormat.YearMonthPattern = "yyyy-MM";
		cultureInfo.DateTimeFormat.MonthDayPattern = "MM-dd";
		cultureInfo.DateTimeFormat.ShortDatePattern = cultureInfo.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
		cultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm";
		cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";
		cultureInfo.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
		cultureInfo.DateTimeFormat.TimeSeparator = ":";
		return cultureInfo;
	}

}