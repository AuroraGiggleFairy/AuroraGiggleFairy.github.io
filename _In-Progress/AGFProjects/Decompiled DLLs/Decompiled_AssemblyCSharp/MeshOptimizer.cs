using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshOptimizer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct Edge
	{
		public const int MaxEdgeTris = 8;

		public int firstTri;

		public int numTris;

		public int v0;

		public int v1;

		public Vector3 vec;

		public Vector3 normal;

		public float len;

		public bool collapsed;

		public bool onModelEdge;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Tri
	{
		public int e0;

		public int e1;

		public int e2;

		public int v0;

		public int v1;

		public int v2;

		public Vector3 normal;

		public bool collapsed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct Vertex
	{
		public const int MaxVertexTris = 256;

		public Vector3 pos;

		public int firstTri;

		public int numTris;

		public int firstEdge;

		public int numEdges;

		public bool collapsed;

		public bool onModelEdge;

		public float collapseCost;

		public int collapseEdge;

		public int emitIndex;

		public int iterations;

		public int sortedPosition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float DegenerateEpsilon = 0.9999f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int Iterations;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumEdges;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumTris;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumVerts;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumComponents;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumOptimizedVerts;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumOptimizedTris;

	[PublicizedFrom(EAccessModifier.Private)]
	public Edge[] EdgeList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Tri[] TriList;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vertex[] VertexList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] ComponentList;

	[PublicizedFrom(EAccessModifier.Private)]
	public int NumSortedEdges;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] SortedEdgeCost;

	public void Optimize(List<Vector3> _vertices, List<int> _indices, float _maxEdgeCost, out Vector3[] optimizedVerts, out int[] optimizedIndices)
	{
		AllocateArrays(_vertices.Count, _indices.Count / 3);
		BuildEdgeList(_vertices, _indices);
		MarkBoundaryEdges();
		BuildSortedEdgeCollapseList();
		CollapseEdges(_maxEdgeCost);
		GenerateOutputMesh(_vertices, _indices, out optimizedVerts, out optimizedIndices);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildEdgeList(List<Vector3> _vertices, List<int> _indices)
	{
		for (int i = 0; i < _indices.Count; i += 3)
		{
			AddTri(_vertices, _indices, i);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int CollapseEdges(float _maxEdgeCost)
	{
		int num = 0;
		while (NumOptimizedTris > 2 && NumSortedEdges > 0)
		{
			for (int i = 0; i < NumVerts; i++)
			{
				int num2 = SortedEdgeCost[i];
				if (!VertexList[num2].collapsed)
				{
					if (VertexList[num2].collapseCost > _maxEdgeCost)
					{
						Iterations++;
						FindBestVertexCollapse(5);
						return num;
					}
					int collapseEdge = VertexList[num2].collapseEdge;
					if (!EdgeList[collapseEdge].collapsed)
					{
						int v = ((EdgeList[collapseEdge].v0 == num2) ? EdgeList[collapseEdge].v1 : EdgeList[collapseEdge].v0);
						_ = 53;
						CollapseVertex(num2, v, collapseEdge);
						NumSortedEdges--;
						num++;
						break;
					}
					Iterations++;
					if (FindBestVertexCollapse(num2))
					{
						ResortVert(num2);
						break;
					}
				}
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SwapSortedVert(int v0, int v1)
	{
		int num = SortedEdgeCost[v0];
		SortedEdgeCost[v0] = SortedEdgeCost[v1];
		VertexList[SortedEdgeCost[v0]].sortedPosition = v0;
		SortedEdgeCost[v1] = num;
		VertexList[SortedEdgeCost[v1]].sortedPosition = v1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int NextSortedVert(int v, int dir)
	{
		v += dir;
		while (v >= 0 && v < NumVerts)
		{
			int num = SortedEdgeCost[v];
			if (!VertexList[num].collapsed)
			{
				return v;
			}
			v += dir;
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ValidateSort()
	{
		for (int i = 0; i < NumVerts; i++)
		{
			if (!VertexList[i].collapsed && i != SortedEdgeCost[VertexList[i].sortedPosition])
			{
				throw new Exception("Failed SANITY_CHECK in MeshOptimizer!");
			}
		}
		float num = float.MaxValue;
		int num2 = 0;
		for (int j = 0; j < NumVerts; j++)
		{
			if (!VertexList[SortedEdgeCost[j]].collapsed)
			{
				num = VertexList[SortedEdgeCost[j]].collapseCost;
				num2 = 0;
				break;
			}
		}
		for (int k = num2; k < NumVerts; k++)
		{
			if (!VertexList[SortedEdgeCost[k]].collapsed)
			{
				if (num > VertexList[SortedEdgeCost[k]].collapseCost)
				{
					throw new Exception("Failed SANITY_CHECK in MeshOptimizer!");
				}
				num = VertexList[SortedEdgeCost[k]].collapseCost;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ResortVert(int v)
	{
		v = VertexList[v].sortedPosition;
		int num = 0;
		while (true)
		{
			int num2 = v;
			int num3 = SortedEdgeCost[v];
			if (v > 0 && num != 1)
			{
				int num4 = NextSortedVert(v, -1);
				if (num4 != -1)
				{
					int num5 = SortedEdgeCost[num4];
					if (VertexList[num3].collapseCost < VertexList[num5].collapseCost)
					{
						v = num4;
						num = -1;
					}
				}
			}
			if (v < NumVerts - 1 && num != -1)
			{
				int num6 = NextSortedVert(v, 1);
				if (num6 != -1)
				{
					int num7 = SortedEdgeCost[num6];
					if (VertexList[num3].collapseCost > VertexList[num7].collapseCost)
					{
						v = num6;
						num = 1;
					}
				}
			}
			if (num2 != v)
			{
				SwapSortedVert(num2, v);
				continue;
			}
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkBoundaryEdge(int edge)
	{
		if (EdgeList[edge].collapsed)
		{
			return;
		}
		int firstTri = EdgeList[edge].firstTri;
		int numTris = EdgeList[edge].numTris;
		int num = 0;
		for (int i = 0; i < numTris; i++)
		{
			int num2 = ComponentList[firstTri + i];
			if (!TriList[num2].collapsed)
			{
				num++;
				if (num > 1)
				{
					break;
				}
			}
		}
		EdgeList[edge].onModelEdge = num < 2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CollapseEdgeList(int v)
	{
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		for (int i = 0; i < numEdges; i++)
		{
			EdgeList[ComponentList[firstEdge + i]].collapsed = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CollapseTri(int tri)
	{
		TriList[tri].collapsed = true;
		CollapseEdge(TriList[tri].e0);
		CollapseEdge(TriList[tri].e1);
		CollapseEdge(TriList[tri].e2);
		CollapseVert(TriList[tri].v0);
		CollapseVert(TriList[tri].v1);
		CollapseVert(TriList[tri].v2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CollapseEdge(int edge)
	{
		if (EdgeList[edge].collapsed)
		{
			return;
		}
		int firstTri = EdgeList[edge].firstTri;
		int numTris = EdgeList[edge].numTris;
		bool collapsed = true;
		for (int i = 0; i < numTris; i++)
		{
			int num = ComponentList[firstTri + i];
			if (!TriList[num].collapsed)
			{
				collapsed = false;
				break;
			}
		}
		EdgeList[edge].collapsed = collapsed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CollapseVert(int v)
	{
		if (VertexList[v].collapsed)
		{
			return;
		}
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		bool collapsed = true;
		for (int i = 0; i < numEdges; i++)
		{
			int num = ComponentList[firstEdge + i];
			if (!EdgeList[num].collapsed)
			{
				collapsed = false;
				break;
			}
		}
		VertexList[v].collapsed = collapsed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CollapseVertex(int v0, int v1, int edge)
	{
		int firstTri = EdgeList[edge].firstTri;
		int numTris = EdgeList[edge].numTris;
		for (int i = 0; i < numTris; i++)
		{
			int num = ComponentList[firstTri + i];
			if (!TriList[num].collapsed)
			{
				CollapseTri(num);
				NumOptimizedTris--;
			}
		}
		VertexList[v1].collapsed = false;
		MergeEdgeList(v0, v1);
		MarkBoundaryEdges(v1);
		VertexList[v0].collapsed = true;
		NumOptimizedVerts--;
		Iterations++;
		int firstTri2 = VertexList[v1].firstTri;
		int numTris2 = VertexList[v1].numTris;
		for (int j = 0; j < numTris2; j++)
		{
			int num2 = ComponentList[firstTri2 + j];
			if (!TriList[num2].collapsed)
			{
				if (FindBestVertexCollapse(TriList[num2].v0))
				{
					ResortVert(TriList[num2].v0);
				}
				if (FindBestVertexCollapse(TriList[num2].v1))
				{
					ResortVert(TriList[num2].v1);
				}
				if (FindBestVertexCollapse(TriList[num2].v2))
				{
					ResortVert(TriList[num2].v2);
				}
			}
		}
		if (VertexList[v1].iterations != Iterations && FindBestVertexCollapse(v1))
		{
			ResortVert(v1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkBoundaryEdges(int v)
	{
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		for (int i = 0; i < numEdges; i++)
		{
			int edge = ComponentList[firstEdge + i];
			MarkBoundaryEdge(edge);
		}
		MarkBoundaryVert(v);
		for (int j = 0; j < numEdges; j++)
		{
			int num = ComponentList[firstEdge + j];
			int v2 = EdgeList[num].v0;
			int v3 = EdgeList[num].v1;
			if (v2 == v)
			{
				MarkBoundaryVert(v3);
			}
			else
			{
				MarkBoundaryVert(v2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkBoundaryVert(int v)
	{
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		VertexList[v].onModelEdge = false;
		for (int i = 0; i < numEdges; i++)
		{
			int num = ComponentList[firstEdge + i];
			if (!EdgeList[num].collapsed && EdgeList[num].onModelEdge)
			{
				VertexList[v].onModelEdge = true;
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool EdgeWillMerge(int v0, int v1, int edge)
	{
		if (EdgeList[edge].collapsed)
		{
			return true;
		}
		int num = ((EdgeList[edge].v0 == v0) ? EdgeList[edge].v1 : EdgeList[edge].v0);
		int firstEdge = VertexList[v1].firstEdge;
		int numEdges = VertexList[v1].numEdges;
		for (int i = 0; i < numEdges; i++)
		{
			int num2 = ComponentList[firstEdge + i];
			if (!EdgeList[num2].collapsed && ((EdgeList[num2].v0 == v1 && EdgeList[num2].v1 == num) || (EdgeList[num2].v0 == num && EdgeList[num2].v1 == v1)))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MergeEdgeList(int v0, int v1)
	{
		CollapseEdgeList(v0);
		int firstTri = VertexList[v0].firstTri;
		int numTris = VertexList[v0].numTris;
		int num = 0;
		for (int i = 0; i < numTris; i++)
		{
			int num2 = ComponentList[firstTri + i];
			if (TriList[num2].collapsed)
			{
				continue;
			}
			if (TriList[num2].v0 == v0)
			{
				TriList[num2].e0 = AddEdge(v1, TriList[num2].v1, num2);
				TriList[num2].e2 = AddEdge(TriList[num2].v2, v1, num2);
				TriList[num2].v0 = v1;
			}
			else if (TriList[num2].v1 == v0)
			{
				TriList[num2].e0 = AddEdge(TriList[num2].v0, v1, num2);
				TriList[num2].e1 = AddEdge(v1, TriList[num2].v2, num2);
				TriList[num2].v1 = v1;
			}
			else
			{
				if (TriList[num2].v2 != v0)
				{
					continue;
				}
				TriList[num2].e1 = AddEdge(TriList[num2].v1, v1, num2);
				TriList[num2].e2 = AddEdge(v1, TriList[num2].v0, num2);
				TriList[num2].v2 = v1;
			}
			if (TriList[num2].collapsed)
			{
				NumOptimizedTris--;
				continue;
			}
			VertexList[v1].collapsed = false;
			AddVertex(v1, num2, null, addTriToVerts: true);
			int e = TriList[num2].e0;
			int e2 = TriList[num2].e1;
			Vector3 vector = EdgeList[e].normal;
			Vector3 vector2 = EdgeList[e2].normal;
			if (EdgeList[e].v0 != TriList[num2].v0)
			{
				vector = -vector;
			}
			if (EdgeList[e2].v0 != TriList[num2].v1)
			{
				vector2 = -vector2;
			}
			TriList[num2].normal = Vector3.Cross(vector, vector2).normalized;
			num++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateOutputMesh(List<Vector3> _vertices, List<int> _indices, out Vector3[] optimizedVerts, out int[] optimizedIndices)
	{
		optimizedVerts = new Vector3[NumOptimizedVerts];
		optimizedIndices = new int[NumOptimizedTris * 3];
		int num = 0;
		for (int i = 0; i < NumVerts; i++)
		{
			if (!VertexList[i].collapsed)
			{
				VertexList[i].emitIndex = num;
				optimizedVerts[num] = VertexList[i].pos;
				num++;
			}
		}
		int num2 = 0;
		for (int j = 0; j < NumTris; j++)
		{
			if (!TriList[j].collapsed)
			{
				optimizedIndices[num2] = VertexList[TriList[j].v0].emitIndex;
				optimizedIndices[num2 + 1] = VertexList[TriList[j].v1].emitIndex;
				optimizedIndices[num2 + 2] = VertexList[TriList[j].v2].emitIndex;
				num2 += 3;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BuildSortedEdgeCollapseList()
	{
		Iterations++;
		SortedEdgeCost = new int[NumVerts];
		for (int i = 0; i < NumVerts; i++)
		{
			FindBestVertexCollapse(i);
			SortedEdgeCost[i] = i;
		}
		NumSortedEdges = NumVerts;
		Array.Sort(SortedEdgeCost, [PublicizedFrom(EAccessModifier.Private)] (int a, int b) => VertexList[a].collapseCost.CompareTo(VertexList[b].collapseCost));
		for (int num = 0; num < NumVerts; num++)
		{
			VertexList[SortedEdgeCost[num]].sortedPosition = num;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MarkBoundaryEdges()
	{
		for (int i = 0; i < NumEdges; i++)
		{
			if (EdgeList[i].numTris < 2)
			{
				EdgeList[i].onModelEdge = true;
				VertexList[EdgeList[i].v0].onModelEdge = true;
				VertexList[EdgeList[i].v1].onModelEdge = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool FindBestVertexCollapse(int v)
	{
		if (VertexList[v].iterations >= Iterations)
		{
			return false;
		}
		VertexList[v].iterations = Iterations;
		VertexList[v].collapseEdge = -1;
		if (VertexList[v].collapsed)
		{
			VertexList[v].collapseCost = float.MaxValue;
			return true;
		}
		bool onModelEdge = VertexList[v].onModelEdge;
		float num = float.MaxValue;
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		for (int i = 0; i < numEdges; i++)
		{
			int num2 = ComponentList[firstEdge + i];
			Edge edge = EdgeList[num2];
			if (!edge.collapsed && (!onModelEdge || edge.onModelEdge))
			{
				int num3 = edge.v0;
				int num4 = edge.v1;
				if (num4 == v)
				{
					num4 = num3;
					num3 = v;
				}
				float num5 = CalculateEdgeCost(num3, num4, num2);
				if (num5 < num)
				{
					num = num5;
					VertexList[v].collapseEdge = num2;
				}
			}
		}
		VertexList[v].collapseCost = num;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalculateBoundaryEdgeCost(int v0, int v1, int edge)
	{
		float num = 0f;
		int firstEdge = VertexList[v0].firstEdge;
		int numEdges = VertexList[v0].numEdges;
		Vector3 normal = EdgeList[edge].normal;
		float len = EdgeList[edge].len;
		for (int i = 0; i < numEdges; i++)
		{
			int num2 = ComponentList[firstEdge + i];
			if (num2 != edge && !EdgeList[num2].collapsed && EdgeList[num2].onModelEdge)
			{
				float num3 = Mathf.Abs(Vector3.Dot(normal, EdgeList[num2].normal));
				num = Mathf.Max(num, (1f - num3) * (len + EdgeList[num2].len));
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool DetectDegenerateCollapse(int v0, int v1, int edge)
	{
		int firstTri = VertexList[v0].firstTri;
		int numTris = VertexList[v0].numTris;
		for (int i = 0; i < numTris; i++)
		{
			int num = ComponentList[firstTri + i];
			if (TriList[num].collapsed || TriList[num].e0 == edge || TriList[num].e1 == edge || TriList[num].e2 == edge)
			{
				continue;
			}
			if (TriList[num].v0 == v0)
			{
				Vector3 vector = VertexList[TriList[num].v1].pos - VertexList[v1].pos;
				if (vector.sqrMagnitude < 0.0001f)
				{
					return true;
				}
				vector.Normalize();
				if (Mathf.Abs(Vector3.Dot(vector, EdgeList[TriList[num].e1].normal)) >= DegenerateEpsilon)
				{
					return true;
				}
				if (EdgeList[TriList[num].e1].v0 != TriList[num].v1)
				{
					vector = -vector;
				}
				if (Vector3.Dot(Vector3.Cross(vector, EdgeList[TriList[num].e1].normal), TriList[num].normal) < 0f)
				{
					return true;
				}
			}
			else if (TriList[num].v1 == v0)
			{
				Vector3 vector2 = VertexList[TriList[num].v2].pos - VertexList[v1].pos;
				if (vector2.sqrMagnitude < 0.0001f)
				{
					return true;
				}
				vector2.Normalize();
				if (Mathf.Abs(Vector3.Dot(vector2, EdgeList[TriList[num].e2].normal)) >= DegenerateEpsilon)
				{
					return true;
				}
				if (EdgeList[TriList[num].e2].v0 != TriList[num].v2)
				{
					vector2 = -vector2;
				}
				if (Vector3.Dot(Vector3.Cross(vector2, EdgeList[TriList[num].e2].normal), TriList[num].normal) < 0f)
				{
					return true;
				}
			}
			else if (TriList[num].v2 == v0)
			{
				Vector3 vector3 = VertexList[TriList[num].v0].pos - VertexList[v1].pos;
				if (vector3.sqrMagnitude < 0.0001f)
				{
					return true;
				}
				vector3.Normalize();
				if (Mathf.Abs(Vector3.Dot(vector3, EdgeList[TriList[num].e0].normal)) >= DegenerateEpsilon)
				{
					return true;
				}
				if (EdgeList[TriList[num].e0].v0 != TriList[num].v0)
				{
					vector3 = -vector3;
				}
				if (Vector3.Dot(Vector3.Cross(vector3, EdgeList[TriList[num].e0].normal), TriList[num].normal) < 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CalculateEdgeCost(int v0, int v1, int edge)
	{
		if (EdgeList[edge].collapsed || DetectDegenerateCollapse(v0, v1, edge))
		{
			return float.MaxValue;
		}
		if (EdgeList[edge].onModelEdge)
		{
			return CalculateBoundaryEdgeCost(v0, v1, edge);
		}
		float num = 0f;
		int firstTri = EdgeList[edge].firstTri;
		int numTris = EdgeList[edge].numTris;
		int firstTri2 = VertexList[v0].firstTri;
		int numTris2 = VertexList[v0].numTris;
		for (int i = 0; i < numTris2; i++)
		{
			int num2 = ComponentList[firstTri2 + i];
			if (TriList[num2].collapsed || TriList[num2].e0 == edge || TriList[num2].e1 == edge || TriList[num2].e2 == edge)
			{
				continue;
			}
			float num3 = 1f;
			for (int j = 0; j < numTris; j++)
			{
				int num4 = ComponentList[firstTri + j];
				if (num4 != num2 && !TriList[num4].collapsed)
				{
					float num5 = Vector3.Dot(TriList[num4].normal, TriList[num2].normal);
					num3 = Mathf.Min(num3, (1f - num5) / 2f);
				}
			}
			num = Mathf.Max(num, num3);
		}
		return EdgeList[edge].len * num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddTri(List<Vector3> _vertices, List<int> _indices, int index)
	{
		int num = index / 3;
		TriList[num].collapsed = false;
		int num2 = AddVertex(_indices[index], num, _vertices, addTriToVerts: true);
		int num3 = AddVertex(_indices[index + 1], num, _vertices, addTriToVerts: true);
		int num4 = AddVertex(_indices[index + 2], num, _vertices, addTriToVerts: true);
		TriList[num].v0 = num2;
		TriList[num].v1 = num3;
		TriList[num].v2 = num4;
		TriList[num].e0 = -1;
		TriList[num].e1 = -1;
		TriList[num].e2 = -1;
		int num5 = AddEdge(num2, num3, num);
		TriList[num].e0 = num5;
		int num6 = AddEdge(num3, num4, num);
		TriList[num].e1 = num6;
		int e = AddEdge(num4, num2, num);
		TriList[num].e2 = e;
		Vector3 vector = EdgeList[num5].normal;
		Vector3 vector2 = EdgeList[num6].normal;
		if (EdgeList[num5].v0 != num2)
		{
			vector = -vector;
		}
		if (EdgeList[num6].v0 != num3)
		{
			vector2 = -vector2;
		}
		TriList[num].normal = Vector3.Cross(vector, vector2).normalized;
		NumTris++;
		NumOptimizedTris++;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddEdgeToVertex(int v, int edge)
	{
		bool flag = false;
		int firstEdge = VertexList[v].firstEdge;
		int numEdges = VertexList[v].numEdges;
		for (int i = 0; i < numEdges; i++)
		{
			int num = ComponentList[firstEdge + i];
			if (EdgeList[num].collapsed)
			{
				ComponentList[firstEdge + i] = edge;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			if (VertexList[v].numEdges == 256)
			{
				VertexList[v].collapsed = true;
				return;
			}
			ComponentList[VertexList[v].firstEdge + VertexList[v].numEdges] = edge;
			VertexList[v].numEdges++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int AddEdge(int v0, int v1, int tri)
	{
		int num = FindTriEdge(v0, v1);
		if (num == -1)
		{
			num = NumEdges++;
			EdgeList[num].collapsed = false;
			EdgeList[num].onModelEdge = false;
			EdgeList[num].v0 = v0;
			EdgeList[num].v1 = v1;
			EdgeList[num].vec = VertexList[v1].pos - VertexList[v0].pos;
			EdgeList[num].normal = EdgeList[num].vec.normalized;
			EdgeList[num].firstTri = AllocateComponents(8);
			EdgeList[num].numTris = 0;
			EdgeList[num].len = EdgeList[num].vec.magnitude;
			AddEdgeToVertex(v0, num);
			AddEdgeToVertex(v1, num);
		}
		bool flag = false;
		int firstTri = EdgeList[num].firstTri;
		int numTris = EdgeList[num].numTris;
		for (int i = 0; i < numTris; i++)
		{
			int num2 = ComponentList[firstTri + i];
			if (TriList[num2].collapsed)
			{
				ComponentList[firstTri + i] = tri;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			if (EdgeList[num].numTris == 8)
			{
				EdgeList[num].collapsed = true;
				return num;
			}
			ComponentList[EdgeList[num].firstTri + EdgeList[num].numTris] = tri;
			EdgeList[num].numTris++;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindTriEdge(int v0, int v1)
	{
		int firstTri = VertexList[v0].firstTri;
		for (int i = 0; i < VertexList[v0].numTris; i++)
		{
			int num = ComponentList[firstTri + i];
			if (!TriList[num].collapsed)
			{
				int v2 = TriList[num].v0;
				int v3 = TriList[num].v1;
				int v4 = TriList[num].v2;
				if (((v2 == v0 && v3 == v1) || (v2 == v1 && v3 == v0)) && TriList[num].e0 != -1)
				{
					return TriList[num].e0;
				}
				if (((v3 == v0 && v4 == v1) || (v3 == v1 && v4 == v0)) && TriList[num].e1 != -1)
				{
					return TriList[num].e1;
				}
				if (((v4 == v0 && v2 == v1) || (v4 == v1 && v2 == v0)) && TriList[num].e2 != -1)
				{
					return TriList[num].e2;
				}
			}
		}
		return -1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int AddVertex(int v, int tri, List<Vector3> _vertices, bool addTriToVerts)
	{
		if (VertexList[v].collapsed)
		{
			if (_vertices == null)
			{
				throw new Exception("Error: model vertex not found!");
			}
			NumVerts = ((v + 1 > NumVerts) ? (v + 1) : NumVerts);
			NumOptimizedVerts = NumVerts;
			VertexList[v].collapsed = false;
			VertexList[v].onModelEdge = false;
			VertexList[v].firstTri = AllocateComponents(256);
			VertexList[v].numTris = 0;
			VertexList[v].firstEdge = AllocateComponents(256);
			VertexList[v].numEdges = 0;
			VertexList[v].iterations = 0;
			VertexList[v].sortedPosition = -1;
			VertexList[v].pos = _vertices[v];
		}
		if (addTriToVerts)
		{
			bool flag = false;
			int firstTri = VertexList[v].firstTri;
			int numTris = VertexList[v].numTris;
			for (int i = 0; i < numTris; i++)
			{
				int num = ComponentList[firstTri + i];
				if (TriList[num].collapsed)
				{
					ComponentList[firstTri + i] = tri;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (VertexList[v].numTris == 256)
				{
					VertexList[v].collapsed = true;
					return v;
				}
				ComponentList[VertexList[v].firstTri + VertexList[v].numTris] = tri;
				VertexList[v].numTris++;
			}
		}
		return v;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int AllocateComponents(int num)
	{
		int numComponents = NumComponents;
		NumComponents += num;
		return numComponents;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AllocateArrays(int _numVerts, int _numTriangles)
	{
		NumVerts = 0;
		NumTris = 0;
		NumEdges = 0;
		NumComponents = 0;
		NumOptimizedTris = 0;
		NumOptimizedVerts = 0;
		int num = _numTriangles * 8 * 2;
		if (VertexList == null || VertexList.Length < _numVerts)
		{
			VertexList = new Vertex[_numVerts];
		}
		if (EdgeList == null || EdgeList.Length < num)
		{
			EdgeList = new Edge[num];
		}
		if (TriList == null || TriList.Length < _numTriangles)
		{
			TriList = new Tri[_numTriangles];
		}
		int num2 = _numVerts * 256 * 2 + num * 8;
		if (ComponentList == null || ComponentList.Length < num2)
		{
			ComponentList = new int[num2];
		}
		for (int i = 0; i < _numVerts; i++)
		{
			VertexList[i].collapsed = true;
		}
		for (int j = 0; j < num; j++)
		{
			EdgeList[j].collapsed = true;
		}
		for (int k = 0; k < _numTriangles; k++)
		{
			TriList[k].collapsed = true;
		}
	}
}
