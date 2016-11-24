using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ComputerHandler : NeitriBehavior
{
	public string computerId;

	TextDisplayDevice terminal;

	Computer machine;
	bool typingEnabled;

	public TextDisplayDevice[] displays;
	public InputDevice input; 

	protected override void OnEnable()
	{
		if (string.IsNullOrEmpty(computerId)) computerId = this.GetInstanceID().ToString();

		machine = new Computer();
		machine.computerId = computerId;

		machine.ConnectDevice(input);
		displays.ForEach(machine.ConnectDevice);
		machine.ConnectDevice(new StorageDevice(Application.dataPath + "/../VirtualDevicesData/computer_" + computerId + "_disc_1.txt"));
		machine.ConnectDevice(new StorageDevice(Application.dataPath + "/../VirtualDevicesData/computer_" + computerId + "_disc_1.txt"));
	}

	public void BootUp()
	{
		machine.Bootup();
	}

	protected override void OnDisable()
	{
		ShutDown();
	}

	public void ShutDown()
	{
		machine.ShutDown();
	}

}