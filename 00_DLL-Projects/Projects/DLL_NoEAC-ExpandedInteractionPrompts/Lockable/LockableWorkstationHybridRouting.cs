using System;
using System.Collections.Generic;

namespace LockableWorkstations
{
	internal static class LockableWorkstationHybridRouting
	{
		private static readonly HashSet<string> ModClientUsers = new HashSet<string>(StringComparer.Ordinal);
		private static readonly Dictionary<int, string> ModClientByEntityId = new Dictionary<int, string>();

		public static void MarkClientCapability(int entityId, string userCombined)
		{
			if (string.IsNullOrEmpty(userCombined))
			{
				return;
			}

			ModClientUsers.Add(userCombined);
			if (entityId >= 0)
			{
				ModClientByEntityId[entityId] = userCombined;
			}
		}

		public static void ForgetClientByEntityId(int entityId)
		{
			if (entityId >= 0)
			{
				ModClientByEntityId.Remove(entityId);
			}
		}

		public static bool HasClientCapability(ClientInfo clientInfo)
		{
			if (clientInfo == null)
			{
				return false;
			}

			if (clientInfo.entityId >= 0 && ModClientByEntityId.TryGetValue(clientInfo.entityId, out string byEntity) && !string.IsNullOrEmpty(byEntity))
			{
				return true;
			}

			string userCombined = clientInfo.InternalId?.CombinedString;
			return !string.IsNullOrEmpty(userCombined) && ModClientUsers.Contains(userCombined);
		}

		public static bool HasClientCapabilityEntityId(int entityId)
		{
			if (entityId < 0)
			{
				return false;
			}

			ConnectionManager manager = SingletonMonoBehaviour<ConnectionManager>.Instance;
			ClientInfo clientInfo = manager?.Clients?.ForEntityId(entityId);
			return HasClientCapability(clientInfo);
		}
	}
}
