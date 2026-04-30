using System;
using System.Collections.Generic;
using System.Reflection;
using Platform;
using UnityEngine;

namespace LockableWorkstations
{
	public class ConsoleCmdLockableWorkstations : ConsoleCmdAbstract
	{
		private const float FocusDistanceMeters = 5f;

		public override string[] getCommands()
		{
			return new[] { "lw", "lockws", "lockableworkstations" };
		}

		public override string getDescription()
		{
			return "Manage lockable workstation lock state from chat/console.";
		}

		public override string getHelp()
		{
			return "Usage:\n"
				+ "  lw help\n"
				+ "  lw status\n"
				+ "  lw lock\n"
				+ "  lw unlock\n"
				+ "  lw code set <code>\n"
				+ "  lw code clear\n"
				+ "  lw code use <code>\n"
				+ "Notes:\n"
				+ "  - commands operate on the currently focused block within 5 meters.\n"
				+ "  - admins can manage any lockable workstation.\n"
				+ "  - non-admin players can manage only their own/ACL workstations.\n"
				+ "  - console/telnet cannot use focused-block commands.";
		}

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			World world = GameManager.Instance?.World;
			if (world == null)
			{
				SdtdConsole.Instance.Output("[LW] World is not ready.");
				return;
			}

			if (!TryGetSenderContext(_senderInfo, out int senderEntityId, out PlatformUserIdentifierAbs senderUserId, out _))
			{
				SdtdConsole.Instance.Output("[LW] Unable to resolve command sender context.");
				return;
			}

			bool isAdmin = LockableWorkstationHelpers.IsAdminEntityId(senderEntityId);
			string subcommand = (_params != null && _params.Count > 0 ? _params[0] : "help")?.Trim().ToLowerInvariant() ?? "help";
			switch (subcommand)
			{
				case "help":
					SdtdConsole.Instance.Output(getHelp());
					return;
				case "status":
					HandleStatusCommand(world, senderEntityId);
					return;
				case "lock":
					HandleLockToggle(world, senderEntityId, senderUserId, isAdmin, lockNow: true);
					return;
				case "unlock":
					HandleLockToggle(world, senderEntityId, senderUserId, isAdmin, lockNow: false);
					return;
				case "code":
					HandleCodeCommand(world, _params, senderEntityId, senderUserId, isAdmin);
					return;
				default:
					SdtdConsole.Instance.Output("[LW] Unknown subcommand. Use: lw help");
					return;
			}
		}

		private static void HandleStatusCommand(World world, int senderEntityId)
		{
			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				SdtdConsole.Instance.Output("[LW] " + error);
				return;
			}

