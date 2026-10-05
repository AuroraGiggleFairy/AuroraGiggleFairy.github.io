using System;
using System.Collections.Generic;
using System.Reflection;
using Platform;
using UnityEngine;

namespace LockableWorkstations
{
	public class ConsoleCmdLockableWorkstations : ConsoleCmdAbstract
	{
		private const float FocusDistanceMeters = 15f;
		private const string ColorOk = "B8A3D8";
		private const string ColorOpt = "86A67F";
		private const string ColorErr = "C38FAE";

		public override string[] getCommands()
		{
			return new[]
			{
				"agf-ls",
				"agfls",
				"agf-lw",
				"agf-lockws",
				"agf-lockableworkstations",
				"lw",
				"lockws",
				"lockableworkstations"
			};
		}

		public override string getDescription()
		{
			return "Admin controls for Lockable Stations defaultlock and nearby lock.";
		}

		public override string getHelp()
		{
			return HelpText;
		}

		internal const string HelpText = "Usage:\n"
			+ "  agf-ls defaultlock <true|false>\n"
			+ "  agf-ls nearby lock\n"
			+ "  agf-ls nearby unlock\n";

		private static Action<string> s_reply;

		public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
		{
			World world = GameManager.Instance?.World;
			if (world == null)
			{
				ConsoleLine("World is not ready.");
				return;
			}

			if (!TryGetSenderContext(_senderInfo, out int senderEntityId, out _, out _))
			{
				ConsoleLine("Unable to resolve command sender context.");
				return;
			}

			ExecuteConsole(_params, senderEntityId);
		}

		public static bool TryHandleChatMessage(string message, int senderEntityId, ClientInfo clientInfo)
		{
			if (!TryParseChat(message, out List<string> args))
				return false;

			World world = GameManager.Instance?.World;
			if (world == null)
			{
				Whisper(senderEntityId, Err("World is not ready."));
				return true;
			}

			PlatformUserIdentifierAbs userId = clientInfo?.InternalId
				?? LockableWorkstationHelpers.ResolveUserIdentifier(world, senderEntityId);
			s_reply = line => Whisper(senderEntityId, line);
			try
			{
				ExecuteChat(args, senderEntityId, userId);
			}
			finally
			{
				s_reply = null;
			}

			return true;
		}

		private static void ExecuteChat(List<string> _params, int senderEntityId, PlatformUserIdentifierAbs senderUserId)
		{
			World world = GameManager.Instance?.World;
			if (world == null)
			{
				Out(Err("World is not ready."));
				return;
			}

			bool isAdmin = LockableWorkstationHelpers.IsAdminEntityId(senderEntityId);
			string subcommand = (_params != null && _params.Count > 0 ? _params[0] : "help")?.Trim().ToLowerInvariant() ?? "help";
			switch (subcommand)
			{
				case "help":
					Out(Ok("Lockable Stations"));
					Out(Opt("Options: /agfls lock, unlock, status, pin"));
					Out(Opt("/agfls pin set <pin>, pin use <pin>, pin clear"));
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
				case "pin":
				case "code":
					HandleCodeCommand(world, _params, senderEntityId, senderUserId, isAdmin);
					return;
				default:
					Out(Err("Use /agfls"));
					return;
			}
		}

		private static void ExecuteConsole(List<string> _params, int senderEntityId)
		{
			if (!IsConsoleAdmin(senderEntityId))
			{
				ConsoleLine("admin permission required.");
				return;
			}

			string subcommand = (_params != null && _params.Count > 0 ? _params[0] : "help")?.Trim().ToLowerInvariant() ?? "help";
			switch (subcommand)
			{
				case "help":
				case "defaultlock":
					if (subcommand == "defaultlock" && _params != null && _params.Count >= 2)
					{
						HandleDefaultLockCommand(_params);
						return;
					}

					OutputConsoleUsage();
					return;
				case "nearby":
					HandleNearbyCommand(_params, senderEntityId);
					return;
				default:
					ConsoleLine("invalid option. Use: agf-ls help.");
					return;
			}
		}

		private static bool IsConsoleAdmin(int senderEntityId)
		{
			if (senderEntityId < 0)
				return true;

			return LockableWorkstationHelpers.IsAdminEntityId(senderEntityId);
		}

		private static void ConsoleLine(string text)
		{
			SdtdConsole.Instance.Output("[LockableStations] " + text);
		}

		private static void OutputConsoleUsage()
		{
			string current = LockableWorkstationHelpers.DefaultLockedOnPlace ? "ON" : "OFF";
			ConsoleLine("Usage:");
			ConsoleLine("  agf-ls defaultlock <true|false>");
			ConsoleLine("  agf-ls nearby lock");
			ConsoleLine("  agf-ls nearby unlock");
			ConsoleLine("defaultlock currently set to " + current);
			ConsoleLine("Newly placed stations and first-seen stations on this save use this lock state. This session only.");
			ConsoleLine("Nearby uses the land claim if you are in one, otherwise land-claim size around you.");
		}

