using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdTestLoop : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int cMeshCount = 10000;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh[] meshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform containerT;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform containerBaseT;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[1] { "testloop" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Test code in a loop";
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getHelp()
	{
		return "Commands:\np - player";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (_params.Count == 0)
		{
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output(GetHelp());
			return;
		}
		World world = GameManager.Instance.World;
		string text = _params[0].ToLower();
		MicroStopwatch microStopwatch = new MicroStopwatch();
		switch (text)
		{
		case "container":
		{
			int result = 1;
			if (_params.Count >= 2)
			{
				int.TryParse(_params[1], out result);
			}
			if (!containerT)
			{
				containerT = new GameObject("containerTest").transform;
				containerBaseT = new GameObject("containerBaseTest").transform;
			}
			for (int j = 0; j < result; j++)
			{
				Transform transform = new GameObject("child").transform;
				transform.SetParent(containerT, worldPositionStays: false);
				transform.SetAsFirstSibling();
			}
			break;
		}
		case "containermove":
			if ((bool)containerT)
			{
				if (!containerT.parent)
				{
					containerT.SetParent(containerBaseT, worldPositionStays: false);
					containerT.SetAsFirstSibling();
				}
				else
				{
					containerT.SetParent(null, worldPositionStays: false);
				}
				Log.Warning("container {0} of {1}", containerT.hierarchyCount, containerT.hierarchyCapacity);
			}
			break;
		case "f":
		{
			VoxelMeshTerrain voxelMeshTerrain = new VoxelMeshTerrain(5, 100000);
			Vector2 zero = Vector2.zero;
			for (int k = 0; k < 100000; k++)
			{
				voxelMeshTerrain.Uvs.Add(zero);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			break;
		}
		case "f2":
		{
			VoxelMeshTerrain voxelMeshTerrain2 = new VoxelMeshTerrain(5, 100000);
			Vector2 zero2 = Vector2.zero;
			_ = voxelMeshTerrain2.Uvs;
			for (int num2 = 0; num2 < 100000; num2++)
			{
				voxelMeshTerrain2.m_Uvs.Add(zero2);
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			break;
		}
		case "mat":
		{
			MeshRenderer meshRenderer = new GameObject().AddComponent<MeshRenderer>();
			Material material = (meshRenderer.sharedMaterial = Object.Instantiate(Resources.Load<Material>("Materials/DistantPOI")));
			Material material3 = meshRenderer.material;
			Log.Warning("mat {0} ({1:x}), m2 {2} ({3:x})", material.name, material.GetInstanceID(), material3.name, material3.GetInstanceID());
			Object.Destroy(material);
			Object.Destroy(material);
			Object.Destroy(material);
			break;
		}
		case "meshf":
		{
			MeshFilter meshFilter = new GameObject().AddComponent<MeshFilter>();
			Mesh mesh = new Mesh();
			mesh.name = "Test";
			Vector3[] vertices = new Vector3[1] { Vector3.up };
			int[] triangles = new int[3];
			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			meshFilter.sharedMesh = mesh;
			Mesh mesh2 = meshFilter.mesh;
			Log.Warning("sm {0} ({1:x}), mesh {2} ({3:x}) ", mesh.name, mesh.GetInstanceID(), mesh2.name, mesh2.GetInstanceID());
			Object.Destroy(mesh2);
			break;
		}
		case "meshclr":
			if (meshes != null && (bool)meshes[0])
			{
				for (int n = 0; n < 10000; n++)
				{
					meshes[n].Clear(keepVertexLayout: false);
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time clear {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			}
			break;
		case "meshnew":
		{
			meshes = new Mesh[10000];
			for (int l = 0; l < 10000; l++)
			{
				meshes[l] = new Mesh();
			}
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time new {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			break;
		}
		case "meshd":
			if (meshes != null && (bool)meshes[0])
			{
				for (int num = 0; num < 10000; num++)
				{
					Object.Destroy(meshes[num]);
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time destroy {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			}
			break;
		case "p":
			if (world != null)
			{
				EntityPlayerLocal primaryPlayer2 = world.GetPrimaryPlayer();
				for (int m = 0; m < 10000; m++)
				{
					primaryPlayer2.PlayerStats.UpdatePlayerHealthOT(0.05f);
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			}
			break;
		case "pd":
			if (world != null)
			{
				EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
				for (int i = 0; i < 10000; i++)
				{
					GameUtils.FindDeepChildActive(primaryPlayer.transform, "test");
				}
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output("time {0}ms", (float)microStopwatch.ElapsedMicroseconds * 0.001f);
			}
			break;
		default:
			SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Unknown command " + text);
			break;
		}
	}
}
