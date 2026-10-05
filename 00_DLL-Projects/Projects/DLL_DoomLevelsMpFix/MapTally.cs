using System.Collections.Generic;
using DoomLevels;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Scripting;

namespace DoomLevelsMpFix
{
	/// <summary>
	/// Kills, items, and secrets live on the server instance. This copies the six
	/// numbers to the machine that draws the map, only when they change.
	/// </summary>
	internal static class MapTally
	{
		private const float TickSeconds = 0.5f;

		internal static bool Active;
		internal static int Kills;
		internal static int Items;
		internal static int Secrets;
		internal static int TotalKills;
		internal static int TotalItems;
		internal static int TotalSecrets;
		internal static string Map = "";
		internal static int Par;
		internal static int Seconds;
		internal static float SyncedAt;
		internal static Vector3i Origin;
		internal static Vector3i Size;

		internal static float Elapsed => Seconds + Mathf.Max(0f, Time.unscaledTime - SyncedAt);

		private static float _next;
		private static readonly Dictionary<int, string> Last = new Dictionary<int, string>();
		private static readonly List<int> Drop = new List<int>();

		private static XUiController _tracker;
		private static bool _vanillaHeld;

		internal static void SyncTracker()
		{
			XUi xui = LocalPlayerUI.primaryUI?.xui;
			if (_tracker == null || _tracker.ViewComponent == null)
			{
				_tracker = xui?.FindWindowGroupByName("toolbelt")?.GetChildById("windowDoomTracker");
			}

			if (_tracker?.ViewComponent != null && _tracker.ViewComponent.IsVisible != Active)
			{
				_tracker.ViewComponent.IsVisible = Active;
			}

			if (Active)
			{
				_vanillaHeld = true;
				HoldVanilla(xui);
				return;
			}

			if (!_vanillaHeld)
			{
				return;
			}

			_vanillaHeld = false;
			Release(xui?.GetWindow("windowQuestTracker"));
			Release(xui?.GetWindow("windowRecipeTracker"));
		}

		internal static void HoldVanilla(XUi xui)
		{
			Hide(xui?.GetWindow("windowQuestTracker"));
			Hide(xui?.GetWindow("windowRecipeTracker"));
		}

		private static void Hide(XUiV_Window window)
		{
			if (window != null && window.IsVisible)
			{
				window.IsVisible = false;
			}
		}

		private static void Release(XUiV_Window window)
		{
			window?.Controller?.RefreshBindings();
		}

		internal static void Reset()
		{
			_tracker = null;
			_vanillaHeld = false;
			Active = false;
			Kills = Items = Secrets = 0;
			TotalKills = TotalItems = TotalSecrets = 0;
			Map = "";
			Par = Seconds = 0;
			SyncedAt = 0f;
			Origin = Vector3i.zero;
			Size = Vector3i.zero;
			Last.Clear();
		}

		internal static void Apply(bool active, int kills, int items, int secrets,
			int totalKills, int totalItems, int totalSecrets, string map, int par, int seconds,
			Vector3i origin, Vector3i size)
		{
			Active = active;
			Kills = kills;
			Items = items;
			Secrets = secrets;
			TotalKills = totalKills;
			TotalItems = totalItems;
			TotalSecrets = totalSecrets;
			Map = map ?? "";
			Par = par;
			Seconds = seconds;
			SyncedAt = Time.unscaledTime;
			Origin = origin;
			Size = size;
		}

		internal static void Tick()
		{
			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			if (net == null || !net.IsServer || Time.unscaledTime < _next)
			{
				return;
			}

			_next = Time.unscaledTime + TickSeconds;
			World world = GameManager.Instance?.World;
			var players = world?.Players?.list;
			if (players == null)
			{
				return;
			}

			for (int i = 0; i < players.Count; i++)
			{
				EntityPlayer player = players[i];
				if (player == null)
				{
					continue;
				}

				Stats stats = Instances.StatsFor(player.entityId);
				bool active = stats != null && Instances.IsInside(player.entityId);
				Vector3i origin = Vector3i.zero;
				Vector3i size = Vector3i.zero;
				Level level;
				int cell;
				if (active && Instances.TryGet(player.entityId, out origin, out level, out cell) && level != null)
				{
					size = level.Size;
				}

				string map = active ? Instances.MapFor(player.entityId) ?? "" : "";
				string key = active
					? map + "," + origin.x + "," + origin.z + "," + stats.Kills + "," + stats.Items + "," +
					  stats.Secrets + "," + stats.TotalKills + "," + stats.TotalItems + "," + stats.TotalSecrets + "," +
					  stats.Par
					: "off";
				string prev;
				if (Last.TryGetValue(player.entityId, out prev) && prev == key)
				{
					continue;
				}

				Last[player.entityId] = key;
				Publish(net, player, active, stats);
			}

			if (Last.Count == players.Count)
			{
				return;
			}

			Drop.Clear();
			foreach (KeyValuePair<int, string> entry in Last)
			{
				bool seen = false;
				for (int i = 0; i < players.Count; i++)
				{
					if (players[i] != null && players[i].entityId == entry.Key)
					{
						seen = true;
						break;
					}
				}

				if (!seen)
				{
					Drop.Add(entry.Key);
				}
			}

			for (int i = 0; i < Drop.Count; i++)
			{
				Last.Remove(Drop[i]);
			}
		}

