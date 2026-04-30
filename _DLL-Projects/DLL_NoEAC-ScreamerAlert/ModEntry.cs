using HarmonyLib;
using UnityEngine;

public class ModEntry : MonoBehaviour
{
    private void Awake()
    {
        // PatchAll is handled in ModAPI.InitMod; keeping this empty avoids duplicate patching.
    }
}
