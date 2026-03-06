using System;
using UnityEngine;

[Serializable]
public class vp_ItemType : ScriptableObject
{
	public string IndefiniteArticle = "a";

	public string DisplayName;

	public string Description;

	public Texture2D Icon;

	public float Space;

	[SerializeField]
	public string DisplayNameFull => IndefiniteArticle + " " + DisplayName;
}
