using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class HideDlcSettings
{
	private const string PrefKey = "AGF.HideDLCCosmetics.HiddenPacks";
	private const string PrefKeyCosmeticList = "AGF.HideDLCCosmetics.CosmeticList";
	private const string PrefKeyUnpurchasedLegacy = "AGF.HideDLCCosmetics.HideUnpurchased";
	public const string CosmeticListShow = "Show";
	public const string CosmeticListUnpurchased = "Unpurchased";
	public const string CosmeticListAll = "AllDlc";
	private static readonly object Sync = new object();
	private static readonly HashSet<string> HiddenPacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	private static bool loaded;
	private static string cosmeticListFilter = CosmeticListShow;

	public static bool IsHidden(string packKey)
	{
		if (string.IsNullOrEmpty(packKey))
		{
			return false;
		}

		lock (Sync)
		{
			EnsureLoaded();
			return HiddenPacks.Contains(packKey);
		}
	}

	public static string CosmeticListFilter
	{
		get
		{
			lock (Sync)
			{
				EnsureLoaded();
				return cosmeticListFilter;
			}
		}
		set
		{
			lock (Sync)
			{
				EnsureLoaded();
				string normalized = NormalizeCosmeticList(value);
				if (string.Equals(cosmeticListFilter, normalized, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}

				cosmeticListFilter = normalized;
				try
				{
					PlayerPrefs.SetString(PrefKeyCosmeticList, cosmeticListFilter);
					PlayerPrefs.Save();
				}
				catch (Exception ex)
				{
					Console.WriteLine("[HideDLCCosmetics] Failed writing cosmetic list filter: " + ex.Message);
				}
			}
		}
	}

	public static string NormalizeCosmeticList(string raw)
	{
		if (string.Equals(raw, CosmeticListUnpurchased, StringComparison.OrdinalIgnoreCase))
		{
			return CosmeticListUnpurchased;
		}

		if (string.Equals(raw, CosmeticListAll, StringComparison.OrdinalIgnoreCase))
		{
			return CosmeticListAll;
		}

		return CosmeticListShow;
	}

	public static void SetHiddenKeys(IEnumerable<string> hiddenKeys)
	{
		lock (Sync)
		{
			EnsureLoaded();
			HiddenPacks.Clear();
			if (hiddenKeys != null)
			{
				foreach (string key in hiddenKeys)
				{
					if (HideDlcCatalog.TryGetPack(key, out HideDlcPack pack))
					{
						HiddenPacks.Add(pack.Key);
					}
				}
			}

			SaveUnsafe();
		}
	}

	public static string GetPreset()
	{
		lock (Sync)
		{
			EnsureLoaded();
			int hiddenCount = HiddenPacks.Count;
			if (hiddenCount <= 0)
			{
				return "ShowAll";
			}

			if (hiddenCount >= HideDlcCatalog.Packs.Length)
			{
				return "HideAll";
			}

			return "Custom";
		}
	}

	private static void EnsureLoaded()
	{
		if (loaded)
		{
			return;
		}

		loaded = true;
		try
		{
			string stored = PlayerPrefs.GetString(PrefKey, string.Empty);
			if (!string.IsNullOrEmpty(stored))
			{
				AddKeysFromCsv(stored);
			}

			string storedList = PlayerPrefs.GetString(PrefKeyCosmeticList, string.Empty);
			if (!string.IsNullOrEmpty(storedList))
			{
				cosmeticListFilter = NormalizeCosmeticList(storedList);
			}
			else if (PlayerPrefs.GetInt(PrefKeyUnpurchasedLegacy, 0) != 0)
			{
				cosmeticListFilter = CosmeticListUnpurchased;
				PlayerPrefs.SetString(PrefKeyCosmeticList, cosmeticListFilter);
				PlayerPrefs.Save();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("[HideDLCCosmetics] Failed reading settings: " + ex.Message);
		}
	}

	private static void SaveUnsafe()
	{
		try
		{
			StringBuilder sb = new StringBuilder();
			bool first = true;
			foreach (string key in HiddenPacks)
			{
				if (!first)
				{
					sb.Append(',');
				}

				sb.Append(key);
				first = false;
			}

			PlayerPrefs.SetString(PrefKey, sb.ToString());
			PlayerPrefs.Save();
		}
		catch (Exception ex)
		{
			Console.WriteLine("[HideDLCCosmetics] Failed writing settings: " + ex.Message);
		}
	}

	private static void AddKeysFromCsv(string csv)
	{
		string[] parts = csv.Split(new[] { ',', ';', '|', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < parts.Length; i++)
		{
			string key = parts[i].Trim();
			if (HideDlcCatalog.TryGetPack(key, out HideDlcPack pack))
			{
				HiddenPacks.Add(pack.Key);
			}
		}
	}
}
