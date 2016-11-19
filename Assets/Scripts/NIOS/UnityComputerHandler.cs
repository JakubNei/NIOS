using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnityComputerHandler : MonoBehaviour, IPlayerTouched
{
	public string computerId;

	UnityTerminal terminal;

	Computer machine;
	bool typingEnabled;

	void OnEnable()
	{
		machine = new Computer();
		terminal = new UnityTerminal(GetComponentInChildren<Text>());
		if (string.IsNullOrEmpty(computerId)) computerId = this.GetInstanceID().ToString();

		machine.ConnectDevice(terminal);
		machine.ConnectDevice(new StorageDevice(Application.dataPath + "/../VirtualDevicesData/computer_" + computerId + "_disc_1.txt"));
		machine.ConnectDevice(new StorageDevice(Application.dataPath + "/../VirtualDevicesData/computer_" + computerId + "_disc_1.txt"));
	}

	public void BootUp()
	{
		machine.Bootup();
	}

	public void ShutDown()
	{
		machine.ShutDown();
	}

	void Update()
	{
		terminal.UnityUpdate();

		if (typingEnabled)
		{
			terminal.DoType(Input.inputString);
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				typingEnabled = false;
				player.InputEnabled(true);
			}
		}
	}

	PlayerControl player;
	public void OnTouched(PlayerControl player)
	{
		this.player = player;
		player.InputEnabled(false);
		this.typingEnabled = true;
	}
}