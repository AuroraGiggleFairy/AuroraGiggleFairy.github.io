using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// On a dedicated server the pickup only gave the item to a local player, then
	/// deleted the block anyway. The sprite vanished with no sound, and the item
	/// arrived later only if the client touch still won. The server grants it first.
	/// </summary>
	internal static class ItemPickups
	{
		private const float TickSeconds = 0.05f;

		private static float _next;

		internal static void Tick()
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || net.IsClient || Time.unscaledTime < _next)
			{
				return;
			}

			_next = Time.unscaledTime + TickSeconds;
			if (Item.Active == null || Item.Active.Count == 0)
			{
				return;
			}

			World world = GameManager.Instance?.World;
			if (world?.Players?.list == null)
			{
				return;
			}

			for (int i = Item.Active.Count - 1; i >= 0; i--)
			{
				Item item = Item.Active[i];
				if (item == null)
				{
					continue;
				}

				Collider volume = item.Volume();
				if (volume == null)
				{
					continue;
				}

				for (int p = 0; p < world.Players.list.Count; p++)
				{
					EntityPlayer player = world.Players.list[p];
					if (player == null || player.IsDead() || !Instances.IsInstanceSpace(player.position))
					{
						continue;
					}

					if (!PickupBody.Overlaps(player, volume, out Collider body))
					{
						continue;
					}

					item.Touched(body);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Item), nameof(Item.Touched))]
	internal static class Patch_ItemGrantRemote
	{
		private static readonly MethodInfo Reachable = AccessTools.Method(typeof(Item), "IsReachable");
		private static readonly FieldInfo ItemName = AccessTools.Field(typeof(Item), "_item");
		private static readonly FieldInfo BuffName = AccessTools.Field(typeof(Item), "_buff");
		private static readonly FieldInfo Amount = AccessTools.Field(typeof(Item), "_amount");
		private static readonly FieldInfo SoundName = AccessTools.Field(typeof(Item), "_sound");
		private static readonly FieldInfo EffectName = AccessTools.Field(typeof(Item), "_effect");
		private static readonly FieldInfo Counts = AccessTools.Field(typeof(Item), "_counts");
		private static readonly FieldInfo Taken = AccessTools.Field(typeof(Item), "_taken");
		private static readonly FieldInfo Granted = AccessTools.Field(typeof(Item), "_granted");

		private static bool Prefix(Item __instance, Collider other)
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || __instance == null)
			{
				return true;
			}

			if (net.IsClient && !net.IsServer)
			{
				return false;
			}

			EntityPlayer player = other == null ? null : other.GetComponentInParent<EntityPlayer>();
			if (player == null || player is EntityPlayerLocal)
			{
				return true;
			}

			if (AlreadyTaken(__instance))
			{
				return false;
			}

			if (Reachable == null || !(bool)Reachable.Invoke(__instance, new object[] { player }))
			{
				return false;
			}

			if (!Give(__instance, player))
			{
				return false;
			}

			if (Granted != null)
			{
				Granted.SetValue(__instance, true);
			}

			return true;
		}

		private static bool AlreadyTaken(Item item)
		{
			if (Taken != null && (bool)Taken.GetValue(item))
			{
				return true;
			}

			return Granted != null && (bool)Granted.GetValue(item);
		}

		private static bool Give(Item item, EntityPlayer player)
		{
			string buff = BuffName == null ? null : BuffName.GetValue(item) as string;
			string name = ItemName == null ? null : ItemName.GetValue(item) as string;
			int amount = Amount == null ? 0 : (int)Amount.GetValue(item);
			string sound = SoundName == null ? "item" : SoundName.GetValue(item) as string;
			string effect = EffectName == null ? null : EffectName.GetValue(item) as string;

			bool itemGrant = string.IsNullOrEmpty(buff);
			if (!itemGrant)
			{
				if (player.Buffs.AddBuff(buff, -1, true, false, -1f) != EntityBuffs.BuffStatus.Added)
				{
					return false;
				}
			}
			else
			{
				ItemValue value = ItemClass.GetItem(name, false);
				if (value == null || value.IsEmpty() || amount <= 0 || !Fits(player, new ItemStack(value, amount)))
				{
					return false;
				}
			}

			if (!itemGrant && !string.IsNullOrEmpty(effect))
			{
				player.Buffs.AddBuff(effect, -1, true, false, -1f);
			}

			if (Counts != null && (bool)Counts.GetValue(item))
			{
				Stats stats = Instances.StatsFor(player.entityId);
				if (stats != null && RunStats.AllowGrant(player.entityId))
				{
					stats.Items++;
				}
			}

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net != null && net.IsServer)
			{
				string played = sound == "weapon" ? "doom_dswpnup" : "doom_dsitemup";
				string clientEffect = itemGrant ? effect ?? "" : "";
				net.SendPackage(PackageEmit.Take<NetPackageDoomPickupFx>()
					.Setup(played, itemGrant ? name : "", amount, clientEffect), false, player.entityId);
			}

			return true;
		}

		private static bool Fits(EntityPlayer player, ItemStack stack)
		{
			if (player.bag != null && player.bag.CanTakeItem(stack))
			{
				return true;
			}

			return player.inventory != null && player.inventory.CanStack(stack);
		}
	}

	/// <summary>
	/// A listen host is also a client, so Item.Touched grants and then returns
	/// before it marks the spot or removes the block. Dedicated already does both.
	/// </summary>
	[HarmonyPatch(typeof(Item), nameof(Item.Touched))]
	[HarmonyPriority(Priority.Last)]
	internal static class Patch_FinishTaken
	{
		private static readonly FieldInfo Taken = AccessTools.Field(typeof(Item), "_taken");
		private static readonly FieldInfo Granted = AccessTools.Field(typeof(Item), "_granted");
		private static readonly FieldInfo BlockPos = AccessTools.Field(typeof(Item), "_blockPos");

		private static void Postfix(Item __instance)
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || !net.IsServer || __instance == null || Taken == null || Granted == null || BlockPos == null)
			{
				return;
			}

			if (!(bool)Granted.GetValue(__instance) || (bool)Taken.GetValue(__instance))
			{
				return;
			}

			Taken.SetValue(__instance, true);
			Vector3i pos = (Vector3i)BlockPos.GetValue(__instance);
			GameManager.Instance.World?.SetBlockRPC(new BlockValueRef(pos), BlockValue.Air);
			RunStats.ItemTaken(pos);
		}
	}

	[Preserve]
	public abstract class NetPackageDoomPickupFx : NetPackage
	{
		private string _sound = "";
		private string _item = "";
		private int _amount;
		private string _effect = "";

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageDoomPickupFx Setup(string sound, string item, int amount, string effect)
		{
			_sound = sound ?? "";
			_item = item ?? "";
			_amount = amount;
			_effect = effect ?? "";
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_sound = _br.ReadString();
			_item = _br.ReadString();
			_amount = _br.ReadInt32();
			_effect = _br.ReadString();
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			((System.IO.BinaryWriter)_bw).Write(_sound);
			((System.IO.BinaryWriter)_bw).Write(_item);
			((System.IO.BinaryWriter)_bw).Write(_amount);
			((System.IO.BinaryWriter)_bw).Write(_effect);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			if (_world == null)
			{
				return;
			}

			EntityPlayerLocal local = _world.GetPrimaryPlayer();
			ItemValue value = string.IsNullOrEmpty(_item) ? null : ItemClass.GetItem(_item, false);
			bool received = false;
			if (local != null && value != null && !value.IsEmpty() && _amount > 0)
			{
				ItemStack give = new ItemStack(value, _amount);
				if ((local.bag == null || !local.bag.AddItem(give)) &&
					(local.inventory == null || !local.inventory.AddItem(give)))
				{
					if (give != null && !give.IsEmpty())
					{
						GameManager.Instance.ItemDropServer(give, local.GetPosition(), Vector3.zero, local.entityId);
					}
				}
				else
				{
					local.AddUIHarvestingItem(new ItemStack(value, _amount), false);
					received = true;
				}
			}

			if (!string.IsNullOrEmpty(_sound) && (received || string.IsNullOrEmpty(_item)))
			{
				Audio.Manager.PlayInsidePlayerHead(_sound, -1, 0f, false, false);
			}

			if (received && local != null && !string.IsNullOrEmpty(_effect))
			{
				local.Buffs.AddBuff(_effect);
			}
		}

		public int Length()
		{
			return 32;
		}
	}
}
