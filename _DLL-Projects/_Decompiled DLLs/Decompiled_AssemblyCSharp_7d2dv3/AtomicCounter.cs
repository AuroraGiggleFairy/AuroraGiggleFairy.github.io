using System.Threading;

public sealed class AtomicCounter
{
	[PublicizedFrom(EAccessModifier.Private)]
	public int m_counter;

	public int Value => m_counter;

	public int Increment()
	{
		return Interlocked.Increment(ref m_counter);
	}

	public int Decrement()
	{
		return Interlocked.Decrement(ref m_counter);
	}
}
