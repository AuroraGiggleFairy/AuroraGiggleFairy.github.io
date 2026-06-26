using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vp_Timer : MonoBehaviour
{
	public delegate void Callback();

	public delegate void ArgCallback(object args);

	public struct Stats
	{
		public int Created;

		public int Inactive;

		public int Active;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class Event
	{
		public int Id;

		public Callback Function;

		public ArgCallback ArgFunction;

		public object Arguments;

		public int Iterations = 1;

		public float Interval = -1f;

		public float DueTime;

		public float StartTime;

		public float LifeTime;

		public bool Paused;

		public bool CancelOnLoad = true;

		public string MethodName
		{
			get
			{
				if (Function != null)
				{
					if (Function.Method != null)
					{
						if (Function.Method.Name[0] == '<')
						{
							return "delegate";
						}
						return Function.Method.Name;
					}
				}
				else if (ArgFunction != null && ArgFunction.Method != null)
				{
					if (ArgFunction.Method.Name[0] == '<')
					{
						return "delegate";
					}
					return ArgFunction.Method.Name;
				}
				return null;
			}
		}

		public string MethodInfo
		{
			get
			{
				string methodName = MethodName;
				if (!string.IsNullOrEmpty(methodName))
				{
					methodName += "(";
					if (Arguments != null)
					{
						if (Arguments.GetType().IsArray)
						{
							object[] array = (object[])Arguments;
							object[] array2 = array;
							foreach (object obj in array2)
							{
								methodName += obj.ToString();
								if (Array.IndexOf(array, obj) < array.Length - 1)
								{
									methodName += ", ";
								}
							}
						}
						else
						{
							methodName += Arguments;
						}
					}
					return methodName + ")";
				}
				return "(function = null)";
			}
		}

		public void Execute()
		{
			if (Id == 0 || DueTime == 0f)
			{
				Recycle();
				return;
			}
			if (Function != null)
			{
				Function();
			}
			else
			{
				if (ArgFunction == null)
				{
					Error("Aborted event because function is null.");
					Recycle();
					return;
				}
				ArgFunction(Arguments);
			}
			if (Iterations > 0)
			{
				Iterations--;
				if (Iterations < 1)
				{
					Recycle();
					return;
				}
			}
			DueTime = Time.time + Interval;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Recycle()
		{
			Id = 0;
			DueTime = 0f;
			StartTime = 0f;
			CancelOnLoad = true;
			Function = null;
			ArgFunction = null;
			Arguments = null;
			if (m_Active.Remove(this))
			{
				m_Pool.Add(this);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Destroy()
		{
			m_Active.Remove(this);
			m_Pool.Remove(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Error(string message)
		{
			Debug.LogError("Error: (" + this?.ToString() + ") " + message);
		}
	}

	public class Handle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public Event m_Event;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_Id;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_StartIterations = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public float m_FirstDueTime;

		public bool Paused
		{
			get
			{
				if (Active)
				{
					return m_Event.Paused;
				}
				return false;
			}
			set
			{
				if (Active)
				{
					m_Event.Paused = value;
				}
			}
		}

		public float TimeOfInitiation
		{
			get
			{
				if (Active)
				{
					return m_Event.StartTime;
				}
				return 0f;
			}
		}

		public float TimeOfFirstIteration
		{
			get
			{
				if (Active)
				{
					return m_FirstDueTime;
				}
				return 0f;
			}
		}

		public float TimeOfNextIteration
		{
			get
			{
				if (Active)
				{
					return m_Event.DueTime;
				}
				return 0f;
			}
		}

		public float TimeOfLastIteration
		{
			get
			{
				if (Active)
				{
					return Time.time + DurationLeft;
				}
				return 0f;
			}
		}

		public float Delay => Mathf.Round((m_FirstDueTime - TimeOfInitiation) * 1000f) / 1000f;

		public float Interval
		{
			get
			{
				if (Active)
				{
					return m_Event.Interval;
				}
				return 0f;
			}
		}

		public float TimeUntilNextIteration
		{
			get
			{
				if (Active)
				{
					return m_Event.DueTime - Time.time;
				}
				return 0f;
			}
		}

		public float DurationLeft
		{
			get
			{
				if (Active)
				{
					return TimeUntilNextIteration + (float)(m_Event.Iterations - 1) * m_Event.Interval;
				}
				return 0f;
			}
		}

		public float DurationTotal
		{
			get
			{
				if (Active)
				{
					return Delay + (float)m_StartIterations * ((m_StartIterations > 1) ? Interval : 0f);
				}
				return 0f;
			}
		}

		public float Duration
		{
			get
			{
				if (Active)
				{
					return m_Event.LifeTime;
				}
				return 0f;
			}
		}

		public int IterationsTotal => m_StartIterations;

		public int IterationsLeft
		{
			get
			{
				if (Active)
				{
					return m_Event.Iterations;
				}
				return 0;
			}
		}

		public int Id
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;
				if (m_Id == 0)
				{
					m_Event.DueTime = 0f;
					return;
				}
				m_Event = null;
				for (int num = m_Active.Count - 1; num > -1; num--)
				{
					if (m_Active[num].Id == m_Id)
					{
						m_Event = m_Active[num];
						break;
					}
				}
				if (m_Event == null)
				{
					Debug.LogError("Error: (" + this?.ToString() + ") Failed to assign event with Id '" + m_Id + "'.");
				}
				m_StartIterations = m_Event.Iterations;
				m_FirstDueTime = m_Event.DueTime;
			}
		}

		public bool Active
		{
			get
			{
				if (m_Event == null || Id == 0 || m_Event.Id == 0)
				{
					return false;
				}
				return m_Event.Id == Id;
			}
		}

		public string MethodName
		{
			get
			{
				if (Active)
				{
					return m_Event.MethodName;
				}
				return "";
			}
		}

		public string MethodInfo
		{
			get
			{
				if (Active)
				{
					return m_Event.MethodInfo;
				}
				return "";
			}
		}

		public bool CancelOnLoad
		{
			get
			{
				if (Active)
				{
					return m_Event.CancelOnLoad;
				}
				return true;
			}
			set
			{
				if (Active)
				{
					m_Event.CancelOnLoad = value;
				}
				else
				{
					Debug.LogWarning("Warning: (" + this?.ToString() + ") Tried to set CancelOnLoad on inactive timer handle.");
				}
			}
		}

		public void Cancel()
		{
			vp_Timer.Cancel(this);
		}

		public void Execute()
		{
			m_Event.DueTime = Time.time;
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject m_MainObject = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Event> m_Active = new List<Event>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Event> m_Pool = new List<Event>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static Event m_NewEvent = null;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_EventCount = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_EventBatch = 0;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int m_EventIterator = 0;

	public static int MaxEventsPerFrame = 500;

	public bool WasAddedCorrectly
	{
		get
		{
			if (!Application.isPlaying)
			{
				return false;
			}
			if (base.gameObject != m_MainObject)
			{
				return false;
			}
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		if (!WasAddedCorrectly)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		m_EventBatch = 0;
		while (m_Active.Count > 0 && m_EventBatch < MaxEventsPerFrame)
		{
			if (m_EventIterator < 0)
			{
				m_EventIterator = m_Active.Count - 1;
				break;
			}
			if (m_EventIterator > m_Active.Count - 1)
			{
				m_EventIterator = m_Active.Count - 1;
			}
			if (Time.time >= m_Active[m_EventIterator].DueTime || m_Active[m_EventIterator].Id == 0)
			{
				m_Active[m_EventIterator].Execute();
			}
			else if (m_Active[m_EventIterator].Paused)
			{
				m_Active[m_EventIterator].DueTime += Time.deltaTime;
			}
			else
			{
				m_Active[m_EventIterator].LifeTime += Time.deltaTime;
			}
			m_EventIterator--;
			m_EventBatch++;
		}
	}

	public static void In(float delay, Callback callback, Handle timerHandle = null)
	{
		Schedule(delay, callback, null, null, timerHandle, 1, -1f);
	}

	public static void In(float delay, Callback callback, int iterations, Handle timerHandle = null)
	{
		Schedule(delay, callback, null, null, timerHandle, iterations, -1f);
	}

	public static void In(float delay, Callback callback, int iterations, float interval, Handle timerHandle = null)
	{
		Schedule(delay, callback, null, null, timerHandle, iterations, interval);
	}

	public static void In(float delay, ArgCallback callback, object arguments, Handle timerHandle = null)
	{
		Schedule(delay, null, callback, arguments, timerHandle, 1, -1f);
	}

	public static void In(float delay, ArgCallback callback, object arguments, int iterations, Handle timerHandle = null)
	{
		Schedule(delay, null, callback, arguments, timerHandle, iterations, -1f);
	}

	public static void In(float delay, ArgCallback callback, object arguments, int iterations, float interval, Handle timerHandle = null)
	{
		Schedule(delay, null, callback, arguments, timerHandle, iterations, interval);
	}

	public static void Start(Handle timerHandle)
	{
		Schedule(315360000f, [PublicizedFrom(EAccessModifier.Internal)] () =>
		{
		}, null, null, timerHandle, 1, -1f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Schedule(float time, Callback func, ArgCallback argFunc, object args, Handle timerHandle, int iterations, float interval)
	{
		if (func == null && argFunc == null)
		{
			Debug.LogError("Error: (vp_Timer) Aborted event because function is null.");
			return;
		}
		if (m_MainObject == null)
		{
			m_MainObject = new GameObject("Timers");
			m_MainObject.AddComponent<vp_Timer>();
			UnityEngine.Object.DontDestroyOnLoad(m_MainObject);
		}
		time = Mathf.Max(0f, time);
		iterations = Mathf.Max(0, iterations);
		interval = ((interval == -1f) ? time : Mathf.Max(0f, interval));
		m_NewEvent = null;
		if (m_Pool.Count > 0)
		{
			m_NewEvent = m_Pool[0];
			m_Pool.Remove(m_NewEvent);
		}
		else
		{
			m_NewEvent = new Event();
		}
		m_EventCount++;
		m_NewEvent.Id = m_EventCount;
		if (func != null)
		{
			m_NewEvent.Function = func;
		}
		else if (argFunc != null)
		{
			m_NewEvent.ArgFunction = argFunc;
			m_NewEvent.Arguments = args;
		}
		m_NewEvent.StartTime = Time.time;
		m_NewEvent.DueTime = Time.time + time;
		m_NewEvent.Iterations = iterations;
		m_NewEvent.Interval = interval;
		m_NewEvent.LifeTime = 0f;
		m_NewEvent.Paused = false;
		m_Active.Add(m_NewEvent);
		if (timerHandle != null)
		{
			if (timerHandle.Active)
			{
				timerHandle.Cancel();
			}
			timerHandle.Id = m_NewEvent.Id;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Cancel(Handle handle)
	{
		if (handle != null && handle.Active)
		{
			handle.Id = 0;
		}
	}

	public static void CancelAll()
	{
		for (int num = m_Active.Count - 1; num > -1; num--)
		{
			m_Active[num].Id = 0;
		}
	}

	public static void CancelAll(string methodName)
	{
		for (int num = m_Active.Count - 1; num > -1; num--)
		{
			if (m_Active[num].MethodName == methodName)
			{
				m_Active[num].Id = 0;
			}
		}
	}

	public static void DestroyAll()
	{
		m_Active.Clear();
		m_Pool.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += NotifyLevelWasLoaded;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= NotifyLevelWasLoaded;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		for (int num = m_Active.Count - 1; num > -1; num--)
		{
			if (m_Active[num].CancelOnLoad)
			{
				m_Active[num].Id = 0;
			}
		}
	}

	public static Stats EditorGetStats()
	{
		Stats result = default(Stats);
		result.Created = m_Active.Count + m_Pool.Count;
		result.Inactive = m_Pool.Count;
		result.Active = m_Active.Count;
		return result;
	}

	public static string EditorGetMethodInfo(int eventIndex)
	{
		if (eventIndex < 0 || eventIndex > m_Active.Count - 1)
		{
			return "Argument out of range.";
		}
		return m_Active[eventIndex].MethodInfo;
	}

	public static int EditorGetMethodId(int eventIndex)
	{
		if (eventIndex < 0 || eventIndex > m_Active.Count - 1)
		{
			return 0;
		}
		return m_Active[eventIndex].Id;
	}
}