		private static bool TryParseChat(string message, out List<string> args)
		{
			args = null;
			if (string.IsNullOrWhiteSpace(message))
				return false;

			string trimmed = message.Trim();
			if (!trimmed.StartsWith("/", StringComparison.Ordinal))
				return false;

			string[] parts = trimmed.Substring(1).Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 0)
				return false;

			string root = parts[0].Trim().ToLowerInvariant();
			if (root != "agfls" && root != "agf-ls" && root != "agf-lw" && root != "agf-lockws"
				&& root != "agf-lockableworkstations" && root != "lw" && root != "lockws" && root != "lockableworkstations")
			{
				return false;
			}

			args = new List<string>();
			for (int i = 1; i < parts.Length; i++)
				args.Add(parts[i]);
			return true;
		}

		private static void Whisper(int senderEntityId, string message)
		{
			if (senderEntityId < 0 || string.IsNullOrEmpty(message))
				return;

			GameManager.Instance?.ChatMessageServer(
				null,
				EChatType.Whisper,
				-1,
				message,
				new List<int> { senderEntityId },
				EMessageSender.Server);
		}

		private static void Out(string message)
		{
			if (string.IsNullOrEmpty(message))
				return;

			if (s_reply != null)
			{
				string[] lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < lines.Length; i++)
					s_reply(lines[i]);
				return;
			}

