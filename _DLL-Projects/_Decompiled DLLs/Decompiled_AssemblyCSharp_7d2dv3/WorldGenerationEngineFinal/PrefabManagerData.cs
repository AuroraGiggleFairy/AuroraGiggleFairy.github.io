using System;
using System.Collections.Generic;

namespace WorldGenerationEngineFinal;

public class PrefabManagerData
{
	public readonly Dictionary<string, PrefabData> AllPrefabDatas = new Dictionary<string, PrefabData>();

	public readonly List<PrefabData> prefabDataList = new List<PrefabData>();

	public readonly FastTags<TagGroup.Poi> PartsAndTilesTags = FastTags<TagGroup.Poi>.Parse("streettile,part");

	public readonly FastTags<TagGroup.Poi> WildernessTags = FastTags<TagGroup.Poi>.Parse("wilderness");

	public readonly FastTags<TagGroup.Poi> TraderTags = FastTags<TagGroup.Poi>.Parse("trader");

	public readonly FastTags<TagGroup.Poi> HasTags = FastTags<TagGroup.Poi>.Parse("has");

	[PublicizedFrom(EAccessModifier.Private)]
	public int previewSeed;

	public void LoadPrefabs()
	{
		if (AllPrefabDatas.Count != 0)
		{
			return;
		}
		MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
		List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList(null, _ignoreDuplicateNames: true);
		FastTags<TagGroup.Poi> other = FastTags<TagGroup.Poi>.Parse("navonly,devonly,testonly,biomeonly");
		for (int i = 0; i < availablePathsList.Count; i++)
		{
			PathAbstractions.AbstractedLocation location = availablePathsList[i];
			int num = location.Folder.LastIndexOf("/Prefabs/");
			if (num >= 0 && location.Folder.Substring(num + 8, 5).EqualsCaseInsensitive("/test"))
			{
				continue;
			}
			PrefabData prefabData = PrefabData.LoadPrefabData(location);
			try
			{
				if (prefabData != null && !prefabData.Tags.Test_AnySet(other) && !prefabData.Tags.IsEmpty)
				{
					AllPrefabDatas[location.Name.ToLower()] = prefabData;
				}
			}
			catch (Exception)
			{
				Log.Error("Could not load prefab data for " + location.Name);
			}
		}
		Log.Out("LoadPrefabs {0} of {1} in {2} s", AllPrefabDatas.Count, availablePathsList.Count, (float)microStopwatch.ElapsedMilliseconds * 0.001f);
	}

	public void ShufflePrefabData(int _seed)
	{
		prefabDataList.Clear();
		AllPrefabDatas.CopyValuesTo(prefabDataList);
		prefabDataList.Shuffle(_seed);
	}

	public void Cleanup()
	{
		AllPrefabDatas.Clear();
		prefabDataList.Clear();
	}

	public Prefab GetPreviewPrefabWithAnyTags(FastTags<TagGroup.Poi> _tags, int _townshipId, Vector2i size = default(Vector2i), bool useAnySizeSmaller = false)
	{
		Vector2i minSize = (useAnySizeSmaller ? Vector2i.zero : size);
		List<PrefabData> list = prefabDataList.FindAll([PublicizedFrom(EAccessModifier.Internal)] (PrefabData _pd) => !_pd.Tags.Test_AnySet(PartsAndTilesTags) && PrefabManager.isSizeValid(_pd, minSize, size) && _pd.Tags.Test_AnySet(_tags));
		if (list.Count == 0)
		{
			return null;
		}
		list.Shuffle(previewSeed);
		Prefab prefab = new Prefab();
		prefab.Load(list[0].location);
		previewSeed++;
		return prefab;
	}
}
