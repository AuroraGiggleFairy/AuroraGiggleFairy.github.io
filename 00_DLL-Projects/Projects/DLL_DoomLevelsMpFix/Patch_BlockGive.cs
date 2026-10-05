using System;
using System.Collections.Generic;
using HarmonyLib;

namespace DoomLevelsMpFix
{
	[HarmonyPatch(typeof(SdtdConsole), "executeCommand")]
	internal static class Patch_BlockGive
	{
		private static readonly HashSet<string> Blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"gunHandgunT0PipePistol",
			"gunHandgunT1Pistol",
			"gunHandgunT3SMG5",
			"gunHandgunT2Magnum44",
			"gunHandgunT3DesertVulture",
			"gunShotgunT0PipeShotgun",
			"gunShotgunT1DoubleBarrel",
			"gunShotgunT2PumpShotgun",
			"gunShotgunT3AutoShotgun",
			"gunRifleT0PipeRifle",
			"gunRifleT1HuntingRifle",
			"gunRifleT2LeverActionRifle",
			"gunRifleT3SniperRifle",
			"gunMGT0PipeMachineGun",
			"gunMGT1AK47",
			"gunMGT2TacticalAR",
			"gunMGT3M60",
			"gunExplosivesT3RocketLauncher",
			"gunBowT1WoodenBow",
			"gunBowT3CompoundBow",
			"gunBowT1IronCrossbow",
			"gunBowT3CompoundCrossbow",
			"gunBotT1JunkSledge",
			"gunBotT2JunkTurret",
			"gunBotT3JunkDrone",
			"meleeWpnBladeT0BoneKnife",
			"meleeWpnBladeT1HuntingKnife",
			"meleeWpnBladeT1CandyKnife",
			"meleeWpnBladeT3Machete",
			"meleeWpnClubT0WoodenClub",
			"meleeWpnClubT1BaseballBat",
			"meleeWpnClubT1CandyClub",
			"meleeWpnClubT3SteelClub",
			"meleeWpnBatonT0PipeBaton",
			"meleeWpnBatonT2StunBaton",
			"meleeWpnBatonT3PlasmaBaton",
			"meleeWpnSpearT0StoneSpear",
			"meleeWpnSpearT1IronSpear",
			"meleeWpnSpearT3SteelSpear",
			"meleeWpnSledgeT0StoneSledgehammer",
			"meleeWpnSledgeT1IronSledgehammer",
			"meleeWpnSledgeT3SteelSledgehammer",
			"meleeWpnKnucklesT0LeatherKnuckles",
			"meleeWpnKnucklesT1IronKnuckles",
			"meleeWpnKnucklesT3SteelKnuckles",
			"meleeToolRepairT0TazasStoneAxe",
			"meleeToolRepairT1ClawHammer",
			"meleeToolRepairT3Nailgun",
			"meleeToolAxeT1IronFireaxe",
			"meleeToolAxeT2SteelAxe",
			"meleeToolPickT1IronPickaxe",
			"meleeToolPickT2SteelPickaxe",
			"meleeToolShovelT1IronShovel",
			"meleeToolShovelT2SteelShovel",
			"meleeToolAxeT3Chainsaw",
			"meleeToolPickT3Auger",
			"meleeToolSalvageT1Wrench",
			"meleeToolSalvageT2Ratchet",
			"meleeToolSalvageT3ImpactDriver",
			"modArmorHelmetLight",
			"modArmorNightVision",
			"modArmorHelmetLightSchematic",
			"ammo9mmBulletBall",
			"ammo9mmBulletHP",
			"ammo9mmBulletAP",
			"ammo44MagnumBulletBall",
			"ammo44MagnumBulletHP",
			"ammo44MagnumBulletAP",
			"ammo762mmBulletBall",
			"ammo762mmBulletHP",
			"ammo762mmBulletAP",
			"ammoShotgunShell",
			"ammoShotgunSlug",
			"ammoShotgunBreachingSlug",
			"ammoJunkTurretRegular",
			"ammoJunkTurretShell",
			"ammoJunkTurretAP",
			"ammoRocketHE",
			"ammoRocketFrag",
			"ammoArrowStone",
			"ammoArrowIron",
			"ammoArrowSteelAP",
			"ammoArrowFlaming",
			"ammoArrowExploding",
			"ammoCrossbowBoltStone",
			"ammoCrossbowBoltIron",
			"ammoCrossbowBoltSteelAP",
			"ammoCrossbowBoltFlaming",
			"ammoCrossbowBoltExploding",
			"ammoBundle9mmBulletBall",
			"ammoBundle9mmBulletHP",
			"ammoBundle9mmBulletAP",
			"ammoBundle44MagnumBulletBall",
			"ammoBundle44MagnumBulletHP",
			"ammoBundle44MagnumBulletAP",
			"ammoBundle762mmBulletBall",
			"ammoBundle762mmBulletHP",
			"ammoBundle762mmBulletAP",
			"ammoBundleShotgunShell",
			"ammoBundleShotgunSlug",
			"ammoBundleShotgunBreachingSlug",
			"ammoBundleJunkTurretRegular",
			"ammoBundleJunkTurretShell",
			"ammoBundleJunkTurretAP",
			"ammoBundleArrowStone",
			"ammoBundleArrowIron",
			"ammoBundleArrowSteelAP",
			"ammoBundleArrowFlaming",
			"ammoBundleArrowExploding",
			"ammoBundleCrossbowBoltStone",
			"ammoBundleCrossbowBoltIron",
			"ammoBundleCrossbowBoltSteelAP",
			"ammoBundleCrossbowBoltFlaming",
			"ammoBundleCrossbowBoltExploding"
		};

		private static bool Prefix(string _command, ref List<string> __result)
		{
			if (string.IsNullOrEmpty(_command))
			{
				return true;
			}

			string blocked = FindBlocked(_command);
			if (blocked == null)
			{
				return true;
			}

			__result = new List<string>
			{
				"That item is disabled in this pack: " + blocked
			};
			return false;
		}

		private static string FindBlocked(string command)
		{
			string first = null;
			int start = 0;
			while (start < command.Length)
			{
				char c = command[start];
				if (c == ' ' || c == '\t')
				{
					start++;
					continue;
				}

				string token = NextToken(command, ref start);
				if (token.Length == 0)
				{
					continue;
				}

				if (first == null)
				{
					first = token;
					if (!first.Equals("giveself", StringComparison.OrdinalIgnoreCase)
						&& !first.Equals("give", StringComparison.OrdinalIgnoreCase))
					{
						return null;
					}

					continue;
				}

				if (Blocked.Contains(token))
				{
					return token;
				}
			}

			return null;
		}

		private static string NextToken(string command, ref int start)
		{
			if (start >= command.Length)
			{
				return string.Empty;
			}

			if (command[start] == '"')
			{
				start++;
				int from = start;
				while (start < command.Length && command[start] != '"')
				{
					start++;
				}

				string quoted = command.Substring(from, start - from);
				if (start < command.Length)
				{
					start++;
				}

				return quoted;
			}

			int fromUnquoted = start;
			while (start < command.Length && command[start] != ' ' && command[start] != '\t')
			{
				start++;
			}

			return command.Substring(fromUnquoted, start - fromUnquoted);
		}
	}
}
