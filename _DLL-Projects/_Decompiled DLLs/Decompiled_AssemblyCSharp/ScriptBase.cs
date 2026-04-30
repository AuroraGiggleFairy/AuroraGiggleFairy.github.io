using System;
using UnityEngine;

public abstract class ScriptBase : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Transform myTransform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject myGameObject;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public string className;

	public new Transform transform
	{
		get
		{
			if (myTransform != null)
			{
				return myTransform;
			}
			return myTransform = base.transform;
		}
	}

	public new GameObject gameObject
	{
		get
		{
			if (myGameObject != null)
			{
				return myGameObject;
			}
			return myGameObject = base.gameObject;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		className = GetType().Name;
		sbAwake();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbAwake()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		sbUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		sbFixedUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbFixedUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		sbLateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void sbLateUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ScriptBase()
	{
	}
}
