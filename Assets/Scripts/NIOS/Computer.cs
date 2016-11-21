using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using StdLib;

public class Computer
{
	List<IDevice> devices = new List<IDevice>();
	List<WeakReference> threads = new List<WeakReference>();

	public string computerId;

	public IEnumerable<IDevice> Devices { get { return devices; } }

	public event Action<IDevice> OnDeviceConnected;
	public event Action<IDevice> OnDeviceDisconnected;

	public IDevice GetFirstDeviceByPrefix(string prefix)
	{
		var dev = devices.FirstOrDefault(d => d.DeviceType.NamePrefix == prefix);
		return dev;
	}

	void Write(Action<StreamWriter> streamWriterAction)
	{
		var d = GetFirstDeviceByPrefix("tty");
		if (d == null) return;
		var sr = new StreamWriter(d.OpenWrite());
		streamWriterAction.Raise(sr);
		sr.Dispose();
	}

	public void Write(string a)
	{
		Write(sr => sr.Write(a));
	}
	public void WriteLine(string a)
	{
		Write(sr => sr.WriteLine(a));
	}
	public void Clear()
	{
		Write(sr => new StdLib.Ecma48.Client(sr).EraseDisplay());
	}

	public void ConnectDevice(IDevice device)
	{
		devices.Add(device);
		OnDeviceConnected.Raise(device);
	}

	public void DisconnectDevice(IDevice device)
	{
		devices.Remove(device);
		OnDeviceDisconnected.Raise(device);
	}

	public Thread CreateThread(ThreadStart start)
	{
		var t = new Thread(start);
		t.IsBackground = true;
		t.Priority = ThreadPriority.Lowest;
		t.Name = computerId + "_#" + threads.Count;
		Utils.SetDefaultCultureInfo(t);
		threads.Add(new WeakReference(t));
		return t;
	}


	public void ShutDown()
	{
		foreach (var w in threads)
		{
			if (w.IsAlive)
			{
				var t = w.Target as Thread;
				if (t != null && t.IsAlive)
					t.Abort();
			}
		}
		threads.Clear();

		Clear();
	}

	public void Bootup(IDevice preferredBootDevice = null)
	{
		ShutDown();

		CreateThread(() =>
		{
			if (preferredBootDevice == null) preferredBootDevice = devices.FirstOrDefault(d => d.DeviceType.IOType == DeviceIOType.Block);
			if (preferredBootDevice == null)
				WriteLine("unable to boot up, no block devices attached");
			/*
			string bootSector;
			using (var sr = new StreamReader(preferredBootDevice.OpenRead()))
				bootSector = sr.ReadToEnd();

			if (bootSector == OperatingSystem.bootSectorBase64)*/
			{
				WriteLine("found operating system");
				Write("booting up");
				for (int i = 0; i < 20; i++)
				{
					Write(".");
					Thread.Sleep(10);
				}

				Clear();

				var os = new OperatingSystem();
				os.StartUp(this);
			}
		}).Start();
	}

}