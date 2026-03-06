using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Discord;

internal struct Image : IDisposable
{
	private bool _isDisposed;

	public Stream Stream
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public Image(Stream stream)
	{
		_isDisposed = false;
		Stream = stream;
	}

	public Image(string path)
	{
		_isDisposed = false;
		Stream = File.OpenRead(path);
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
