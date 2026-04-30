using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabGroupsEditorList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return EditPrefab.editorGroups.Contains(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		EditPrefab.editorGroups.Add(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		if (EditPrefab.editorGroups.Contains(_featureName))
		{
			EditPrefab.editorGroups.Remove(_featureName);
		}
		else
		{
			EditPrefab.editorGroups.Add(_featureName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllGroups(groupsResult, EditPrefab);
	}
}
