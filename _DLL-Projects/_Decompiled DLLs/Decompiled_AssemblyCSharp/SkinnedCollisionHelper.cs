using System;
using UnityEngine;

public class SkinnedCollisionHelper : MonoBehaviour
{
	public bool forceUpdate;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CWeightList[] nodeWeights;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] newVert;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh mesh;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshCollider collide;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		SkinnedMeshRenderer skinnedMeshRenderer = GetComponent(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
		collide = GetComponent(typeof(MeshCollider)) as MeshCollider;
		if (collide != null && skinnedMeshRenderer != null)
		{
			Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
			mesh = new Mesh();
			mesh.vertices = sharedMesh.vertices;
			mesh.uv = sharedMesh.uv;
			mesh.triangles = sharedMesh.triangles;
			newVert = new Vector3[sharedMesh.vertices.Length];
			nodeWeights = new CWeightList[skinnedMeshRenderer.bones.Length];
			for (short num = 0; num < skinnedMeshRenderer.bones.Length; num++)
			{
				nodeWeights[num] = new CWeightList();
				nodeWeights[num].transform = skinnedMeshRenderer.bones[num];
			}
			for (short num = 0; num < sharedMesh.vertices.Length; num++)
			{
				BoneWeight boneWeight = sharedMesh.boneWeights[num];
				if (boneWeight.weight0 != 0f)
				{
					Vector3 p = sharedMesh.bindposes[boneWeight.boneIndex0].MultiplyPoint3x4(sharedMesh.vertices[num]);
					nodeWeights[boneWeight.boneIndex0].weights.Add(new CVertexWeight(num, p, boneWeight.weight0));
				}
				if (boneWeight.weight1 != 0f)
				{
					Vector3 p = sharedMesh.bindposes[boneWeight.boneIndex1].MultiplyPoint3x4(sharedMesh.vertices[num]);
					nodeWeights[boneWeight.boneIndex1].weights.Add(new CVertexWeight(num, p, boneWeight.weight1));
				}
				if (boneWeight.weight2 != 0f)
				{
					Vector3 p = sharedMesh.bindposes[boneWeight.boneIndex2].MultiplyPoint3x4(sharedMesh.vertices[num]);
					nodeWeights[boneWeight.boneIndex2].weights.Add(new CVertexWeight(num, p, boneWeight.weight2));
				}
				if (boneWeight.weight3 != 0f)
				{
					Vector3 p = sharedMesh.bindposes[boneWeight.boneIndex3].MultiplyPoint3x4(sharedMesh.vertices[num]);
					nodeWeights[boneWeight.boneIndex3].weights.Add(new CVertexWeight(num, p, boneWeight.weight3));
				}
			}
			UpdateCollisionMesh();
		}
		else
		{
			Log.Error(base.gameObject.name + ": SkinnedCollisionHelper: this object either has no SkinnedMeshRenderer or has no MeshCollider!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateCollisionMesh()
	{
		if (!(mesh != null))
		{
			return;
		}
		for (int i = 0; i < newVert.Length; i++)
		{
			newVert[i] = new Vector3(0f, 0f, 0f);
		}
		CWeightList[] array = nodeWeights;
		foreach (CWeightList cWeightList in array)
		{
			foreach (CVertexWeight weight in cWeightList.weights)
			{
				newVert[weight.index] += cWeightList.transform.localToWorldMatrix.MultiplyPoint3x4(weight.localPosition) * weight.weight;
			}
		}
		for (int k = 0; k < newVert.Length; k++)
		{
			newVert[k] = base.transform.InverseTransformPoint(newVert[k]);
		}
		mesh.vertices = newVert;
		mesh.RecalculateBounds();
		collide.sharedMesh = null;
		collide.sharedMesh = mesh;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Update()
	{
		if (forceUpdate)
		{
			forceUpdate = false;
			UpdateCollisionMesh();
		}
	}
}
