using System;
using System.Collections.Generic;
using UnityEngine;

public class OnPostRenderDispatcher : MonoBehaviour
{
	public static OnPostRenderDispatcher Instance;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<IOnPostRender> listeners = new List<IOnPostRender>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		Instance = this;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnPostRender()
	{
		for (int i = 0; i < listeners.Count; i++)
		{
			listeners[i].OnPostRender();
		}
	}

	public void Add(IOnPostRender _p)
	{
		listeners.Add(_p);
	}

	public void Remove(IOnPostRender _p)
	{
		listeners.Remove(_p);
	}
}
