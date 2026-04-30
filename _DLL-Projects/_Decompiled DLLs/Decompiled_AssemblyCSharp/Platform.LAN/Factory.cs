using System;
using UnityEngine.Scripting;

namespace Platform.LAN;

[Preserve]
[PlatformFactory(EPlatformIdentifier.LAN)]
public class Factory : AbsPlatform
{
	public override void CreateInstances()
	{
		if (!base.AsServerOnly)
		{
			throw new NotSupportedException("This platform can only be used as a server platform.");
		}
		base.ServerListAnnouncer = new LANMasterServerAnnouncer();
	}
}
