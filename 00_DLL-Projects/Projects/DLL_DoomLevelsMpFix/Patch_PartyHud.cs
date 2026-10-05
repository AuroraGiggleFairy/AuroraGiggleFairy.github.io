using System.Collections.Generic;
using System.Reflection;
using DoomLevels;
using HarmonyLib;
using UnityEngine;

namespace DoomLevelsMpFix
{
	internal static class PartyHud
	{
		internal const string ArrowSprite = "ui_game_symbol_map_player_arrow";
		internal const string TeleporterSprite = "DoomTeleporterHD";
		internal const string UiAtlas = "UIAtlas";
		internal const string ItemAtlas = "ItemIconAtlas";
		internal const string BackSprite = "menu_empty3px";

		private static readonly Dictionary<XUiC_PartyEntry, int> Fallback = new Dictionary<XUiC_PartyEntry, int>();
		private static readonly Dictionary<int, float> PresentSince = new Dictionary<int, float>();
		private static readonly FieldInfo EntryListField = AccessTools.Field(typeof(XUiC_PartyEntryList), "entryList");
		private static readonly FieldInfo ArrowField = AccessTools.Field(typeof(XUiC_PartyEntry), "arrowContent");
		private static readonly FieldInfo LeaderIconField = AccessTools.Field(typeof(XUiC_PartyEntry), "leaderIcon");
		private static int _refreshFrames;

		internal static void RequestRefresh()
		{
			_refreshFrames = 45;
		}

		internal static void Tick()
		{
			if (_refreshFrames <= 0)
			{
				return;
			}

			_refreshFrames--;
			RefreshNow();
		}

		internal static void Reset()
		{
			_refreshFrames = 0;
			Fallback.Clear();
			PresentSince.Clear();
			PartyIds.Clear();
			InstanceSync.Clear();
		}

		internal static bool InLevel(EntityPlayer player)
		{
			if (player == null)
			{
				return false;
			}

			if (Instances.IsInside(player.entityId) || Instances.IsInstanceSpace(player.position))
			{
				return true;
			}

			return false;
		}

		internal static bool InLevel(int entityId, EntityPlayer player)
		{
			if (player != null)
			{
				return InLevel(player);
			}

			return entityId > 0 && InstanceSync.Known(entityId);
		}

		internal static bool Away(EntityPlayer local, EntityPlayer member, int memberId = 0)
		{
			bool localIn = InLevel(local);
			int id = member != null ? member.entityId : memberId;
			bool memberIn = InLevel(id, member);
			if (localIn != memberIn)
			{
				return true;
			}

			if (!localIn || local == null || id <= 0)
			{
				return false;
			}

			if (!InstanceSync.TryGet(local.entityId, out _, out _, out int localCell) ||
				!InstanceSync.TryGet(id, out _, out _, out int memberCell))
			{
				return false;
			}

			return localCell != memberCell;
		}

		internal static int FallbackId(XUiC_PartyEntry entry)
		{
			if (entry != null && Fallback.TryGetValue(entry, out int id))
			{
				return id;
			}

			return 0;
		}

		private static void SetFallback(XUiC_PartyEntry entry, int entityId)
		{
			if (entry == null)
			{
				return;
			}

			if (entityId <= 0)
			{
				Fallback.Remove(entry);
				return;
			}

			Fallback[entry] = entityId;
		}

		private static void RefreshNow()
		{
			World world = GameManager.Instance?.World;
			List<EntityPlayerLocal> locals = world?.GetLocalPlayers();
			if (locals == null)
			{
				return;
			}

			for (int i = 0; i < locals.Count; i++)
			{
				EntityPlayerLocal local = locals[i];
				LocalPlayerUI ui = local != null ? LocalPlayerUI.GetUIForPlayer(local) : null;
				XUiC_PartyEntryList list = ui?.xui?.GetChildByType<XUiC_PartyEntryList>();
				list?.RefreshPartyList();
			}
		}

