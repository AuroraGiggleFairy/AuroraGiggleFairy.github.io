using System;
using Audio;
using UnityEngine;

public class GUIWindow
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string id;

	public bool bActionSetEnabled;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool bDrawBackground;

	public Rect internalWindowRect;

	public bool isShowing;

	public bool isModal;

	public bool alwaysUsesMouseCursor;

	public bool isEscClosable;

	public bool isInputActive;

	public bool isDimBackground;

	public GUIWindowManager windowManager;

	public NGUIWindowManager nguiWindowManager;

	public LocalPlayerUI playerUI;

	public Matrix4x4 matrix = Matrix4x4.identity;

	public bool bCenterWindow;

	public string openWindowOnEsc = string.Empty;

	public Action OnWindowClose;

	public Rect windowRect
	{
		get
		{
			return internalWindowRect;
		}
		set
		{
			internalWindowRect = value;
			matrix.SetTRS(new Vector3(internalWindowRect.x, internalWindowRect.y, 0f), Quaternion.identity, Vector3.one);
		}
	}

	public string Id => id;

	public GUIWindow(string _id, int _w, int _h, bool _bDrawBackground)
		: this(_id, _w, _h, _bDrawBackground, _isDimBackground: true)
	{
	}

	public GUIWindow(string _id, int _w, int _h, bool _bDrawBackground, bool _isDimBackground)
		: this(_id, new Rect((float)(Screen.width - _w) / 2f, (float)(Screen.height - _h) / 2f, _w, _h), _bDrawBackground, _isDimBackground)
	{
		bCenterWindow = true;
	}

	public GUIWindow(string _id, Rect _rect)
		: this(_id, _rect, _bDrawBackground: false)
	{
	}

	public GUIWindow(string _id)
		: this(_id, default(Rect))
	{
	}

	public GUIWindow(string _id, Rect _rect, bool _bDrawBackground)
		: this(_id, _rect, _bDrawBackground, _isDimBackground: true)
	{
	}

	public GUIWindow(string _id, Rect _rect, bool _bDrawBackground, bool _isDimBackground)
	{
		windowRect = _rect;
		bDrawBackground = _bDrawBackground;
		isDimBackground = _isDimBackground;
		id = _id;
		bActionSetEnabled = false;
	}

	public bool GUIButton(Rect _rect, string _text)
	{
		if (GUI.Button(_rect, _text))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIButton(Rect _rect, GUIContent _guiContent)
	{
		if (GUI.Button(_rect, _guiContent))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIButton(Rect _rect, GUIContent _guiContent, GUIStyle _guiStyle)
	{
		if (GUI.Button(_rect, _guiContent, _guiStyle))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUILayoutButton(string _text)
	{
		return GUILayoutButton(_text, GUILayout.ExpandWidth(expand: false));
	}

	public bool GUILayoutButton(string _text, GUILayoutOption options)
	{
		if (GUILayout.Button(_text, options))
		{
			Manager.PlayButtonClick();
			return true;
		}
		return false;
	}

	public bool GUIToggle(Rect _rect, bool _v, string _s)
	{
		bool num = GUI.Toggle(_rect, _v, _s);
		if (num != _v)
		{
			Manager.PlayButtonClick();
		}
		return num;
	}

	public bool GUILayoutToggle(bool _v, string _s)
	{
		return GUILayoutToggle(_v, _s, null);
	}

	public bool GUILayoutToggle(bool _v, string _s, GUILayoutOption options)
	{
		bool num = ((options != null) ? GUILayout.Toggle(_v, _s, options) : GUILayout.Toggle(_v, _s));
		if (num != _v)
		{
			Manager.PlayButtonClick();
		}
		return num;
	}

	public virtual void OnGUI(bool _inputActive)
	{
		if (bDrawBackground)
		{
			GUI.Box(new Rect(0f, 0f, windowRect.width, windowRect.height), "");
		}
		if (bCenterWindow)
		{
			SetPosition(((float)Screen.width - windowRect.width) / 2f, ((float)Screen.height - windowRect.height) / 2f);
		}
	}

	public void SetPosition(float _x, float _y)
	{
		windowRect = new Rect(_x, _y, windowRect.width, windowRect.height);
	}

	public void SetSize(float _w, float _h)
	{
		windowRect = new Rect(((float)Screen.width - _w) / 2f, ((float)Screen.height - _h) / 2f, _w, _h);
	}

	public virtual void Update()
	{
	}

	public virtual void OnOpen()
	{
	}

	public virtual void OnClose()
	{
		OnWindowClose?.Invoke();
		OnWindowClose = null;
	}

	public virtual void OnXPressed()
	{
		windowManager.Close(this);
	}

	public virtual PlayerActionsBase GetActionSet()
	{
		return playerUI.playerInput.GUIActions;
	}

	public virtual bool HasActionSet()
	{
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is GUIWindow)
		{
			return ((GUIWindow)obj).id.Equals(id);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}

	public virtual void Cleanup()
	{
	}
}
