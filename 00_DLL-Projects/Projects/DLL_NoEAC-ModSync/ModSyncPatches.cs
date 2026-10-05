using System;
using System.Collections.Generic;
using HarmonyLib;

namespace ModSync
{
	/// <summary>
	/// The server sends its package id mappings before login. Vanilla kicks the client the
	/// moment one name is unknown, which is what "requires the client to install mods" means.
	/// We take over that step: map everything we do know, note what we are missing, and ask
	/// the server for its mods instead of dropping the connection.
	/// </summary>
	/// <summary>
	/// The game sizes several per-connection arrays by the local KnownPackageCount and then
	/// indexes them with the package id. A modded server knows more package types than we do,
	/// so its ids run past the end of those arrays. Reserving spare slots before the connection
	/// is built keeps every one of them large enough.
	/// </summary>
	public static class ModSyncPad
	{
		private const string NamePrefix = "__ModSyncReserved";
		private const int ReservedSlots = 512;

		private static readonly List<string> added = new List<string>();

		public static void Ensure()
		{
			if (added.Count > 0)
			{
				return;
			}

			try
			{
				for (int i = 0; i < ReservedSlots; i++)
				{
					string name = NamePrefix + i;
					if (!NetPackageManager.knownPackageTypes.ContainsKey(name))
					{
						NetPackageManager.knownPackageTypes.Add(name, typeof(NetPackageModSyncReserved));
						added.Add(name);
					}
				}
				ModSyncCommon.Info("Reserved " + added.Count + " spare package slots for modded servers.");
			}
			catch (Exception ex)
			{
				ModSyncCommon.Warn("Could not reserve package slots: " + ex.Message);
			}
		}

		/// <summary>Spare slots are client-only. A server must never advertise them.</summary>
		public static void Remove()
		{
			for (int i = 0; i < added.Count; i++)
			{
				NetPackageManager.knownPackageTypes.Remove(added[i]);
			}
			added.Clear();
		}
	}

	[HarmonyPatch(typeof(ConnectionManager), "Connect", new Type[] { typeof(GameServerInfo) })]
	public static class Patch_ConnectionManager_Connect
	{
		public static void Prefix()
		{
			ModSyncPad.Ensure();
		}
	}

	[HarmonyPatch(typeof(NetPackageManager), "StartServer")]
	public static class Patch_NetPackageManager_StartServer
	{
		public static void Prefix()
		{
			ModSyncPad.Remove();
		}
	}

	[HarmonyPatch(typeof(NetPackagePackageIds), "ProcessPackage")]
	public static class Patch_NetPackagePackageIds_ProcessPackage
	{
		public static bool Prefix(NetPackagePackageIds __instance)
		{
			try
			{
				ConnectionManager cm = SingletonMonoBehaviour<ConnectionManager>.Instance;
				if (cm == null || cm.IsServer)
				{
					return true;
				}

				string[] mappings = __instance.mappings;
				if (mappings == null || mappings.Length == 0)
				{
					return true;
				}

				// A different game version needs a different download, not a mod sync.
				if (!__instance.compatVersion.EqualsMinor(Constants.cVersionInformation))
				{
					ModSyncCommon.Warn("Server runs " + __instance.compatVersion.LongStringNoBuild
						+ " but this game is " + Constants.cVersionInformation.LongStringNoBuild
						+ ". Mod sync cannot fix a version mismatch.");
					return true;
				}

				// EAC servers cannot load the DLL mods this is built for.
				if (__instance.serverUseEAC)
				{
					return true;
				}

				if (!ServerRunsModSync(mappings))
				{
					ModSyncCommon.Info("Server does not run ModSync. Joining normally.");
					return true;
				}

				List<string> unknown = InstallMappings(mappings);
				if (unknown.Count > 0)
				{
					ModSyncCommon.Info("Server uses " + unknown.Count
						+ " package type(s) this game does not have yet: " + string.Join(", ", unknown.ToArray()));
				}

				ModSyncClient.Begin();
				return false;
			}
			catch (Exception ex)
			{
				ModSyncCommon.Error("Handshake takeover failed, letting the game handle it: " + ex);
				return true;
			}
		}

		private static bool ServerRunsModSync(string[] mappings)
		{
			string ours = typeof(NetPackageModSyncHello).Name;
			for (int i = 0; i < mappings.Length; i++)
			{
				if (mappings[i] == ours)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Same as NetPackageManager.IdMappingsReceived, minus the kick. Arrays are sized to the
		/// server's list because a modded server knows more package types than we do, and the
		/// mapping is by index so every id we can resolve keeps its correct slot.
		/// </summary>
		private static List<string> InstallMappings(string[] mappings)
		{
			List<string> unknown = new List<string>();
			int size = Math.Max(mappings.Length, NetPackageManager.KnownPackageCount);

			NetPackageManager.packageIdToClass = new Type[size];
			NetPackageManager.packageIdToPackageInformation = new NetPackageManager.IPackageInformation[size];
			NetPackageManager.packageClassToPackageId = new Dictionary<Type, int>();
			NetPackageManager.AddPackageMapping(0, NetPackageManager.packageIdsType);

			for (int i = 0; i < mappings.Length; i++)
			{
				Type type;
				if (!NetPackageManager.knownPackageTypes.TryGetValue(mappings[i], out type))
				{
					unknown.Add(mappings[i]);
					continue;
				}

				if (type != NetPackageManager.packageIdsType)
				{
					NetPackageManager.AddPackageMapping(i, type);
				}
			}

			return unknown;
		}
	}
}
