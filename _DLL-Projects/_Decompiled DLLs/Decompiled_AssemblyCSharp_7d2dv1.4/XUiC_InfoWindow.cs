using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_InfoWindow : XUiController
{
	public virtual void Deselect()
	{
	}

	public override void Init()
	{
		base.Init();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
	}

	public override void OnVisibilityChanged(bool _isVisible)
	{
		bool flag = windowGroup == null || windowGroup.isShowing;
		if (_isVisible)
		{
			List<XUiC_InfoWindow> childrenByType = base.xui.GetChildrenByType<XUiC_InfoWindow>();
			for (int i = 0; i < childrenByType.Count; i++)
			{
				if (childrenByType[i] != this && (flag || !childrenByType[i].windowGroup.isShowing))
				{
					childrenByType[i].ViewComponent.IsVisible = false;
				}
			}
		}
		else
		{
			Deselect();
		}
		base.OnVisibilityChanged(_isVisible);
	}
}
