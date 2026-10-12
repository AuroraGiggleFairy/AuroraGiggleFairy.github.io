using System;
using System.Collections.Generic;
using UnityEngine;

namespace RepairToolsPlus
{
    public struct GridEdge : IEquatable<GridEdge>
    {
        public Vector3i A;
        public Vector3i B;

        public GridEdge(Vector3i p1, Vector3i p2)
        {
            if (p1.x < p2.x || (p1.x == p2.x && (p1.y < p2.y || (p1.y == p2.y && p1.z < p2.z))))
            {
                A = p1;
                B = p2;
            }
            else
            {
                A = p2;
                B = p1;
            }
        }

        public bool Equals(GridEdge other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is GridEdge other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                return (A.GetHashCode() * 397) ^ B.GetHashCode();
            }
        }
    }

    public class TierWireframeRenderer
    {
        public GameObject GameObject;
        public MeshFilter Filter;
        public MeshRenderer Renderer;
        public Material Material;
        public Mesh Mesh;

        public readonly List<Vector3> Vertices = new List<Vector3>(512);
        public readonly List<int> Triangles = new List<int>(1024);

        public void Init(Transform parent, string name, Color color)
        {
            GameObject = new GameObject(name);
            GameObject.transform.SetParent(parent, false);
            GameObject.transform.localPosition = Vector3.zero;

            Filter = GameObject.AddComponent<MeshFilter>();
            Renderer = GameObject.AddComponent<MeshRenderer>();
            Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Renderer.receiveShadows = false;

            Mesh = new Mesh { name = name + "_Mesh" };
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Filter.mesh = Mesh;

            // Load game shader that supports rendering through walls
            Material masterMat = Resources.Load<Material>("Materials/SleeperVolumeFrame");
            if (masterMat != null)
            {
                Material = UnityEngine.Object.Instantiate(masterMat);
            }
            else
            {
                Shader shader = Resources.Load<Shader>("Shaders/SelectionBox")
                             ?? Shader.Find("Game/UI/Sleeper")
                             ?? Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    Material = new Material(shader);
                }
            }

            if (Material != null)
            {
                Material.SetInt(Shader.PropertyToID("_ZTest"), 8); // 8 = Always (render through walls)
                Material.SetInt(Shader.PropertyToID("_Cull"), 0);  // 0 = Off (double-sided)
                Material.renderQueue = 4001; // Render in transparent/overlay queue

                if (Material.HasProperty("_Color"))
                {
                    Material.SetColor(Shader.PropertyToID("_Color"), color);
                }
                Material.color = color;
                Renderer.material = Material;

                Log.Out($"[RepairToolsPlus] Created wireframe renderer '{name}' with shader '{Material.shader?.name}'.");
            }
            else
            {
                Log.Warning($"[RepairToolsPlus] Failed to create wireframe material for '{name}'!");
            }

            GameObject.SetActive(false);
        }

        public void BeginFrame()
        {
            Vertices.Clear();
            Triangles.Clear();
        }

        public void AddBeam(GridEdge edge, float thickness, Vector3 originPos)
        {
            Vector3 vA = edge.A.ToVector3() - originPos;
            Vector3 vB = edge.B.ToVector3() - originPos;
            float h = thickness * 0.5f;

            Vector3 bMin;
            Vector3 bMax;

            if (edge.A.x != edge.B.x)
            {
                bMin = new Vector3(Mathf.Min(vA.x, vB.x) - h, vA.y - h, vA.z - h);
                bMax = new Vector3(Mathf.Max(vA.x, vB.x) + h, vA.y + h, vA.z + h);
            }
            else if (edge.A.y != edge.B.y)
            {
                bMin = new Vector3(vA.x - h, Mathf.Min(vA.y, vB.y) - h, vA.z - h);
                bMax = new Vector3(vA.x + h, Mathf.Max(vA.y, vB.y) + h, vA.z + h);
            }
            else
            {
                bMin = new Vector3(vA.x - h, vA.y - h, Mathf.Min(vA.z, vB.z) - h);
                bMax = new Vector3(vA.x + h, vA.y + h, Mathf.Max(vA.z, vB.z) + h);
            }

            int v = Vertices.Count;

            Vertices.Add(new Vector3(bMin.x, bMin.y, bMin.z)); // 0
            Vertices.Add(new Vector3(bMax.x, bMin.y, bMin.z)); // 1
            Vertices.Add(new Vector3(bMax.x, bMin.y, bMax.z)); // 2
            Vertices.Add(new Vector3(bMin.x, bMin.y, bMax.z)); // 3
            Vertices.Add(new Vector3(bMin.x, bMax.y, bMin.z)); // 4
            Vertices.Add(new Vector3(bMax.x, bMax.y, bMin.z)); // 5
            Vertices.Add(new Vector3(bMax.x, bMax.y, bMax.z)); // 6
            Vertices.Add(new Vector3(bMin.x, bMax.y, bMax.z)); // 7

            // Bottom
            AddQuad(v, 0, 3, 2, 1);
            // Top
            AddQuad(v, 4, 5, 6, 7);
            // Front
            AddQuad(v, 0, 1, 5, 4);
            // Back
            AddQuad(v, 2, 3, 7, 6);
            // Left
            AddQuad(v, 3, 0, 4, 7);
            // Right
            AddQuad(v, 1, 2, 6, 5);
        }

        private void AddQuad(int v, int a, int b, int c, int d)
        {
            Triangles.Add(v + a);
            Triangles.Add(v + b);
            Triangles.Add(v + c);
            Triangles.Add(v + a);
            Triangles.Add(v + c);
            Triangles.Add(v + d);
        }

        public void EndFrame()
        {
            if (Vertices.Count == 0)
            {
                if (GameObject.activeSelf) GameObject.SetActive(false);
                return;
            }

            Mesh.Clear();
            Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Mesh.SetVertices(Vertices);
            Mesh.SetTriangles(Triangles, 0);
            Mesh.RecalculateNormals();

            // Expand bounds to avoid camera frustum culling
            Mesh.bounds = new Bounds(Vector3.zero, new Vector3(2000f, 2000f, 2000f));

            if (!GameObject.activeSelf) GameObject.SetActive(true);
        }

        public void Hide()
        {
            if (GameObject != null && GameObject.activeSelf)
            {
                GameObject.SetActive(false);
            }
        }
    }
}
