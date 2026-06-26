using System.Collections.Generic;
using UnityEngine;

public class DetectLeaks : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGUI()
	{
		Object[] array = Object.FindObjectsOfType(typeof(Object));
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string key = array2[i].GetType().ToString();
			if (dictionary.ContainsKey(key))
			{
				dictionary[key]++;
			}
			else
			{
				dictionary[key] = 1;
			}
		}
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(dictionary);
		list.Sort([PublicizedFrom(EAccessModifier.Internal)] (KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair) => nextPair.Value.CompareTo(firstPair.Value));
		foreach (KeyValuePair<string, int> item in list)
		{
			GUILayout.Label(item.Key + ": " + item.Value);
		}
	}
}
