using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// http://askubuntu.com/questions/341939/why-cant-i-create-a-directory-in-sys
/// https://www.kernel.org/doc/Documentation/filesystems/sysfs.txt
/// </summary>
public class SysFs : IFileSystem
{
	public SysFs(DirEntry mountPoint)
	{
		//this.mountPoint = mountPoint;
	}

	public void CreateDirectory(DirEntry directory, DirectorySecurity directorySecurity)
	{
		throw new NotImplementedException();
	}

	public void DeleteDirectory(DirEntry directory)
	{
		throw new NotImplementedException();
	}

	public void DeleteFile(FileEntry file)
	{
		throw new NotImplementedException();
	}

	public Stream Open(FileEntry file, FileMode mode, FileAccess access, FileShare share)
	{
		throw new NotImplementedException();
	}

	public void UpdateDirectoryInfo(DirEntry.UpdateHandle directory)
	{
		throw new NotImplementedException();
	}
}

