using System;
using System.Collections;
using UnityEngine;

public static class GCUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static bool isWorking;

	public static void Collect()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public static void UnloadAndCollectStart()
	{
		isWorking = true;
		ThreadManager.StartCoroutine(UnloadAndCollectCo());
	}

	public static IEnumerator UnloadAndCollectCo()
	{
		isWorking = true;
		Collect();
		yield return Resources.UnloadUnusedAssets();
		Collect();
		isWorking = false;
	}

	public static IEnumerator WaitForIdle()
	{
		while (isWorking)
		{
			yield return null;
		}
	}
}
