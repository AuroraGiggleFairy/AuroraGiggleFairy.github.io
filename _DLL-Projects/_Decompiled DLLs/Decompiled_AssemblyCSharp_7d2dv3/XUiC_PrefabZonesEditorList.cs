using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabZonesEditorList : XUiC_PrefabFeatureEditorList
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool FeatureEnabled(string _featureName)
	{
		return EditPrefab.IsAllowedZone(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void AddNewFeature(string _featureName)
	{
		EditPrefab.AddAllowedZone(_featureName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ToggleFeature(string _featureName)
	{
		if (EditPrefab.IsAllowedZone(_featureName))
		{
			EditPrefab.RemoveAllowedZone(_featureName);
		}
		else
		{
			EditPrefab.AddAllowedZone(_featureName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void GetSupportedFeatures()
	{
		PrefabEditModeManager.Instance.GetAllZones(groupsResult, EditPrefab);
	}
}
