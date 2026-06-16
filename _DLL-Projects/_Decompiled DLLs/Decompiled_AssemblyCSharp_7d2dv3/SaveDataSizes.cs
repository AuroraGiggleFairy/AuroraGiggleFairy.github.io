public readonly struct SaveDataSizes(long total, long remaining)
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly long m_total = total;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly long m_remaining = remaining;

	public long Total => m_total;

	public long Used => m_total - m_remaining;

	public long Remaining => m_remaining;
}