		internal static bool FillList(XUiC_PartyEntryList list)
		{
			if (list == null || EntryListField == null)
			{
				return true;
			}

			List<XUiC_PartyEntry> entries = EntryListField.GetValue(list) as List<XUiC_PartyEntry>;
			if (entries == null)
			{
				return true;
			}

			EntityPlayer local = list.xui?.playerUI?.entityPlayer;
			World world = GameManager.Instance?.World;
			int slot = 0;
			int[] ids = local?.Party != null ? PartyIds.For(local.Party) : null;
			if (local?.Party != null && ids != null)
			{
				PartyMembers.SyncLive(local.Party, ids, world);
				for (int i = 0; i < ids.Length; i++)
				{
					int id = ids[i];
					if (id <= 0 || (local != null && id == local.entityId))
					{
						continue;
					}

					if (slot >= entries.Count)
					{
						break;
					}

					EntityPlayer live = PartyMembers.Find(world, id);
					if (live != null)
					{
						SetFallback(entries[slot], 0);
						entries[slot].SetPlayer(live);
						slot++;
						continue;
					}

					SetFallback(entries[slot], id);
					entries[slot].SetPlayer(null);
					entries[slot].IsDirty = true;
					ApplyVitalsFill(entries[slot], id);
					entries[slot].RefreshBindings();
					slot++;
				}
			}

			for (int i = slot; i < entries.Count; i++)
			{
				SetFallback(entries[i], 0);
				entries[i].SetPlayer(null);
			}

			return false;
		}

		internal static void ApplyArrow(XUiC_PartyEntry entry)
		{
			if (entry == null || ArrowField == null)
			{
				return;
			}

			XUiV_Sprite arrow = ArrowField.GetValue(entry) as XUiV_Sprite;
			if (arrow == null)
			{
				return;
			}

			int fallback = FallbackId(entry);
			int vitalsId = entry.Player != null ? entry.Player.entityId : fallback;
			if (vitalsId > 0 && (entry.Player == null || Away(entry.xui?.playerUI?.entityPlayer, entry.Player, fallback)))
			{
				ApplyVitalsFill(entry, vitalsId);
			}

			int shownId = entry.Player != null ? entry.Player.entityId : fallback;
			bool away = Away(entry.xui?.playerUI?.entityPlayer, entry.Player, fallback);
			if (away)
			{
				PresentSince.Remove(shownId);
				int id = shownId;
				Color32 tint = PartyIds.ColorOf(entry.xui?.playerUI?.entityPlayer?.Party, id);
				XUiV_Sprite back = ChildSprite(entry, "arrowBack");
				XUiV_Sprite icon = ChildSprite(entry, "doomLevelIcon");
				SetVisible(arrow, false);
				SetBacking(back, true, tint);
				SetItemIcon(icon, true);
				if (arrow.UiTransform != null)
				{
					arrow.UiTransform.localEulerAngles = Vector3.zero;
				}

				return;
			}

			HoldReturnHealth(entry, shownId);
			SetVisible(ChildSprite(entry, "arrowBack"), false);
			SetVisible(ChildSprite(entry, "doomLevelIcon"), false);
			if (arrow.UIAtlas != UiAtlas)
			{
				arrow.UIAtlas = UiAtlas;
			}

			if (arrow.SpriteName != ArrowSprite)
			{
				arrow.SpriteName = ArrowSprite;
			}
		}

		internal static XUiV_Sprite ChildSprite(XUiController parent, string name)
		{
			return parent?.GetChildById(name)?.ViewComponent as XUiV_Sprite;
		}

		internal static void SetVisible(XUiV_Sprite sprite, bool visible)
		{
			if (sprite != null)
			{
				sprite.IsVisible = visible;
			}
		}

		internal static void SetBacking(XUiV_Sprite sprite, bool visible, Color32 tint)
		{
			if (sprite == null)
			{
				return;
			}

			sprite.IsVisible = visible;
			if (!visible)
			{
				return;
			}

			if (sprite.UIAtlas != UiAtlas)
			{
				sprite.UIAtlas = UiAtlas;
			}

			if (sprite.SpriteName != BackSprite)
			{
				sprite.SpriteName = BackSprite;
			}

			sprite.Color = new Color(tint.r / 255f, tint.g / 255f, tint.b / 255f, 1f);
		}

