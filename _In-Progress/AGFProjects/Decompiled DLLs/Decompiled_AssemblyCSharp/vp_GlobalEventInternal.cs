using System;
using System.Collections;

[PublicizedFrom(EAccessModifier.Internal)]
public static class vp_GlobalEventInternal
{
	public class UnregisterException : Exception
	{
		public UnregisterException(string msg)
			: base(msg)
		{
		}
	}

	public class SendException : Exception
	{
		public SendException(string msg)
			: base(msg)
		{
		}
	}

	public static Hashtable Callbacks = new Hashtable();

	public static UnregisterException ShowUnregisterException(string name)
	{
		return new UnregisterException($"Attempting to Unregister the event {name} but vp_GlobalEvent has not registered this event.");
	}

	public static SendException ShowSendException(string name)
	{
		return new SendException($"Attempting to Send the event {name} but vp_GlobalEvent has not registered this event.");
	}
}
