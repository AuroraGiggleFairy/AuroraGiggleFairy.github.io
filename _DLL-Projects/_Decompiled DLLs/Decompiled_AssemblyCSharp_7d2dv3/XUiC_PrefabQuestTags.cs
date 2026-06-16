using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabQuestTags : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return EditPrefab.GetQuestTag(FastTags<TagGroup.Global>.Parse(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		EditPrefab.ToggleQuestTag(FastTags<TagGroup.Global>.Parse(_featureName));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		EditPrefab.ToggleQuestTag(FastTags<TagGroup.Global>.GetTag(_featureName));
		RebuildList();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllQuestTags(groupsResult, EditPrefab);
	}
}
