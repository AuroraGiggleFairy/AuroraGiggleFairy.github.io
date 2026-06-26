using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelTools3Window : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_SimpleButton[] buttons;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ToggleButton[] toggles;

	[PublicizedFrom(EAccessModifier.Private)]
	public NGuiAction[] actions;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool btnsInitialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public int buttonsCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnLevelStartPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnEntitySpawner;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnSleeperVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTeleportVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnTriggerVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnInfoVolume;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiController btnWallVolume;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		btnEntitySpawner = GetChildById("btnEntitySpawner");
		btnEntitySpawner.GetChildById("clickable").OnPress += BtnEntitySpawner_Controller_OnPress;
		btnSleeperVolume = GetChildById("btnSleeperVolume");
		btnSleeperVolume.GetChildById("clickable").OnPress += BtnSleeperVolume_Controller_OnPress;
		btnTeleportVolume = GetChildById("btnTeleportVolume");
		btnTeleportVolume.GetChildById("clickable").OnPress += BtnTeleportVolume_Controller_OnPress;
		btnTriggerVolume = GetChildById("btnTriggerVolume");
		btnTriggerVolume.GetChildById("clickable").OnPress += BtnTriggerVolume_Controller_OnPress;
		btnInfoVolume = GetChildById("btnInfoVolume");
		btnInfoVolume.GetChildById("clickable").OnPress += BtnInfoVolume_Controller_OnPress;
		btnWallVolume = GetChildById("btnWallVolume");
		btnWallVolume.GetChildById("clickable").OnPress += BtnWallVolume_Controller_OnPress;
		XUiController childById = GetChildById("buttons");
		buttons = new XUiC_SimpleButton[childById.Children.Count];
		toggles = new XUiC_ToggleButton[childById.Children.Count];
		actions = new NGuiAction[childById.Children.Count];
		for (int i = 0; i < childById.Children.Count; i++)
		{
			buttons[i] = childById.Children[i].GetChildById("button").GetChildByType<XUiC_SimpleButton>();
			toggles[i] = childById.Children[i].GetChildById("toggle").GetChildByType<XUiC_ToggleButton>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEntitySpawner_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSleeperVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
			PrefabSleeperVolumeManager.Instance.AddSleeperVolumeServer(hitPointBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTeleportVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
			PrefabVolumeManager.Instance.AddTeleportVolumeServer(hitPointBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnTriggerVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
			PrefabTriggerVolumeManager.Instance.AddTriggerVolumeServer(hitPointBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnInfoVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
			PrefabVolumeManager.Instance.AddInfoVolumeServer(hitPointBlockPos);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnWallVolume_Controller_OnPress(XUiController _sender, int _mouseButton)
	{
		Vector3 raycastHitPoint = getRaycastHitPoint();
		if (!raycastHitPoint.Equals(Vector3.zero))
		{
			Vector3i hitPointBlockPos = World.worldToBlockPos(raycastHitPoint);
			PrefabVolumeManager.Instance.AddWallVolumeServer(hitPointBlockPos);
		}
	}

	public static Vector3 getRaycastHitPoint(float _maxDistance = 100f, float _offsetUp = 0f)
	{
		Camera finalCamera = GameManager.Instance.World.GetPrimaryPlayer().finalCamera;
		Ray ray = finalCamera.ScreenPointToRay(new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f));
		ray.origin += Origin.position;
		Transform transform = finalCamera.transform;
		ray.origin += transform.forward * 0.1f;
		ray.origin += transform.up * _offsetUp;
		if (Voxel.Raycast(GameManager.Instance.World, ray, _maxDistance, 4095, 0f))
		{
			return Voxel.voxelRayHitInfo.hit.pos - ray.direction * 0.05f;
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ToggleButton_OnPress(NGuiAction _action)
	{
		_action.OnClick();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SimpleButton_OnPress(NGuiAction _action)
	{
		_action.OnClick();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!btnsInitialized)
		{
			btnsInitialized = true;
			buttonsCount = 0;
			BlockToolSelection blockToolSelection = (BlockToolSelection)((GameManager.Instance.GetActiveBlockTool() is BlockToolSelection) ? GameManager.Instance.GetActiveBlockTool() : null);
			if (blockToolSelection != null)
			{
				buttonsCount++;
				foreach (NGuiAction action in blockToolSelection.GetActions())
				{
					if (action == NGuiAction.Separator)
					{
						buttonsCount++;
					}
					else
					{
						SetButton(ref buttonsCount, action);
					}
				}
			}
		}
		for (int i = 0; i < buttonsCount && i < buttons.Length; i++)
		{
			if (actions[i] == null)
			{
				buttons[i].ViewComponent.IsVisible = false;
				toggles[i].ViewComponent.IsVisible = false;
			}
			else if (actions[i].IsToggle())
			{
				buttons[i].ViewComponent.IsVisible = false;
			}
			else
			{
				toggles[i].ViewComponent.IsVisible = false;
			}
		}
		if (buttonsCount < buttons.Length)
		{
			for (int j = buttonsCount; j < buttons.Length; j++)
			{
				buttons[j].ViewComponent.IsVisible = false;
				toggles[j].ViewComponent.IsVisible = false;
			}
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			btnEntitySpawner.ViewComponent.IsVisible = false;
			btnLevelStartPoint.ViewComponent.IsVisible = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetButton(ref int _buttonIndex, NGuiAction _action)
	{
		if (_buttonIndex < actions.Length)
		{
			actions[_buttonIndex] = _action;
			if (_action != null)
			{
				string text = _action.GetText() + " " + _action.GetHotkey().GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.KeyboardWithParentheses);
				string tooltip = _action.GetTooltip();
				if (_action.IsToggle())
				{
					toggles[_buttonIndex].Label = text;
					toggles[_buttonIndex].OnValueChanged += [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ToggleButton _sender, bool _newValue) =>
					{
						ToggleButton_OnPress(_action);
					};
					toggles[_buttonIndex].Tooltip = tooltip;
				}
				else
				{
					buttons[_buttonIndex].Text = text;
					buttons[_buttonIndex].OnPressed += [PublicizedFrom(EAccessModifier.Internal)] (XUiController _sender, int _mouseButton) =>
					{
						SimpleButton_OnPress(_action);
					};
					buttons[_buttonIndex].Tooltip = tooltip;
				}
			}
		}
		else
		{
			Log.Warning("[XUi] Could not add further buttons to XUiC_LevelToolsWindow");
		}
		_buttonIndex++;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		for (int i = 0; i < buttonsCount; i++)
		{
			if (actions[i] != null)
			{
				if (actions[i].IsToggle())
				{
					toggles[i].Value = actions[i].IsChecked();
				}
				else
				{
					buttons[i].Enabled = actions[i].IsEnabled();
				}
			}
		}
	}
}
