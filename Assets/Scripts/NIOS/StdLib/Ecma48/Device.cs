using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StdLib.Ecma48
{

	/// <summary>
	/// Console control sequences wrapper.
	/// Supposed to simulate http://man7.org/linux/man-pages/man4/console_codes.4.html
	/// Further development can also include more of http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-048.pdf
	/// </summary>
	public class Device
	{

		public struct CharData
		{
			public char character;
			public bool bold;
			public Color foregroundColor;
			public Color backgroundColor;
			public void Reset()
			{
				character = ASCII.Space;
				bold = false;
				foregroundColor = Color.White;
				backgroundColor = Color.Default;
			}
			public override string ToString()
			{
				return character.ToString();
			}
		}

		CharData[] screen;

		public CharData this[uint column, uint row]
		{
			get
			{
				return screen[column + row * ColumnsCount];
			}
		}

		public uint ColumnsCount { get; private set; }
		public uint RowsCount { get; private set; }

		public ulong DataVersion { get; private set; }

		public Device(uint columns, uint rows)
		{
			ColumnsCount = columns;
			RowsCount = rows;

			screen = new CharData[ColumnsCount * RowsCount];
			for (int i = 0; i < screen.Length; i++) screen[i].Reset();

			currentChar.Reset();
		}

		public event Action OnBeep;


		int cursorRow;
		int cursorColumn;

		public void Parse(string text)
		{
			foreach (var c in text)
				Parse(c);
		}

		bool escapeSequence = false;
		bool ecma48Sequence = false;
		string ecma48SequenceParameter = string.Empty;
		List<int> GetEcma48Params(int requiredLength = 1, int defaultValue = 0)
		{
			var ret = new List<int>();
			foreach (var part in ecma48SequenceParameter.Split(';'))
			{
				var i = int.Parse(part);
				ret.Add(i);
			}
			while (ret.Count < requiredLength) ret.Add(defaultValue);

			ecma48SequenceParameter = string.Empty;
			ecma48Sequence = false;
			return ret;
		}
		public void Parse(char character)
		{
			if (character == ASCII.ESC)
			{
				escapeSequence = true;
			}
			else if (escapeSequence)
			{
				switch (character)
				{
					case '[':
						ecma48Sequence = true;
						break;
				}
				escapeSequence = false;
			}
			else if (ecma48Sequence)
			{
				if(ecma48SequenceParameter.Length > 200)
				{
					// failsafe
					ecma48Sequence = false;
					ecma48SequenceParameter = string.Empty;
					return;
				}
				switch (character)
				{
					case '@': // Insert the indicated # of blank characters.
						{
							var num = GetEcma48Params(1, 1)[0];
							while (num-- > 0) RawAddText(ASCII.Space);
							DataVersion++;
						}
						break;
					case 'A': // Move cursor up the indicated # of rows.
						{
							var rows = GetEcma48Params(1, 1)[0];
							cursorRow -= rows;
							DataVersion++;
						}
						break;
					case 'B': // Move cursor down the indicated # of rows.
						{
							var rows = GetEcma48Params(1, 1)[0];
							cursorRow += rows;
							DataVersion++;
						}
						break;
					case 'C': // Move cursor right the indicated # of columns.
						{
							var columns = GetEcma48Params(1, 1)[0];
							cursorColumn += columns;
							DataVersion++;
						}
						break;
					case 'D': // Move cursor left the indicated # of columns.
						{
							var columns = GetEcma48Params(1, 1)[0];
							cursorColumn -= columns;
							DataVersion++;
						}
						break;
					case 'E': // Move cursor down the indicated # of rows, to column 1.
						{
							var rows = GetEcma48Params(1, 1)[0];
							cursorRow += rows;
							cursorColumn = 0;
							DataVersion++;
						}
						break;
					case 'F': // Move cursor up the indicated # of rows, to column 1.
						{
							var row = GetEcma48Params(1, 1)[0];
							cursorRow -= row;
							cursorColumn = 0;
							DataVersion++;
						}
						break;
					case 'G': // Move cursor to indicated column in current row.
						{
							var column = GetEcma48Params(1, 1)[0];
							cursorColumn = column - 1; // simulate origin at 1
							DataVersion++;
						}
						break;
					case 'H': // Move cursor to the indicated row, column (origin at 1,1).
						{
							var p = GetEcma48Params(2, 1);
							cursorRow = p[0] - 1; // simulate origin at 1
							cursorColumn = p[1] - 1; // simulate origin at 1
							DataVersion++;
						}
						break;
					case 'J': // Erase display (default: from cursor to end of display).
							  // ESC[1 J: erase from start to cursor.
							  // ESC[2 J: erase whole display.
							  // ESC[3 J: erase whole display including scroll - back buffer
						{
							for (int i = 0; i < screen.Length; i++) screen[i].Reset();

							var p = GetEcma48Params(1, 0)[0];
							if (p == 2 || p == 3)
							{
								// TODO
							}
							DataVersion++;
						}
						break;

					case 'm': // The ECMA-48 SGR sequence ESC [ parameters m sets display attributes.
							  // Several attributes can be set in the same sequence, separated by
							  // semicolons.  An empty parameter (between semicolons or string
							  // initiator or terminator) is interpreted as a zero.
						{
							var parameters = GetEcma48Params(1, 0);
							foreach (var p in parameters)
							{
								if (p == 0)
								{
									currentChar.Reset();
								}
								else if (p == 1)
								{
									currentChar.bold = true;
								}
								else if (p >= 30 && p <= 39)
								{
									var color = (Color)(p - 30);
									currentChar.foregroundColor = color;
								}
								else if (p >= 40 && p <= 49)
								{
									var color = (Color)(p - 40);
									currentChar.backgroundColor = color;
								}
							}
						}
						break;


					default:
						ecma48SequenceParameter += character;
						break;
				}
			}
			else if (character == ASCII.NewLine)
			{
				cursorColumn = 0;
				cursorRow++;
				PositionSanityCheck();
				DataVersion++;
			}
			else if (character == ASCII.CarriageReturn)
			{
				cursorColumn = 0;
				DataVersion++;
			}
			else if (character == ASCII.BackSpace)
			{
				cursorColumn--;
				screen[cursorColumn + cursorRow * (int)ColumnsCount].Reset();
				PositionSanityCheck();
				DataVersion++;
			}
			else
			{
				RawAddText(character);
				DataVersion++;
			}
		}

		CharData currentChar = new CharData();


		void RawAddText(string text)
		{
			foreach (var c in text)
				RawAddText(c);
		}
		void PositionSanityCheck()
		{
			if (cursorColumn < 0)
			{
				cursorColumn = -cursorColumn;

				var rowsBack = 1 + (int)(cursorColumn / ColumnsCount);
				cursorRow -= rowsBack;
				cursorColumn = rowsBack * (int)ColumnsCount - cursorColumn;
			}
			if (cursorColumn >= ColumnsCount)
			{
				cursorColumn = 0;
				cursorRow++;
			}

			if (cursorRow >= RowsCount)
			{
				var rowsMoveUp = (cursorRow - RowsCount) + 1;

				cursorRow -= (int)rowsMoveUp;
				ScreenShiftBy(-(int)(rowsMoveUp * ColumnsCount));
			}
		}
		void ScreenShiftBy(int shiftToRightBy)
		{
			if (shiftToRightBy > 0)
			{
				Array.Copy(screen, 0, screen, shiftToRightBy, screen.Length - shiftToRightBy);
				for (int i = 0; i < shiftToRightBy; i++)
					screen[i].Reset();
			}
			else
			{
				var shiftToLeftBy = -shiftToRightBy;
				Array.Copy(screen, shiftToLeftBy, screen, 0, screen.Length - shiftToLeftBy);
				for (int i = screen.Length - shiftToLeftBy; i < screen.Length; i++)
					screen[i].Reset();
			}
		}
		void RawAddText(char c)
		{
			PositionSanityCheck();
			currentChar.character = c;
			screen[cursorColumn + cursorRow * (int)ColumnsCount] = currentChar;
			cursorColumn++;
		}
	}


}