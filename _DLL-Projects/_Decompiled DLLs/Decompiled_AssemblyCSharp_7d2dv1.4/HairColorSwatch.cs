using UnityEngine;

[CreateAssetMenu(fileName = "HairColorSwatch", menuName = "Hair Color Management/Hair Color Swatch", order = 1)]
public class HairColorSwatch : ScriptableObject
{
	[ColorUsage(false, false)]
	public Color tint1 = Color.red;

	[ColorUsage(false, false)]
	public Color tint2 = Color.green;

	[ColorUsage(false, false)]
	public Color tint3 = Color.blue;

	[Range(0f, 1f)]
	public float tintSharpness = 0.5f;

	[Range(0f, 1f)]
	public float idMapStrength;

	[Range(0f, 1f)]
	public float rootDarkening;

	[Range(0f, 1f)]
	public float metallic;

	[ColorUsage(false, true)]
	public Color cuticleSpecularColor = Color.white;

	[ColorUsage(false, true)]
	public Color cortexSpecularColor = Color.white;

	[Range(0f, 1f)]
	public float indirectSpecularStrength;

	[ColorUsage(false, false)]
	public Color subsurfaceAmbient = Color.white;

	[ColorUsage(false, false)]
	public Color subsurfaceColor = Color.white;

	public void ApplyToMaterial(Material material)
	{
		material.SetColor("_Tint1", tint1);
		material.SetColor("_Tint2", tint2);
		material.SetColor("_Tint3", tint3);
		material.SetFloat("_TintSharpness", tintSharpness);
		material.SetFloat("_IDMapStrength", idMapStrength);
		material.SetFloat("_RootDarkening", rootDarkening);
		material.SetFloat("_Metallic", metallic);
		material.SetColor("_CuticleSpecularColor", cuticleSpecularColor);
		material.SetColor("_CortexSpecularColor", cortexSpecularColor);
		material.SetFloat("_IndirectSpecularStrength", indirectSpecularStrength);
		material.SetColor("_SubsurfaceAmbient", subsurfaceAmbient);
		material.SetColor("_SubsurfaceColor", subsurfaceColor);
	}

	public void ApplySwatchToGameObject(GameObject targetGameObject)
	{
		Shader shader = Shader.Find("Game/SDCS/Hair");
		if (targetGameObject != null)
		{
			Renderer[] componentsInChildren = targetGameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] array = ((!Application.isPlaying) ? renderer.sharedMaterials : renderer.materials);
				Material[] array2 = array;
				foreach (Material material in array2)
				{
					if (material.shader == shader)
					{
						ApplyToMaterial(material);
					}
				}
			}
		}
		else
		{
			Debug.LogWarning("No target GameObject selected.");
		}
	}
}
