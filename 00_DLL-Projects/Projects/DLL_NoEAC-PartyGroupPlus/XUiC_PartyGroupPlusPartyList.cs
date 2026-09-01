using System;
using System.Collections.Generic;
using PartyGroupPlus;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyGroupPlusPartyList : XUiController
{
	public const int ColumnCount = 3;
	public const int RowsPerColumn = 13;

	static readonly Color RowColor = new Color(64f / 255f, 64f / 255f, 64f / 255f, 1f);
	static readonly Color AltRowColor = new Color(32f / 255f, 32f / 255f, 32f / 255f, 1f);

	readonly List<EntityPlayer> sorted = new List<EntityPlayer>(32);
	readonly XUiC_PartyGroupPlusPartyEntry[][] columns = new XUiC_PartyGroupPlusPartyEntry[ColumnCount][];
	XUiC_Paging pager;
	XUiV_Label countLabel;
	int page;
	int lastMemberKey;
	float nextDistanceTime;

	int PageLength => ColumnCount * RowsPerColumn;

	public override void Init()
	{
		base.Init();
		pager = GetChildByType<XUiC_Paging>();
		if (pager != null)
		{
			pager.OnPageChanged += OnPageChanged;
		}

		BindColumns();
		XUiController count = GetChildById("numberOfParty");
		countLabel = count != null ? count.ViewComponent as XUiV_Label : null;
	}

	public override void OnOpen()
	{
		base.OnOpen();
		BindColumns();
		PartyGroupPlusPriorityStore.Load();
		PartyGroupPlusPriorityStore.Changed += OnPriorityChanged;
		RebuildList();
	}

	public override void OnClose()
	{
		PartyGroupPlusPriorityStore.Changed -= OnPriorityChanged;
		base.OnClose();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		EntityPlayer local = XUiC_PartyGroupPlusColorChoice.SafeLocal(xui);
		Party party = local != null ? local.Party : null;
		int key = party != null ? PartyGroupPlusHud.MemberKey(party) : 0;
		if (key != lastMemberKey)
		{
			RebuildList();
			return;
		}

		if (Time.time >= nextDistanceTime)
		{
			RefreshVisibleDistances();
			nextDistanceTime = Time.time + 0.5f;
		}
	}

	public void RebuildList()
	{
		sorted.Clear();
		EntityPlayer local = XUiC_PartyGroupPlusColorChoice.SafeLocal(xui);
		Party party = local != null ? local.Party : null;
		lastMemberKey = party != null ? PartyGroupPlusHud.MemberKey(party) : 0;
		if (party != null && party.MemberList != null)
		{
			List<EntityPlayer> members = party.MemberList;
			for (int i = 0; i < members.Count; i++)
			{
				EntityPlayer member = members[i];
				if (member != null && member != local)
				{
					sorted.Add(member);
				}
			}

			sorted.Sort(CompareMembers);
		}

		int lastPage = 0;
		if (sorted.Count > PageLength)
		{
			lastPage = (sorted.Count - 1) / PageLength;
		}

		if (page > lastPage)
		{
			page = lastPage;
		}

		if (pager != null)
		{
			pager.SetLastPageByElementsAndPageLength(sorted.Count, PageLength);
			pager.SetPage(page);
		}

		if (countLabel != null)
		{
			countLabel.Text = sorted.Count.ToString();
		}

		FillPage();
		nextDistanceTime = Time.time + 0.5f;
	}

	void BindColumns()
	{
		for (int col = 0; col < ColumnCount; col++)
		{
			XUiController grid = GetChildById("partyCol" + col);
			columns[col] = grid != null
				? grid.GetChildrenByType<XUiC_PartyGroupPlusPartyEntry>()
				: new XUiC_PartyGroupPlusPartyEntry[0];
		}
	}

	void FillPage()
	{
		int start = page * PageLength;
		for (int col = 0; col < ColumnCount; col++)
		{
			XUiC_PartyGroupPlusPartyEntry[] rows = columns[col];
			if (rows == null)
			{
				continue;
			}

			for (int row = 0; row < rows.Length; row++)
			{
				int index = start + col * RowsPerColumn + row;
				rows[row].SetAlternating(row % 2 == 1, RowColor, AltRowColor);
				rows[row].SetMember(index < sorted.Count ? sorted[index] : null);
			}
		}
	}

	void RefreshVisibleDistances()
	{
		for (int col = 0; col < ColumnCount; col++)
		{
			XUiC_PartyGroupPlusPartyEntry[] rows = columns[col];
			if (rows == null)
			{
				continue;
			}

			for (int row = 0; row < rows.Length; row++)
			{
				rows[row].RefreshDistance();
			}
		}
	}

	void OnPageChanged()
	{
		if (pager != null)
		{
			page = pager.CurrentPageNumber;
		}

		FillPage();
	}

	void OnPriorityChanged()
	{
		RebuildList();
	}

	static int CompareMembers(EntityPlayer a, EntityPlayer b)
	{
		bool pa = PartyGroupPlusPriorityStore.IsPriority(a);
		bool pb = PartyGroupPlusPriorityStore.IsPriority(b);
		if (pa != pb)
		{
			return pa ? -1 : 1;
		}

		string na = a != null ? a.PlayerDisplayName : string.Empty;
		string nb = b != null ? b.PlayerDisplayName : string.Empty;
		int name = string.Compare(na, nb, StringComparison.OrdinalIgnoreCase);
		if (name != 0)
		{
			return name;
		}

		int idA = a != null ? a.entityId : 0;
		int idB = b != null ? b.entityId : 0;
		return idA.CompareTo(idB);
	}
}

