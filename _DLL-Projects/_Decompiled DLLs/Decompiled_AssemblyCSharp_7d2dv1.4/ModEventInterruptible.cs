using System;

public class ModEventInterruptible : ModEventInterruptibleAbs<Func<bool>>
{
	public Mod Invoke()
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc())
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
public class ModEventInterruptible<T1> : ModEventInterruptibleAbs<Func<T1, bool>>
{
	public Mod Invoke(T1 _a1)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
public class ModEventInterruptible<T1, T2> : ModEventInterruptibleAbs<Func<T1, T2, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
public class ModEventInterruptible<T1, T2, T3> : ModEventInterruptibleAbs<Func<T1, T2, T3, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2, T3 _a3)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2, _a3))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
public class ModEventInterruptible<T1, T2, T3, T4, T5> : ModEventInterruptibleAbs<Func<T1, T2, T3, T4, T5, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2, T3 _a3, T4 _a4, T5 _a5)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2, _a3, _a4, _a5))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
public class ModEventInterruptible<T1, T2, T3, T4, T5, T6> : ModEventInterruptibleAbs<Func<T1, T2, T3, T4, T5, T6, bool>>
{
	public Mod Invoke(T1 _a1, T2 _a2, T3 _a3, T4 _a4, T5 _a5, T6 _a6)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				if (!receiver.DelegateFunc(_a1, _a2, _a3, _a4, _a5, _a6))
				{
					return receiver.Mod;
				}
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
		return null;
	}
}
