using System;
using System.Collections.Generic;
using UnityEngine;

public class POIBoundsHelper : MonoBehaviour
{
	public enum WallVisibilityStates
	{
		None,
		Showing,
		Visible,
		ReadyToHide,
		Hiding
	}

	public List<POIBoundsSideHelper> SideHelpers = new List<POIBoundsSideHelper>();

	public List<POIBoundsSideHelper> ActivatedHelpers = new List<POIBoundsSideHelper>();

	public WallVisibilityStates CurrentState;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float showTime = 3f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float fullAlpha = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (SideHelpers != null && SideHelpers.Count > 0)
		{
			SideHelpers[0].Setup();
			material = new Material(SideHelpers[0].MeshRenderer.material);
			fullAlpha = material.color.a;
			material.color = new Color(material.color.r, material.color.g, material.color.b, 0f);
			for (int i = 0; i < SideHelpers.Count; i++)
			{
				SideHelpers[i].Owner = this;
				SideHelpers[i].Setup();
				SideHelpers[i].MeshRenderer.material = material;
				SideHelpers[i].MeshRenderer.enabled = false;
			}
		}
	}

	public void SetSidesVisible(bool visible)
	{
		for (int i = 0; i < SideHelpers.Count; i++)
		{
			SideHelpers[i].MeshRenderer.enabled = visible;
		}
	}

	public void SetPosition(Vector3 position, Vector3 size)
	{
		base.transform.position = position;
		for (int i = 0; i < SideHelpers.Count; i++)
		{
			SideHelpers[i].SetSize(size);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		switch (CurrentState)
		{
		case WallVisibilityStates.Showing:
		{
			float num2 = Mathf.MoveTowards(material.color.a, fullAlpha, Time.deltaTime);
			material.color = new Color(material.color.r, material.color.g, material.color.b, num2);
			if (num2 == fullAlpha)
			{
				showTime = 3f;
				CurrentState = WallVisibilityStates.Visible;
			}
			break;
		}
		case WallVisibilityStates.ReadyToHide:
			showTime -= Time.deltaTime;
			if (showTime <= 0f)
			{
				CurrentState = WallVisibilityStates.Hiding;
			}
			break;
		case WallVisibilityStates.Hiding:
		{
			float num = Mathf.MoveTowards(material.color.a, 0f, Time.deltaTime);
			material.color = new Color(material.color.r, material.color.g, material.color.b, num);
			if (num == 0f)
			{
				CurrentState = WallVisibilityStates.None;
				SetSidesVisible(visible: false);
			}
			break;
		}
		case WallVisibilityStates.None:
		case WallVisibilityStates.Visible:
			break;
		}
	}

	public void AddSideEntered(POIBoundsSideHelper side)
	{
		if (!ActivatedHelpers.Contains(side))
		{
			ActivatedHelpers.Add(side);
		}
		if (ActivatedHelpers.Count > 0)
		{
			CurrentState = WallVisibilityStates.Showing;
			SetSidesVisible(visible: true);
		}
	}

	public void RemoveSideEntered(POIBoundsSideHelper side)
	{
		if (ActivatedHelpers.Contains(side))
		{
			ActivatedHelpers.Remove(side);
		}
		if (ActivatedHelpers.Count == 0)
		{
			CurrentState = WallVisibilityStates.ReadyToHide;
			showTime = 3f;
		}
	}
}
