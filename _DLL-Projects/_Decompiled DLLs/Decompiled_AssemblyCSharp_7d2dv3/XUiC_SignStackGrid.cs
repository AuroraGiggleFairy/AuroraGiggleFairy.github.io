using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SignStackGrid : XUiController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public int curPageIdx;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int numPages;

	[PublicizedFrom(EAccessModifier.Private)]
	public int page;

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_SignStack[] signStackControllers;

	[PublicizedFrom(EAccessModifier.Private)]
	public GlobalSignId selectedId;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GlobalSignId> currentList;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int Length
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Page
	{
		get
		{
			return page;
		}
		set
		{
			page = value;
			IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		signStackControllers = GetChildrenByType<XUiC_SignStack>();
		Length = signStackControllers.Length;
		IsDirty = false;
	}

	public void SetSignIds(List<GlobalSignId> signIdList, GlobalSignId newSelectedId)
	{
		xui.GetChildByType<XUiC_SignGalleryWindow>();
		int count = signIdList.Count;
		currentList = signIdList;
		selectedId = newSelectedId;
		for (int i = 0; i < Length; i++)
		{
			int num = i + Length * page;
			XUiC_SignStack xUiC_SignStack = signStackControllers[i];
			if (num < count)
			{
				xUiC_SignStack.SignId = signIdList[num];
				if (xUiC_SignStack.SignId == selectedId)
				{
					xUiC_SignStack.IsSelected = true;
				}
				else if (xUiC_SignStack.IsSelected)
				{
					xUiC_SignStack.IsSelected = false;
				}
			}
			else
			{
				xUiC_SignStack.SignId = GlobalSignId.InvalidId;
				if (xUiC_SignStack.IsSelected)
				{
					xUiC_SignStack.IsSelected = false;
				}
			}
		}
		IsDirty = false;
	}

	public override void OnOpen()
	{
		if (base.ViewComponent != null && !base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = true;
		}
	}

	public override void OnClose()
	{
		if (base.ViewComponent != null && base.ViewComponent.IsVisible)
		{
			base.ViewComponent.IsVisible = false;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (IsDirty)
		{
			SetSignIds(currentList, selectedId);
			IsDirty = false;
		}
	}
}
