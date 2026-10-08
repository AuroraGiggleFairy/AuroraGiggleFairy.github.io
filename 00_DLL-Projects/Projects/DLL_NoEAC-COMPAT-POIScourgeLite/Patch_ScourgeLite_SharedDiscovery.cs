using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace AGF.Compat.POIScourgeLite
{
	/// <summary>
	/// An X is stored when the center of a sleeper place is revealed on a map,
	/// then shared the way a check is. The place is one a clear can turn into a
	/// check, including tier 0 places with no quest tags. Street tiles and spawned
	/// parts are left out. A player who joins, or whose marks did not load,
	/// receives the stored list and the icons are drawn again until they show.
	/// </summary>
	[HarmonyPatch]
	public static class Patch_ScourgeLite_SharedDiscovery
	{
		const string SeenPrefix = "agf_scourge_seen_";
		const float TickSeconds = 1f;

		static readonly List<PoiSpot> Spots = new List<PoiSpot>();
		static readonly Dictionary<string, PoiSpot> ById = new Dictionary<string, PoiSpot>();
		static readonly Dictionary<long, List<PoiSpot>> ByChunk = new Dictionary<long, List<PoiSpot>>();
		static readonly HashSet<string> Shared = new HashSet<string>();
		static readonly HashSet<string> SharedClears = new HashSet<string>();
		static readonly Dictionary<string, float> ClearValues = new Dictionary<string, float>();
		static readonly HashSet<string> Shown = new HashSet<string>();
		static readonly HashSet<int> Absorbed = new HashSet<int>();
		static readonly Dictionary<int, int> CvarCount = new Dictionary<int, int>();
		static readonly List<EntityPlayer> LocalOnly = new List<EntityPlayer>(1);
		static FastTags<TagGroup.Poi> _skipTags;
		static bool _skipTagsReady;
		const int CheckRetryLimit = 40;
		const float CheckRetrySeconds = 3f;
		static int _checkRetries;
		static float _nextCheckRetry;

		static FieldInfo _masterClear;
		static MethodInfo _createMark;
		static MethodInfo _restoreMarks;
		static PropertyInfo _showChecks;
		static object _settings;
		static PropertyInfo _showSetting;
		static PropertyInfo _tier0Setting;
		static bool _hooked;
		static bool _catalogReady;
		static bool _showKnown;
		static bool _show = true;
		static float _next;
		static string _lastError;

		static bool Prepare()
		{
			if (ModManager.GetMod("POI_Scourge_Lite") == null)
			{
				return false;
			}

			Type handler = AccessTools.TypeByName("POIScourgeLite.ScourgeLiteHandler");
			_createMark = handler == null ? null : AccessTools.Method(handler, "CreateUnclearedNavObject");
			_restoreMarks = handler == null ? null : AccessTools.Method(handler, "RestoreMarkers");
			_masterClear = handler == null ? null : AccessTools.Field(handler, "MasterClearDictionary");
			if (_createMark == null)
			{
				return false;
			}

			if (!_hooked)
			{
				_hooked = true;
				ModEvents.GameUpdate.RegisterHandler(OnUpdate);
				ModEvents.WorldShuttingDown.RegisterHandler(OnWorldDown);
				Console.WriteLine("[NoEACCompatibilities] Scourge checks and Xs sync once on login, and a new X syncs when a map records it.");
			}

			return AccessTools.Method(handler, "UpdateUnclearedMarkersVisibility") != null;
		}

		static MethodBase TargetMethod()
		{
			Type handler = AccessTools.TypeByName("POIScourgeLite.ScourgeLiteHandler");
			return handler == null ? null : AccessTools.Method(handler, "UpdateUnclearedMarkersVisibility");
		}

		static bool Prefix()
		{
			return false;
		}

		static void OnWorldDown(ref ModEvents.SWorldShuttingDownData data)
		{
			Spots.Clear();
			ById.Clear();
			ByChunk.Clear();
			Shared.Clear();
			SharedClears.Clear();
			ClearValues.Clear();
			Shown.Clear();
			Absorbed.Clear();
			CvarCount.Clear();
			_checkRetries = 0;
			_nextCheckRetry = 0f;
			_catalogReady = false;
			_showKnown = false;
			_next = 0f;
		}

		static void OnUpdate(ref ModEvents.SGameUpdateData data)
		{
			if (Time.time < _next)
			{
				return;
			}

			_next = Time.time + TickSeconds;
			try
			{
				Tick();
			}
			catch (Exception ex)
			{
				if (_lastError != ex.Message)
				{
					_lastError = ex.Message;
					Console.WriteLine("[NoEACCompatibilities] Scourge X share failed: " + ex.Message);
				}
			}
		}

		static void Tick()
		{
			World world = GameManager.Instance?.World;
			List<EntityPlayer> players = world?.Players?.list;
			if (players == null)
			{
				return;
			}

			EnsureCatalog();
			if (Spots.Count == 0)
			{
				return;
			}

			bool show = ShowMarks();
			if (_showKnown && show != _show && !show)
			{
				HideShown();
			}

			_show = show;
			_showKnown = true;

			ConnectionManager net = SingletonMonoBehaviour<ConnectionManager>.Instance;
			bool server = net != null && net.IsServer;
			if (server)
			{
				for (int i = 0; i < players.Count; i++)
				{
					Consider(players[i], players);
				}
			}

			EntityPlayerLocal local = world.GetPrimaryPlayer();
			if (!server && local != null)
			{
				LocalOnly.Clear();
				LocalOnly.Add(local);
				Consider(local, LocalOnly);
			}

			DrawFromCVars(local);
			RestoreChecks(local);
		}

		static void Consider(EntityPlayer player, List<EntityPlayer> players)
		{
			if (player?.Buffs == null)
			{
				return;
			}

			if (!Absorbed.Contains(player.entityId))
			{
				Absorbed.Add(player.entityId);
				GiveFullList(player);
			}

			int count = player.Buffs.CVars.Count;
			if (!CvarCount.TryGetValue(player.entityId, out int previous))
			{
				previous = -1;
			}

			if (count > 0 && count != previous)
			{
				CvarCount[player.entityId] = count;
				AbsorbSaved(player, players);
			}

			IMapChunkDatabase map = player.ChunkObserver?.mapDatabase;
			if (map == null)
			{
				return;
			}

			for (int i = 0; i < Spots.Count; i++)
			{
				PoiSpot spot = Spots[i];
				if (Shared.Contains(spot.Id) || !map.Contains(spot.ChunkKey))
				{
					continue;
				}

				Note(spot, players);
			}
		}

		static void AbsorbSaved(EntityPlayer player, List<EntityPlayer> players)
		{
			foreach (KeyValuePair<string, float> pair in player.Buffs.CVars)
			{
				if (pair.Value <= 0f)
				{
					continue;
				}

				if (IsSavedClear(pair.Key))
				{
					ShareClear(pair.Key, pair.Value, players);
					continue;
				}

				if (!pair.Key.StartsWith(SeenPrefix, StringComparison.Ordinal))
				{
					continue;
				}

				string id = pair.Key.Substring(SeenPrefix.Length);
				if (ById.TryGetValue(id, out PoiSpot spot))
				{
					Note(spot, players);
				}
			}
		}

		static bool IsSavedClear(string key)
		{
			if (!key.StartsWith("scourge_", StringComparison.Ordinal) || key.IndexOf("signal", StringComparison.Ordinal) >= 0)
			{
				return false;
			}

			string[] parts = key.Split('_');
			return parts.Length >= 3 && int.TryParse(parts[1], out _) && int.TryParse(parts[2], out _);
		}

		static void ShareClear(string key, float value, List<EntityPlayer> players)
		{
			ClearValues[key] = value;
			RememberMaster(key, value);
			if (!SharedClears.Add(key))
			{
				return;
			}

			if (SharedClears.Count == 1)
			{
				Console.WriteLine("[NoEACCompatibilities] Existing Scourge checks are being shared.");
			}

			for (int i = 0; i < players.Count; i++)
			{
				EntityBuffs buffs = players[i]?.Buffs;
				if (buffs != null)
				{
					WriteClear(buffs, key, value);
				}
			}
		}

		static void WriteClear(EntityBuffs buffs, string key, float value)
		{
			if (buffs.GetCustomVar(key) != value)
			{
				buffs.SetCustomVar(key, value, true, CVarOperation.set, true);
			}
		}

		static void RememberMaster(string key, float value)
		{
			if (_masterClear?.GetValue(null) is IDictionary table && !table.Contains(key))
			{
				table[key] = value;
			}
		}

		static void GiveFullList(EntityPlayer player)
		{
			EntityBuffs buffs = player?.Buffs;
			if (buffs == null)
			{
				return;
			}

			if (_masterClear?.GetValue(null) is IDictionary table)
			{
				foreach (DictionaryEntry entry in table)
				{
					string key = entry.Key as string;
					float value = ValueOf(entry.Value);
					if (key == null || value <= 0f || !IsSavedClear(key))
					{
						continue;
					}

					ClearValues[key] = value;
					SharedClears.Add(key);
					WriteClear(buffs, key, value);
				}
			}

			foreach (KeyValuePair<string, float> clear in ClearValues)
			{
				if (SharedClears.Contains(clear.Key))
				{
					WriteClear(buffs, clear.Key, clear.Value);
				}
			}

			foreach (string id in Shared)
			{
				if (ById.TryGetValue(id, out PoiSpot spot) && !Cleared(spot, player))
				{
					Write(buffs, spot);
				}
			}
		}

		static void Note(PoiSpot spot, List<EntityPlayer> players)
		{
			if (!Shared.Add(spot.Id) || Cleared(spot, players))
			{
				return;
			}

			for (int i = 0; i < players.Count; i++)
			{
				EntityBuffs buffs = players[i]?.Buffs;
				if (buffs != null)
				{
					Write(buffs, spot);
				}
			}
		}

		static void Write(EntityBuffs buffs, PoiSpot spot)
		{
			float mark = spot.Tier + 1f;
			if (buffs.GetCustomVar(spot.SeenKey) != mark)
			{
				buffs.SetCustomVar(spot.SeenKey, mark, true, CVarOperation.set, true);
			}
		}

		static void DrawFromCVars(EntityPlayerLocal player)
		{
			if (player?.Buffs == null || NavObjectManager.Instance == null)
			{
				return;
			}

			foreach (KeyValuePair<string, float> pair in player.Buffs.CVars)
			{
				if (pair.Value <= 0f)
				{
					continue;
				}

				if (IsSavedClear(pair.Key))
				{
					continue;
				}

				if (!_show || !pair.Key.StartsWith(SeenPrefix, StringComparison.Ordinal))
				{
					continue;
				}

				string id = pair.Key.Substring(SeenPrefix.Length);
				if (Shown.Contains(id) || !ById.TryGetValue(id, out PoiSpot spot))
				{
					continue;
				}

				if (Cleared(spot, player))
				{
					Shown.Add(id);
					continue;
				}

				object nav = _createMark.Invoke(null, new object[] { spot.Pos, spot.Tier });
				if (nav != null)
				{
					Shown.Add(id);
				}
			}
		}

		static void RestoreChecks(EntityPlayerLocal player)
		{
			if (player?.Buffs == null || _restoreMarks == null || !ShowChecks() || _checkRetries >= CheckRetryLimit)
			{
				return;
			}

			int clears = 0;
			foreach (KeyValuePair<string, float> pair in player.Buffs.CVars)
			{
				if (pair.Value <= 0f || !IsSavedClear(pair.Key))
				{
					continue;
				}

				clears++;
				RememberMaster(pair.Key, pair.Value);
			}

			if (clears == 0 || CountCheckNavs() >= clears)
			{
				if (clears > 0)
				{
					_checkRetries = CheckRetryLimit;
				}

				return;
			}

			if (Time.time < _nextCheckRetry)
			{
				return;
			}

			_checkRetries++;
			_nextCheckRetry = Time.time + CheckRetrySeconds;
			try
			{
				_restoreMarks.Invoke(null, new object[] { player });
			}
			catch (Exception ex)
			{
				Console.WriteLine("[NoEACCompatibilities] Scourge check restore failed: " + ex.Message);
			}

			if (CountCheckNavs() >= clears)
			{
				_checkRetries = CheckRetryLimit;
			}
		}

		static int CountCheckNavs()
		{
			List<NavObject> list = NavObjectManager.Instance?.NavObjectList;
			if (list == null)
			{
				return 0;
			}

			int count = 0;
			for (int i = 0; i < list.Count; i++)
			{
				string name = list[i]?.NavObjectClass?.NavObjectClassName;
				if (name != null && name.StartsWith("scourge_marker_", StringComparison.Ordinal))
				{
					count++;
				}
			}

			return count;
		}

		static bool ShowChecks()
		{
			if (_settings == null)
			{
				Type config = AccessTools.TypeByName("POIScourgeLite.ScourgeLiteConfig");
				_settings = config == null ? null : AccessTools.Property(config, "Settings")?.GetValue(null, null);
			}

			if (_settings == null)
			{
				return true;
			}

			if (_showChecks == null)
			{
				_showChecks = AccessTools.Property(_settings.GetType(), "ShowMapMarkers");
			}

			return _showChecks?.GetValue(_settings, null) is bool value ? value : true;
		}

		static bool Cleared(PoiSpot spot, EntityPlayer player)
		{
			if (MasterCleared(spot.ClearKey))
			{
				return true;
			}

			return player?.Buffs != null && player.Buffs.GetCustomVar(spot.ClearKey) > 0f;
		}

		static bool Cleared(PoiSpot spot, List<EntityPlayer> players)
		{
			if (MasterCleared(spot.ClearKey))
			{
				return true;
			}

			for (int i = 0; i < players.Count; i++)
			{
				EntityBuffs buffs = players[i]?.Buffs;
				if (buffs != null && buffs.GetCustomVar(spot.ClearKey) > 0f)
				{
					return true;
				}
			}

			return false;
		}

		static bool MasterCleared(string key)
		{
			object raw = _masterClear?.GetValue(null);
			if (raw is IDictionary table && table.Contains(key))
			{
				return ValueOf(table[key]) > 0f;
			}

			return false;
		}

		static void HideShown()
		{
			foreach (string id in Shown)
			{
				if (ById.TryGetValue(id, out PoiSpot spot))
				{
					Hide(spot);
				}
			}

			Shown.Clear();
		}

		static void Hide(PoiSpot spot)
		{
			NavObjectManager manager = NavObjectManager.Instance;
			if (manager == null)
			{
				return;
			}

			manager.UnRegisterNavObjectByPosition(spot.Pos, "scourge_uncleared_marker_" + Mathf.Clamp(spot.Tier, 0, 6));
		}

		static void EnsureCatalog()
		{
			if (_catalogReady)
			{
				return;
			}

			DynamicPrefabDecorator decorator = Decorator();
			if (decorator == null)
			{
				return;
			}

			List<PrefabInstance> prefabs = new List<PrefabInstance>();
			decorator.GetAllPrefabs(prefabs);
			if (prefabs.Count == 0)
			{
				return;
			}

			bool tier0 = ReadFlag("ShowUnclearedTier0", true);
			Spots.Clear();
			ById.Clear();
			ByChunk.Clear();
			for (int i = 0; i < prefabs.Count; i++)
			{
				PrefabInstance item = prefabs[i];
				Prefab prefab = item?.prefab;
				if (prefab?.SleeperVolumeList == null || !prefab.SleeperVolumeList.AnyUsedEntry || !IsCheckPlace(prefab))
				{
					continue;
				}

				int tier = Mathf.Clamp(prefab.DifficultyTier, 0, 6);
				if (tier == 0 && !tier0)
				{
					continue;
				}

				Vector3 center = item.GetAABB().center;
				int x = (int)center.x;
				int z = (int)center.z;
				string id = x + "_" + z;
				if (ById.ContainsKey(id))
				{
					continue;
				}

				long chunkKey = WorldChunkCache.MakeChunkKey(World.toChunkXZ(x), World.toChunkXZ(z));
				PoiSpot spot = new PoiSpot
				{
					Id = id,
					Tier = tier,
					Pos = center,
					ChunkKey = chunkKey,
					SeenKey = SeenPrefix + id,
					ClearKey = "scourge_" + id
				};
				Spots.Add(spot);
				ById[id] = spot;
				if (!ByChunk.TryGetValue(chunkKey, out List<PoiSpot> list))
				{
					list = new List<PoiSpot>();
					ByChunk[chunkKey] = list;
				}

				list.Add(spot);
			}

			_catalogReady = Spots.Count > 0;
		}

		static bool IsCheckPlace(Prefab prefab)
		{
			if (!_skipTagsReady)
			{
				_skipTags = FastTags<TagGroup.Poi>.Parse("part,streettile");
				_skipTagsReady = true;
			}

			if (prefab.Tags.Test_AnySet(_skipTags))
			{
				return false;
			}

			string name = prefab.PrefabName;
			if (string.IsNullOrEmpty(name))
			{
				return true;
			}

			if (name.StartsWith("part_", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return name.IndexOf("rwg_tile", StringComparison.OrdinalIgnoreCase) < 0;
		}

		static DynamicPrefabDecorator Decorator()
		{
			return GameManager.Instance?.World?.ChunkCache?.ChunkProvider?.GetDynamicPrefabDecorator();
		}

		static bool ShowMarks()
		{
			return ReadFlag("ShowUnclearedMapMarkers", true);
		}

		static bool ReadFlag(string name, bool fallback)
		{
			if (_settings == null)
			{
				Type config = AccessTools.TypeByName("POIScourgeLite.ScourgeLiteConfig");
				_settings = config == null ? null : AccessTools.Property(config, "Settings")?.GetValue(null, null);
			}

			if (_settings == null)
			{
				return fallback;
			}

			PropertyInfo property = name == "ShowUnclearedMapMarkers" ? _showSetting : _tier0Setting;
			if (property == null)
			{
				property = AccessTools.Property(_settings.GetType(), name);
				if (name == "ShowUnclearedMapMarkers")
				{
					_showSetting = property;
				}
				else
				{
					_tier0Setting = property;
				}
			}

			return property?.GetValue(_settings, null) is bool value ? value : fallback;
		}

		static float ValueOf(object value)
		{
			if (value is float number)
			{
				return number;
			}

			if (value is double wide)
			{
				return (float)wide;
			}

			if (value is int whole)
			{
				return whole;
			}

			return 0f;
		}

		sealed class PoiSpot
		{
			public string Id;
			public int Tier;
			public Vector3 Pos;
			public long ChunkKey;
			public string SeenKey;
			public string ClearKey;
		}
	}
}
