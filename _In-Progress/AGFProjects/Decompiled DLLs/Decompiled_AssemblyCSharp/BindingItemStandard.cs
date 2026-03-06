public class BindingItemStandard : BindingItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly char[] bindingTypeSplitChar = new char[1] { '|' };

	public BindingItemStandard(BindingInfo _parent, XUiView _view, string _sourceText)
		: base(_sourceText)
	{
		string[] array = FieldName.Split(bindingTypeSplitChar);
		if (array.Length > 1)
		{
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i].EqualsCaseInsensitive("once"))
				{
					BindingType = BindingTypes.Once;
				}
			}
			FieldName = array[0];
		}
		for (XUiController xUiController = _view.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				DataContext = xUiController;
				string _value = "";
				if (DataContext.GetBindingValue(ref _value, FieldName))
				{
					DataContext.AddBinding(_parent);
					break;
				}
			}
		}
	}

	public override string GetValue(bool _forceAll = false)
	{
		if (BindingType == BindingTypes.Complete && !_forceAll)
		{
			return CurrentValue;
		}
		if (!DataContext.GetBindingValue(ref CurrentValue, FieldName))
		{
			return CurrentValue;
		}
		if (CurrentValue != null && CurrentValue.Contains("{cvar("))
		{
			CurrentValue = ParseCVars(CurrentValue);
		}
		if (BindingType == BindingTypes.Once)
		{
			BindingType = BindingTypes.Complete;
		}
		return CurrentValue;
	}
}
