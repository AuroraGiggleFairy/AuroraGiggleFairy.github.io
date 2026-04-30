using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GearBoneMap : MonoBehaviour
{
	[Serializable]
	public class PartBones
	{
		[Tooltip("e.g. head, body, feet, hands")]
		public string partName;

		[Tooltip("The unique bones actually referenced by skin weights for this part")]
		public List<Transform> bones = new List<Transform>();
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<PartBones> parts = new List<PartBones>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] DefaultParts = new string[4] { "head", "body", "feet", "hands" };

	public IReadOnlyList<Transform> GetPartBones(string partName)
	{
		if (string.IsNullOrEmpty(partName))
		{
			return Array.Empty<Transform>();
		}
		PartBones partBones = parts.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
		if (partBones != null && partBones.bones != null)
		{
			return partBones.bones;
		}
		int num = partName.IndexOf("_");
		if (num < 0)
		{
			num = partName.Length;
		}
		partName = partName.Substring(0, num);
		partBones = parts.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
		if (partBones != null && partBones.bones != null)
		{
			return partBones.bones;
		}
		return Array.Empty<Transform>();
	}

	public IReadOnlyList<string> GetPartNames()
	{
		return parts.Select([PublicizedFrom(EAccessModifier.Internal)] (PartBones p) => p.partName).ToArray();
	}

	public void SetBones(string partName, IEnumerable<Transform> newBones)
	{
		if (!string.IsNullOrEmpty(partName))
		{
			PartBones partBones = parts.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
			if (partBones == null)
			{
				partBones = new PartBones
				{
					partName = partName
				};
				parts.Add(partBones);
			}
			List<Transform> collection = (from t in newBones.Where([PublicizedFrom(EAccessModifier.Internal)] (Transform t) => t != null).Distinct()
				orderby GetHierarchyPath(t), t.name
				select t).ToList();
			partBones.bones.Clear();
			partBones.bones.AddRange(collection);
		}
	}

	public void ClearAll()
	{
		parts.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetHierarchyPath(Transform t)
	{
		if (t == null)
		{
			return string.Empty;
		}
		Stack<string> stack = new Stack<string>();
		Transform transform = t;
		while (transform != null)
		{
			stack.Push(transform.name);
			transform = transform.parent;
		}
		return string.Join("/", stack);
	}

	public void Bake()
	{
		List<Transform> list = new List<Transform>();
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			int num = transform.name.IndexOf("_");
			if (num < 0)
			{
				num = transform.name.Length;
			}
			if (transform.parent == base.transform && DefaultParts.Contains(transform.name.Substring(0, num)))
			{
				list.Add(transform);
			}
		}
		foreach (Transform item in list)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = item.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			HashSet<Transform> hashSet = new HashSet<Transform>();
			SkinnedMeshRenderer[] array = componentsInChildren2;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				if (skinnedMeshRenderer == null)
				{
					continue;
				}
				Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
				if (sharedMesh == null)
				{
					Debug.LogWarning("[GearBoneMap] " + skinnedMeshRenderer.name + " has no sharedMesh.");
					continue;
				}
				try
				{
					NativeArray<byte> bonesPerVertex = sharedMesh.GetBonesPerVertex();
					NativeArray<BoneWeight1> allBoneWeights = sharedMesh.GetAllBoneWeights();
					Transform[] bones = skinnedMeshRenderer.bones;
					int num2 = 0;
					for (int k = 0; k < bonesPerVertex.Length; k++)
					{
						int num3 = bonesPerVertex[k];
						for (int l = 0; l < num3; l++)
						{
							BoneWeight1 boneWeight = allBoneWeights[num2++];
							if (boneWeight.weight <= 0f)
							{
								continue;
							}
							int boneIndex = boneWeight.boneIndex;
							if (boneIndex >= 0 && bones != null && boneIndex < bones.Length)
							{
								Transform transform2 = bones[boneIndex];
								if (transform2 != null)
								{
									hashSet.Add(transform2);
								}
							}
						}
					}
					if (skinnedMeshRenderer.rootBone != null)
					{
						hashSet.Add(skinnedMeshRenderer.rootBone);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("[GearBoneMap] Failed reading bone weights for " + skinnedMeshRenderer.name + ": " + ex.Message);
				}
			}
			SetBones(item.name, hashSet);
		}
	}
}
