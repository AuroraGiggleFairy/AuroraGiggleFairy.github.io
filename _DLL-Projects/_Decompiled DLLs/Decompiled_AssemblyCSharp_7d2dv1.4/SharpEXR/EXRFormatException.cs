using System;

namespace SharpEXR;

public class EXRFormatException : Exception
{
	public EXRFormatException()
	{
	}

	public EXRFormatException(string message)
		: base(message)
	{
	}

	public EXRFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
