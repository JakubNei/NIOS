using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class CSV : List<List<string>>
{
	public char separator = ',';
	public static readonly char[] possibleQuotasCharacters = new char[] { '"', '\'' };
	public char usedQutaCharacter = '"';

	public static CSV FromLines(string[] lines)
	{
		var csv = new CSV();
		csv.AddLines(lines);
		return csv;
	}

	public static CSV From(StreamReader reader)
	{
		return From(reader.ReadToEnd());
	}

	public static CSV From(string text)
	{
		var csv = new CSV();
		csv.Parse(text);
		csv.FinishParse();
		return csv;
	}

	public void AddLines(string[] lines)
	{
		foreach (var line in lines)
		{
			Parse(line + "\n");
		}
		FinishParse();
	}

	List<string> parser_currentRow = new List<string>();
	string parser_currentCell = string.Empty;
	char parser_isInsideQuota = (char)0;

	void Parse(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			var c = text[i];

			if (possibleQuotasCharacters.Contains(c))
			{
				if (parser_isInsideQuota == (char)0)
				{
					parser_isInsideQuota = c;
					continue;
				}
				else if (c == parser_isInsideQuota)
				{
					if (i + 1 < text.Length && text[i + 1] == c) // we have double quotes, inside quotes, that means escaped single quote
					{
						parser_currentCell += c;
						i++;
						continue;
					}
					parser_isInsideQuota = (char)0;
					continue;
				}
			}

			if (parser_isInsideQuota == (char)0)
			{
				if (c == separator)
				{
					FinishCellParse();
					continue;
				}
				else if (c == '\n')
				{
					FinishParse();
					continue;
				}
				else if (c == '\r') // skip those characters
				{
					continue;
				}
			}

			parser_currentCell += c;
		}
	}

	void FinishCellParse()
	{
		if (!(string.IsNullOrEmpty(parser_currentCell) && parser_currentRow.Count == 0))
			parser_currentRow.Add(parser_currentCell);
		parser_currentCell = string.Empty;
	}

	public void FinishParse() // finish row parse
	{
		FinishCellParse();

		if (parser_currentRow.Count > 0)
			this.Add(parser_currentRow);
		parser_currentRow = new List<string>();
	}

	public string[] GetLinesToSave()
	{
		return this.Select(row =>
		{
			var cells = row.Select(cell =>
			{
				var mustEncloseInQuotes = false;
				if (cell.Contains(separator) || cell.Contains('\n') || cell.Contains('\r'))
				{
					mustEncloseInQuotes = true;
				}
				if (cell.Contains(usedQutaCharacter))
				{
					cell.Replace(usedQutaCharacter.ToString(), usedQutaCharacter.ToString() + usedQutaCharacter.ToString());
					mustEncloseInQuotes = true;
				}
				if (mustEncloseInQuotes)
					cell = usedQutaCharacter + cell + usedQutaCharacter;
				return cell;
			}).ToArray();
			return string.Join(separator.ToString(), cells);
		}).ToArray();
	}

	public void Add(params string[] cellsInRow)
	{
		Add(new List<string>(cellsInRow));
	}
}