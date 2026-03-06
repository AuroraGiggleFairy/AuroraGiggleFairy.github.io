using System;
using System.Runtime.InteropServices;

namespace Discord.Audio;

internal class OpusEncoder : OpusConverter
{
	public AudioApplication Application { get; }

	public int BitRate { get; }

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_create")]
	private static extern IntPtr CreateEncoder(int Fs, int channels, int application, out OpusError error);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_destroy")]
	private static extern void DestroyEncoder(IntPtr encoder);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encode")]
	private unsafe static extern int Encode(IntPtr st, byte* pcm, int frame_size, byte* data, int max_data_bytes);

	[DllImport("opus", CallingConvention = CallingConvention.Cdecl, EntryPoint = "opus_encoder_ctl")]
	private static extern OpusError EncoderCtl(IntPtr st, OpusCtl request, int value);

	public OpusEncoder(int bitrate, AudioApplication application, int packetLoss)
	{
		if (bitrate < 1 || bitrate > 131072)
		{
			throw new ArgumentOutOfRangeException("bitrate");
		}
		Application = application;
		BitRate = bitrate;
		OpusApplication application2;
		OpusSignal value;
		switch (application)
		{
		case AudioApplication.Mixed:
			application2 = OpusApplication.MusicOrMixed;
			value = OpusSignal.Auto;
			break;
		case AudioApplication.Music:
			application2 = OpusApplication.MusicOrMixed;
			value = OpusSignal.Music;
			break;
		case AudioApplication.Voice:
			application2 = OpusApplication.Voice;
			value = OpusSignal.Voice;
			break;
		default:
			throw new ArgumentOutOfRangeException("application");
		}
		_ptr = CreateEncoder(48000, 2, (int)application2, out var error);
		OpusConverter.CheckError(error);
		OpusConverter.CheckError(EncoderCtl(_ptr, OpusCtl.SetSignal, (int)value));
		OpusConverter.CheckError(EncoderCtl(_ptr, OpusCtl.SetPacketLossPercent, packetLoss));
		OpusConverter.CheckError(EncoderCtl(_ptr, OpusCtl.SetInbandFEC, 1));
		OpusConverter.CheckError(EncoderCtl(_ptr, OpusCtl.SetBitrate, bitrate));
	}

	public unsafe int EncodeFrame(byte[] input, int inputOffset, byte[] output, int outputOffset)
	{
		int result;
		fixed (byte* ptr = input)
		{
			fixed (byte* ptr2 = output)
			{
				result = Encode(_ptr, ptr + inputOffset, 960, ptr2 + outputOffset, output.Length - outputOffset);
			}
		}
		OpusConverter.CheckError(result);
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (_ptr != IntPtr.Zero)
			{
				DestroyEncoder(_ptr);
			}
			base.Dispose(disposing);
		}
	}
}
