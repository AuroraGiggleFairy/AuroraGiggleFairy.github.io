using UnityEngine;

public class ModEntry : MonoBehaviour
{
	private void Awake()
	{
		HarmonyPatches.ApplyPatches();
	}
}
