using System.Collections.Generic;
using RaycastPathing;
using UnityEngine.Scripting;

[Preserve]
public class RaycastPathManager
{
	public static bool DebugModeEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<RaycastPath> paths = new List<RaycastPath>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static RaycastPathManager instance;

	public static RaycastPathManager Instance => instance;

	public static void Init()
	{
		instance = new RaycastPathManager();
		instance._Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _Init()
	{
	}

	public void Add(RaycastPath path)
	{
		if (!paths.Contains(path))
		{
			paths.Add(path);
		}
	}

	public void Remove(RaycastPath path)
	{
		if (paths.Contains(path))
		{
			paths.Remove(path);
		}
	}

	public void Update()
	{
		if (DebugModeEnabled)
		{
			_DebugDraw();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _DebugDraw()
	{
		for (int i = 0; i < paths.Count; i++)
		{
			paths[i].DebugDraw();
		}
	}

	public static implicit operator bool(RaycastPathManager exists)
	{
		return exists != null;
	}
}
