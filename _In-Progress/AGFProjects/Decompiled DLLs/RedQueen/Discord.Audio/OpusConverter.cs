using System;

namespace Discord.Audio;

internal abstract class OpusConverter : IDisposable
{
	protected IntPtr _ptr;

	public const int SamplingRate = 48000;

	public const int Channels = 2;

	public const int FrameMillis = 20;

	public const int SampleBytes = 4;

	public const int FrameSamplesPerChannel = 960;

	public const int FrameSamples = 1920;

	public const int FrameBytes = 3840;

	protected bool _isDisposed;

	protected virtual void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			_isDisposed = true;
		}
	}

	~OpusConverter()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected static void CheckError(int result)
	{
		if (result < 0)
		{
			throw new Exception($"Opus Error: {(OpusError)result}");
		}
	}

	protected static void CheckError(OpusError error)
	{
		if (error < OpusError.OK)
		{
			throw new Exception($"Opus Error: {error}");
		}
	}
}
