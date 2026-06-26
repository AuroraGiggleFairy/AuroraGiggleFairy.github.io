using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabThemeTagList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return EditPrefab.ThemeTags.Test_AllSet(FastTags<TagGroup.Poi>.GetTag(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		EditPrefab.ThemeTags |= FastTags<TagGroup.Poi>.GetTag(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		FastTags<TagGroup.Poi> tag = FastTags<TagGroup.Poi>.GetTag(_featureName);
		if (EditPrefab.ThemeTags.Test_AnySet(tag))
		{
			EditPrefab.ThemeTags = EditPrefab.ThemeTags.Remove(tag);
		}
		else
		{
			EditPrefab.ThemeTags |= tag;
		}
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllThemeTags(groupsResult, EditPrefab);
	}
}
