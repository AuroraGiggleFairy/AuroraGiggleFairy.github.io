using System;

public class DebugWrapperException : Exception
{
	public DebugWrapperException()
	{
	}

	public DebugWrapperException(string message)
		: base(message)
	{
	}

	public DebugWrapperException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
