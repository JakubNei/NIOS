using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StdLib.Ecma48
{
	/// <summary>
	/// Should be similar to http://tldp.org/HOWTO/NCURSES-Programming-HOWTO/
	/// </summary>
	public class Client
	{
		public readonly string Ecma48SequenceStart = new string(new char[] { ASCII.Escape, '[' });

		public TextWriter TextWriter { get; private set; }

		public Client(TextWriter textWriter)
		{
			TextWriter = textWriter;
		}

		public void Write(string text)
		{
			TextWriter.Write(text);
		}
		public void Write(char text)
		{
			TextWriter.Write(text);
		}

		public void SetForegroundColor(Color color)
		{
			Write(Ecma48SequenceStart + ((int)color + 30).ToString() + "m");
		}

		public void SetBackgroundColor(Color color)
		{
			Write(Ecma48SequenceStart + ((int)color + 40).ToString() + "m");
		}

		public void ResetAttributes()
		{
			Write(Ecma48SequenceStart + "0m");
		}

		public void SetCursorPos(uint row, uint column)
		{
			Write(Ecma48SequenceStart + row.ToString() + ";" + column.ToString() + "H");
		}
		public void EraseDisplay()
		{
			Write(Ecma48SequenceStart + "3J");
			SetCursorPos(1, 1);
		}

		public void Beep()
		{
			Write(ASCII.BEL);
		}
		public void Backspace()
		{
			Write(ASCII.BackSpace);
		}
	}

}