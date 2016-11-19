using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Inspired by linux file system driver simple example implementation:
/// https://github.com/psankar/simplefs/blob/master/simple.c
/// and by blog post: http://kukuruku.co/hub/nix/writing-a-file-system-in-linux-kernel
/// or https://sysplay.in/blog/tag/struct-inode_operations/
/// </summary>
public interface IFileSystem
{
	// read file contents
	// write file contents
	// create node (mkdir)
	// delete node
	// lookup node (dir)
	// mount, returns root dir

	void GatherDirectoryInfo(DirEntry directory);

	void CreateDirectory(DirEntry directory, DirectorySecurity directorySecurity);

	void DeleteDirectory(DirEntry directory);

	// void MoveTo(DirEntry directory, DirEntry destinationDirectory);

	void DeleteFile(FileEntry file);

	// void SetAccessControl(FileEntry file, FileSecurity fileSecurity);

	Stream Open(FileEntry file, FileMode mode, FileAccess access, FileShare share);
}