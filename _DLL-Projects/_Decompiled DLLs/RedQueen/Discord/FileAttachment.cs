using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Discord;

internal struct FileAttachment : IDisposable
{
	private bool _isDisposed;

	public string FileName
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		set; }

	public string Description
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		set; }

	public bool IsSpoiler
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
		set; }

	public Stream Stream
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public FileAttachment(Stream stream, string fileName, string description = null, bool isSpoiler = false)
	{
		_isDisposed = false;
		FileName = fileName;
		Description = description;
		Stream = stream;
		try
		{
			Stream.Position = 0L;
		}
		catch
		{
		}
		IsSpoiler = isSpoiler;
	}

	public FileAttachment(string path, string fileName = null, string description = null, bool isSpoiler = false)
	{
		_isDisposed = false;
		Stream = File.OpenRead(path);
		FileName = fileName ?? Path.GetFileName(path);
		Description = description;
		IsSpoiler = isSpoiler;
	}

	public void Dispose()
	{
		if (!_isDisposed)
		{
			Stream?.Dispose();
			_isDisposed = true;
		}
	}
}
