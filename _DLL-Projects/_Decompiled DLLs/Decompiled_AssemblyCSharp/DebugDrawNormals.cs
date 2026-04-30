using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DebugDrawNormals : MonoBehaviour
{
	[Serializable]
	public class Data
	{
		public List<Vector3> verts = new List<Vector3>();

		public List<Vector3> normals = new List<Vector3>();

		public List<int> indices = new List<int>();
	}

	public float VertexNormalScale = 0.05f;

	public float TriangleNormalScale = 0.05f;

	public int MeshCount;

	public int TriangleCount;

	public int VertCount;

	public bool Record;

	public List<Data> list = new List<Data>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool die;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (die)
		{
			UnityEngine.Object.DestroyImmediate(this);
			return;
		}
		TriangleCount = 0;
		VertCount = 0;
		MeshFilter component = GetComponent<MeshFilter>();
		if ((bool)component)
		{
			MeshCount = 1;
			Draw(component);
			return;
		}
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
		MeshCount = componentsInChildren.Length;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Draw(componentsInChildren[i]);
		}
		if (componentsInChildren.Length == 0)
		{
			SetDie();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Draw(MeshFilter mf)
	{
		Mesh sharedMesh = mf.sharedMesh;
		if (!sharedMesh)
		{
			return;
		}
		Data data;
		if (list.Count > 0 && !Record)
		{
			data = list[0];
		}
		else
		{
			data = new Data();
			list.Add(data);
			Record = false;
		}
		sharedMesh.GetVertices(data.verts);
		sharedMesh.GetNormals(data.normals);
		sharedMesh.GetTriangles(data.indices, 0);
		VertCount += data.verts.Count;
		TriangleCount += data.indices.Count / 3;
		Matrix4x4 localToWorldMatrix = mf.transform.localToWorldMatrix;
		for (int i = 0; i < list.Count; i++)
		{
			Data data2 = list[i];
			for (int j = 0; j < data2.normals.Count; j++)
			{
				Utils.DrawRay(localToWorldMatrix.MultiplyPoint(data2.verts[j]), localToWorldMatrix.MultiplyVector(data2.normals[j]) * VertexNormalScale, Color.white, Color.blue, 3);
			}
			for (int k = 0; k < data2.indices.Count - 2; k += 3)
			{
				Vector3 vector = data2.verts[data2.indices[k]];
				Vector3 vector2 = data2.verts[data2.indices[k + 1]];
				Vector3 vector3 = data2.verts[data2.indices[k + 2]];
				Vector3 point = (vector + vector2 + vector3) * (1f / 3f);
				Vector3 normalized = Vector3.Cross(vector2 - vector, vector3 - vector).normalized;
				Utils.DrawRay(localToWorldMatrix.MultiplyPoint(point), localToWorldMatrix.MultiplyVector(normalized) * TriangleNormalScale, Color.yellow, Color.red, 3);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			SetDie();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDie()
	{
		list = null;
		die = true;
	}
}
