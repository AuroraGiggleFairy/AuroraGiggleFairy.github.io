using System;
using System.Runtime.InteropServices;

namespace Discord.Audio;

internal static class SecretBox
{
	[DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_easy")]
	private unsafe static extern int SecretBoxEasy(byte* output, byte* input, long inputLength, byte[] nonce, byte[] secret);

	[DllImport("libsodium", CallingConvention = CallingConvention.Cdecl, EntryPoint = "crypto_secretbox_open_easy")]
	private unsafe static extern int SecretBoxOpenEasy(byte* output, byte* input, long inputLength, byte[] nonce, byte[] secret);

	public unsafe static int Encrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
	{
		fixed (byte* ptr = input)
		{
			fixed (byte* ptr2 = output)
			{
				int num = SecretBoxEasy(ptr2 + outputOffset, ptr + inputOffset, inputLength, nonce, secret);
				if (num != 0)
				{
					throw new Exception($"Sodium Error: {num}");
				}
				return inputLength + 16;
			}
		}
	}

	public unsafe static int Decrypt(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
	{
		fixed (byte* ptr = input)
		{
			fixed (byte* ptr2 = output)
			{
				int num = SecretBoxOpenEasy(ptr2 + outputOffset, ptr + inputOffset, inputLength, nonce, secret);
				if (num != 0)
				{
					throw new Exception($"Sodium Error: {num}");
				}
				return inputLength - 16;
			}
		}
	}
}
