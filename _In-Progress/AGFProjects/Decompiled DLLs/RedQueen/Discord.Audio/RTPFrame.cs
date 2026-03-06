namespace Discord.Audio;

internal struct RTPFrame(ushort sequence, uint timestamp, byte[] payload, bool missed)
{
	public readonly ushort Sequence = sequence;

	public readonly uint Timestamp = timestamp;

	public readonly byte[] Payload = payload;

	public readonly bool Missed = missed;
}
