using System;

public class GlobSearch
{
	public bool ignoreCase;
	public bool anyFromStart;
	public bool anyFromEnd;
	public string searchPattern;
	public GlobSearch(string searchPattern)
	{
		this.searchPattern = searchPattern;
		while (this.searchPattern.StartsWith("*"))
		{
			anyFromStart = true;
			this.searchPattern = this.searchPattern.Substring(1);
		}
		while (this.searchPattern.EndsWith("*"))
		{
			anyFromEnd = true;
			this.searchPattern = this.searchPattern.Substring(0, this.searchPattern.Length - 1);
		}
	}

	public bool Matches(string otherName)
	{
		if (searchPattern == "*") return true;
		var comparisonType = StringComparison.InvariantCulture;
		if (ignoreCase) comparisonType = StringComparison.InvariantCultureIgnoreCase;

		if (anyFromEnd && anyFromStart) return otherName.StartsWith(searchPattern, comparisonType) || otherName.EndsWith(searchPattern, comparisonType);
		if (anyFromStart) return otherName.StartsWith(searchPattern, comparisonType);
		if (anyFromEnd) return otherName.EndsWith(searchPattern, comparisonType);
		return otherName.Equals(searchPattern, comparisonType);
	}

}