		private static void Publish(ConnectionManager net, EntityPlayer player, bool active, Stats stats)
		{
			int kills = active ? stats.Kills : 0;
			int items = active ? stats.Items : 0;
			int secrets = active ? stats.Secrets : 0;
			int totalKills = active ? stats.TotalKills : 0;
			int totalItems = active ? stats.TotalItems : 0;
			int totalSecrets = active ? stats.TotalSecrets : 0;
			string map = active ? Instances.MapFor(player.entityId) ?? "" : "";
			int par = active ? stats.Par : 0;
			int seconds = active ? Mathf.FloorToInt(stats.Seconds) : 0;
			Vector3i origin = Vector3i.zero;
			Vector3i size = Vector3i.zero;
			Level level;
			int cell;
			if (active && Instances.TryGet(player.entityId, out origin, out level, out cell) && level != null)
			{
				size = level.Size;
			}

			List<int> lines = active ? RunStats.LinesFor(player.entityId) : null;
			if (player is EntityPlayerLocal)
			{
				Apply(active, kills, items, secrets, totalKills, totalItems, totalSecrets, map, par, seconds, origin, size);
				MapExplore.Show(map, lines);
				return;
			}

			net.SendPackage(PackageEmit.Take<NetPackageDoomMapTally>()
				.Setup(active, kills, items, secrets, totalKills, totalItems, totalSecrets, map, par, seconds, origin, size),
				false, player.entityId);
			if (active)
			{
				net.SendPackage(PackageEmit.Take<NetPackageDoomMapPaint>()
					.Setup(map, lines), false, player.entityId);
			}
		}
	}

	[Preserve]
	public abstract class NetPackageDoomMapTally : NetPackage
	{
		private bool _active;
		private int _kills;
		private int _items;
		private int _secrets;
		private int _totalKills;
		private int _totalItems;
		private int _totalSecrets;
		private string _map = "";
		private int _par;
		private int _seconds;
		private Vector3i _origin;
		private Vector3i _size;

		public override NetPackageDirection PackageDirection => NetPackageDirection.ToClient;

		public NetPackageDoomMapTally Setup(bool active, int kills, int items, int secrets,
			int totalKills, int totalItems, int totalSecrets, string map, int par, int seconds,
			Vector3i origin, Vector3i size)
		{
			_active = active;
			_kills = kills;
			_items = items;
			_secrets = secrets;
			_totalKills = totalKills;
			_totalItems = totalItems;
			_totalSecrets = totalSecrets;
			_map = map ?? "";
			_par = par;
			_seconds = seconds;
			_origin = origin;
			_size = size;
			return this;
		}

		public override void read(PooledBinaryReader _br)
		{
			_active = _br.ReadBoolean();
			_kills = _br.ReadInt32();
			_items = _br.ReadInt32();
			_secrets = _br.ReadInt32();
			_totalKills = _br.ReadInt32();
			_totalItems = _br.ReadInt32();
			_totalSecrets = _br.ReadInt32();
			_map = _br.ReadString();
			_par = _br.ReadInt32();
			_seconds = _br.ReadInt32();
			_origin = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
			_size = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());
		}

		public override void write(PooledBinaryWriter _bw)
		{
			base.write(_bw);
			System.IO.BinaryWriter writer = (System.IO.BinaryWriter)_bw;
			writer.Write(_active);
			writer.Write(_kills);
			writer.Write(_items);
			writer.Write(_secrets);
			writer.Write(_totalKills);
			writer.Write(_totalItems);
			writer.Write(_totalSecrets);
			writer.Write(_map);
			writer.Write(_par);
			writer.Write(_seconds);
			writer.Write(_origin.x);
			writer.Write(_origin.y);
			writer.Write(_origin.z);
			writer.Write(_size.x);
			writer.Write(_size.y);
			writer.Write(_size.z);
		}

		public override void ProcessPackage(World _world, GameManager _callbacks)
		{
			MapTally.Apply(_active, _kills, _items, _secrets, _totalKills, _totalItems, _totalSecrets,
				_map, _par, _seconds, _origin, _size);
		}

