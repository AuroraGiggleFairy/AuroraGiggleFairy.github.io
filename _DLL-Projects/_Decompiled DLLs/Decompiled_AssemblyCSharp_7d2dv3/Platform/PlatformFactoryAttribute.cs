using System;

namespace Platform;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class PlatformFactoryAttribute : Attribute
{
	public readonly EPlatformIdentifier TargetPlatform;

	public PlatformFactoryAttribute(EPlatformIdentifier _targetPlatform)
	{
		TargetPlatform = _targetPlatform;
	}
}