			SdtdConsole.Instance.Output(message);
		}

		private static string Ok(string text)
		{
			return "[" + ColorOk + "][" + text + "][-]";
		}

		private static string Opt(string text)
		{
			return "[" + ColorOpt + "][" + text + "][-]";
		}

		private static string Err(string text)
		{
			return "[" + ColorErr + "][ERROR][-][" + ColorOpt + "][" + text + "][-]";
		}

		private static void HandleDefaultLockCommand(List<string> _params)
		{
			if (_params == null || _params.Count < 2)
			{
				OutputConsoleUsage();
				return;
			}

			if (!bool.TryParse(_params[1], out bool parsed))
			{
				ConsoleLine("invalid option. Use: agf-ls defaultlock <true|false>.");
				return;
			}

			LockableWorkstationHelpers.DefaultLockedOnPlace = parsed;
			if (parsed)
				ConsoleLine("defaultlock set to ON. Newly placed stations and first-seen stations on this save will start locked.");
			else
				ConsoleLine("defaultlock set to OFF. Newly placed stations and first-seen stations on this save will start unlocked.");
		}

		private static void HandleNearbyCommand(List<string> _params, int senderEntityId)
		{
			if (_params == null || _params.Count < 2)
			{
				ConsoleLine("invalid option. Use: agf-ls nearby lock|unlock.");
				return;
			}

			string action = _params[1].Trim().ToLowerInvariant();
			bool lockNow;
			if (action == "lock")
				lockNow = true;
			else if (action == "unlock")
				lockNow = false;
			else
			{
				ConsoleLine("invalid option. Use: agf-ls nearby lock|unlock.");
				return;
			}

			if (senderEntityId < 0)
			{
				ConsoleLine("nearby needs an in-world player.");
				return;
			}

			World world = GameManager.Instance?.World;
			Entity senderEntity = world?.GetEntity(senderEntityId);
			if (world == null || senderEntity == null)
			{
				ConsoleLine("nearby needs an in-world player.");
				return;
			}

			Vector3i origin = World.worldToBlockPos(senderEntity.position);
			int count = NearbyStationScanner.SetLockedNearby(world, origin, lockNow, out bool usedLandClaim);
			string where = usedLandClaim ? "land claim" : "land-claim size around player";
			ConsoleLine("nearby " + (lockNow ? "lock" : "unlock") + " applied to " + count + " stations (" + where + ").");
		}

		private static void HandleStatusCommand(World world, int senderEntityId)
		{
			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				Out(Err(error));
				return;
			}

			string owner = adapter.GetOwner()?.CombinedString ?? "(none)";
			Out(Ok("Target " + FormatPos(targetPos)
				+ " | locked=" + adapter.IsLocked()
				+ " | owner=" + owner
				+ " | hasCode=" + adapter.HasPassword()
				+ " | allowedUsers=" + adapter.GetUsers().Count));
		}

		private static void HandleLockToggle(World world, int senderEntityId, PlatformUserIdentifierAbs senderUserId, bool isAdmin, bool lockNow)
		{
			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				Out(Err(error));
				return;
			}

			if (!CanManage(world, adapter, senderUserId, isAdmin))
			{
				Out(Err("You are not the owner/admin for this workstation."));
				return;
			}

			if (adapter.GetOwner() == null && senderUserId != null)
			{
				adapter.SetOwner(senderUserId);
			}

			adapter.SetLocked(lockNow);
			Out(Ok("Lockable Stations = " + (lockNow ? "LOCKED" : "UNLOCKED")));
		}

		private static void HandleCodeCommand(World world, List<string> args, int senderEntityId, PlatformUserIdentifierAbs senderUserId, bool isAdmin)
		{
			if (args == null || args.Count < 2)
			{
				Out(Err("Use /agfls code set, clear, or use"));
				return;
			}

			if (!TryResolveFocusedTarget(world, senderEntityId, out _, out TileEntityLockAdapter adapter, out Vector3i targetPos, out string error))
			{
				Out(Err(error));
				return;
			}

			string action = args[1]?.Trim().ToLowerInvariant() ?? string.Empty;
			switch (action)
			{
				case "set":
					if (args.Count < 3)
					{
						Out(Err("Use /agfls code set <code>"));
						return;
					}

					if (!CanManage(world, adapter, senderUserId, isAdmin))
					{
						Out(Err("You are not the owner/admin for this workstation."));
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

					Out(Ok("Keypad code set"));
					return;
				case "clear":
					if (!CanManage(world, adapter, senderUserId, isAdmin))
					{
						Out(Err("You are not the owner/admin for this workstation."));
						return;
					}

					adapter.ApplyServerPasswordHash(string.Empty);
					Out(Ok("Keypad code cleared"));
					return;
				case "use":
					if (senderUserId == null)
					{
						Out(Err("A player identity is required to use code access."));
						return;
					}

					if (args.Count < 3)
					{
						Out(Err("Use /agfls code use <code>"));
						return;
					}

					string hashed = Utils.HashString(args[2] ?? string.Empty);
					if (string.Equals(adapter.GetPassword(), hashed, StringComparison.Ordinal))
					{
						adapter.AddAllowedUserServer(senderUserId);
						Out(Ok("Access granted"));
					}
					else
					{
						Out(Err("Invalid code"));
					}

					return;
				default:
					Out(Err("Unknown code action. Use set, clear, or use"));
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
				error = "This command needs an in-world player looking at a station.";
				return false;
			}

			Entity senderEntity = world.GetEntity(senderEntityId);
			if (!(senderEntity is EntityAlive senderAlive))
			{
				error = "Could not resolve player entity for focused block lookup.";
				return false;
			}

			Vector3i hitPos = Vector3i.zero;
			BlockValue hitBlock = BlockValue.Air;
			bool haveHit = false;
			float maxDistSq = FocusDistanceMeters * FocusDistanceMeters;

			EntityPlayerLocal local = senderAlive as EntityPlayerLocal;
			WorldRayHitInfo promptHit = local != null ? local.HitInfo : null;
			if (promptHit != null && promptHit.bHitValid && promptHit.hit.distanceSq <= maxDistSq)
			{
				hitPos = promptHit.hit.blockPos;
				hitBlock = promptHit.hit.blockValue;
				if (hitPos.Equals(Vector3i.zero) && promptHit.lastBlockPos != Vector3i.zero)
					hitPos = promptHit.lastBlockPos;
				haveHit = !hitPos.Equals(Vector3i.zero) || !hitBlock.isair;
			}

			if (!haveHit)
			{
				Ray lookRay = senderAlive.GetLookRay();
				const int hitMask = 69;
				bool voxelHit = Voxel.Raycast(world, lookRay, FocusDistanceMeters, -555528221, hitMask, 0f);
				if (!voxelHit)
					voxelHit = Voxel.Raycast(world, lookRay, FocusDistanceMeters, -555266077, hitMask, 0f);

				WorldRayHitInfo voxelInfo = Voxel.voxelRayHitInfo;
				if (!voxelHit || voxelInfo == null || !voxelInfo.bHitValid)
				{
					error = "No station in view.";
					return false;
				}

				hitPos = voxelInfo.hit.blockPos;
				hitBlock = voxelInfo.hit.blockValue;
				if (hitPos.Equals(Vector3i.zero) && voxelInfo.lastBlockPos != Vector3i.zero)
					hitPos = voxelInfo.lastBlockPos;
			}

			if (hitBlock.ischild && hitBlock.Block != null && hitBlock.Block.multiBlockPos != null)
				hitPos = hitBlock.Block.multiBlockPos.GetParentPos(hitPos, hitBlock);

			hitPos = LockableWorkstationHelpers.ResolveParentIfChild(world, 0, hitPos);

			if (!LockableWorkstationHelpers.TryGetAdapter(world, 0, hitPos, out tileEntity, out adapter)
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

			for (int x = centerPos.x - 2; x <= centerPos.x + 2; x++)
			{
				for (int y = centerPos.y - 2; y <= centerPos.y + 2; y++)
				{
					for (int z = centerPos.z - 2; z <= centerPos.z + 2; z++)
					{
						Vector3i candidate = LockableWorkstationHelpers.ResolveParentIfChild(world, 0, new Vector3i(x, y, z));
						if (LockableWorkstationHelpers.TryGetAdapter(world, 0, candidate, out tileEntity, out adapter))
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
