using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshMorphMatrix", menuName = "Mesh Morphing/MeshMorphMatrix", order = 1)]
public class MeshMorphMatrix : ScriptableObject
{
	public enum MeshMorphMatrixType
	{
		Hair,
		Headgear
	}

	[Serializable]
	public struct MorphTarget
	{
		public int blendshapeIndex;

		public string name;
	}

	[Serializable]
	public struct MeshData
	{
		public int blendshapeIndex;

		public string typeName;

		public GameObject gameObject;

		public string MeshName
		{
			get
			{
				if (!(gameObject != null))
				{
					return "null";
				}
				return gameObject.name;
			}
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorphMatrixType matrixType;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer morphTargetsSource;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance = 0.1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float normalBias;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MorphTarget[] morphTargets;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshData[] meshes;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorph[] morphedMeshes;

	[HideInInspector]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public SkinnedMeshRenderer MorphTargetsSource => morphTargetsSource;

	public MorphTarget[] MorphTargets => morphTargets;

	public MeshData[] Meshes => meshes;

	public MeshMorph[] MorphedMeshes => morphedMeshes;

	public float MaxDistance => maxDistance;

	public float NormalBias => normalBias;
}
