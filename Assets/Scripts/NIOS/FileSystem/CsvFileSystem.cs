using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class CsvFileSystem : IFileSystem
{
	FileEntry device;

	enum EntryType
	{
		TreeEntry = 0,
		FileContents = 1,
	}

	class Entry
	{
		public ulong parentId;
		public string name;
	}

	class FileContents
	{
		public byte[] contents = new byte[0];
	}

	Dictionary<ulong, Entry> entries = new Dictionary<ulong, Entry>();
	Dictionary<ulong, FileContents> fileContents = new Dictionary<ulong, FileContents>();
	ulong nextId = 1;

	//string firstLineIsBootSector;

	DirEntry mountPoint;

	public CsvFileSystem(DirEntry mountPoint, FileEntry device, OperatingSystem os)
	{
		this.device = device;
		this.mountPoint = mountPoint;

		Load();
	}

	class FileSystemException : Error
	{
		public FileSystemException(string message) : base(message)
		{

		}
	}


	public void UpdateDirectoryInfo(DirEntry.UpdateHandle dir)
	{
		var path = mountPoint.GetRelativePathTo(dir.DirEntry);

		ulong id = 0;
		GetEntry(path, ref id);

		var filtered = entries.Where(e => e.Value.parentId == id).ToArray();
		var files = filtered.Where(e => fileContents.ContainsKey(e.Key)).ToArray();
		var dirs = filtered.Except(files).ToArray();

		foreach (var d in dirs) dir.AddDirectory(d.Value.name);
		foreach (var f in files) dir.AddFile(f.Value.name);
	}


	bool GetEntry(string path, ref ulong id)
	{
		if (path.StartsWith("/")) path = path.Substring(1);
		if (path == string.Empty) { id = 0; return true; }
		if (path == ".") { id = 0; return true; }
		var pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		if (pathParts.Length == 0) { id = 0; return true; }
		ulong parent = 0;
		for (int i = 0; i < pathParts.Length; i++)
		{
			var pathPart = pathParts[i];
			var filtered = entries.Where(e => e.Value.parentId == parent && e.Value.name == pathPart);
			if (filtered.Count() > 1) throw new Exception("can not have multiple path part names with same parent, path part name: " + pathPart + ", parent: " + parent);
			if (filtered.Count() == 0) return false; // ("entry with path part not found, path part: " + pathPart);

			var entryId = filtered.First().Key;
			if (i == pathParts.Length - 1) { id = entryId; return true; } // end of path
			else parent = entryId;
		}
		return false;
	}

	ulong GetOrCreateEntry(string path)
	{
		if (path.StartsWith("/")) path = path.Substring(1);
		if (path == string.Empty) return 0;
		if (path == ".") return 0;
		var pathParts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
		if (pathParts.Length == 0) return 0;
		ulong parent = 0;
		for (int i = 0; i < pathParts.Length; i++)
		{
			var pathPart = pathParts[i];
			ulong entryId = 0;
			var filteredEntries = entries.Where(e => e.Value.parentId == parent && e.Value.name == pathPart);
			if (filteredEntries.Count() > 1) throw new FileSystemException("can not have multiple path part names with same parent, path part name: " + pathPart + ", parent: " + parent);
			if (filteredEntries.Count() == 0)
			{
				// no entry in path, lets create it
				entries[nextId] = new Entry() { parentId = parent, name = pathPart };
				entryId = nextId;
				nextId++;
			}
			else
			{
				entryId = filteredEntries.First().Key;
			}

			if (i == pathParts.Length - 1) return entryId; // end of path
			else parent = entryId;
		}
		throw new Exception("not expected");
	}

	void Save()
	{
		using (var w = new StreamWriter(device.OpenWrite()))
		{
			//w.WriteLine(firstLineIsBootSector);
			var csv = new CSV();
			foreach (var e in entries)
			{
				csv.Add(((int)EntryType.TreeEntry).ToString(), e.Key.ToString(), e.Value.parentId.ToString(), e.Value.name);
			}
			foreach (var f in fileContents)
			{
				var contents = f.Value.contents;
				var asText = Encoding.ASCII.GetString(contents);
				if (contents.Length == 0 || Regex.IsMatch(asText, @"^[\s\x20-\xFF]+$")) // encode in asci
					csv.Add(((int)EntryType.FileContents).ToString(), f.Key.ToString(), "1", asText);
				else // encode in base64
					csv.Add(((int)EntryType.FileContents).ToString(), f.Key.ToString(), "0", Convert.ToBase64String(f.Value.contents));
			}

			foreach (var l in csv.GetLinesToSave())
				w.WriteLine(l);
		}
	}

	void Load()
	{
		entries.Clear();
		fileContents.Clear();
		nextId = 1;

		using (var r = new StreamReader(device.OpenRead()))
		{
			//firstLineIsBootSector = r.ReadLine();
			var csv = CSV.From(r);
			foreach (var row in csv)
			{
				var entryType = (EntryType)int.Parse(row[0]);

				var entryId = ulong.Parse(row[1]);
				if (entryId > nextId) nextId = entryId + 1;

				if (entryType == EntryType.TreeEntry)
				{
					var parentId = ulong.Parse(row[2]);
					var name = row[3];
					entries.Add(entryId, new Entry() { name = name, parentId = parentId });
				}

				if (entryType == EntryType.FileContents)
				{
					if (row[2] == "0") // base 64 encoded
					{
						var contents = Convert.FromBase64String(row[3]);
						fileContents.Add(entryId, new FileContents() { contents = contents });
					}
					else if (row[2] == "1") // asci encoded
					{
						var contents = Encoding.ASCII.GetBytes(row[3]);
						fileContents.Add(entryId, new FileContents() { contents = contents });
					}
				}
			}
		}
	}

	void RemoveChildEntries(ulong parent)
	{
		var toRemove = entries.Where(e => e.Value.parentId == parent).ToArray();
		foreach (var entry in toRemove)
		{
			var id = entry.Key;
			if (fileContents.ContainsKey(id))
				fileContents.Remove(id);
			entries.Remove(id);
			RemoveChildEntries(id);
		}
	}

	public void CreateDirectory(DirEntry directory, DirectorySecurity directorySecurity)
	{
		var path = mountPoint.GetRelativePathTo(directory);
		var id = GetOrCreateEntry(path);
		Save();
	}


	public void DeleteDirectory(DirEntry dir)
	{
		var path = mountPoint.GetRelativePathTo(dir);
		ulong id = 0;
		if (!GetEntry(path, ref id))
			throw new System.IO.DirectoryNotFoundException("'" + dir + "' doesnt exist");
		if (fileContents.ContainsKey(id)) // this is file
			throw new System.IO.DirectoryNotFoundException("'" + dir + "' is a file, use delete file instead");
		entries.Remove(id);
		RemoveChildEntries(id);
		Save();
	}

	public void DeleteFile(FileEntry file)
	{
		var path = mountPoint.GetRelativePathTo(file);
		ulong id = 0;
		if (!GetEntry(path, ref id))
			throw new System.IO.FileNotFoundException("'" + file + "' doesnt exist");
		if (!fileContents.ContainsKey(id)) // this is directory
			throw new System.IO.FileNotFoundException("'" + file + "' is a directory, use delete directory instead");
		entries.Remove(id);
		fileContents.Remove(id);
		Save();
	}

	class MyStream : Stream
	{
		byte[] data;
		long length;
		int Capacity { get { return data.Length; } }

		public override bool CanRead { get { return true; } }

		public override bool CanSeek { get { return true; } }

		public override bool CanWrite { get { return true; } }

		public override long Length { get { return length; } }

		public override long Position { get; set; }

		FileContents file;
		CsvFileSystem fs;

		public MyStream(FileContents file, CsvFileSystem fs)
		{
			this.file = file;
			data = file.contents;
			length = file.contents.Length;
			this.fs = fs;
		}

		public override void Flush()
		{
			file.contents = new byte[length];
			Array.Copy(data, file.contents, length);
			fs.Save();
		}

		protected override void Dispose(bool disposing)
		{
			Flush();
			base.Dispose(disposing);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position + count > Length) count = (int)(Length - Position);
			Array.Copy(data, (int)Position, buffer, offset, count);
			Position += count;
			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin) Position = offset;
			if (origin == SeekOrigin.Current) Position += offset;
			if (origin == SeekOrigin.End) Position = Length - offset;
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count < 0) throw new Exception("wtf count < 0"); // this probably never happens ? better be sure ?
			if (Position + count > Capacity)
			{
				int newCapacity;
				if (Capacity == 0)
				{
					if (count < 2) newCapacity = 2;
					else newCapacity = count;
				}
				else newCapacity = data.Length * 2;
				while (Position + count > newCapacity) newCapacity *= 2;

				var n = new byte[newCapacity];
				Array.Copy(data, n, data.Length);
				data = n;
			}

			Array.Copy(buffer, offset, data, Position, count);
			Position += count;

			if (Position > length) length = Position;
		}
	}


	bool MustNotExist(FileMode mode)
	{
		return mode == FileMode.CreateNew;
	}
	bool MustExist(FileMode mode)
	{
		return mode == FileMode.Open || mode == FileMode.Append;
	}
	bool CanCreateNew(FileMode mode)
	{
		return mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.OpenOrCreate || mode == FileMode.Append;
	}

	public Stream Open(FileEntry file, FileMode mode, FileAccess access, FileShare share)
	{
		var path = mountPoint.GetRelativePathTo(file);

		ulong id = 0;
		var entryExists = GetEntry(path, ref id);

		if (entryExists)
		{
			if (MustNotExist(mode))
				throw new System.IO.IOException("file '" + file + "' already exists, mode: " + mode + ", access: " + access);
			if (!fileContents.ContainsKey(id)) // this is a directory, not a file
				throw new System.IO.FileNotFoundException("'" + file + "' is existing directory, canot open as file");
			if (mode == FileMode.Truncate)
				fileContents[id] = new FileContents();
		}
		else
		{
			if (MustExist(mode))
				throw new System.IO.FileNotFoundException("file '" + file + "' doesnt exist, mode: " + mode + ", access: " + access);
			id = GetOrCreateEntry(path);
			fileContents[id] = new FileContents();
		}

		if (access == FileAccess.Read)
		{
			var bytes = fileContents[id].contents;
			return new MemoryStream(bytes, false);
		}
		if (access == FileAccess.Write)
		{
			var fileContent = fileContents[id];
			var s = new MyStream(fileContent, this);
			if (mode == FileMode.Append) s.Position = s.Length;
			return s;
		}
		throw new NotImplementedException(access.ToString());
	}
}