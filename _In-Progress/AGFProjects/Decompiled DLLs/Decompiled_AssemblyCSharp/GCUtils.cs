using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class GCUtils
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_working;

	[PublicizedFrom(EAccessModifier.Private)]
	public static void FullCollect()
	{
		GC.Collect();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PreUnload()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PostUnload()
	{
		GC.WaitForPendingFinalizers();
		FullCollect();
	}

	public static void UnloadAndCollectStart()
	{
		ThreadManager.StartCoroutine(UnloadAndCollectCo());
	}

	public static IEnumerator UnloadAndCollectCo()
	{
		Interlocked.Increment(ref m_working);
		try
		{
			Task preUnload = Task.Run((Action)PreUnload);
			yield return Resources.UnloadUnusedAssets();
			while (!preUnload.IsCompleted)
			{
				yield return null;
			}
			Task postUnload = Task.Run((Action)PostUnload);
			while (!postUnload.IsCompleted)
			{
				yield return null;
			}
		}
		finally
		{
			Interlocked.Decrement(ref m_working);
		}
	}

	public static IEnumerator WaitForIdle()
	{
		while (m_working > 0)
		{
			yield return null;
		}
	}
}
