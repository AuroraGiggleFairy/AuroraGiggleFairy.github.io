using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ConsoleCmdMemoryProfiler : ConsoleCmdAbstract
{
	[PublicizedFrom(EAccessModifier.Private)]
	public bool enabled;

	public override bool AllowedInMainMenu => true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string[] getCommands()
	{
		return new string[2] { "memprofile", "mprof" };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string getDescription()
	{
		return "Toggles screen Memory Profiler UI";
	}

	public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
	{
		if (!enabled)
		{
			enabled = true;
			UnityMemoryProfilerLabel[] array = Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
			if (array == null || array.Length == 0)
			{
				Object original = Resources.Load("GUI/Prefabs/Debug_ProfilerLabel");
				{
					foreach (UIRoot item in UIRoot.list)
					{
						Transform transform = item.gameObject.transform;
						if (item.gameObject.GetComponentInChildren<UIAnchor>() != null)
						{
							transform = item.gameObject.GetComponentInChildren<UIAnchor>().transform;
						}
						Object.Instantiate(original, transform);
					}
					return;
				}
			}
			UnityMemoryProfilerLabel[] array2 = Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.SetActive(value: true);
			}
		}
		else
		{
			enabled = false;
			UnityMemoryProfilerLabel[] array2 = Object.FindObjectsOfType<UnityMemoryProfilerLabel>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].gameObject.SetActive(value: false);
			}
		}
	}
}
