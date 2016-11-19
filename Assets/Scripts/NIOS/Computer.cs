using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public class Computer
{
	List<IDevice> devices = new List<IDevice>();
	List<WeakReference> threads = new List<WeakReference>();

	public IEnumerable<IDevice> Devices
	{
		get
		{
			return devices;
		}
	}

	public T GetFirstDeviceOfType<T>() where T : class, IDevice
	{
		var dev = devices.FirstOrDefault(d => d.GetType() == typeof(T) || typeof(T).IsAssignableFrom(d.GetType()));
		if (dev == null) return null;
		return dev as T;
	}

	public void Write(string a)
	{
		var terminal = GetFirstDeviceOfType<ITerminal>();
		if (terminal != null) terminal.GetWriter().Write(a);
	}
	public void WriteLine(string a)
	{
		var terminal = GetFirstDeviceOfType<ITerminal>();
		if (terminal != null) terminal.GetWriter().WriteLine(a);
	}
	public void Clear()
	{
		var terminal = GetFirstDeviceOfType<ITerminal>();
		if (terminal != null) terminal.Clear();
	}

	public void ConnectDevice(IDevice device)
	{
		devices.Add(device);
	}

	public Thread CreateThread(ThreadStart start)
	{
		var t = new Thread(start);
		t.IsBackground = true;
		t.Priority = ThreadPriority.Lowest;
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
			if (preferredBootDevice == null) preferredBootDevice = devices.FirstOrDefault(d => d.Type == DeviceType.Block);
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