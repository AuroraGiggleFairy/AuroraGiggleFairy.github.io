using System.Collections.Generic;
using Platform.LAN;
using Platform.Shared;
using UnityEngine.Scripting;

namespace Platform.Local;

[Preserve]
[PlatformFactory(EPlatformIdentifier.Local)]
public class Factory : AbsPlatform
{
	public override void CreateInstances()
	{
		base.Api = new Api();
		if (!base.AsServerOnly)
		{
			base.User = new User();
			LocalServerDetect localServerDetect = new LocalServerDetect();
			base.ServerListInterfaces = new List<IServerListInterface>
			{
				localServerDetect,
				new FavoriteServers(),
				new LANServerList()
			};
			base.ServerLookupInterface = localServerDetect;
			base.Utils = new Platform.Shared.Utils();
			base.Input = new PlayerInputManager();
			base.RemoteFileStorage = new RemoteFileStorage();
			base.EntitlementValidators = new List<IEntitlementValidator>
			{
				new DownloadableContentValidator()
			};
		}
	}
}
