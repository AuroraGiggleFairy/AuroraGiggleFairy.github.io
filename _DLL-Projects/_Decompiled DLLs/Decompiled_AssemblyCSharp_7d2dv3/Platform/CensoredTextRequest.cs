using System;

namespace Platform;

public struct CensoredTextRequest
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Input { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int CensoredLength { get; set; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public Action<CensoredTextResult> Callback { get; }

	public CensoredTextRequest(string _input, Action<CensoredTextResult> _callback)
	{
		Input = _input;
		CensoredLength = _input.Length;
		Callback = _callback;
	}
}
