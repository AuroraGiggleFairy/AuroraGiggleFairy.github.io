using System.Collections.Generic;
using UnityEngine;

[PublicizedFrom(EAccessModifier.Internal)]
public static class XUiUpdater
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static List<XUi> uiToUpdate = new List<XUi>();

	public static void Add(XUi _ui)
	{
		if (!uiToUpdate.Contains(_ui))
		{
			uiToUpdate.Add(_ui);
		}
	}

	public static void Remove(XUi _ui)
	{
		uiToUpdate.Remove(_ui);
	}

	public static void Update()
	{
		if (uiToUpdate.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < uiToUpdate.Count; i++)
		{
			if (uiToUpdate[i] != null)
			{
				uiToUpdate[i].OnUpdateDeltaTime(Time.deltaTime);
				uiToUpdate[i].OnUpdateInput();
			}
		}
	}
}