		internal static void SetItemIcon(XUiV_Sprite sprite, bool visible)
		{
			if (sprite == null)
			{
				return;
			}

			sprite.IsVisible = visible;
			if (!visible)
			{
				return;
			}

			if (sprite.UIAtlas != ItemAtlas)
			{
				sprite.UIAtlas = ItemAtlas;
			}

			if (sprite.SpriteName != TeleporterSprite)
			{
				sprite.SpriteName = TeleporterSprite;
			}

			sprite.Color = Color.white;
		}

		internal static bool ApplyBinding(XUiC_PartyEntry entry, ref string value, string bindingName)
		{
			int fallback = FallbackId(entry);
			EntityPlayer local = entry?.xui?.playerUI?.entityPlayer;
			int vitalsId = entry?.Player != null ? entry.Player.entityId : fallback;
			if (Away(local, entry?.Player, fallback) && BindVitals(ref value, bindingName, vitalsId))
			{
				return true;
			}

			if (fallback > 0 && entry.Player == null)
			{
				switch (bindingName)
				{
				case "partyvisible":
					value = "true";
					return true;
				case "name":
					value = GameUtils.SafeStringFormat(PartyIds.NameOf(fallback) ?? "");
					return true;
				case "distance":
					value = "";
					return true;
				case "showarrow":
					value = "false";
					return true;
				case "voicevisible":
					value = "false";
					return true;
				case "showicon1":
					value = "false";
					return true;
				case "showicon2":
					value = "false";
					return true;
				case "icon1":
					if (PartyIds.LeaderOf(local?.Party) == fallback && LeaderIconField != null)
					{
						value = LeaderIconField.GetValue(entry) as string ?? "";
					}
					else
					{
						value = "";
					}

					return true;
				case "icon2":
					value = "";
					return true;
				case "arrowcolor":
					value = ArrowColor(entry, fallback);
					return true;
				}
				if (BindVitals(ref value, bindingName, fallback))
				{
					return true;
				}

				return false;
			}

			if (!Away(local, entry?.Player, fallback))
			{
				return false;
			}

			if (bindingName == "distance")
			{
				value = "";
				return true;
			}

			if (bindingName == "showarrow" || bindingName == "showicon1" || bindingName == "showicon2")
			{
				value = "false";
				return true;
			}

			return false;
		}

		internal static string ArrowColor(XUiC_PartyEntry entry, int fallback = 0)
		{
			try
			{
				EntityPlayer player = entry?.Player;
				EntityPlayer local = entry?.xui?.playerUI?.entityPlayer;
				int id = player != null ? player.entityId : fallback;
				Color32 color = PartyIds.ColorOf(player?.Party ?? local?.Party, id);
				return color.r + "," + color.g + "," + color.b + "," + color.a;
			}
			catch
			{
				return "255,255,255,255";
			}
		}

		private static bool BindVitals(ref string value, string bindingName, int entityId)
		{
			if (entityId <= 0)
			{
				return false;
			}

			switch (bindingName)
			{
			case "healthcurrent":
				if (InstanceSync.TryVitals(entityId, out int hpNow, out int hpMaxNow, out _) && hpMaxNow > 0)
				{
					value = hpNow.ToString();
					return true;
				}

				return false;
			case "healthcurrentwithmax":
				if (InstanceSync.TryVitals(entityId, out int hp, out int hpMax, out _) && hpMax > 0)
				{
					value = hp + "/" + hpMax;
					return true;
				}

				return false;
			case "healthfill":
				if (InstanceSync.TryVitals(entityId, out int hpFill, out int hpFillMax, out _) && hpFillMax > 0)
				{
					value = ((float)hpFill / hpFillMax).ToCultureInvariantString();
					return true;
				}

				return false;
			case "healthmodifiedmax":
				value = "1";
				return true;
			default:
				return false;
			}
		}

