public class CoroutineCancellationToken
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancelled;

	public void Cancel()
	{
		cancelled = true;
	}

	public bool IsCancelled()
	{
		return cancelled;
	}
}
