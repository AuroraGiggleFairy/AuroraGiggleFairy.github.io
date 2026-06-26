using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LevelToolsWindow : XUiController
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
	public static readonly string[] goNamesUnpaintable = new string[5] { "_BlockEntities", "models", "modelsCollider", "cutout", "cutoutCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesPaintable = new string[2] { "opaque", "opaqueCollider" };

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] goNamesTerrain = new string[2] { "terrain", "terrainCollider" };

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
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
			foreach (KeyValuePair<string, SelectionCategory> category2 in SelectionBoxManager.Instance.GetCategories())
			{
				string name = category2.Value.name;
				NGuiAction action = new NGuiAction(Localization.Get("selectionCategory" + name), null, _isToggle: true);
				action.SetDescription(name);
				action.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
				{
					SelectionCategory category = SelectionBoxManager.Instance.GetCategory(action.GetDescription());
					category.SetVisible(!category.IsVisible());
				});
				action.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SelectionBoxManager.Instance.GetCategory(action.GetDescription()).IsVisible());
				SetButton(ref buttonsCount, action);
				if (name == "SleeperVolume")
				{
					NGuiAction nGuiAction = new NGuiAction(Localization.Get("leveltoolsSleeperXRay"), null, _isToggle: true);
					nGuiAction.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
					{
						SleeperVolumeToolManager.SetXRay(!SleeperVolumeToolManager.GetXRay());
					});
					nGuiAction.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => SleeperVolumeToolManager.GetXRay());
					SetButton(ref buttonsCount, nGuiAction);
				}
			}
			buttonsCount++;
			NGuiAction nGuiAction2 = new NGuiAction(Localization.Get("leveltoolsShowUnpaintable"), null, _isToggle: true);
			nGuiAction2.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowUnpaintables = !GameManager.bShowUnpaintables;
				setChunkPartVisible(goNamesUnpaintable, GameManager.bShowUnpaintables);
			});
			nGuiAction2.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowUnpaintables);
			SetButton(ref buttonsCount, nGuiAction2);
			NGuiAction nGuiAction3 = new NGuiAction(Localization.Get("leveltoolsShowPaintable"), null, _isToggle: true);
			nGuiAction3.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowPaintables = !GameManager.bShowPaintables;
				setChunkPartVisible(goNamesPaintable, GameManager.bShowPaintables);
			});
			nGuiAction3.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowPaintables);
			SetButton(ref buttonsCount, nGuiAction3);
			NGuiAction nGuiAction4 = new NGuiAction(Localization.Get("leveltoolsShowTerrain"), null, _isToggle: true);
			nGuiAction4.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowTerrain = !GameManager.bShowTerrain;
				setChunkPartVisible(goNamesTerrain, GameManager.bShowTerrain);
			});
			nGuiAction4.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowTerrain);
			SetButton(ref buttonsCount, nGuiAction4);
			NGuiAction nGuiAction5 = new NGuiAction(Localization.Get("leveltoolsShowDecor"), null, _isToggle: true);
			nGuiAction5.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowDecorBlocks = !GameManager.bShowDecorBlocks;
				foreach (Chunk item in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					item.NeedsRegeneration = true;
				}
			});
			nGuiAction5.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowDecorBlocks);
			SetButton(ref buttonsCount, nGuiAction5);
			NGuiAction nGuiAction6 = new NGuiAction(Localization.Get("leveltoolsShowLoot"), null, _isToggle: true);
			nGuiAction6.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				GameManager.bShowLootBlocks = !GameManager.bShowLootBlocks;
				foreach (Chunk item2 in GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync())
				{
					item2.NeedsRegeneration = true;
				}
			});
			nGuiAction6.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => GameManager.bShowLootBlocks);
			SetButton(ref buttonsCount, nGuiAction6);
			NGuiAction nGuiAction7 = new NGuiAction(Localization.Get("leveltoolsShowQuestLoot"), null, _isToggle: true);
			nGuiAction7.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.HighlightQuestLoot = !PrefabEditModeManager.Instance.HighlightQuestLoot;
			});
			nGuiAction7.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.HighlightQuestLoot);
			SetButton(ref buttonsCount, nGuiAction7);
			NGuiAction nGuiAction8 = new NGuiAction(Localization.Get("leveltoolsShowBlockTriggers"), null, _isToggle: true);
			nGuiAction8.SetClickActionDelegate([PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				PrefabEditModeManager.Instance.HighlightBlockTriggers = !PrefabEditModeManager.Instance.HighlightBlockTriggers;
			});
			nGuiAction8.SetIsCheckedDelegate([PublicizedFrom(EAccessModifier.Internal)] () => PrefabEditModeManager.Instance.HighlightBlockTriggers);
			SetButton(ref buttonsCount, nGuiAction8);
		}
		for (int num = 0; num < buttonsCount && num < buttons.Length; num++)
		{
			if (actions[num] == null)
			{
				buttons[num].ViewComponent.IsVisible = false;
				toggles[num].ViewComponent.IsVisible = false;
			}
			else if (actions[num].IsToggle())
			{
				buttons[num].ViewComponent.IsVisible = false;
			}
			else
			{
				toggles[num].ViewComponent.IsVisible = false;
			}
		}
		if (buttonsCount < buttons.Length)
		{
			for (int num2 = buttonsCount; num2 < buttons.Length; num2++)
			{
				buttons[num2].ViewComponent.IsVisible = false;
				toggles[num2].ViewComponent.IsVisible = false;
			}
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

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(string[] _matchedNames, bool _visible, List<ChunkGameObject> _cgos = null)
	{
		if (_cgos == null)
		{
			_cgos = GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects();
		}
		foreach (ChunkGameObject _cgo in _cgos)
		{
			setChunkPartVisible(_cgo.transform, _matchedNames, _visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void setChunkPartVisible(Transform _parent, string[] _matchedNames, bool _visible)
	{
		for (int i = 0; i < _parent.childCount; i++)
		{
			Transform child = _parent.GetChild(i);
			string name = child.name;
			if (_matchedNames.ContainsCaseInsensitive(name))
			{
				child.gameObject.SetActive(_visible);
			}
			else if (child.childCount > 0)
			{
				setChunkPartVisible(child, _matchedNames, _visible);
			}
		}
	}
}
