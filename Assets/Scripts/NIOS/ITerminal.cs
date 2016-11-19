using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public interface ITerminal : IDevice
{
	void Clear();

	TextWriter GetWriter();

	TextReader GetReader();

	//event Action<TerminalInput> OnInputTyped;
}

public struct TerminalInput
{
	public char input;
}