			string owner = adapter.GetOwner()?.CombinedString ?? "(none)";
			SdtdConsole.Instance.Output("[LW] Target " + FormatPos(targetPos)
				+ " | locked=" + adapter.IsLocked()
				+ " | owner=" + owner
				+ " | hasCode=" + adapter.HasPassword()
				+ " | allowedUsers=" + adapter.GetUsers().Count + ".");
		}

		private static void HandleLockToggle(World world, int senderEntityId, PlatformUserIdentifierAbs senderUserId, bool isAdmin, bool lockNow)
		{
			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				SdtdConsole.Instance.Output("[LW] " + error);
				return;
			}

			if (!CanManage(world, adapter, senderUserId, isAdmin))
			{
				SdtdConsole.Instance.Output("[LW] You are not the owner/admin for this workstation.");
				return;
			}

			if (adapter.GetOwner() == null && senderUserId != null)
			{
				adapter.SetOwner(senderUserId);
			}

			adapter.SetLocked(lockNow);
			SdtdConsole.Instance.Output("[LW] " + (lockNow ? "Locked" : "Unlocked") + " workstation at " + FormatPos(targetPos) + ".");
		}

		private static void HandleCodeCommand(World world, List<string> args, int senderEntityId, PlatformUserIdentifierAbs senderUserId, bool isAdmin)
		{
			if (args == null || args.Count < 2)
			{
				SdtdConsole.Instance.Output("[LW] Usage: lw code <set|clear|use> [code]");
				return;
			}

			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				SdtdConsole.Instance.Output("[LW] " + error);
				return;
			}

			string action = args[1]?.Trim().ToLowerInvariant() ?? string.Empty;
			switch (action)
			{
				case "set":
					if (args.Count < 3)
					{
						SdtdConsole.Instance.Output("[LW] Usage: lw code set <code>");
						return;
					}

					if (!CanManage(world, adapter, senderUserId, isAdmin))
					{
						SdtdConsole.Instance.Output("[LW] You are not the owner/admin for this workstation.");
						return;
					}

					if (adapter.GetOwner() == null && senderUserId != null)
					{
						adapter.SetOwner(senderUserId);
					}

					adapter.ApplyServerPasswordHash(Utils.HashString(args[2] ?? string.Empty));
					if (!adapter.IsLocked())
					{
						adapter.SetLocked(_isLocked: true);
					}

					SdtdConsole.Instance.Output("[LW] Keypad code set for " + FormatPos(targetPos) + ".");
					return;
				case "clear":
					if (!CanManage(world, adapter, senderUserId, isAdmin))
					{
						SdtdConsole.Instance.Output("[LW] You are not the owner/admin for this workstation.");
						return;
					}

					adapter.ApplyServerPasswordHash(string.Empty);
					SdtdConsole.Instance.Output("[LW] Keypad code cleared for " + FormatPos(targetPos) + ".");
					return;
				case "use":
					if (senderUserId == null)
					{
						SdtdConsole.Instance.Output("[LW] A player identity is required to use code access.");
						return;
					}

					if (args.Count < 3)
					{
						SdtdConsole.Instance.Output("[LW] Usage: lw code use <code>");
						return;
					}

					string hashed = Utils.HashString(args[2] ?? string.Empty);
					if (string.Equals(adapter.GetPassword(), hashed, StringComparison.Ordinal))
					{
						adapter.AddAllowedUserServer(senderUserId);
						SdtdConsole.Instance.Output("[LW] Access granted for " + FormatPos(targetPos) + ".");
					}
					else
					{
						SdtdConsole.Instance.Output("[LW] Invalid code.");
					}

					return;
				default:
					SdtdConsole.Instance.Output("[LW] Unknown code action. Use: set, clear, or use.");
					return;
			}
		}

		private static bool CanManage(World world, TileEntityLockAdapter adapter, PlatformUserIdentifierAbs senderUserId, bool isAdmin)
		{
			if (adapter == null)
			{
				return false;
			}

			if (isAdmin)
			{
				return true;
			}

			if (adapter.GetOwner() == null)
			{
				return true;
			}

			return senderUserId != null && LockableWorkstationHelpers.IsOwnerOrAcl(world, adapter, senderUserId);
		}

		private static bool TryResolveFocusedTarget(World world, int senderEntityId, out TileEntity tileEntity, out TileEntityLockAdapter adapter, out Vector3i pos, out string error)
		{
			tileEntity = null;
			adapter = null;
			pos = Vector3i.zero;
			error = string.Empty;

			if (senderEntityId < 0)
			{
				error = "This command requires an in-world player and focused block within 5m.";
				return false;
			}

			Entity senderEntity = world.GetEntity(senderEntityId);
			if (!(senderEntity is EntityAlive senderAlive))
			{
				error = "Could not resolve player entity for focused block lookup.";
				return false;
			}

			Vector3 eyePos = senderAlive.getHeadPosition();
			Ray lookRay = senderAlive.GetLookRay();
			if (!Physics.Raycast(eyePos, lookRay.direction, out RaycastHit hit, FocusDistanceMeters))
			{
				error = "No focused block within 5m.";
				return false;
			}

			Vector3i hitPos = World.worldToBlockPos(hit.point - lookRay.direction * 0.02f);
			if (!LockableWorkstationHelpers.TryGetAdapter(world, -1, hitPos, out tileEntity, out adapter)
				&& !TryFindNearbyLockable(world, hitPos, out hitPos, out tileEntity, out adapter))
			{
				error = "Focused block is not a supported lockable workstation.";
				return false;
			}

			pos = hitPos;

			return true;
		}

		private static bool TryFindNearbyLockable(World world, Vector3i centerPos, out Vector3i targetPos, out TileEntity tileEntity, out TileEntityLockAdapter adapter)
		{
			targetPos = Vector3i.zero;
			tileEntity = null;
			adapter = null;

			for (int x = centerPos.x - 1; x <= centerPos.x + 1; x++)
			{
				for (int y = centerPos.y - 1; y <= centerPos.y + 1; y++)
				{
					for (int z = centerPos.z - 1; z <= centerPos.z + 1; z++)
					{
						Vector3i candidate = new Vector3i(x, y, z);
						if (LockableWorkstationHelpers.TryGetAdapter(world, -1, candidate, out tileEntity, out adapter))
						{
							targetPos = candidate;
							return true;
						}
					}
				}
			}

			return false;
		}

		private static bool TryGetSenderContext(CommandSenderInfo senderInfo, out int entityId, out PlatformUserIdentifierAbs userId, out string senderKey)
		{
			entityId = -1;
			userId = null;
			senderKey = "console";

			Type senderType = senderInfo.GetType();
			if (TryReadInt(senderType, senderInfo, "entityId", out int senderEntityId)
				|| TryReadInt(senderType, senderInfo, "EntityId", out senderEntityId))
			{
				entityId = senderEntityId;
			}

			object remoteClientInfo = null;
			if (TryReadObject(senderType, senderInfo, "RemoteClientInfo", out remoteClientInfo)
				|| TryReadObject(senderType, senderInfo, "remoteClientInfo", out remoteClientInfo)
				|| TryReadObject(senderType, senderInfo, "ClientInfo", out remoteClientInfo)
				|| TryReadObject(senderType, senderInfo, "clientInfo", out remoteClientInfo))
			{
				if (remoteClientInfo is ClientInfo clientInfo)
				{
					if (entityId < 0)
					{
						entityId = clientInfo.entityId;
					}

					userId = clientInfo.InternalId;
				}
			}

			if (userId == null && entityId >= 0)
			{
				userId = LockableWorkstationHelpers.ResolveUserIdentifier(GameManager.Instance?.World, entityId);
			}

			senderKey = userId?.CombinedString ?? (entityId >= 0 ? "entity:" + entityId : "console");
			return true;
		}

		private static bool TryReadInt(Type type, object instance, string memberName, out int value)
		{
			value = 0;
			PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.PropertyType == typeof(int))
			{
				value = (int)property.GetValue(instance, null);
				return true;
			}

			FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && field.FieldType == typeof(int))
			{
				value = (int)field.GetValue(instance);
				return true;
			}

			return false;
		}

		private static bool TryReadObject(Type type, object instance, string memberName, out object value)
		{
			value = null;
			PropertyInfo property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				value = property.GetValue(instance, null);
				return true;
			}

			FieldInfo field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				value = field.GetValue(instance);
				return true;
			}

			return false;
		}

		private static string FormatPos(Vector3i pos)
		{
			return "(" + pos.x + "," + pos.y + "," + pos.z + ")";
		}
	}
}
