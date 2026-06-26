using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace mumblelib;

[PublicizedFrom(EAccessModifier.Internal)]
public static class Util
{
	public unsafe static void SetVector3(float* _output, Vector3 _input)
	{
		*_output = _input.x;
		_output[1] = _input.y;
		_output[2] = _input.z;
	}

	public unsafe static void SetString<T>(T* _output, string _input, int _max, Encoding _encoding) where T : unmanaged
	{
		byte[] bytes = _encoding.GetBytes(_input + "\0");
		Marshal.Copy(bytes, 0, new IntPtr(_output), Math.Min(bytes.Length, _max * Marshal.SizeOf<T>()));
	}

	public unsafe static void SetContext(byte* _output, uint* _len, string _input)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(_input);
		*_len = (uint)Math.Min(bytes.Length, 256);
		Marshal.Copy(bytes, 0, new IntPtr(_output), (int)(*_len));
	}
}
