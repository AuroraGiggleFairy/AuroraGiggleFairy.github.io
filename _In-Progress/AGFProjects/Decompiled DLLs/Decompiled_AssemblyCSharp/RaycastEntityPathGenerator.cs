using System.Collections;
using System.Collections.Generic;
using RaycastPathing;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class RaycastEntityPathGenerator
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public World GameWorld
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive Entity
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public virtual RaycastPath Path
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isBuildingPath
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool isPathReady
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public RaycastEntityPathGenerator(World _world, EntityAlive _entity)
	{
		GameWorld = _world;
		Entity = _entity;
	}

	public Vector3[] pathToArray()
	{
		Vector3[] array = new Vector3[Path.Nodes.Count - 1];
		for (int i = 0; i < Path.Nodes.Count; i++)
		{
			array[i] = Path.Nodes[i].Position;
		}
		return array;
	}

	public List<Vector3> pathToList()
	{
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < Path.Nodes.Count; i++)
		{
			list.Add(Path.Nodes[i].Position);
		}
		list.Reverse();
		return list;
	}

	public void CreatePath(Vector3 start, Vector3 end, float speed, bool canBreakBlocks, float yHeightOffset = 0f)
	{
		cleanupPath();
		InitPath(start, end);
		beginPathProc();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void InitPath(Vector3 start, Vector3 end)
	{
		Path = new RaycastPath(start, end);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual IEnumerator BuildPathProc()
	{
		finalizePathProc();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void beginPathProc()
	{
		isBuildingPath = true;
		StartCoroutine(BuildPathProc());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void abortPathProc()
	{
		StopCoroutine(BuildPathProc());
		isBuildingPath = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void finalizePathProc()
	{
		isBuildingPath = false;
		isPathReady = true;
	}

	public void Clear()
	{
		cleanupPath();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cleanupPath()
	{
		isPathReady = false;
		if (Path != null)
		{
			abortPathProc();
			Path.Destruct();
			Path = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StartCoroutine(IEnumerator task)
	{
		GameManager.Instance.StartCoroutine(task);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StopCoroutine(IEnumerator task)
	{
		GameManager.Instance.StopCoroutine(task);
	}

	public bool IsConfinedSpace(Vector3 pos, float size, bool debugDraw = false)
	{
		return RaycastPathWorldUtils.IsConfinedSpace(GameWorld, pos, size, debugDraw);
	}
}
