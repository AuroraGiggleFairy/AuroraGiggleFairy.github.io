public class BindingItemCvar : BindingItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;

	public BindingItemCvar(BindingInfo _parent, XUiView _view, string _sourceText)
		: base(_sourceText)
	{
		FieldName = FieldName.Replace("cvar(", "").Replace(")", "");
		if (FieldName.IndexOf(BindingItem.cvarFormatSplitChar) >= 0)
		{
			string[] array = FieldName.Split(BindingItem.cvarFormatSplitCharArray);
			FieldName = array[0];
			format = array[1];
		}
		for (XUiController xUiController = _view.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				DataContext = xUiController;
				DataContext.AddBinding(_parent);
				break;
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (BindingType == BindingTypes.Complete && !_forceAll)
		{
			return CurrentValue;
		}
		CurrentValue = XUiM_Player.GetPlayer().GetCVar(FieldName).ToString(format);
		if (BindingType == BindingTypes.Once)
		{
			BindingType = BindingTypes.Complete;
		}
		return CurrentValue;
	}
}