[Preserve]
public class XUiC_PartyGroupPlusPartyEntry : XUiController
{
	EntityPlayer member;
	XUiC_ToggleButton check;
	XUiV_Sprite arrowSprite;
	XUiV_Sprite rowBackground;
	XUiV_Label nameLabel;
	XUiV_Label distanceLabel;
	XUiV_Button mapButton;
	bool suppressCheck;

	public override void Init()
	{
		base.Init();
		XUiController checkChild = GetChildById("priorityCheck");
		check = checkChild as XUiC_ToggleButton ?? GetChildByType<XUiC_ToggleButton>();
		if (check != null)
		{
			check.OnValueChanged += OnCheckChanged;
		}

		XUiController arrow = GetChildById("memberArrow");
		arrowSprite = arrow != null ? arrow.ViewComponent as XUiV_Sprite : null;
		XUiController background = GetChildById("background");
		rowBackground = background != null ? background.ViewComponent as XUiV_Sprite : null;
		XUiController name = GetChildById("memberName");
		nameLabel = name != null ? name.ViewComponent as XUiV_Label : null;
		XUiController distance = GetChildById("memberDistance");
		distanceLabel = distance != null ? distance.ViewComponent as XUiV_Label : null;
		XUiController map = GetChildById("iconShowOnMap");
		if (map != null)
		{
			mapButton = map.ViewComponent as XUiV_Button;
			map.OnPress += OnShowOnMapPress;
		}
	}

	public void SetAlternating(bool alternating, Color rowColor, Color altColor)
	{
		if (rowBackground != null)
		{
			rowBackground.Color = alternating ? altColor : rowColor;
		}
	}

	public void SetMember(EntityPlayer player)
	{
		member = player;
		bool has = player != null;
		int colorIndex = 0;
		if (has)
		{
			PartyGroupPlusColorStore.TryGet(player.entityId, out colorIndex);
		}

		if (nameLabel != null)
		{
			nameLabel.Text = has ? player.PlayerDisplayName : string.Empty;
		}

		if (check != null)
		{
			suppressCheck = true;
			check.Value = has && PartyGroupPlusPriorityStore.IsPriority(player);
			check.Enabled = has;
			if (check.ViewComponent != null)
			{
				check.ViewComponent.IsVisible = has;
			}

			suppressCheck = false;
		}

		if (arrowSprite != null)
		{
			arrowSprite.IsVisible = has;
			if (has)
			{
				arrowSprite.Color = PartyGroupPlusPalette.Get(colorIndex);
			}
		}

		if (mapButton != null)
		{
			mapButton.IsVisible = has;
			mapButton.Enabled = has;
		}

		RefreshDistance();
	}

	public void RefreshDistance()
	{
		if (distanceLabel == null)
		{
			return;
		}

		if (member == null)
		{
			distanceLabel.Text = string.Empty;
			return;
		}

		EntityPlayer local = XUiC_PartyGroupPlusColorChoice.SafeLocal(xui);
		if (local == null)
		{
			distanceLabel.Text = "--";
			return;
		}

		distanceLabel.Text = FormatDistance(Vector3.Distance(local.position, member.position));
	}

	void OnCheckChanged(XUiC_ToggleButton _, bool value)
	{
		if (suppressCheck || member == null)
		{
			return;
		}

		PartyGroupPlusPriorityStore.SetPriority(member, value);
	}

	void OnShowOnMapPress(XUiController _, int _mouseButton)
	{
		if (member == null || (mapButton != null && !mapButton.Enabled))
		{
			return;
		}

		World world = GameManager.Instance != null ? GameManager.Instance.World : null;
		Entity entity = world != null ? world.GetEntity(member.entityId) : null;
		if (entity == null)
		{
			return;
		}

		EntityPlayerLocal local = XUiC_PartyGroupPlusColorChoice.SafeLocal(xui);
		if (local == null)
		{
			return;
		}

		XUiC_WindowSelector.OpenSelectorAndWindow(local, "map");
		XUiV_Window mapWindow = xui.GetWindow("mapArea");
		if (mapWindow == null)
		{
			return;
		}

		XUiC_MapArea mapArea = mapWindow.Controller as XUiC_MapArea;
		if (mapArea != null)
		{
			mapArea.PositionMapAt(entity.GetPosition());
		}
	}

	static string FormatDistance(float meters)
	{
		if (meters < 1000f)
		{
			return Mathf.RoundToInt(meters) + " m";
		}

		return (meters / 1000f).ToString("0.0") + " km";
	}
}
