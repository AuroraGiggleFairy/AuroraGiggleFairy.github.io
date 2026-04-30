using System.Collections.Generic;
using UnityEngine;

public class MicroSplatKeywords : ScriptableObject
{
	public List<string> keywords = new List<string>();

	public int drawOrder = 100;

	public bool IsKeywordEnabled(string k)
	{
		return keywords.Contains(k);
	}

	public void EnableKeyword(string k)
	{
		if (!IsKeywordEnabled(k))
		{
			keywords.Add(k);
		}
	}

	public void DisableKeyword(string k)
	{
		if (IsKeywordEnabled(k))
		{
			keywords.Remove(k);
		}
	}
}
