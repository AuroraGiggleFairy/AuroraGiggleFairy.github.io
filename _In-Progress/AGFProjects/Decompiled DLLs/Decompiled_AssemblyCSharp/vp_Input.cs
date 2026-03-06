using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_Input : MonoBehaviour
{
	[Serializable]
	public class vp_InputAxis
	{
		public KeyCode Positive;

		public KeyCode Negative;
	}

	public int ControlType;

	public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();

	public List<string> ButtonKeys = new List<string>();

	public List<KeyCode> ButtonValues = new List<KeyCode>();

	public Dictionary<string, vp_InputAxis> Axis = new Dictionary<string, vp_InputAxis>();

	public List<string> AxisKeys = new List<string>();

	public List<vp_InputAxis> AxisValues = new List<vp_InputAxis>();

	public List<string> UnityAxis = new List<string>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string m_FolderPath = "UltimateFPS/Content/Resources/Input";

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string m_PrefabPath = "Assets/UltimateFPS/Content/Resources/Input/vp_Input.prefab";

	public static bool mIsDirty = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static vp_Input m_Instance;

	public static vp_Input Instance
	{
		get
		{
			if (mIsDirty)
			{
				mIsDirty = false;
				if (m_Instance == null)
				{
					if (Application.isPlaying)
					{
						GameObject gameObject = Resources.Load("Input/vp_Input") as GameObject;
						if (gameObject == null)
						{
							m_Instance = new GameObject("vp_Input").AddComponent<vp_Input>();
						}
						else
						{
							m_Instance = gameObject.GetComponent<vp_Input>();
							if (m_Instance == null)
							{
								m_Instance = gameObject.AddComponent<vp_Input>();
							}
						}
					}
					m_Instance.SetupDefaults();
				}
			}
			return m_Instance;
		}
	}

	public static void CreateIfNoExist()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		if (m_Instance == null)
		{
			m_Instance = Instance;
		}
	}

	public virtual void SetDirty(bool dirty)
	{
		mIsDirty = dirty;
	}

	public virtual void SetupDefaults(string type = "")
	{
		if ((type == "" || type == "Buttons") && ButtonKeys.Count == 0)
		{
			AddButton("Attack", KeyCode.Mouse0);
			AddButton("SetNextWeapon", KeyCode.E);
			AddButton("SetPrevWeapon", KeyCode.Q);
			AddButton("ClearWeapon", KeyCode.Backspace);
			AddButton("Zoom", KeyCode.Mouse1);
			AddButton("Reload", KeyCode.R);
			AddButton("Jump", KeyCode.Space);
			AddButton("Crouch", KeyCode.C);
			AddButton("Run", KeyCode.LeftShift);
			AddButton("Interact", KeyCode.F);
			AddButton("Accept1", KeyCode.Return);
			AddButton("Accept2", KeyCode.KeypadEnter);
			AddButton("Pause", KeyCode.P);
			AddButton("Menu", KeyCode.Escape);
		}
		if ((type == "" || type == "Axis") && AxisKeys.Count == 0)
		{
			AddAxis("Vertical", KeyCode.W, KeyCode.S);
			AddAxis("Horizontal", KeyCode.D, KeyCode.A);
		}
		if ((type == "" || type == "UnityAxis") && UnityAxis.Count == 0)
		{
			AddUnityAxis("Mouse X");
			AddUnityAxis("Mouse Y");
		}
		UpdateDictionaries();
	}

	public virtual void AddButton(string n, KeyCode k = KeyCode.None)
	{
		if (ButtonKeys.Contains(n))
		{
			ButtonValues[ButtonKeys.IndexOf(n)] = k;
			return;
		}
		ButtonKeys.Add(n);
		ButtonValues.Add(k);
	}

	public virtual void AddAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None)
	{
		if (AxisKeys.Contains(n))
		{
			AxisValues[AxisKeys.IndexOf(n)] = new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
		}
		else
		{
			AxisKeys.Add(n);
			AxisValues.Add(new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			});
		}
	}

	public virtual void AddUnityAxis(string n)
	{
		if (UnityAxis.Contains(n))
		{
			UnityAxis[UnityAxis.IndexOf(n)] = n;
		}
		else
		{
			UnityAxis.Add(n);
		}
	}

	public virtual void UpdateDictionaries()
	{
		if (Application.isPlaying)
		{
			Buttons.Clear();
			for (int i = 0; i < ButtonKeys.Count; i++)
			{
				Buttons.Add(ButtonKeys[i], ButtonValues[i]);
			}
			Axis.Clear();
			for (int j = 0; j < AxisKeys.Count; j++)
			{
				Axis.Add(AxisKeys[j], new vp_InputAxis
				{
					Positive = AxisValues[j].Positive,
					Negative = AxisValues[j].Negative
				});
			}
		}
	}

	public static bool GetButtonAny(string button)
	{
		return Instance.DoGetButtonAny(button);
	}

	public virtual bool DoGetButtonAny(string button)
	{
		if (Buttons.ContainsKey(button))
		{
			if (!Input.GetKey(Buttons[button]) && !Input.GetKeyDown(Buttons[button]))
			{
				return Input.GetKeyUp(Buttons[button]);
			}
			return true;
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButton(string button)
	{
		return Instance.DoGetButton(button);
	}

	public virtual bool DoGetButton(string button)
	{
		if (Buttons.ContainsKey(button))
		{
			return Input.GetKey(Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButtonDown(string button)
	{
		return Instance.DoGetButtonDown(button);
	}

	public virtual bool DoGetButtonDown(string button)
	{
		if (Buttons.ContainsKey(button))
		{
			return Input.GetKeyDown(Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static bool GetButtonUp(string button)
	{
		return Instance.DoGetButtonUp(button);
	}

	public virtual bool DoGetButtonUp(string button)
	{
		if (Buttons.ContainsKey(button))
		{
			return Input.GetKeyUp(Buttons[button]);
		}
		Debug.LogError("\"" + button + "\" is not in VP Input Manager's Buttons. You must add it for this Button to work.");
		return false;
	}

	public static float GetAxisRaw(string axis)
	{
		return Instance.DoGetAxisRaw(axis);
	}

	public virtual float DoGetAxisRaw(string axis)
	{
		if (Axis.ContainsKey(axis) && ControlType == 0)
		{
			float result = 0f;
			if (Input.GetKey(Axis[axis].Positive))
			{
				result = 1f;
			}
			if (Input.GetKey(Axis[axis].Negative))
			{
				result = -1f;
			}
			return result;
		}
		if (UnityAxis.Contains(axis))
		{
			return Input.GetAxisRaw(axis);
		}
		Debug.LogError("\"" + axis + "\" is not in VP Input Manager's Unity Axis. You must add it for this Axis to work.");
		return 0f;
	}

	public static void ChangeButtonKey(string button, KeyCode keyCode, bool save = false)
	{
		if (!Instance.Buttons.ContainsKey(button))
		{
			Debug.LogWarning("The Button \"" + button + "\" Doesn't Exist");
			return;
		}
		if (save)
		{
			Instance.ButtonValues[Instance.ButtonKeys.IndexOf(button)] = keyCode;
		}
		Instance.Buttons[button] = keyCode;
	}

	public static void ChangeAxis(string n, KeyCode pk = KeyCode.None, KeyCode nk = KeyCode.None, bool save = false)
	{
		if (!Instance.AxisKeys.Contains(n))
		{
			Debug.LogWarning("The Axis \"" + n + "\" Doesn't Exist");
			return;
		}
		if (save)
		{
			Instance.AxisValues[Instance.AxisKeys.IndexOf(n)] = new vp_InputAxis
			{
				Positive = pk,
				Negative = nk
			};
		}
		Instance.Axis[n] = new vp_InputAxis
		{
			Positive = pk,
			Negative = nk
		};
	}
}
