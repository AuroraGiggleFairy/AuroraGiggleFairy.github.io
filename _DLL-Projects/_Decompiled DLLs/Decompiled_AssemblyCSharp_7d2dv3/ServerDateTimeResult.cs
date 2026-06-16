public struct ServerDateTimeResult
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool RequestComplete
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasError
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int SecondsOffset
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ServerDateTimeResult(bool _requestComplete, bool _hasError, int _secondsOffset)
	{
		RequestComplete = _requestComplete;
		HasError = _hasError;
		SecondsOffset = _secondsOffset;
	}
}
