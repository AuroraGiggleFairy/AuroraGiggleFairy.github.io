using System;
using System.Threading;

public static class ReaderWriterLockSlimExtensions
{
	public readonly struct ReadScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;

		public ReadScope(ReaderWriterLockSlim lockSlim)
		{
			m_lockSlim = lockSlim;
			m_lockSlim.EnterReadLock();
		}

		public void Dispose()
		{
			m_lockSlim.ExitReadLock();
		}
	}

	public readonly struct WriteScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;

		public WriteScope(ReaderWriterLockSlim lockSlim)
		{
			m_lockSlim = lockSlim;
			m_lockSlim.EnterWriteLock();
		}

		public void Dispose()
		{
			m_lockSlim.ExitWriteLock();
		}
	}

	public readonly struct UpgradeableReadScope : IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly ReaderWriterLockSlim m_lockSlim;

		public UpgradeableReadScope(ReaderWriterLockSlim lockSlim)
		{
			m_lockSlim = lockSlim;
			m_lockSlim.EnterUpgradeableReadLock();
		}

		public void Dispose()
		{
			m_lockSlim.ExitUpgradeableReadLock();
		}
	}

	public static ReadScope ReadLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new ReadScope(lockSlim);
	}

	public static WriteScope WriteLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new WriteScope(lockSlim);
	}

	public static UpgradeableReadScope UpgradableReadLockScope(this ReaderWriterLockSlim lockSlim)
	{
		return new UpgradeableReadScope(lockSlim);
	}
}
