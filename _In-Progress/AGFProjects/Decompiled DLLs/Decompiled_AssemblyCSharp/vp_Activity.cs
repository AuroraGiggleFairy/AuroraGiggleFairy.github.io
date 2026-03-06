using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_Activity : vp_Event
{
	public delegate void Callback();

	public delegate bool Condition();

	public Callback StartCallbacks;

	public Callback StopCallbacks;

	public Condition StartConditions;

	public Condition StopConditions;

	public Callback FailStartCallbacks;

	public Callback FailStopCallbacks;

	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Timer.Handle m_ForceStopTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	public object m_Argument;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool m_Active;

	public float NextAllowedStartTime;

	public float NextAllowedStopTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_MinPause;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_MinDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	public float m_MaxDuration = -1f;

	public float MinPause
	{
		get
		{
			return m_MinPause;
		}
		set
		{
			m_MinPause = Mathf.Max(0f, value);
		}
	}

	public float MinDuration
	{
		get
		{
			return m_MinDuration;
		}
		set
		{
			m_MinDuration = Mathf.Max(0.001f, value);
			if (m_MaxDuration != -1f && m_MinDuration > m_MaxDuration)
			{
				m_MinDuration = m_MaxDuration;
				Debug.LogWarning("Warning: (vp_Activity) Tried to set MinDuration longer than MaxDuration for '" + base.EventName + "'. Capping at MaxDuration.");
			}
		}
	}

	public float AutoDuration
	{
		get
		{
			return m_MaxDuration;
		}
		set
		{
			if (value == -1f)
			{
				m_MaxDuration = value;
				return;
			}
			m_MaxDuration = Mathf.Max(0.001f, value);
			if (m_MaxDuration < m_MinDuration)
			{
				m_MaxDuration = m_MinDuration;
				Debug.LogWarning("Warning: (vp_Activity) Tried to set MaxDuration shorter than MinDuration for '" + base.EventName + "'. Capping at MinDuration.");
			}
		}
	}

	public object Argument
	{
		get
		{
			if (m_ArgumentType == null)
			{
				Debug.LogError("Error: (" + this?.ToString() + ") Tried to fetch argument from '" + base.EventName + "' but this activity takes no parameters.");
				return null;
			}
			return m_Argument;
		}
		set
		{
			if (m_ArgumentType == null)
			{
				Debug.LogError("Error: (" + this?.ToString() + ") Tried to set argument for '" + base.EventName + "' but this activity takes no parameters.");
			}
			else
			{
				m_Argument = value;
			}
		}
	}

	public bool Active
	{
		get
		{
			return m_Active;
		}
		set
		{
			if (value && !m_Active)
			{
				m_Active = true;
				StartCallbacks();
				NextAllowedStopTime = Time.time + m_MinDuration;
				if (m_MaxDuration > 0f)
				{
					vp_Timer.In(m_MaxDuration, [PublicizedFrom(EAccessModifier.Private)] () =>
					{
						Stop();
					}, m_ForceStopTimer);
				}
			}
			else if (!value && m_Active)
			{
				m_Active = false;
				StopCallbacks();
				NextAllowedStartTime = Time.time + m_MinPause;
				m_Argument = null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void Empty()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool AlwaysOK()
	{
		return true;
	}

	public vp_Activity(string name)
		: base(name)
	{
		InitFields();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitFields()
	{
		m_DelegateTypes = new Type[6]
		{
			typeof(Callback),
			typeof(Callback),
			typeof(Condition),
			typeof(Condition),
			typeof(Callback),
			typeof(Callback)
		};
		m_Fields = new FieldInfo[6]
		{
			GetType().GetField("StartCallbacks"),
			GetType().GetField("StopCallbacks"),
			GetType().GetField("StartConditions"),
			GetType().GetField("StopConditions"),
			GetType().GetField("FailStartCallbacks"),
			GetType().GetField("FailStopCallbacks")
		};
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[6]
		{
			GetType().GetMethod("Empty"),
			GetType().GetMethod("Empty"),
			GetType().GetMethod("AlwaysOK"),
			GetType().GetMethod("AlwaysOK"),
			GetType().GetMethod("Empty"),
			GetType().GetMethod("Empty")
		};
		Prefixes = new Dictionary<string, int>
		{
			{ "OnStart_", 0 },
			{ "OnStop_", 1 },
			{ "CanStart_", 2 },
			{ "CanStop_", 3 },
			{ "OnFailStart_", 4 },
			{ "OnFailStop_", 5 }
		};
		StartCallbacks = Empty;
		StopCallbacks = Empty;
		StartConditions = AlwaysOK;
		StopConditions = AlwaysOK;
		FailStartCallbacks = Empty;
		FailStopCallbacks = Empty;
	}

	public override void Register(object t, string m, int v)
	{
		AddExternalMethodToField(t, m_Fields[v], m, m_DelegateTypes[v]);
		Refresh();
	}

	public override void Unregister(object t)
	{
		RemoveExternalMethodFromField(t, m_Fields[0]);
		RemoveExternalMethodFromField(t, m_Fields[1]);
		RemoveExternalMethodFromField(t, m_Fields[2]);
		RemoveExternalMethodFromField(t, m_Fields[3]);
		RemoveExternalMethodFromField(t, m_Fields[4]);
		RemoveExternalMethodFromField(t, m_Fields[5]);
		Refresh();
	}

	public bool TryStart(bool startIfAllowed = true)
	{
		if (m_Active)
		{
			return false;
		}
		if (Time.time < NextAllowedStartTime)
		{
			m_Argument = null;
			return false;
		}
		Delegate[] invocationList = StartConditions.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			if (!((Condition)invocationList[i])())
			{
				m_Argument = null;
				if (startIfAllowed)
				{
					FailStartCallbacks();
				}
				return false;
			}
		}
		if (startIfAllowed)
		{
			Active = true;
		}
		return true;
	}

	public bool TryStop(bool stopIfAllowed = true)
	{
		if (!m_Active)
		{
			return false;
		}
		if (Time.time < NextAllowedStopTime)
		{
			return false;
		}
		Delegate[] invocationList = StopConditions.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			if (!((Condition)invocationList[i])())
			{
				if (stopIfAllowed)
				{
					FailStopCallbacks();
				}
				return false;
			}
		}
		if (stopIfAllowed)
		{
			Active = false;
		}
		return true;
	}

	public void Start(float forcedActiveDuration = 0f)
	{
		Active = true;
		if (forcedActiveDuration > 0f)
		{
			NextAllowedStopTime = Time.time + forcedActiveDuration;
		}
	}

	public void Stop(float forcedPauseDuration = 0f)
	{
		Active = false;
		if (forcedPauseDuration > 0f)
		{
			NextAllowedStartTime = Time.time + forcedPauseDuration;
		}
	}

	public void Disallow(float duration)
	{
		NextAllowedStartTime = Time.time + duration;
	}
}
public class vp_Activity<V> : vp_Activity
{
	public vp_Activity(string name)
		: base(name)
	{
	}

	public bool TryStart<T>(T argument)
	{
		if (m_Active)
		{
			return false;
		}
		m_Argument = argument;
		return TryStart();
	}
}
