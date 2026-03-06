using System.Collections.Generic;
using UnityEngine;

public class AddSnowToGlass : MonoBehaviour
{
	public Material glassMaterial;

	public Material snowMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		List<Material> list = new List<Material>();
		MeshRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<MeshRenderer>();
		List<int[]> list2 = new List<int[]>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			int num = 0;
			for (int j = 0; j < componentsInChildren[i].materials.Length; j++)
			{
				if (componentsInChildren[i].materials[j].name.Contains(glassMaterial.name))
				{
					num++;
				}
			}
			if (num == 0)
			{
				continue;
			}
			MeshFilter component = componentsInChildren[i].transform.GetComponent<MeshFilter>();
			list.Clear();
			list2.Clear();
			for (int k = 0; k < component.mesh.subMeshCount; k++)
			{
				list2.Add(component.mesh.GetTriangles(k));
			}
			component.mesh.subMeshCount += num;
			int num2 = 0;
			for (int l = 0; l < componentsInChildren[i].materials.Length; l++)
			{
				list.Add(componentsInChildren[i].materials[l]);
				component.mesh.SetTriangles(list2[l], num2++);
				if (componentsInChildren[i].materials[l].name.Contains(glassMaterial.name))
				{
					list.Add(snowMaterial);
					component.mesh.SetTriangles(list2[l], num2++);
				}
			}
			componentsInChildren[i].materials = list.ToArray();
		}
	}
}
