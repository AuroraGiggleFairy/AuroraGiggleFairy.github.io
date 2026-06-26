namespace Platform;

public struct CensoredTextResult
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool Success { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string OriginalText { get; }

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string CensoredText { get; }

	public CensoredTextResult(bool _success, string _originalText, string _censoredText)
	{
		Success = _success;
		OriginalText = _originalText;
		CensoredText = _censoredText;
	}
}
