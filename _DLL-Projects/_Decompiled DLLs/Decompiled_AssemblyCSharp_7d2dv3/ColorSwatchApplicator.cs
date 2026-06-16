using UnityEngine;

public class ColorSwatchApplicator : MonoBehaviour
{
	public static string baseHairColorLoc
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return "@:Entities/Player/Common/HairColorSwatches";
		}
	}

	public void ApplyColorSwatch(string color)
	{
		HairColorSwatch hairColorSwatch = null;
		if (string.IsNullOrEmpty(color))
		{
			return;
		}
		string text = baseHairColorLoc + "/" + color + ".asset";
		ScriptableObject scriptableObject = DataLoader.LoadAsset<ScriptableObject>(text);
		if (scriptableObject == null)
		{
			Log.Warning("SDCSUtils::" + text + " not found for hair color " + color + "!");
		}
		else
		{
			hairColorSwatch = scriptableObject as HairColorSwatch;
			if (hairColorSwatch != null)
			{
				ApplySwatchToGameObject(hairColorSwatch);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplySwatchToGameObject(HairColorSwatch hairSwatch)
	{
		Renderer[] componentsInChildren = base.transform.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			Material[] materials = renderer.materials;
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j].shader.name == "Game/SDCS/Hair" && !materials[j].name.Contains("lashes"))
				{
					materials[j] = new Material(materials[j]);
					hairSwatch.ApplyToMaterial(materials[j]);
				}
			}
			renderer.materials = materials;
		}
	}
}
