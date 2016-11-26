using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public enum SpecialFolder
{
	Desktop = 0,
	Programs = 2,
	MyDocuments = 5,
	Personal = 5,
	Favorites = 6,
	Startup = 7,
	Recent = 8,
	SendTo = 9,
	StartMenu = 11,
	MyMusic = 13,
	DesktopDirectory = 16,
	MyComputer = 17,
	Templates = 21,
	ApplicationData = 26,
	LocalApplicationData = 28,
	InternetCache = 32,
	Cookies = 33,
	History = 34,
	CommonApplicationData = 35,
	System = 37,
	ProgramFiles = 38,
	MyPictures = 39,
	CommonProgramFiles = 43
}

public partial class Session
{

	public partial class ApiClass
	{
		private Session session;

		public OperatingSystem OperatingSystem { get { return session.operatingSystem; } }
		public ConsoleClass Console { get; private set; }
		public EnvironmentClass Environment { get; private set; }
		public PathClass Path { get; private set; }
		public FileClass File { get; private set; }
		public DirectoryClass Directory { get; private set; }
		public ProcessClass Process { get; private set; }
		public ThreadClass Thread { get; private set; }

		public class CommonBase
		{
			protected Session Session { get; set; }

			protected OperatingSystem OperatingSystem { get { return Session.Api.OperatingSystem; } }
			protected ConsoleClass Console { get { return Session.Api.Console; } }
			protected EnvironmentClass Environment { get { return Session.Api.Environment; } }
			protected PathClass Path { get { return Session.Api.Path; } }
			protected FileClass File { get { return Session.Api.File; } }
			protected DirectoryClass Directory { get { return Session.Api.Directory; } }
			protected ThreadClass Thread { get { return Session.Api.Thread; } }
			protected ProcessClass Process { get { return Session.Api.Process; } }
		}

		public class HelperBase : CommonBase
		{
			public HelperBase(Session session)
			{
				this.Session = session;
			}
		}

		public abstract class ProgramBase : CommonBase
		{
			public new Session Session { get { return base.Session; } set { base.Session = value; } }
		}



		public void InitializeSession(Session session)
		{
			this.session = session;
			Console = new ConsoleClass(session);
			Environment = new EnvironmentClass(session);
			Path = new PathClass(session);
			File = new FileClass(session);
			Directory = new DirectoryClass(session);
			Thread = new ThreadClass(session);
			Process = new ProcessClass(session);
		}
	}



	public ApiClass Api { get; private set; }

	public Session()
	{
		Init();
	}

	public void Init()
	{
		Api = new ApiClass();
		Api.InitializeSession(this);
	}

	public Session Clone()
	{
		var s = (Session)this.MemberwiseClone();
		s.Init();
		return s;
	}



	public OperatingSystem operatingSystem;
	public string userName;
	public string cmdLineUsedToStart;
	public string[] argsUsedToStart;
	public string currentDirectory;
	public TextReader stdIn;
	public TextWriter stdOut;
	public TextWriter stdErr;



}