using System;
using System.Runtime.InteropServices;

namespace Discord.Audio;

internal class OpusDecoder : OpusConverter
{
	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_create")]
	private static extern IntPtr CreateDecoder(int Fs, int channels, out OpusError error);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_destroy")]
	private static extern void DestroyDecoder(IntPtr decoder);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decode")]
	private unsafe static extern int Decode(IntPtr st, byte* data, int len, byte* pcm, int max_frame_size, int decode_fec);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_decoder_ctl")]
	private static extern int DecoderCtl(IntPtr st, OpusCtl request, int value);

	public OpusDecoder()
	{
		_ptr = CreateDecoder(48000, 2, out var error);
		OpusConverter.CheckError(error);
	}

	public unsafe int DecodeFrame(byte[] input, int inputOffset, int inputCount, byte[] output, int outputOffset, bool decodeFEC)
	{
		int num;
		fixed (byte* ptr = input)
		{
			fixed (byte* ptr2 = output)
			{
				num = Decode(_ptr, ptr + inputOffset, inputCount, ptr2 + outputOffset, 960, decodeFEC ? 1 : 0);
			}
		}
		OpusConverter.CheckError(num);
		return num * 4;
	}

	protected override void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (_ptr != IntPtr.Zero)
			{
				DestroyDecoder(_ptr);
			}
			base.Dispose(disposing);
		}
	}
}
