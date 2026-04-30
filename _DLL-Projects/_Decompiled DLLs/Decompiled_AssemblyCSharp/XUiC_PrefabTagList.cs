using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabTagList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return EditPrefab.Tags.Test_AllSet(FastTags<TagGroup.Poi>.GetTag(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		EditPrefab.Tags |= FastTags<TagGroup.Poi>.GetTag(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		FastTags<TagGroup.Poi> tag = FastTags<TagGroup.Poi>.GetTag(_featureName);
		if (EditPrefab.Tags.Test_AnySet(tag))
		{
			EditPrefab.Tags = EditPrefab.Tags.Remove(tag);
		}
		else
		{
			EditPrefab.Tags |= tag;
		}
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllTags(groupsResult, EditPrefab);
	}
}
