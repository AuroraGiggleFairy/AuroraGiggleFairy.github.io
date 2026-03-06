using System;
using UnityEngine.Scripting;

namespace Platform.Stubs;

[Preserve]
[PlatformFactory(EPlatformIdentifier.PSN)]
public class FactoryStubPSN : AbsPlatform
{
	public override void CreateInstances()
	{
		if (!base.AsServerOnly)
		{
			throw new NotSupportedException("This platform can only be used as a server platform.");
		}
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		if (crossplatformPlatform == null || crossplatformPlatform.PlatformIdentifier != EPlatformIdentifier.EOS)
		{
			throw new NotSupportedException("This server platform requires EOS as the cross-platform.");
		}
	}
}
