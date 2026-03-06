using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdGameStage : ConsoleCmdAbstract
{
	public override bool IsExecuteOnClient => true;

	public override DeviceFlag AllowedDeviceTypes => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	public override DeviceFlag AllowedDeviceTypesClient => DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX | DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "gamestage" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Shows the gamestage of the local player";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (primaryPlayer != null)
		{
			string text = string.Empty;
			BiomeDefinition biomeStandingOn = primaryPlayer.biomeStandingOn;
			if (biomeStandingOn != null)
			{
				text = biomeStandingOn.m_sBiomeName;
			}
			Log.Out("Player gamestage {0}, lootstage {1}, level {2}, days alive {3}, difficulty {4}, diff bonus {5}, biome {6}", primaryPlayer.gameStage, primaryPlayer.GetLootStage(0f, 0f), primaryPlayer.Progression.Level, (long)(primaryPlayer.world.worldTime - primaryPlayer.gameStageBornAtWorldTime) / 24000L, GameStats.GetInt(EnumGameStats.GameDifficulty), GameStats.GetFloat(EnumGameStats.GameDifficultyBonus).ToCultureInvariantString(), text);
		}
	}
}
