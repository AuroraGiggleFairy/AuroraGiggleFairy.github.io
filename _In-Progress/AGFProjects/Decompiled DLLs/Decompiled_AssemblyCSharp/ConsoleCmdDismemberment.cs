using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdDismemberment : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "testDismemberment", "tds" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Dismemberment testing toggle.";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count > 0 && _params[0].ContainsCaseInsensitive("debuglog"))
		{
			DismembermentManager.DebugLogEnabled = !DismembermentManager.DebugLogEnabled;
			Log.Out("Dismemberment debug log enabled: " + DismembermentManager.DebugLogEnabled);
		}
		if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
		{
			return;
		}
		if (_params.Count == 0)
		{
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			primaryPlayer.DebugDismembermentChance = !primaryPlayer.DebugDismembermentChance;
			Log.Out("Dismemberment testing enabled: " + primaryPlayer.DebugDismembermentChance);
			return;
		}
		for (int i = 0; i < _params.Count; i++)
		{
			if (_params[i].ContainsCaseInsensitive("bodypart"))
			{
				if (_params.Count > i)
				{
					EnumBodyPartHit result = EnumBodyPartHit.None;
					if (Enum.TryParse<EnumBodyPartHit>(_params[i + 1], ignoreCase: true, out result))
					{
						DismembermentManager.DebugBodyPartHit = result;
						Log.Out("Dismemberment test bodypart(s): " + result);
					}
					else
					{
						Log.Out("Dismemberment bodypart unknown: " + _params[i + 1]);
					}
				}
				else
				{
					Log.Out("Dismemberment bodypart(s) invalid number of params: " + _params.Count);
				}
				break;
			}
			if (_params[i].ContainsCaseInsensitive("arms"))
			{
				DismembermentManager.DebugShowArmRotations = !DismembermentManager.DebugShowArmRotations;
				Log.Out("Dismemberment debug arm rotations: " + DismembermentManager.DebugShowArmRotations);
			}
			if (_params[i].ContainsCaseInsensitive("explosions"))
			{
				DismembermentManager.DebugDismemberExplosions = !DismembermentManager.DebugDismemberExplosions;
				Log.Out("Dismemberment debug explosions: " + DismembermentManager.DebugDismemberExplosions);
			}
			if (_params[i].ContainsCaseInsensitive("matrix"))
			{
				DismembermentManager.DebugBulletTime = !DismembermentManager.DebugBulletTime;
				Log.Out("Dismemberment debug bullet time: " + DismembermentManager.DebugBulletTime);
			}
			if (_params[i].ContainsCaseInsensitive("blood"))
			{
				DismembermentManager.DebugBulletTime = !DismembermentManager.DebugBloodParticles;
				Log.Out("Dismemberment debug blood particles: " + DismembermentManager.DebugBloodParticles);
			}
			if (_params[i].ContainsCaseInsensitive("noparts"))
			{
				DismembermentManager.DebugDontCreateParts = !DismembermentManager.DebugDontCreateParts;
				Log.Out("Dismemberment debug dont create parts: " + DismembermentManager.DebugDontCreateParts);
			}
			if (_params[i].ContainsCaseInsensitive("legacy"))
			{
				DismembermentManager.DebugUseLegacy = !DismembermentManager.DebugUseLegacy;
				Log.Out("Dismemberment debug use legacy parts: " + DismembermentManager.DebugUseLegacy);
			}
			if (_params[i].ContainsCaseInsensitive("explosive"))
			{
				DismembermentManager.DebugExplosiveCleanup = !DismembermentManager.DebugExplosiveCleanup;
				Log.Out("Dismemberment debug use explosive cleanup: " + DismembermentManager.DebugExplosiveCleanup);
			}
		}
	}
}