		public int Length()
		{
			return 32;
		}
	}

	[Preserve]
	public class XUiC_DoomMapTally : XUiController
	{
		private const string Cream = "E6DCC8";
		private const string Red = "DC2020";
		private const string Green = "3CC85A";

		private XUiV_Label _levelName;
		private XUiV_Label _numKills;
		private XUiV_Label _numItems;
		private XUiV_Label _numSecrets;
		private XUiV_Label _numTime;
		private XUiV_Label _numPar;
		private string _shownTitle;
		private string _shownKills;
		private string _shownItems;
		private string _shownSecrets;
		private string _shownTime;
		private string _shownPar;

		public override void Init()
		{
			base.Init();
			SetName("nameKills", "doom_kills");
			SetName("nameItems", "doom_items");
			SetName("nameSecrets", "doom_secrets");
			SetName("nameTime", "doom_time");
			SetName("namePar", "doom_par");
			_levelName = Label("levelName");
			_numKills = Label("numKills");
			_numItems = Label("numItems");
			_numSecrets = Label("numSecrets");
			_numTime = Label("numTime");
			_numPar = Label("numPar");
			if (ViewComponent != null)
			{
				ViewComponent.IsVisible = false;
			}
		}

		public override void Update(float _dt)
		{
			bool show = MapTally.Active;
			if (ViewComponent != null && ViewComponent.IsVisible != show)
			{
				ViewComponent.IsVisible = show;
				if (show)
				{
					_shownKills = null;
					_shownTitle = null;
					_shownTime = null;
					_shownPar = null;
				}
			}

			if (!show)
			{
				return;
			}

			WritePlain(_levelName, ref _shownTitle, Title(MapTally.Map));
			Write(_numKills, ref _shownKills, MapTally.Kills, MapTally.TotalKills);
			Write(_numItems, ref _shownItems, MapTally.Items, MapTally.TotalItems);
			Write(_numSecrets, ref _shownSecrets, MapTally.Secrets, MapTally.TotalSecrets);
			WritePlain(_numTime, ref _shownTime, Stats.Clock(MapTally.Elapsed));
			WritePlain(_numPar, ref _shownPar, Stats.Clock(MapTally.Par));
			if (ViewComponent != null && ViewComponent.ID == "windowDoomTracker")
			{
				MapTally.HoldVanilla(xui);
			}

			base.Update(_dt);
		}

		private static string Title(string map)
		{
			if (string.IsNullOrEmpty(map))
			{
				return "";
			}

			string key = "doom_" + map.ToLowerInvariant() + "_name";
			string text = Localization.Get(key);
			return string.IsNullOrEmpty(text) || text == key ? map : text;
		}

		private void SetName(string id, string key)
		{
			XUiV_Label label = Label(id);
			if (label != null)
			{
				label.Text = Localization.Get(key);
			}
		}

		private XUiV_Label Label(string id)
		{
			return GetChildById(id)?.ViewComponent as XUiV_Label;
		}

		private static void Write(XUiV_Label label, ref string shown, int current, int max)
		{
			if (label == null)
			{
				return;
			}

			bool done = current >= max;
			string curColor = done ? Green : Cream;
			string maxColor = done ? Green : Red;
			string text = "[" + curColor + "]" + current.ToString().PadLeft(3) + "[-] / [" + maxColor + "]" + max.ToString().PadLeft(3) + "[-]";
			if (shown == text)
			{
				return;
			}

			shown = text;
			label.Text = text;
		}

		private static void WritePlain(XUiV_Label label, ref string shown, string text)
		{
			if (label == null || shown == text)
			{
				return;
			}

			shown = text;
			label.Text = text;
		}
	}

	[HarmonyPatch(typeof(XUiC_QuestTrackerWindow), "GetBindingValueInternal")]
	internal static class Patch_HideQuestTrackerBinding
	{
		private static void Postfix(ref string value, string bindingName)
		{
			if (MapTally.Active && bindingName == "showquest")
			{
				value = "false";
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_QuestTrackerWindow), nameof(XUiC_QuestTrackerWindow.Update))]
	internal static class Patch_HideQuestTracker
	{
		private static void Postfix(XUiC_QuestTrackerWindow __instance)
		{
			if (MapTally.Active && __instance?.ViewComponent != null)
			{
				__instance.ViewComponent.IsVisible = false;
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeTrackerWindow), "GetBindingValueInternal")]
	internal static class Patch_HideRecipeTrackerBinding
	{
		private static void Postfix(ref string value, string bindingName)
		{
			if (MapTally.Active && bindingName == "showrecipe")
			{
				value = "false";
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_RecipeTrackerWindow), nameof(XUiC_RecipeTrackerWindow.Update))]
	internal static class Patch_HideRecipeTracker
	{
		private static void Postfix(XUiC_RecipeTrackerWindow __instance)
		{
			if (MapTally.Active && __instance?.ViewComponent != null)
			{
				__instance.ViewComponent.IsVisible = false;
			}
		}
	}
}
