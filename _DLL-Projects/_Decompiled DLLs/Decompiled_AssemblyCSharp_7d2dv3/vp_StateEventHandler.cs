using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class vp_StateEventHandler : vp_EventHandler
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<vp_Component> m_StateTargets = new List<vp_Component>();

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		vp_Component[] componentsInChildren = base.transform.root.GetComponentsInChildren<vp_Component>(includeInactive: true);
		foreach (vp_Component vp_Component2 in componentsInChildren)
		{
			if (vp_Component2.Parent == null || vp_Component2.Parent.GetComponent<vp_Component>() == null)
			{
				m_StateTargets.Add(vp_Component2);
			}
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void BindStateToActivity(vp_Activity a)
	{
		BindStateToActivityOnStart(a);
		BindStateToActivityOnStop(a);
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void BindStateToActivityOnStart(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StartCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StartCallbacks, (vp_Activity.Callback)([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: true, recursive: true);
			}
		}));
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public void BindStateToActivityOnStop(vp_Activity a)
	{
		if (!ActivityInitialized(a))
		{
			return;
		}
		string s = a.EventName;
		a.StopCallbacks = (vp_Activity.Callback)Delegate.Combine(a.StopCallbacks, (vp_Activity.Callback)([PublicizedFrom(EAccessModifier.Internal)] () =>
		{
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(s, enabled: false, recursive: true);
			}
		}));
	}

	[Preserve]
	public void RefreshActivityStates()
	{
		foreach (vp_Event value in m_HandlerEvents.Values)
		{
			if (!(value is vp_Activity) && !(value.GetType().BaseType == typeof(vp_Activity)))
			{
				continue;
			}
			foreach (vp_Component stateTarget in m_StateTargets)
			{
				stateTarget.SetState(value.EventName, ((vp_Activity)value).Active, recursive: true);
			}
		}
	}

	[Preserve]
	public void ResetActivityStates()
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.ResetState();
		}
	}

	[Preserve]
	public void SetState(string state, bool setActive = true, bool recursive = true, bool includeDisabled = false)
	{
		foreach (vp_Component stateTarget in m_StateTargets)
		{
			stateTarget.SetState(state, setActive, recursive, includeDisabled);
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool ActivityInitialized(vp_Activity a)
	{
		if (a == null)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Activity is null.");
			return false;
		}
		if (string.IsNullOrEmpty(a.EventName))
		{
			Debug.LogError("Error: (" + this?.ToString() + ") Activity not initialized. Make sure the event handler has run its Awake call before binding layers.");
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_StateEventHandler()
	{
	}
}
