using System;
using JBooth.MicroSplat;
using UnityEngine;

[ExecuteInEditMode]
public class ReadProceduralTextureExample : MonoBehaviour
{
	public MicroSplatProceduralTextureConfig proceduralConfig;

	public MicroSplatKeywords keywords;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int lastHit = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (proceduralConfig == null || !Physics.Raycast(new Ray(base.transform.position, Vector3.down), out var hitInfo))
		{
			return;
		}
		Terrain component = hitInfo.collider.GetComponent<Terrain>();
		if (component == null)
		{
			return;
		}
		Material materialTemplate = component.materialTemplate;
		if (!(materialTemplate == null))
		{
			Vector2 textureCoord = hitInfo.textureCoord;
			Vector3 point = hitInfo.point;
			Vector3 normal = hitInfo.normal;
			Vector3 up = Vector3.up;
			MicroSplatProceduralTextureUtil.NoiseUVMode noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.World;
			if (keywords.IsKeywordEnabled("_PCNOISEUV"))
			{
				noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.UV;
			}
			else if (keywords.IsKeywordEnabled("_PCNOISETRIPLANAR"))
			{
				noiseUVMode = MicroSplatProceduralTextureUtil.NoiseUVMode.Triplanar;
			}
			MicroSplatProceduralTextureUtil.Sample(textureCoord, point, normal, up, noiseUVMode, materialTemplate, proceduralConfig, out var weights, out var indexes);
			if (indexes.x != lastHit)
			{
				string[] obj = new string[10]
				{
					"PC Texture Index : (",
					indexes.x.ToString(),
					", ",
					indexes.y.ToString(),
					", ",
					indexes.z.ToString(),
					", ",
					indexes.z.ToString(),
					")      ",
					null
				};
				Vector4 vector = weights;
				obj[9] = vector.ToString();
				Debug.Log(string.Concat(obj));
				lastHit = indexes.x;
			}
		}
	}
}
