using System;

public class ModEvent : ModEventAbs<Action>
{
	public void Invoke()
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				receiver.DelegateFunc();
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
	}
}
public class ModEvent<T1> : ModEventAbs<Action<T1>>
{
	public void Invoke(T1 _a1)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				receiver.DelegateFunc(_a1);
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
	}
}
public class ModEvent<T1, T2> : ModEventAbs<Action<T1, T2>>
{
	public void Invoke(T1 _a1, T2 _a2)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				receiver.DelegateFunc(_a1, _a2);
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
	}
}
public class ModEvent<T1, T2, T3> : ModEventAbs<Action<T1, T2, T3>>
{
	public void Invoke(T1 _a1, T2 _a2, T3 _a3)
	{
		for (int i = 0; i < receivers.Count; i++)
		{
			Receiver receiver = receivers[i];
			try
			{
				receiver.DelegateFunc(_a1, _a2, _a3);
			}
			catch (Exception e)
			{
				LogError(e, receiver);
			}
		}
	}
}
