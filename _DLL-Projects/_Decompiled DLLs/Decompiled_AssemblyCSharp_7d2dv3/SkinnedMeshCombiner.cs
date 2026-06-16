using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshCombiner : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		CombineMeshes(base.transform);
	}

	public static void CombineMeshes(Transform _transform)
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Quaternion> list3 = new List<Quaternion>();
		Transform transform = _transform;
		while (transform != null)
		{
			list.Add(transform.localPosition);
			list3.Add(transform.localRotation);
			list2.Add(transform.localScale);
			transform.localPosition = Vector3.zero;
			transform.localScale = Vector3.one;
			transform.localRotation = Quaternion.identity;
			transform = transform.parent;
		}
		SkinnedMeshRenderer[] componentsInChildren = _transform.GetComponentsInChildren<SkinnedMeshRenderer>();
		List<Transform> list4 = new List<Transform>();
		List<BoneWeight> list5 = new List<BoneWeight>();
		List<CombineInstance> list6 = new List<CombineInstance>();
		List<Texture2D> list7 = new List<Texture2D>();
		int num = 0;
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			num += skinnedMeshRenderer.sharedMesh.subMeshCount;
		}
		int[] array2 = new int[num];
		int num2 = 0;
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer2 = componentsInChildren[j];
			BoneWeight[] boneWeights = skinnedMeshRenderer2.sharedMesh.boneWeights;
			for (int i = 0; i < boneWeights.Length; i++)
			{
				BoneWeight item = boneWeights[i];
				item.boneIndex0 += num2;
				item.boneIndex1 += num2;
				item.boneIndex2 += num2;
				item.boneIndex3 += num2;
				list5.Add(item);
			}
			num2 += skinnedMeshRenderer2.bones.Length;
			Transform[] bones = skinnedMeshRenderer2.bones;
			foreach (Transform item2 in bones)
			{
				list4.Add(item2);
			}
			if (skinnedMeshRenderer2.material.mainTexture != null)
			{
				list7.Add(skinnedMeshRenderer2.GetComponent<Renderer>().material.mainTexture as Texture2D);
			}
			CombineInstance item3 = new CombineInstance
			{
				mesh = skinnedMeshRenderer2.sharedMesh
			};
			array2[j] = item3.mesh.vertexCount;
			item3.transform = skinnedMeshRenderer2.transform.localToWorldMatrix;
			list6.Add(item3);
			skinnedMeshRenderer2.gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
		}
		List<Matrix4x4> list8 = new List<Matrix4x4>();
		for (int k = 0; k < list4.Count; k++)
		{
			list8.Add(list4[k].worldToLocalMatrix * _transform.worldToLocalMatrix);
		}
		SkinnedMeshRenderer skinnedMeshRenderer3 = _transform.gameObject.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer3.sharedMesh = new Mesh();
		skinnedMeshRenderer3.sharedMesh.CombineMeshes(list6.ToArray(), mergeSubMeshes: true, useMatrices: true);
		Texture2D texture2D = new Texture2D(128, 128);
		Rect[] array3 = texture2D.PackTextures(list7.ToArray(), 0);
		Vector2[] uv = skinnedMeshRenderer3.sharedMesh.uv;
		Vector2[] array4 = new Vector2[uv.Length];
		int num3 = 0;
		int num4 = 0;
		for (int l = 0; l < array4.Length; l++)
		{
			array4[l].x = Mathf.Lerp(array3[num3].xMin, array3[num3].xMax, uv[l].x);
			array4[l].y = Mathf.Lerp(array3[num3].yMin, array3[num3].yMax, uv[l].y);
			if (l >= array2[num3] + num4)
			{
				num4 += array2[num3];
				num3++;
			}
		}
		Material material = new Material(Shader.Find("Transparent/Cutout/VertexLit"));
		material.mainTexture = texture2D;
		material.mainTexture.filterMode = FilterMode.Point;
		skinnedMeshRenderer3.sharedMesh.uv = array4;
		skinnedMeshRenderer3.sharedMaterial = material;
		skinnedMeshRenderer3.bones = list4.ToArray();
		skinnedMeshRenderer3.sharedMesh.boneWeights = list5.ToArray();
		skinnedMeshRenderer3.sharedMesh.bindposes = list8.ToArray();
		skinnedMeshRenderer3.sharedMesh.RecalculateBounds();
		transform = _transform;
		int num5 = 0;
		while (transform != null)
		{
			transform.localPosition = list[num5];
			transform.localScale = list2[num5];
			transform.localRotation = list3[num5];
			num5++;
			transform = transform.parent;
		}
	}
}
