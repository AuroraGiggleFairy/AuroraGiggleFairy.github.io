using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using Force.Crc32;

public static class IOUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int minBufferSize = 8192;

	public const int DefaultBufferSize = 32768;

	public static byte[] CalcHashSync(string _filename, string _algorithmName = "MD5")
	{
		using HashAlgorithm hashAlgorithm = HashAlgorithm.Create(_algorithmName);
		using Stream inputStream = SdFile.OpenRead(_filename);
		return hashAlgorithm.ComputeHash(inputStream);
	}

	public static IEnumerator CalcHashCoroutine(string _filename, Action<byte[]> _resultCallback, int _maxTimePerFrameMs = 5, byte[] _buffer = null, string _algorithmName = "MD5")
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		using HashAlgorithm hashAlgo = HashAlgorithm.Create(_algorithmName);
		using Stream stream = SdFile.OpenRead(_filename);
		if (_buffer == null || _buffer.Length < 8192)
		{
			_buffer = new byte[32768];
		}
		int bufferSize = _buffer.Length;
		int bytesRead;
		do
		{
			bytesRead = stream.Read(_buffer, 0, bufferSize);
			if (bytesRead > 0)
			{
				hashAlgo.TransformBlock(_buffer, 0, bytesRead, null, 0);
			}
			if (msw.ElapsedMilliseconds >= _maxTimePerFrameMs)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		while (bytesRead > 0);
		hashAlgo.TransformFinalBlock(_buffer, 0, 0);
		_resultCallback(hashAlgo.Hash);
	}

	public static IEnumerator CalcCrcCoroutine(string _filename, Action<uint> _resultCallback, int _maxTimePerFrameMs = 5, byte[] _buffer = null)
	{
		MicroStopwatch msw = new MicroStopwatch(_bStart: true);
		using Stream stream = SdFile.OpenRead(_filename);
		if (_buffer == null || _buffer.Length < 8192)
		{
			_buffer = new byte[32768];
		}
		int bufferSize = _buffer.Length;
		uint crc = 0u;
		int bytesRead;
		do
		{
			bytesRead = stream.Read(_buffer, 0, bufferSize);
			if (bytesRead > 0)
			{
				crc = Crc32Algorithm.Append(crc, _buffer, 0, bytesRead);
			}
			if (msw.ElapsedMilliseconds >= _maxTimePerFrameMs)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		while (bytesRead > 0);
		_resultCallback(crc);
	}

	public static uint HashUint(this Crc32Algorithm _crcInstance)
	{
		if (_crcInstance == null)
		{
			throw new ArgumentNullException("_crcInstance");
		}
		byte[] hash = _crcInstance.Hash;
		return (uint)((hash[0] << 24) | (hash[1] << 16) | (hash[2] << 8) | hash[3]);
	}
}
