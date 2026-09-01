using System;
using System.Collections.Generic;

namespace SortingNearbyPlus
{
	public class ConsoleCmdSortingNearbyPlus : ConsoleCmdAbstract
	{
		public override string[] getCommands()
		{
			return new[] { "agf-snp" };
		}

		public override string getDescription()
		{
			return "Admin controls for Sorting Nearby Plus range and land-claim clamp.";
		}

		public override string getHelp()
		{
			return "Usage:\n"
				+ "  agf-snp\n"
				+ "  agf-snp help\n"
				+ "  agf-snp settings\n"
				+ "  agf-snp range <horizontal> <vertical>\n"
				+ "  agf-snp landclaim <on|off>";
		}

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			if (!IsSenderAdmin(_senderInfo))
			{
				SdtdConsole.Instance.Output("[SortingNearbyPlus] admin permission required.");
				return;
			}

			if (_params == null || _params.Count == 0 || string.Equals(_params[0], "help", StringComparison.OrdinalIgnoreCase))
			{
				SdtdConsole.Instance.Output(getHelp());
				SdtdConsole.Instance.Output(SettingsManager.AsString());
				return;
			}

			string sub = _params[0].Trim().ToLowerInvariant();
			switch (sub)
			{
				case "settings":
					SdtdConsole.Instance.Output(SettingsManager.AsString());
					return;
				case "range":
					if (_params.Count < 3 || !int.TryParse(_params[1], out int h) || !int.TryParse(_params[2], out int v))
					{
						SdtdConsole.Instance.Output("[SortingNearbyPlus] use: agf-snp range <horizontal> <vertical>");
						return;
					}

					int nh = SettingsManager.SetHorizontalRange(h);
					int nv = SettingsManager.SetVerticalRange(v);
					SdtdConsole.Instance.Output("[SortingNearbyPlus] range set to H=" + nh + " V=" + nv + ".");
					return;
				case "landclaim":
					if (_params.Count < 2)
					{
						SdtdConsole.Instance.Output("[SortingNearbyPlus] use: agf-snp landclaim <on|off>");
						return;
					}

					string token = _params[1].Trim().ToLowerInvariant();
					if (token != "on" && token != "off")
					{
						SdtdConsole.Instance.Output("[SortingNearbyPlus] use: agf-snp landclaim <on|off>");
						return;
					}

					bool on = token == "on";
					SettingsManager.SetLandClaimClamp(on);
					SdtdConsole.Instance.Output("[SortingNearbyPlus] landclaim=" + (on ? "on" : "off") + ".");
					return;
				default:
					SdtdConsole.Instance.Output("[SortingNearbyPlus] invalid option. Use: agf-snp help.");
					return;
			}
		}

		private static bool IsSenderAdmin(CommandSenderInfo senderInfo)
		{
			if (!TryGetSenderEntityId(senderInfo, out int entityId) || entityId < 0)
			{
				return true;
			}

			EntityPlayer player = GameManager.Instance?.World?.GetEntity(entityId) as EntityPlayer;
			return player != null && player.IsAdmin;
		}

		private static bool TryGetSenderEntityId(CommandSenderInfo senderInfo, out int entityId)
		{
			entityId = -1;
			if (senderInfo.RemoteClientInfo != null)
			{
				entityId = senderInfo.RemoteClientInfo.entityId;
				return true;
			}

			return false;
		}
	}
}
