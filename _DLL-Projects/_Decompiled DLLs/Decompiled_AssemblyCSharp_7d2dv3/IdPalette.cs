using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIdPalette", menuName = "ID Maps/ID Palette", order = 0)]
public class IdPalette : ScriptableObject
{
	public List<PaletteEntry> entries = new List<PaletteEntry>();

	public const string ResourcesPath = "IdPalette";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static IdPalette _instance;

	public static IdPalette Instance
	{
		get
		{
			if ((bool)_instance)
			{
				return _instance;
			}
			_instance = Resources.Load<IdPalette>("IdPalette");
			if (_instance == null)
			{
				Debug.LogError("IdPalette.Instance: Couldn't find Resources/IdPalette.asset. Create one IdPalette and place it under a Resources folder.");
			}
			return _instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResetStatic()
	{
		_instance = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (_instance != null && _instance != this)
		{
			Debug.LogWarning("Multiple IdPalette instances loaded. Using the first one: " + _instance.name);
		}
		else
		{
			_instance = this;
		}
	}
}
