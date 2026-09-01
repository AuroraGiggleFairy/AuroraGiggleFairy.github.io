using System;
using System.Collections.Generic;

namespace SortingNearbyPlus
{
	internal static class ChatCmdSortingNearbyPlus
	{
		public static bool IsSortCommand(string message)
		{
			return TryParse(message, out _);
		}

		public static ModEvents.EModEventResult OnChatMessage(ref ModEvents.SChatMessageData data)
		{
			if (!TryParse(data.Message, out string mode))
			{
				return ModEvents.EModEventResult.Continue;
			}

			if (!StorageUtil.IsServer())
			{
				return ModEvents.EModEventResult.Continue;
			}

			if (data.SenderEntityId < 0)
			{
				return ModEvents.EModEventResult.StopHandlersAndVanilla;
			}

			EntityPlayer player = GameManager.Instance?.World?.GetEntity(data.SenderEntityId) as EntityPlayer;
			if (player == null)
			{
				return ModEvents.EModEventResult.StopHandlersAndVanilla;
			}

			if (!StorageUtil.CanRunSortCommands(player))
			{
				Whisper(data.SenderEntityId, "sort commands need EAC off (single-player or listen-server host).");
				return ModEvents.EModEventResult.StopHandlersAndVanilla;
			}

			int moved;
			string label;
			if (mode == "vehicle-dump")
			{
				moved = SortOperations.SortBagToVehicles(player, false);
				label = "vehicle dump";
			}
			else if (mode == "vehicle")
			{
				moved = SortOperations.SortBagToVehicles(player, true);
				label = "vehicle";
			}
			else
			{
				moved = SortOperations.SortPlayerToChests(player);
				label = "sort";
			}

			Whisper(data.SenderEntityId, moved > 0
				? label + ": moved " + moved + " item(s)."
				: label + ": nothing to move.");
			return ModEvents.EModEventResult.StopHandlersAndVanilla;
		}

		private static bool TryParse(string raw, out string mode)
		{
			mode = null;
			if (string.IsNullOrWhiteSpace(raw))
			{
				return false;
			}

			string text = raw.Trim();
			if (!text.StartsWith("/", StringComparison.Ordinal))
			{
				return false;
			}

			string[] parts = text.Substring(1).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0 || !parts[0].Equals("sort", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (parts.Length == 1)
			{
				mode = "chests";
				return true;
			}

			if (parts[1].Equals("vehicle", StringComparison.OrdinalIgnoreCase))
			{
				mode = parts.Length >= 3 && parts[2].Equals("dump", StringComparison.OrdinalIgnoreCase)
					? "vehicle-dump"
					: "vehicle";
				return true;
			}

			return false;
		}

		private static void Whisper(int entityId, string message)
		{
			GameManager.Instance?.ChatMessageServer(
				null,
				EChatType.Whisper,
				-1,
				"[SortingNearbyPlus] " + message,
				new List<int> { entityId },
				EMessageSender.Server);
		}
	}
}
