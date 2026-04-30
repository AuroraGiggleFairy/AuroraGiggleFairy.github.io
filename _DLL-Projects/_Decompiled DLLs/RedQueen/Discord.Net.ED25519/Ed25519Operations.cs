using System;
using Discord.Net.ED25519.Ed25519Ref10;

namespace Discord.Net.ED25519;

internal class Ed25519Operations
{
	public static bool crypto_sign_verify(byte[] sig, int sigoffset, byte[] m, int moffset, int mlen, byte[] pk, int pkoffset)
	{
		byte[] array = new byte[32];
		if ((sig[sigoffset + 63] & 0xE0) != 0)
		{
			return false;
		}
		if (GroupOperations.ge_frombytes_negate_vartime(out var h, pk, pkoffset) != 0)
		{
			return false;
		}
		Sha512 sha = new Sha512();
		sha.Update(sig, sigoffset, 32);
		sha.Update(pk, pkoffset, 32);
		sha.Update(m, moffset, mlen);
		byte[] array2 = sha.Finalize();
		ScalarOperations.sc_reduce(array2);
		byte[] array3 = new byte[32];
		Array.Copy(sig, sigoffset + 32, array3, 0, 32);
		GroupOperations.ge_double_scalarmult_vartime(out var r, array2, ref h, array3);
		GroupOperations.ge_tobytes(array, 0, ref r);
		bool result = CryptoBytes.ConstantTimeEquals(array, 0, sig, sigoffset, 32);
		CryptoBytes.Wipe(array2);
		CryptoBytes.Wipe(array);
		return result;
	}
}
