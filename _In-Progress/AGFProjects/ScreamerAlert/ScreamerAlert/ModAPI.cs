using System;
using HarmonyLib;
using UnityEngine;


using System;
using HarmonyLib;
using UnityEngine;

namespace ScreamerAlert
{
	public class ModAPI : IModApi
	{
		public void InitMod(Mod modInstance)
		{
			// ...existing code...
			new Harmony("com.agfprojects.screameralert").PatchAll();
			// No explicit NetPackage registration required for custom packages in 7 Days to Die mod API.
			try
			{
				GameObject val = (GameObject)(((object)GameObject.Find("ScreamerAlertManager")) ?? ((object)new GameObject("ScreamerAlertManager")));
				if (((UnityEngine.Object)(object)val.GetComponent<ScreamerAlertManager>()) == (UnityEngine.Object)null)
				{
					val.AddComponent<ScreamerAlertManager>();
				}
				UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)val);
				GameObject val2 = GameObject.Find("ScreamerAlertsController");
				if (((UnityEngine.Object)(object)val2) == (UnityEngine.Object)null)
				{
					val2 = new GameObject("ScreamerAlertsController");
					val2.AddComponent<ScreamerAlertsController>();
				}
				else if (((UnityEngine.Object)(object)val2.GetComponent<ScreamerAlertsController>()) == (UnityEngine.Object)null)
				{
					val2.AddComponent<ScreamerAlertsController>();
				}
				UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)val2);
				_ = ((UnityEngine.Object)(object)val2.GetComponent<ScreamerAlertsController>()) != (UnityEngine.Object)null;
			}
			catch (Exception)
			{
			}
		}
	}
}
