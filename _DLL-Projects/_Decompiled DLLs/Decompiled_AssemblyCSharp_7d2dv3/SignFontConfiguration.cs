using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu]
public class SignFontConfiguration : ScriptableObject
{
	[Serializable]
	public class FontInfo
	{
		public string name;

		public TMP_FontAsset fontAsset;
	}

	public List<FontInfo> fontInfos = new List<FontInfo>();
}
