using System;
using UnityEngine;

public abstract class GUIWindow
{
	public readonly string Id;

	public bool bActionSetEnabled;

	public bool isShowing;

	public bool isModal;

	public bool alwaysUsesMouseCursor;

	public bool isEscClosable;

	public bool isInputActive;

	public GUIWindowManager windowManager;

	public LocalPlayerUI playerUI;

	public string openWindowOnEsc = string.Empty;

	public Action OnWindowClose;

	[PublicizedFrom(EAccessModifier.Protected)]
	public GUIWindow(string _id)
	{
		Id = _id;
	}

	public virtual void OnGUI()
	{
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

	public virtual PlayerActionsBase GetActionSet()
	{
		return playerUI.playerInput.GUIActions;
	}

	public virtual bool HasActionSet()
	{
		return true;
	}

	public override bool Equals(object _obj)
	{
		if (_obj is GUIWindow gUIWindow)
		{
			return gUIWindow.Id.Equals(Id);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public virtual void Cleanup()
	{
	}

	public static Matrix4x4 UiScaleMatrix(out float _targetScale, out float _actualScale, float _x = 0f, float _y = 0f, float _clampMin = 0.4f, float _clampMax = 2f)
	{
		_targetScale = (float)Screen.height / 1080f;
		_targetScale *= GameOptionsManager.GetActiveUiScale();
		_actualScale = Utils.FastClamp(_targetScale, _clampMin, _clampMax);
		Matrix4x4 matrix = GUI.matrix;
		GUI.matrix = Matrix4x4.TRS(new Vector3(_x, _y), Quaternion.identity, new Vector3(_actualScale, _actualScale, 1f));
		return matrix;
	}
}
