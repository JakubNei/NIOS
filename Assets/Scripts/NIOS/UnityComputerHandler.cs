using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnityComputerHandler : NeitriBehavior, IPlayerTouched
{
	public string computerId;

	UnityTerminal terminal;

	Computer machine;
	bool typingEnabled;

	protected override void OnEnable()
	{

		terminal = new UnityTerminal(GetComponentInChildren<Text>());
		if (string.IsNullOrEmpty(computerId)) computerId = this.GetInstanceID().ToString();
		
		machine = new Computer();
		machine.computerId = computerId;

		machine.ConnectDevice(terminal);
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

	protected override void Update()
	{
		if (typingEnabled)
		{
			terminal.DoType(Input.inputString);
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				typingEnabled = false;
				player.InputEnabled(true);
			}
		}

		terminal.DisplayUpdate();
	}

	PlayerControl player;
	public void OnTouched(PlayerControl player)
	{
		this.player = player;
		player.InputEnabled(false);
		this.typingEnabled = true;
	}
}