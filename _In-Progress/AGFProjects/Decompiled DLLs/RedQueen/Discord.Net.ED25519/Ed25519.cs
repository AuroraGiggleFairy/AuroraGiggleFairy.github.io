using System;

namespace Discord.Net.ED25519;

internal static class Ed25519
{
	public const int PublicKeySize = 32;

	public const int SignatureSize = 64;

	public const int PrivateKeySeedSize = 32;

	public const int ExpandedPrivateKeySize = 64;

	public static bool Verify(ArraySegment<byte> signature, ArraySegment<byte> message, ArraySegment<byte> publicKey)
	{
		if (signature.Count != 64)
		{
			throw new ArgumentException($"Sizeof signature doesnt match defined size of {64}");
		}
		if (publicKey.Count != 32)
		{
			throw new ArgumentException($"Sizeof public key doesnt match defined size of {32}");
		}
		return Ed25519Operations.crypto_sign_verify(signature.Array, signature.Offset, message.Array, message.Offset, message.Count, publicKey.Array, publicKey.Offset);
	}

	public static bool Verify(byte[] signature, byte[] message, byte[] publicKey)
	{
		Preconditions.NotNull(signature, "signature");
		Preconditions.NotNull(message, "message");
		Preconditions.NotNull(publicKey, "publicKey");
		if (signature.Length != 64)
		{
			throw new ArgumentException($"Sizeof signature doesnt match defined size of {64}");
		}
		if (publicKey.Length != 32)
		{
			throw new ArgumentException($"Sizeof public key doesnt match defined size of {32}");
		}
		return Ed25519Operations.crypto_sign_verify(signature, 0, message, 0, message.Length, publicKey, 0);
	}
}
