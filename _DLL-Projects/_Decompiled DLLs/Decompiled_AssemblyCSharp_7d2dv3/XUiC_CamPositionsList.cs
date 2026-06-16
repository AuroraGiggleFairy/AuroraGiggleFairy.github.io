using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_CamPositionsList : XUiC_List<XUiC_CamPositionsList.CamPerspectiveEntry>
{
	[Preserve]
	public class CamPerspectiveEntry : XUiListEntry<CamPerspectiveEntry>
	{
		public readonly CameraPerspectives.Perspective Perspective;

		public CamPerspectiveEntry(CameraPerspectives.Perspective _perspective)
		{
			Perspective = _perspective;
		}

		public override int CompareTo(CamPerspectiveEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return string.Compare(Perspective.Name, _otherEntry.Perspective.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			if (string.IsNullOrEmpty(_searchString))
			{
				return true;
			}
			if (!Perspective.Name.ContainsCaseInsensitive(_searchString))
			{
				return Perspective.Comment?.ContainsCaseInsensitive(_searchString) ?? false;
			}
			return true;
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class CamPositionsListEntryController : XUiC_ListEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_CamPositionsList parentList;

		public override void Init()
		{
			base.Init();
			parentList = GetParentByType<XUiC_CamPositionsList>();
			if (GetChildById("camButton") is XUiC_SimpleButton xUiC_SimpleButton)
			{
				xUiC_SimpleButton.OnPressed += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
				{
					GetEntry().Perspective.ToPlayer(xui.playerUI.entityPlayer);
				};
			}
			if (GetChildById("btnDelete")?.ViewComponent is XUiV_Button xUiV_Button)
			{
				xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
				{
					CamPerspectiveEntry entry = GetEntry();
					parentList.perspectives.Perspectives.Remove(entry.Perspective.Name);
					parentList.perspectives.Save();
					parentList.UpdateList();
				};
			}
		}

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			return entryData?.Perspective.Name ?? "";
		}

		[XuiXmlBinding("position")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingPosition()
		{
			if (entryData != null)
			{
				return ValueDisplayFormatters.WorldPos(entryData.Perspective.Position);
			}
			return "";
		}

		[XuiXmlBinding("direction")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDirection()
		{
			return entryData?.Perspective.Direction.ToCultureInvariantString() ?? "";
		}

		[XuiXmlBinding("comment")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingComment()
		{
			return entryData?.Perspective.Comment ?? "";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly CameraPerspectives perspectives = new CameraPerspectives(_load: false);

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showAddCamPositionWindow;

	public bool ShowAddCamPositionWindow
	{
		get
		{
			return showAddCamPositionWindow;
		}
		set
		{
			if (value != showAddCamPositionWindow)
			{
				showAddCamPositionWindow = value;
				RefreshBindings();
			}
		}
	}

	public override void Init()
	{
		base.Init();
		if (GetChildById("btnAddCamPosition")?.ViewComponent is XUiV_Button xUiV_Button)
		{
			xUiV_Button.Controller.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				ShowAddCamPositionWindow = !ShowAddCamPositionWindow;
			};
		}
		base.OnVisiblity += OnVisibilityChanged;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnVisiblity -= OnVisibilityChanged;
	}

	public void UpdateList()
	{
		int num = base.Page;
		RebuildList();
		base.Page = num;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		allEntries.Clear();
		perspectives.Load();
		foreach (var (_, perspective2) in perspectives.Perspectives)
		{
			allEntries.Add(new CamPerspectiveEntry(perspective2));
		}
		allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (_bindingName == "cam_position_add_open")
		{
			_value = ShowAddCamPositionWindow.ToString();
			return true;
		}
		return base.GetBindingValueInternal(ref _value, _bindingName);
	}

	public void OnVisibilityChanged(XUiController _sender, bool _visibleSelf, bool _visibleInScene)
	{
		if (_visibleInScene)
		{
			RebuildList();
		}
	}

	public void Add(string _name, string _comment)
	{
		CameraPerspectives.Perspective perspective = new CameraPerspectives.Perspective(_name, xui.playerUI.entityPlayer, _comment);
		perspectives.Perspectives.Add(perspective.Name, perspective);
		perspectives.Save();
		UpdateList();
	}
}
