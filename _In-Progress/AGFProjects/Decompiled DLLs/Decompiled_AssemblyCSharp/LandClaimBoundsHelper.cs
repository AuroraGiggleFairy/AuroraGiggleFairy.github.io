using System;
using System.Collections.Generic;
using UnityEngine;

public static class LandClaimBoundsHelper
{
	public class BoundsHelperEntry
	{
		public Vector3 Position;

		public Transform Helper;

		public BoundsHelperEntry(Vector3 _position, Transform _helper)
		{
			Position = _position;
			Helper = _helper;
			Origin.OriginChanged = (Action<Vector3>)Delegate.Combine(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnOriginChanged(Vector3 _newOrigin)
		{
			Helper.localPosition = Position - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}

		public void Remove()
		{
			Origin.OriginChanged = (Action<Vector3>)Delegate.Remove(Origin.OriginChanged, new Action<Vector3>(OnOriginChanged));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string landClaimBoundaryMaterialPath = "Materials/LandClaimBoundary";

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform goRoot;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform goPool;

	public static List<BoundsHelperEntry> list = new List<BoundsHelperEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static BoundsHelperEntry GetEntryFromList(Vector3 _worldPos)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Position == _worldPos)
			{
				return list[i];
			}
		}
		return null;
	}

	public static void RemoveBoundsHelper(Vector3 _worldPos)
	{
		Transform transform = null;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Position == _worldPos)
			{
				list[i].Remove();
				transform = list[i].Helper;
				list.RemoveAt(i);
			}
		}
		if (transform != null)
		{
			transform.parent = goPool;
			transform.localPosition = Vector3.zero;
			transform.gameObject.SetActive(value: false);
		}
	}

	public static Transform GetBoundsHelper(Vector3 _worldPos)
	{
		if (goRoot == null)
		{
			InitHelpers();
		}
		Transform transform = null;
		BoundsHelperEntry entryFromList = GetEntryFromList(_worldPos);
		if (entryFromList != null)
		{
			transform = entryFromList.Helper;
		}
		else
		{
			if (goPool.childCount > 0)
			{
				transform = goPool.GetChild(0);
				transform.parent = goRoot;
			}
			else
			{
				List<EntityPlayerLocal> localPlayers = GameManager.Instance.World.GetLocalPlayers();
				if (localPlayers == null || localPlayers.Count <= 0)
				{
					return null;
				}
				_ = LocalPlayerUI.GetUIForPlayer(localPlayers[0]).nguiWindowManager.InGameHUD;
				GameObject gameObject = new GameObject("LandClaimBoundary");
				GameObject gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
				UnityEngine.Object.Destroy(gameObject2.GetComponent<BoxCollider>());
				gameObject2.transform.parent = gameObject.transform;
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				Renderer component = gameObject2.GetComponent<Renderer>();
				Material material = Resources.Load("Materials/LandClaimBoundary", typeof(Material)) as Material;
				component.material = material;
				transform = gameObject.transform;
				transform.transform.parent = goRoot;
			}
			_ = Vector3.one;
			transform.localPosition = new Vector3(0.5f, 0.01f, 0.5f);
			float num = GameStats.GetInt(EnumGameStats.LandClaimSize);
			transform.localScale = new Vector3(num, num * 10000f, num);
			transform.localPosition = _worldPos - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
			list.Add(new BoundsHelperEntry(_worldPos, transform));
		}
		return transform;
	}

	public static void InitHelpers()
	{
		if (goRoot == null)
		{
			goRoot = new GameObject("LandClaimHelpers").transform;
			goPool = new GameObject("Pool").transform;
			goPool.parent = goRoot;
			goPool.localPosition = new Vector3(9999f, 9999f, 9999f);
		}
	}

	public static void CleanupHelpers()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].Remove();
			UnityEngine.Object.Destroy(list[i].Helper);
		}
		list.Clear();
	}
}