		private static void HoldReturnHealth(XUiC_PartyEntry entry, int id)
		{
			if (entry == null || id <= 0)
			{
				return;
			}

			if (!PresentSince.ContainsKey(id))
			{
				PresentSince[id] = Time.time;
			}

			if (Time.time - PresentSince[id] > 2f)
			{
				return;
			}

			EntityPlayer player = entry.Player;
			if (player != null && player.IsDead())
			{
				return;
			}

			if (!InstanceSync.TryVitals(id, out int hp, out int maxHp, out _) || maxHp <= 0 || hp <= 0)
			{
				return;
			}

			float cached = Mathf.Clamp01((float)hp / maxHp);
			XUiV_Sprite bar = FindSprite(entry, "BarHealth");
			if (bar != null && bar.Fill + 0.05f < cached)
			{
				bar.Fill = cached;
			}
		}

		internal static void ApplyVitalsFill(XUiC_PartyEntry entry, int entityId)
		{
			if (entry == null || entityId <= 0 || !InstanceSync.TryVitals(entityId, out int health, out int maxHealth, out _))
			{
				return;
			}

			XUiV_Sprite healthBar = FindSprite(entry, "BarHealth");
			if (healthBar != null && maxHealth > 0)
			{
				healthBar.Fill = (float)health / maxHealth;
			}

		}

		internal static void ApplyMemberArmour(XUiC_PartyEntry entry)
		{
			if (entry == null)
			{
				return;
			}

			int id = entry.Player != null ? entry.Player.entityId : FallbackId(entry);
			if (id <= 0)
			{
				return;
			}

			XUiV_Sprite armourBar = FindSprite(entry, "BarArmour");
			if (armourBar == null)
			{
				return;
			}

			bool away = entry.Player == null || Away(entry.xui?.playerUI?.entityPlayer, entry.Player, id);
			if (away)
			{
				if (InstanceSync.TryVitals(id, out _, out _, out float pct))
				{
					armourBar.Fill = pct;
				}

				return;
			}

			armourBar.Fill = InstanceSync.ArmourPercent(entry.Player);
		}

		private static XUiV_Sprite FindSprite(XUiController root, string name)
		{
			if (root == null)
			{
				return null;
			}

			if (root.ViewComponent is XUiV_Sprite sprite && root.ViewComponent.ID == name)
			{
				return sprite;
			}

			List<XUiController> children = root.Children;
			if (children == null)
			{
				return null;
			}

			for (int i = 0; i < children.Count; i++)
			{
				XUiV_Sprite found = FindSprite(children[i], name);
				if (found != null)
				{
					return found;
				}
			}

			return null;
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyEntryList), nameof(XUiC_PartyEntryList.RefreshPartyList))]
	internal static class Patch_PartyEntryList_Refresh
	{
		private static bool Prefix(XUiC_PartyEntryList __instance)
		{
			try
			{
				return PartyHud.FillList(__instance);
			}
			catch
			{
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyEntry), "GetBindingValueInternal")]
	internal static class Patch_PartyEntry_Bindings
	{
		private static bool Prefix(XUiC_PartyEntry __instance, ref string value, string bindingName, ref bool __result)
		{
			if (bindingName != "arrowcolor")
			{
				return true;
			}

			value = PartyHud.ArrowColor(__instance, PartyHud.FallbackId(__instance));
			__result = true;
			return false;
		}

		private static void Postfix(XUiC_PartyEntry __instance, ref string value, string bindingName)
		{
			PartyHud.ApplyBinding(__instance, ref value, bindingName);
		}
	}

	[HarmonyPatch(typeof(XUiC_PartyEntry), nameof(XUiC_PartyEntry.Update))]
	internal static class Patch_PartyEntry_Update
	{
		private static void Postfix(XUiC_PartyEntry __instance)
		{
			PartyHud.ApplyArrow(__instance);
			PartyHud.ApplyMemberArmour(__instance);
		}
	}
}
