using System;
using System.Collections.Generic;

namespace PartyGroupPlus
{
	public class ConsoleCmdPartyGroupPlus : ConsoleCmdAbstract
	{
		public override string[] getCommands()
		{
			return new[] { "agf-pgp" };
		}

		public override string getDescription()
		{
			return "Admin controls for Party Group Plus.";
		}

		public override string getHelp()
		{
			return "Usage:\n"
				+ "  agf-pgp\n"
				+ "  agf-pgp help\n"
				+ "  agf-pgp settings\n"
				+ "  agf-pgp forceparty <off|join|enforce>\n"
				+ "    off      no auto-join\n"
				+ "    join     new players join the server party, can leave later\n"
				+ "    enforce  everyone stays in the server party";
		}

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			if (!IsSenderAdmin(_senderInfo))
			{
				SdtdConsole.Instance.Output("[PartyGroupPlus] admin permission required.");
				return;
			}

			if (_params == null || _params.Count == 0 || string.Equals(_params[0], "help", StringComparison.OrdinalIgnoreCase))
			{
				SdtdConsole.Instance.Output(getHelp());
				SdtdConsole.Instance.Output(PartyGroupPlusSettings.AsString());
				return;
			}

			string sub = _params[0].Trim().ToLowerInvariant();
			switch (sub)
			{
				case "settings":
					SdtdConsole.Instance.Output(PartyGroupPlusSettings.AsString());
					return;
				case "forceparty":
					if (_params.Count < 2 || !PartyGroupPlusSettings.TryParseMode(_params[1], out ForcePartyMode mode))
					{
						SdtdConsole.Instance.Output("[PartyGroupPlus] use: agf-pgp forceparty <off|join|enforce>");
						return;
					}

					PartyGroupPlusSettings.SetMode(mode);
					SdtdConsole.Instance.Output("[PartyGroupPlus] forceparty=" + PartyGroupPlusSettings.ModeToken(mode) + ".");
					SdtdConsole.Instance.Output("[PartyGroupPlus] serverparty=" + PartyGroupPlusForceParty.DescribeServerParty());
					return;
				default:
					SdtdConsole.Instance.Output("[PartyGroupPlus] invalid option. Use: agf-pgp help.");
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
