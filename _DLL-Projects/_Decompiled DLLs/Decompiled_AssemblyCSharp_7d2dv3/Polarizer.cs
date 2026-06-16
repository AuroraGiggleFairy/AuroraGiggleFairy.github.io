using System;
using UnityEngine;

public class Polarizer : MonoBehaviour
{
	public enum ViewEnums
	{
		None,
		Normals,
		Albedo,
		Specular
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	public static ViewEnums currDebugView;

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Start()
	{
		Shader shader = GlobalAssets.FindShader("Custom/DebugView");
		if (shader != null)
		{
			material = new Material(shader);
		}
	}

	public static void SetDebugView(ViewEnums view)
	{
		currDebugView = view;
	}

	public static ViewEnums GetDebugView()
	{
		return currDebugView;
	}

	public void OnPreRender()
	{
	}

	public void OnPostRender()
	{
	}
}
