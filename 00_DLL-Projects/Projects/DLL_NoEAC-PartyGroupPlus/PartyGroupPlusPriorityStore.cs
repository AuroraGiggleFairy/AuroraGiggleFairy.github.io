using System;
using System.Collections.Generic;
using System.IO;

namespace PartyGroupPlus
{
	public static class PartyGroupPlusPriorityStore
	{
		static readonly HashSet<string> Keys = new HashSet<string>(StringComparer.Ordinal);
		static bool Loaded;

		public static event Action Changed;

		public static void Load()
		{
			if (Loaded)
			{
				return;
			}

			Loaded = true;
			Keys.Clear();
			try
			{
				string path = StorePath();
				if (string.IsNullOrEmpty(path) || !File.Exists(path))
				{
					return;
				}

				string[] lines = File.ReadAllLines(path);
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i] != null ? lines[i].Trim() : string.Empty;
					if (line.Length > 0)
					{
						Keys.Add(line);
					}
				}
			}
			catch
			{
			}
		}

		public static bool IsPriority(EntityPlayer player)
		{
			Load();
			CollectKeys(player, out string combined, out string nameKey);
			if (!string.IsNullOrEmpty(combined) && Keys.Contains(combined))
			{
				return true;
			}

			return !string.IsNullOrEmpty(nameKey) && Keys.Contains(nameKey);
		}

		public static void SetPriority(EntityPlayer player, bool priority)
		{
			Load();
			CollectKeys(player, out string combined, out string nameKey);
			string primary = !string.IsNullOrEmpty(combined) ? combined : nameKey;
			if (string.IsNullOrEmpty(primary))
			{
				return;
			}

			bool changed = false;
			if (priority)
			{
				changed |= Keys.Add(primary);
				if (!string.IsNullOrEmpty(combined) && !string.IsNullOrEmpty(nameKey))
				{
					changed |= Keys.Remove(nameKey);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(combined))
				{
					changed |= Keys.Remove(combined);
				}

				if (!string.IsNullOrEmpty(nameKey))
				{
					changed |= Keys.Remove(nameKey);
				}
			}

			if (!changed)
			{
				return;
			}

			Save();
			PartyGroupPlusHud.PriorityDirty = true;
			Changed?.Invoke();
			if (PartyGroupPlusHud.CachedList != null)
			{
				PartyGroupPlusHud.CachedList.RefreshPartyList();
			}
		}

		static void CollectKeys(EntityPlayer player, out string combined, out string nameKey)
		{
			combined = null;
			nameKey = null;
			if (player == null)
			{
				return;
			}

			try
			{
				PersistentPlayerData data = GameManager.Instance?.persistentPlayers?.GetPlayerDataFromEntityID(player.entityId);
				string id = data != null && data.PrimaryId != null ? data.PrimaryId.CombinedString : null;
				if (!string.IsNullOrEmpty(id))
				{
					combined = id;
				}
			}
			catch
			{
			}

			string name = player.PlayerDisplayName;
			if (!string.IsNullOrEmpty(name))
			{
				nameKey = "name:" + name;
			}
		}

		static void Save()
		{
			try
			{
				string path = StorePath();
				if (string.IsNullOrEmpty(path))
				{
					return;
				}

				string[] lines = new string[Keys.Count];
				Keys.CopyTo(lines);
				File.WriteAllLines(path, lines);
			}
			catch
			{
			}
		}

		static string StorePath()
		{
			string dir = GameIO.GetUserGameDataDir();
			if (string.IsNullOrEmpty(dir))
			{
				return null;
			}

			return Path.Combine(dir, "PartyGroupPlusPriority.txt");
		}
	}
}
