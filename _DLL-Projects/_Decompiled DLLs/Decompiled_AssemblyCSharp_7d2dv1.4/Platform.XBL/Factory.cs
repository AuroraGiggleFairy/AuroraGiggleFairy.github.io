using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Platform.XBL;

[Preserve]
[PlatformFactory(EPlatformIdentifier.XBL)]
public class Factory : AbsPlatform
{
	public override void CreateInstances()
	{
		if (!base.AsServerOnly)
		{
			IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
			if (crossplatformPlatform == null || crossplatformPlatform.PlatformIdentifier != EPlatformIdentifier.EOS)
			{
				Application.Quit(1);
				throw new Exception("[XBL] This platform requires EOS as cross platform provider");
			}
		}
		base.Api = new Api();
		base.Utils = new Utils();
		if (!base.AsServerOnly)
		{
			XblXuidMapper.Enable();
			base.User = new User();
			base.AchievementManager = new AchievementManager();
			base.Input = new PlayerInputManager();
			base.JoinSessionGameInviteListener = new JoinSessionGameInviteListener();
		}
	}
}
