using UnityEngine;

public static class ServerHelper
{
	public static void SetupForServer(GameObject obj)
	{
		Component[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = false;
		}
	}
}
