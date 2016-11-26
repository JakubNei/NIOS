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

public class InputDevice : NeitriBehavior, IDevice, IInteraction
{
	public DeviceType DeviceType { get { return DeviceType.Keyboard; } }

	Encoding Encoding { get { return Encoding.ASCII; } }

	InputControlContext input;
	bool typingEnabled;

	MyReadStream myReadStream;

	Guid guid;
	public Guid Guid { get { return guid; } }


	protected override void Start()
	{
		myReadStream = new MyReadStream(this);
		guid = Utils.IntToGuid(GetInstanceID());
	}

	protected override void Update()
	{
		if (typingEnabled)
		{
			DoType(Input.inputString);
		}
	}

	public void OnTouched(InteractionEvent data)
	{
		input = data.player.OverrideInput();
		input.OnExit += () => {
			typingEnabled = false;
		};
		typingEnabled = true;
	}

	public void DoType(string text)
	{
		myReadStream.DoType(text);
	}

	class MyReadStream : Stream
	{
		InputDevice p;
		string pendingTextToRead = string.Empty;

		ManualResetEvent canRead = new ManualResetEvent(false);

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return false; } }

		public override bool CanWrite { get { return false; } }

		public override long Length { get { return pendingTextToRead.Length; } }

		public override long Position { get { return 0; } set { throw new NotImplementedException(); } }

		public MyReadStream(InputDevice p)
		{
			this.p = p;
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
			while (pendingTextToRead.Length == 0)
			{
				canRead.Reset();
				canRead.WaitOne();
			}
			
			var maxByteLen = this.p.Encoding.GetByteCount(pendingTextToRead);
			if (count > maxByteLen) count = maxByteLen;

			var p = this.p.Encoding.GetBytes(pendingTextToRead, 0, count, buffer, offset);

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
		return Stream.Null;
	}


}