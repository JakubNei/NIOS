using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using StdLib;

public class UnityTerminal : IDevice
{
	Text textComponent;
	public DeviceType DeviceType { get { return DeviceType.Terminal; } }
	public Encoding Encoding { get { return Encoding.ASCII; } }

	public string NewLine { get; set; }

	CultureInfo cultureInfo = new CultureInfo("", false);

	public CultureInfo CultureInfo { get { return cultureInfo; } }

	MyWriteStream myWriteStream;
	MyReadStream myReadStream;

	StdLib.Ecma48.Device device;
	StdLib.Ecma48.Client client;
	ulong deviceLastDataVersion;

	public UnityTerminal(Text textComponent)
	{
		this.textComponent = textComponent;

		NewLine = "\n";

		myWriteStream = new MyWriteStream(this);
		myReadStream = new MyReadStream(this);

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = textComponent.GetGenerationSettings(textComponent.rectTransform.rect.size);

		var areaHeight = textComponent.rectTransform.rect.height;
		float textHeight = textGen.GetPreferredHeight("M", generationSettings);
		var maxNumberOfLines = (uint)Mathf.FloorToInt(areaHeight / textHeight);

		var areaWidth = textComponent.rectTransform.rect.width;
		float textWidth = textGen.GetPreferredWidth("M", generationSettings);
		var maxNumberOfColumns = (uint)Mathf.FloorToInt(areaWidth / textWidth);

		device = new StdLib.Ecma48.Device(maxNumberOfColumns, maxNumberOfLines);
		var writer = new StreamWriter(OpenWrite());
		writer.AutoFlush = true;
		client = new StdLib.Ecma48.Client(writer);

	}

	DateTime lastSignalReceived = DateTime.MinValue;
	bool noMessageReceivedPrinted = false;
	public void DisplayUpdate()
	{
		const int timeoutSeconds = 60;
		if (lastSignalReceived.IsOver(seconds: timeoutSeconds).InPastComparedTo(DateTime.UtcNow))
		{
			lastSignalReceived = DateTime.UtcNow;
			client.WriteLine();
			client.WriteLine();
			client.WriteLine("no data received for over " + timeoutSeconds + " seconds");
			client.WriteLine("tap to bootup");
			client.WriteLine("debug info:");
			client.WriteLine("	current time: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
			client.WriteLine("	columns: " + device.ColumnsCount);
			client.WriteLine("	rows: " + device.RowsCount);
			client.WriteLine();
			client.WriteLine();
		}


		myWriteStream.UnityUpdate();
		UpdateTextArea();
	}

	public void DoType(string text)
	{
		myReadStream.DoType(text);
	}


	void UpdateTextArea()
	{
		if (device.DataVersion == deviceLastDataVersion) return;
		deviceLastDataVersion = device.DataVersion;

		var defaultForegroundColor = StdLib.Ecma48.Color.White;
		var lastForegroundColor = defaultForegroundColor;

		var lastBold = false;
		var shouldEndBoldElement = false;

		var sb = new StringBuilder();
		for (uint row = 0; row < device.RowsCount; row++)
		{
			for (uint column = 0; column < device.ColumnsCount; column++)
			{
				var c = device[column, row];
				if (lastForegroundColor != c.foregroundColor)
				{
					/*
					// buggy
					if (shouldEndBoldElement)
					{
						sb.Append("</b>");
						shouldEndBoldElement = false;
						lastBold = false;
					}
					if (c.bold)
					{
						sb.Append("<b>");
						shouldEndBoldElement = true;
						lastBold = true;
					}
					*/
					if (lastForegroundColor != defaultForegroundColor)
					{
						sb.Append("</color>");
						lastForegroundColor = defaultForegroundColor;
					}
					if (c.foregroundColor != defaultForegroundColor)
					{
						sb.Append("<color=" + c.foregroundColor.ToString().ToLower() + ">");
						lastForegroundColor = c.foregroundColor;
					}
				}
				sb.Append(c.character);
			}
			sb.AppendLine();
		}

		if (lastForegroundColor != defaultForegroundColor)
		{
			sb.Append("</color>");
			lastForegroundColor = defaultForegroundColor;
		}

		textComponent.text = sb.ToString();
	}


	class MyWriteStream : Stream
	{
		string pendingWriteText = string.Empty;
		UnityTerminal unityTerminal;

		public override bool CanRead { get { return false; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return true; } }

		public override long Length { get { return long.MaxValue; } }

		public override long Position { get { return pendingWriteText.Length; } set { throw new NotImplementedException(); } }

		public MyWriteStream(UnityTerminal u)
		{
			this.unityTerminal = u;
		}

		public void UnityUpdate()
		{
			if (pendingWriteText.Length > 0)
			{
				unityTerminal.device.Parse(pendingWriteText);
				pendingWriteText = string.Empty;
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			unityTerminal.lastSignalReceived = DateTime.UtcNow;
			var dst = new byte[count];
			Array.Copy(buffer, offset, dst, 0, count);
			pendingWriteText += unityTerminal.Encoding.GetString(dst);
		}
	}

	class MyReadStream : Stream
	{
		UnityTerminal unityTerminal;
		string pendingTextToRead = string.Empty;

		ManualResetEvent canRead = new ManualResetEvent(false);

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return false; } }

		public override long Length { get { return pendingTextToRead.Length; } }

		public override long Position { get { return 0; } set { throw new NotImplementedException(); } }

		public MyReadStream(UnityTerminal u)
		{
			this.unityTerminal = u;
		}
		public void DoType(string text)
		{
			text = text.Replace("\r", "\n"); // Unity says enter is only \r, but we want new line
			if (text.Length > 0)
			{
				pendingTextToRead += text;
				canRead.Set();
			}
		}

		public override void Flush()
		{
			pendingTextToRead = string.Empty;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			while (Length == 0)
			{
				canRead.Reset();
				canRead.WaitOne();
			}
			if (count > Length) count = (int)Length;

			var p = unityTerminal.Encoding.GetBytes(pendingTextToRead, 0, count, buffer, offset);

			//DEBUG
			//var c = pendingTextToRead.Substring(0, p);
			//Debug.Log("reading " + c + " = " + string.Join(",", c.Select(x => ((int)x).ToString()).ToArray()));

			pendingTextToRead = pendingTextToRead.Substring(p);
			return p;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}

	public Stream OpenRead()
	{
		return myReadStream;
	}

	public Stream OpenWrite()
	{
		return myWriteStream;
	}


}