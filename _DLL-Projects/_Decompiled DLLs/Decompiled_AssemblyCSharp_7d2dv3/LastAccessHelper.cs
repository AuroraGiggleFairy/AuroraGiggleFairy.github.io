using System;

public class LastAccessHelper
{
	public struct Scope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public LastAccessHelper m_parent;

		public Scope(LastAccessHelper parent)
		{
			m_parent = parent;
			m_parent.Increment();
		}

		public void Dispose()
		{
			m_parent?.Decrement();
			m_parent = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime m_time = DateTime.Now;

	public DateTime Time
	{
		get
		{
			lock (m_lock)
			{
				return (m_numActive > 0) ? DateTime.Now : m_time;
			}
		}
	}

	public Scope CreateScope()
	{
		return new Scope(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Increment()
	{
		lock (m_lock)
		{
			m_numActive++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Decrement()
	{
		lock (m_lock)
		{
			m_numActive--;
			if (m_numActive == 0)
			{
				m_time = DateTime.Now;
			}
		}
	}
}
