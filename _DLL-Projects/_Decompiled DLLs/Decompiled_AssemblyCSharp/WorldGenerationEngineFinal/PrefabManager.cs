using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UniLinq;
using UnityEngine;

namespace WorldGenerationEngineFinal;

public class PrefabManager
{
	public class POIWeightData
	{
		public string PartialPOIName;

		public FastTags<TagGroup.Poi> Tags;

		public FastTags<TagGroup.Poi> BiomeTags;

		public float Weight;

		public float Bias;

		public int MinCount;

		public int MaxCount;

		public POIWeightData(string _partialPOIName, FastTags<TagGroup.Poi> _tags, FastTags<TagGroup.Poi> _biomeTags, float _weight, float _bias, int minCount, int maxCount)
		{
			PartialPOIName = _partialPOIName.ToLower();
			Tags = _tags;
			BiomeTags = _biomeTags;
			Weight = _weight;
			Bias = _bias;
			MinCount = minCount;
			MaxCount = maxCount;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly PrefabManagerData prefabManagerData = new PrefabManagerData();

	public readonly Dictionary<string, int> StreetTilesUsed = new Dictionary<string, int>();

	public readonly List<PrefabDataInstance> UsedPrefabsWorld = new List<PrefabDataInstance>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, int> WorldUsedPrefabNames = new Dictionary<string, int>();

	public int PrefabInstanceId;

	public PrefabManager(WorldBuilder _worldBuilder)
	{
		worldBuilder = _worldBuilder;
	}

	public IEnumerator LoadPrefabs()
	{
		ClearDisplayed();
		yield return prefabManagerData.LoadPrefabs();
	}

	public void ShufflePrefabData(int _seed)
	{
		prefabManagerData.ShufflePrefabData(_seed);
	}

	public void Clear()
	{
		StreetTilesUsed.Clear();
	}

	public void ClearDisplayed()
	{
		UsedPrefabsWorld.Clear();
		WorldUsedPrefabNames.Clear();
	}

	public void Cleanup()
	{
		prefabManagerData.Cleanup();
		ClearDisplayed();
	}

	public static bool isSizeValid(PrefabData prefab, Vector2i minSize, Vector2i maxSize)
	{
		if (maxSize == default(Vector2i) || (prefab.size.x <= maxSize.x && prefab.size.z <= maxSize.y) || (prefab.size.z <= maxSize.x && prefab.size.x <= maxSize.y))
		{
			if (!(minSize == default(Vector2i)) && (prefab.size.x < minSize.x || prefab.size.z < minSize.y))
			{
				if (prefab.size.z >= minSize.x)
				{
					return prefab.size.x >= minSize.y;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isThemeValid(PrefabData prefab, Vector2i prefabPos, List<PrefabDataInstance> prefabInstances, int distance)
	{
		if (prefab.ThemeTags.IsEmpty)
		{
			return true;
		}
		prefabPos.x -= worldBuilder.WorldSize / 2;
		prefabPos.y -= worldBuilder.WorldSize / 2;
		int num = distance * distance;
		foreach (PrefabDataInstance prefabInstance in prefabInstances)
		{
			if (!prefabInstance.prefab.ThemeTags.IsEmpty && prefabInstance.prefab.ThemeTags.Test_AnySet(prefab.ThemeTags) && Vector2i.DistanceSqr(prefabInstance.CenterXZ, prefabPos) < (float)num)
			{
				return false;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isNameValid(PrefabData prefab, Vector2i prefabPos, List<PrefabDataInstance> prefabInstances, int distance)
	{
		prefabPos.x -= worldBuilder.WorldSize / 2;
		prefabPos.y -= worldBuilder.WorldSize / 2;
		int num = distance * distance;
		foreach (PrefabDataInstance prefabInstance in prefabInstances)
		{
			if (!(prefabInstance.prefab.Name != prefab.Name) && Vector2i.DistanceSqr(prefabInstance.CenterXZ, prefabPos) < (float)num)
			{
				return false;
			}
		}
		return true;
	}

	public PrefabData GetPrefabWithDistrict(District _district, FastTags<TagGroup.Poi> _markerTags, Vector2i minSize, Vector2i maxSize, Vector2i center, float densityPointsLeft, float _distanceScale)
	{
		bool flag = !_district.tag.IsEmpty;
		bool flag2 = !_markerTags.IsEmpty;
		PrefabData result = null;
		float num = float.MinValue;
		int worldSizeDistDiv = worldBuilder.WorldSizeDistDiv;
		for (int i = 0; i < prefabManagerData.prefabDataList.Count; i++)
		{
			PrefabData prefabData = prefabManagerData.prefabDataList[i];
			if (prefabData.DensityScore > densityPointsLeft || prefabData.Tags.Test_AnySet(prefabManagerData.PartsAndTilesTags) || (flag && !prefabData.Tags.Test_AllSet(_district.tag)))
			{
				continue;
			}
			if (flag2)
			{
				if (!prefabData.Tags.Test_AnySet(_markerTags))
				{
					continue;
				}
			}
			else if (prefabData.Tags.Test_AnySet(prefabManagerData.HasTags))
			{
				continue;
			}
			if (!isSizeValid(prefabData, minSize, maxSize))
			{
				continue;
			}
			int num2 = prefabData.ThemeRepeatDistance;
			if (prefabData.ThemeTags.Test_AnySet(prefabManagerData.TraderTags))
			{
				num2 /= worldSizeDistDiv;
			}
			if (isThemeValid(prefabData, center, UsedPrefabsWorld, num2) && (!(_distanceScale > 0f) || isNameValid(prefabData, center, UsedPrefabsWorld, (int)((float)prefabData.DuplicateRepeatDistance * _distanceScale))))
			{
				float scoreForPrefab = getScoreForPrefab(prefabData, center);
				if (scoreForPrefab > num)
				{
					num = scoreForPrefab;
					result = prefabData;
				}
			}
		}
		return result;
	}

	public PrefabData GetWildernessPrefab(FastTags<TagGroup.Poi> _withoutTags, FastTags<TagGroup.Poi> _markerTags, Vector2i minSize = default(Vector2i), Vector2i maxSize = default(Vector2i), Vector2i center = default(Vector2i), bool _isRetry = false)
	{
		PrefabData prefabData = null;
		float num = float.MinValue;
		for (int i = 0; i < prefabManagerData.prefabDataList.Count; i++)
		{
			PrefabData prefabData2 = prefabManagerData.prefabDataList[i];
			if (!prefabData2.Tags.Test_AnySet(prefabManagerData.PartsAndTilesTags) && (prefabData2.Tags.Test_AnySet(prefabManagerData.WildernessTags) || prefabData2.Tags.Test_AnySet(prefabManagerData.TraderTags)) && (_markerTags.IsEmpty || prefabData2.Tags.Test_AnySet(_markerTags) || prefabData2.ThemeTags.Test_AnySet(_markerTags)) && isSizeValid(prefabData2, minSize, maxSize) && isThemeValid(prefabData2, center, UsedPrefabsWorld, prefabData2.ThemeRepeatDistance) && (_isRetry || isNameValid(prefabData2, center, UsedPrefabsWorld, prefabData2.DuplicateRepeatDistance)))
			{
				float scoreForPrefab = getScoreForPrefab(prefabData2, center);
				if (scoreForPrefab > num)
				{
					num = scoreForPrefab;
					prefabData = prefabData2;
				}
			}
		}
		if (prefabData == null && !_isRetry)
		{
			return GetWildernessPrefab(_withoutTags, _markerTags, minSize, maxSize, center, _isRetry: true);
		}
		return prefabData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int getRandomVal(int min, int maxExclusive, int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		int result = gameRandom.RandomRange(min, maxExclusive);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		return result;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float getRandomVal(float min, float max, int seed)
	{
		GameRandom gameRandom = GameRandomManager.Instance.CreateGameRandom(seed);
		float result = gameRandom.RandomRange(min, max);
		GameRandomManager.Instance.FreeGameRandom(gameRandom);
		return result;
	}

	public PrefabData GetPrefabByName(string _lowerCaseName)
	{
		if (!prefabManagerData.AllPrefabDatas.TryGetValue(_lowerCaseName.ToLower(), out var value))
		{
			return null;
		}
		return value;
	}

	public PrefabData GetStreetTile(string _lowerCaseName, Vector2i centerPoint, bool useExactString = false)
	{
		GameRandom rnd = GameRandomManager.Instance.CreateGameRandom(worldBuilder.Seed + (centerPoint.x + centerPoint.x * centerPoint.y * centerPoint.y));
		string text = (from c in prefabManagerData.AllPrefabDatas.Keys
			where ((useExactString && c.Equals(_lowerCaseName)) || (!useExactString && c.StartsWith(_lowerCaseName))) && (!PrefabManagerStatic.TileMinMaxCounts.TryGetValue(c, out var value) || !StreetTilesUsed.TryGetValue(c, out var value2) || value2 < value.y)
			orderby (float)(PrefabManagerStatic.TileMinMaxCounts.TryGetValue(c, out var value) ? value.x : 0) + rnd.RandomRange(0f, 1f) descending
			select c).FirstOrDefault();
		GameRandomManager.Instance.FreeGameRandom(rnd);
		if (text == null)
		{
			Log.Warning("Tile starting with " + _lowerCaseName + " not found!");
			return null;
		}
		return prefabManagerData.AllPrefabDatas[text];
	}

	public bool SavePrefabData(Stream _stream)
	{
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.CreateXmlDeclaration();
			XmlElement node = xmlDocument.AddXmlElement("prefabs");
			for (int i = 0; i < UsedPrefabsWorld.Count; i++)
			{
				PrefabDataInstance prefabDataInstance = UsedPrefabsWorld[i];
				if (prefabDataInstance != null)
				{
					string value = "";
					if (prefabDataInstance.prefab != null && prefabDataInstance.prefab.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabDataInstance.prefab.location.Name;
					}
					else if (prefabDataInstance.location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						value = prefabDataInstance.location.Name;
					}
					node.AddXmlElement("decoration").SetAttrib("type", "model").SetAttrib("name", value)
						.SetAttrib("position", prefabDataInstance.boundingBoxPosition.ToStringNoBlanks())
						.SetAttrib("rotation", prefabDataInstance.rotation.ToString());
				}
			}
			xmlDocument.Save(_stream);
			return true;
		}
		catch (Exception e)
		{
			Log.Exception(e);
			return false;
		}
	}

	public void GetPrefabsAround(Vector3 _position, float _distance, Dictionary<int, PrefabDataInstance> _prefabs)
	{
		for (int i = 0; i < UsedPrefabsWorld.Count; i++)
		{
			_ = UsedPrefabsWorld[i];
			_prefabs[UsedPrefabsWorld[i].id] = UsedPrefabsWorld[i];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float getScoreForPrefab(PrefabData prefab, Vector2i center)
	{
		float num = 1f;
		float num2 = 1f;
		FastTags<TagGroup.Poi> other = FastTags<TagGroup.Poi>.Parse(worldBuilder.GetBiome(center).ToString());
		POIWeightData pOIWeightData = null;
		for (int i = 0; i < PrefabManagerStatic.prefabWeightData.Count; i++)
		{
			POIWeightData pOIWeightData2 = PrefabManagerStatic.prefabWeightData[i];
			bool flag = pOIWeightData2.PartialPOIName.Length > 0 && prefab.Name.Contains(pOIWeightData2.PartialPOIName, StringComparison.OrdinalIgnoreCase);
			if (flag && !pOIWeightData2.BiomeTags.IsEmpty && !pOIWeightData2.BiomeTags.Test_AnySet(other))
			{
				return float.MinValue;
			}
			if (flag || (!pOIWeightData2.Tags.IsEmpty && ((!prefab.Tags.IsEmpty && prefab.Tags.Test_AnySet(pOIWeightData2.Tags)) || (!prefab.ThemeTags.IsEmpty && prefab.ThemeTags.Test_AnySet(pOIWeightData2.Tags)))))
			{
				pOIWeightData = pOIWeightData2;
				break;
			}
		}
		if (pOIWeightData != null)
		{
			num2 = pOIWeightData.Weight;
			num += pOIWeightData.Bias;
			int value;
			int num3 = (WorldUsedPrefabNames.TryGetValue(prefab.Name, out value) ? value : 0);
			if (num3 < pOIWeightData.MinCount)
			{
				num += (float)(pOIWeightData.MinCount - num3);
			}
			if (WorldUsedPrefabNames.TryGetValue(prefab.Name, out var value2) && value2 >= pOIWeightData.MaxCount)
			{
				num2 = 0f;
			}
		}
		num += (float)prefab.DifficultyTier / 5f;
		if (WorldUsedPrefabNames.TryGetValue(prefab.Name, out var value3))
		{
			num /= (float)value3 + 1f;
		}
		return num * num2;
	}

	public void AddUsedPrefab(string prefabName)
	{
		if (WorldUsedPrefabNames.TryGetValue(prefabName, out var value))
		{
			WorldUsedPrefabNames[prefabName] = value + 1;
		}
		else
		{
			WorldUsedPrefabNames.Add(prefabName, 1);
		}
	}

	public void AddUsedPrefabWorld(int townshipID, PrefabDataInstance pdi)
	{
		UsedPrefabsWorld.Add(pdi);
		AddUsedPrefab(pdi.prefab.Name);
	}
}
