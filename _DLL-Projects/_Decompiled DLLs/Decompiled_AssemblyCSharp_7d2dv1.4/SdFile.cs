using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class SdFile
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Encoding UTF8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	public static StreamReader OpenText(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpenText(managedPath);
		}
		return File.OpenText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamReader ManagedOpenText(SaveDataManagedPath path)
	{
		return new StreamReader(ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read));
	}

	public static StreamWriter CreateText(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedCreateText(managedPath);
		}
		return File.CreateText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedCreateText(SaveDataManagedPath path)
	{
		return new StreamWriter(ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read));
	}

	public static StreamWriter AppendText(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedAppendText(managedPath);
		}
		return File.AppendText(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static StreamWriter ManagedAppendText(SaveDataManagedPath path)
	{
		return new StreamWriter(ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read));
	}

	public static void Copy(string sourceFileName, string destFileName)
	{
		SaveDataManagedPath managedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out managedPath);
		SaveDataManagedPath managedPath2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out managedPath2);
		if (flag && flag2)
		{
			ManagedToManagedCopy(managedPath, managedPath2, overwrite: false);
		}
		else if (flag)
		{
			ManagedToUnmanagedCopy(managedPath, destFileName, overwrite: false);
		}
		else if (flag2)
		{
			UnmanagedToManagedCopy(sourceFileName, managedPath2, overwrite: false);
		}
		else
		{
			File.Copy(sourceFileName, destFileName);
		}
	}

	public static void Copy(string sourceFileName, string destFileName, bool overwrite)
	{
		SaveDataManagedPath managedPath;
		bool flag = SaveDataUtils.TryGetManagedPath(sourceFileName, out managedPath);
		SaveDataManagedPath managedPath2;
		bool flag2 = SaveDataUtils.TryGetManagedPath(destFileName, out managedPath2);
		if (flag && flag2)
		{
			ManagedToManagedCopy(managedPath, managedPath2, overwrite);
		}
		else if (flag)
		{
			ManagedToUnmanagedCopy(managedPath, destFileName, overwrite);
		}
		else if (flag2)
		{
			UnmanagedToManagedCopy(sourceFileName, managedPath2, overwrite);
		}
		else
		{
			File.Copy(sourceFileName, destFileName, overwrite);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToManagedCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using Stream source = ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		using Stream destination = ManagedOpen(destFileName, (!overwrite) ? FileMode.CreateNew : FileMode.Create, FileAccess.Write, FileShare.Read);
		StreamUtils.StreamCopy(source, destination);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedToUnmanagedCopy(SaveDataManagedPath sourceFileName, string destFileName, bool overwrite)
	{
		using Stream source = ManagedOpen(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		using FileStream destination = File.Open(destFileName, (!overwrite) ? FileMode.CreateNew : FileMode.Create, FileAccess.Write, FileShare.Read);
		StreamUtils.StreamCopy(source, destination);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void UnmanagedToManagedCopy(string sourceFileName, SaveDataManagedPath destFileName, bool overwrite)
	{
		using FileStream source = File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
		using Stream destination = ManagedOpen(destFileName, (!overwrite) ? FileMode.CreateNew : FileMode.Create, FileAccess.Write, FileShare.Read);
		StreamUtils.StreamCopy(source, destination);
	}

	public static Stream Create(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedCreate(managedPath);
		}
		return File.Create(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedCreate(SaveDataManagedPath path)
	{
		return ManagedOpen(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
	}

	public static void Delete(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedDelete(managedPath);
		}
		else
		{
			File.Delete(path);
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void ManagedDelete(SaveDataManagedPath path)
	{
		SaveDataUtils.SaveDataManager.ManagedFileDelete(path);
	}

	public static bool Exists(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedExists(managedPath);
		}
		return File.Exists(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static bool ManagedExists(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileExists(path);
	}

	public static Stream Open(string path, FileMode mode)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpen(managedPath, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
		}
		return File.Open(path, mode);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpen(managedPath, mode, access, FileShare.None);
		}
		return File.Open(path, mode, access);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpen(managedPath, mode, access, share);
		}
		return File.Open(path, mode, access, share);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static Stream ManagedOpen(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileOpen(path, mode, access, share);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedGetLastWriteTime(managedPath);
		}
		return File.GetLastWriteTime(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DateTime ManagedGetLastWriteTime(SaveDataManagedPath path)
	{
		return ManagedGetLastWriteTimeUtc(path).ToLocalTime();
	}

	public static DateTime GetLastWriteTimeUtc(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedGetLastWriteTimeUtc(managedPath);
		}
		return File.GetLastWriteTimeUtc(path);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static DateTime ManagedGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return SaveDataUtils.SaveDataManager.ManagedFileGetLastWriteTimeUtc(path);
	}

	public static Stream OpenRead(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpen(managedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		}
		return File.OpenRead(path);
	}

	public static Stream OpenWrite(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedOpen(managedPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
		}
		return File.OpenWrite(path);
	}

	public static string ReadAllText(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadAllText(managedPath, Encoding.UTF8);
		}
		return File.ReadAllText(path);
	}

	public static string ReadAllText(string path, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadAllText(managedPath, encoding);
		}
		return File.ReadAllText(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static string ManagedReadAllText(SaveDataManagedPath path, Encoding encoding)
	{
		using Stream stream = ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream, encoding);
		return streamReader.ReadToEnd();
	}

	public static void WriteAllText(string path, string contents)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllText(managedPath, contents, UTF8NoBom);
		}
		else
		{
			File.WriteAllText(path, contents);
		}
	}

	public static void WriteAllText(string path, string contents, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllText(managedPath, contents, encoding);
		}
		else
		{
			File.WriteAllText(path, contents, encoding);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using Stream stream = ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read);
		using StreamWriter streamWriter = new StreamWriter(stream, encoding);
		streamWriter.Write(contents);
	}

	public static byte[] ReadAllBytes(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadAllBytes(managedPath);
		}
		return File.ReadAllBytes(path);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] ManagedReadAllBytes(SaveDataManagedPath path)
	{
		using PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
		using Stream stream = ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		stream.CopyTo(pooledExpandableMemoryStream);
		return pooledExpandableMemoryStream.ToArray();
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllBytes(managedPath, bytes);
		}
		else
		{
			File.WriteAllBytes(path, bytes);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllBytes(SaveDataManagedPath path, byte[] bytes)
	{
		using Stream stream = ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read);
		stream.Write(bytes, 0, bytes.Length);
	}

	public static string[] ReadAllLines(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadAllLines(managedPath, Encoding.UTF8);
		}
		return File.ReadAllLines(path);
	}

	public static string[] ReadAllLines(string path, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadAllLines(managedPath, encoding);
		}
		return File.ReadAllLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string[] ManagedReadAllLines(SaveDataManagedPath path, Encoding encoding)
	{
		List<string> list = new List<string>();
		using Stream stream = ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader streamReader = new StreamReader(stream, encoding);
		while (true)
		{
			string text = streamReader.ReadLine();
			if (text == null)
			{
				break;
			}
			list.Add(text);
		}
		return list.ToArray();
	}

	public static IEnumerable<string> ReadLines(string path)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadLines(managedPath, Encoding.UTF8);
		}
		return File.ReadLines(path);
	}

	public static IEnumerable<string> ReadLines(string path, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			return ManagedReadLines(managedPath, encoding);
		}
		return File.ReadLines(path, encoding);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerable<string> ManagedReadLines(SaveDataManagedPath path, Encoding encoding)
	{
		using Stream inputStream = ManagedOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		using StreamReader reader = new StreamReader(inputStream, encoding);
		while (true)
		{
			string text = reader.ReadLine();
			if (text != null)
			{
				yield return text;
				continue;
			}
			break;
		}
	}

	public static void WriteAllLines(string path, string[] contents)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllLines(managedPath, contents, UTF8NoBom);
		}
		else
		{
			File.WriteAllLines(path, contents);
		}
	}

	public static void WriteAllLines(string path, string[] contents, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllLines(managedPath, contents, encoding);
		}
		else
		{
			File.WriteAllLines(path, contents, encoding);
		}
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllLines(managedPath, contents, UTF8NoBom);
		}
		else
		{
			File.WriteAllLines(path, contents);
		}
	}

	public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedWriteAllLines(managedPath, contents, encoding);
		}
		else
		{
			File.WriteAllLines(path, contents, encoding);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using Stream stream = ManagedOpen(path, FileMode.Create, FileAccess.Write, FileShare.Read);
		using StreamWriter writer = new StreamWriter(stream, encoding);
		ManagedWriteAllLines(writer, contents);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedWriteAllLines(TextWriter writer, IEnumerable<string> contents)
	{
		using (writer)
		{
			foreach (string content in contents)
			{
				writer.WriteLine(content);
			}
		}
	}

	public static void AppendAllText(string path, string contents)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedAppendAllText(managedPath, contents, UTF8NoBom);
		}
		else
		{
			File.AppendAllText(path, contents);
		}
	}

	public static void AppendAllText(string path, string contents, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedAppendAllText(managedPath, contents, encoding);
		}
		else
		{
			File.AppendAllText(path, contents, encoding);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllText(SaveDataManagedPath path, string contents, Encoding encoding)
	{
		using Stream stream = ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read);
		using StreamWriter streamWriter = new StreamWriter(stream, encoding);
		streamWriter.Write(contents);
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedAppendAllLines(managedPath, contents, UTF8NoBom);
		}
		else
		{
			File.AppendAllLines(path, contents);
		}
	}

	public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
	{
		if (SaveDataUtils.TryGetManagedPath(path, out var managedPath))
		{
			ManagedAppendAllLines(managedPath, contents, encoding);
		}
		else
		{
			File.AppendAllLines(path, contents, encoding);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ManagedAppendAllLines(SaveDataManagedPath path, IEnumerable<string> contents, Encoding encoding)
	{
		using Stream stream = ManagedOpen(path, FileMode.Append, FileAccess.Write, FileShare.Read);
		using StreamWriter writer = new StreamWriter(stream, encoding);
		ManagedWriteAllLines(writer, contents);
	}
}
