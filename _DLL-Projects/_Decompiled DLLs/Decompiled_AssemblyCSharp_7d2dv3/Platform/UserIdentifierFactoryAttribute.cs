using System;

namespace Platform;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class UserIdentifierFactoryAttribute : Attribute
{
	public readonly EPlatformIdentifier TargetPlatform;

	public UserIdentifierFactoryAttribute(EPlatformIdentifier _targetPlatform)
	{
		TargetPlatform = _targetPlatform;
	}
}
