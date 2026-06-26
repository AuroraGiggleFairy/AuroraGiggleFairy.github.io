using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicMusic;

public class ContentLoader
{
	public static ContentLoader instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine Loader;

	public Queue<IEnumerator> LoadQueue;

	public static ContentLoader Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new ContentLoader();
			}
			return instance;
		}
	}

	public void Start()
	{
		LoadQueue = new Queue<IEnumerator>();
		Loader = GameManager.Instance.StartCoroutine(Load());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator Load()
	{
		while (true)
		{
			yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => LoadQueue.Count > 0);
			yield return LoadQueue.Dequeue();
		}
	}

	public void Cleanup()
	{
		if (Loader != null)
		{
			GameManager.Instance.StopCoroutine(Loader);
		}
		if (LoadQueue != null)
		{
			LoadQueue.Clear();
		}
		Loader = null;
		LoadQueue = null;
	}
}
