namespace Platform;

public abstract class AbsUserIdentifierFactory
{
	public abstract PlatformUserIdentifierAbs FromId(string _userId);

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsUserIdentifierFactory()
	{
	}
}
