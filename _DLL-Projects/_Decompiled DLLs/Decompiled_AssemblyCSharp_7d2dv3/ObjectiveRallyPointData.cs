using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveRallyPointData : MonoBehaviour
{
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] Flags;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] FlagParents;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject Root;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject HelperFlag;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject Highlight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<GameObject, List<GameObject>> flagNodes = new Dictionary<GameObject, List<GameObject>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<GameObject, List<GameObject>> highlightNodes = new Dictionary<GameObject, List<GameObject>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int[] flagCounts;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int highlightFlag = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		for (int i = 0; i < FlagParents.Length; i++)
		{
			GameObject gameObject = FlagParents[i];
			List<GameObject> list = new List<GameObject>();
			List<GameObject> list2 = new List<GameObject>();
			Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (gameObject.transform != transform)
				{
					GameObject gameObject2 = transform.gameObject;
					if (gameObject2.name.EndsWith("Highlight"))
					{
						list2.Add(gameObject2);
					}
					else
					{
						list.Add(gameObject2);
					}
				}
			}
			list.Sort([PublicizedFrom(EAccessModifier.Internal)] (GameObject x, GameObject y) => x.name.CompareTo(y.name));
			flagNodes.Add(gameObject, list);
			list2.Sort([PublicizedFrom(EAccessModifier.Internal)] (GameObject x, GameObject y) => x.name.CompareTo(y.name));
			highlightNodes.Add(gameObject, list2);
			flagCounts = new int[Flags.Length];
		}
		GameObject[] flags = Flags;
		for (int j = 0; j < flags.Length; j++)
		{
			flags[j].SetActive(value: false);
		}
		HelperFlag.SetActive(value: true);
		Highlight.SetActive(value: false);
		Root.SetActive(GameManager.Instance.World.IsEditor());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	public void ClearAllFlags()
	{
		for (int i = 0; i < Flags.Length; i++)
		{
			flagCounts[i] = 0;
		}
		highlightFlag = -1;
	}

	public void AddFlag(string flagName, bool highLight)
	{
		for (int i = 0; i < Flags.Length; i++)
		{
			if (Flags[i].name.Equals(flagName))
			{
				if (highLight)
				{
					highlightFlag = i;
				}
				flagCounts[i]++;
			}
		}
	}

	public void RemoveFlag(string flagName)
	{
		for (int i = 0; i < Flags.Length; i++)
		{
			if (Flags[i].name.Equals(flagName) && flagCounts[i] > 0)
			{
				flagCounts[i]--;
			}
		}
	}

	public void UpdateAllFlags()
	{
		int i;
		for (i = 0; i < flagCounts.Length && flagCounts[i] <= 0; i++)
		{
		}
		bool flag = i < flagCounts.Length;
		Root.SetActive(flag);
		Highlight.SetActive(highlightFlag >= 0);
		if (!flag)
		{
			return;
		}
		Highlight.SetActive(value: false);
		HelperFlag.SetActive(value: false);
		List<GameObject> list = new List<GameObject>();
		int num = -1;
		for (i = 0; i < flagCounts.Length; i++)
		{
			bool flag2 = flagCounts[i] > 0;
			GameObject gameObject = Flags[i];
			if (flag2)
			{
				if (i == highlightFlag)
				{
					num = list.Count;
				}
				list.Add(gameObject);
			}
			gameObject.SetActive(flag2);
		}
		int count = list.Count;
		for (i = 0; i < count; i++)
		{
			Transform parent = flagNodes[FlagParents[count - 1]][i].transform;
			list[i].transform.SetParent(parent, worldPositionStays: false);
			if (i == num)
			{
				Highlight.transform.SetParent(highlightNodes[FlagParents[count - 1]][i].transform, worldPositionStays: false);
				Highlight.SetActive(value: true);
			}
		}
	}

	public GameObject GetFlagParentNode(int flagCount, int flagIndex)
	{
		if (flagCount >= 0 && flagCount < FlagParents.Length)
		{
			List<GameObject> list = flagNodes[FlagParents[flagCount]];
			if (flagIndex >= 0 && flagIndex < list.Count)
			{
				return list[flagIndex];
			}
		}
		return null;
	}
}
