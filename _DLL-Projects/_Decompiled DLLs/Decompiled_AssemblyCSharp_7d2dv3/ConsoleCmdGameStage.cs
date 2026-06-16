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
			float num = 0f;
			float num2 = 0f;
			BiomeDefinition biomeStandingOn = primaryPlayer.biomeStandingOn;
			if (biomeStandingOn != null)
			{
				text = biomeStandingOn.m_sBiomeName;
			}
			if (primaryPlayer.QuestJournal.ActiveQuest != null)
			{
				num = primaryPlayer.QuestJournal.ActiveQuest.QuestClass.GameStageMod;
				num2 = primaryPlayer.QuestJournal.ActiveQuest.QuestClass.GameStageBonus;
			}
			Log.Out("\nPlayer Game Stage = (((Player Level * (1 + Biome Modifier + Quest Modifier) + Days Alive) + Biome Bonus + Quest Bonus) * Difficulty Bonus) * Global Game Stage Modifier\"\nPlayer Game Stage {0} = ((({2} * (1 + {3} + {4}) + {5}) + {6} + {7}) * {8}) * {9}\nPlayer Game Stage {0}\nPlayer Level {2}\nBiome Modifier {3}\nQuest Modifier {4}\nDays Alive {5}\nBiome Bonus {6}\nQuest Bonus {7}\nDifficulty Bonus {8}\nGlobal Game Stage Modifier {9}\nPlayer Loot Stage {1}\nBiome {10}", primaryPlayer.gameStage, primaryPlayer.GetLootStage(0f, 0f), primaryPlayer.Progression.Level, primaryPlayer.biomeStandingOn.GameStageMod, num, (long)(primaryPlayer.world.worldTime - primaryPlayer.gameStageBornAtWorldTime) / 24000L, primaryPlayer.biomeStandingOn.GameStageBonus, num2, GameStageDefinition.DifficultyBonus, EntityPlayer.GlobalGameStageModifier, text);
		}
	}
}
