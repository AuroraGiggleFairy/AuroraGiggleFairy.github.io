public class BindingItemStandard : BindingItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController dataContext;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XuiBindingDelegate bindingDelegate;

	public BindingItemStandard(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		for (XUiController xUiController = _parent.View.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				dataContext = xUiController;
				string _value = "";
				if (dataContext.GetBindingValue(ref _value, fieldName))
				{
					dataContext.Bindings.AddBinding(_parent);
					return;
				}
				if (BindingMethodCache.Instance.TryGetBindingDelegate(xUiController, fieldName, out bindingDelegate))
				{
					dataContext.Bindings.AddBinding(_parent);
					return;
				}
			}
		}
		if (XUiFromXml.DebugXuiLoading == XUiFromXml.DebugLevel.Verbose)
		{
			Log.Warning("[XUi] Binding name '" + fieldName + "' not found! Hierarchy: " + _parent.View.GetXuiHierarchy());
		}
	}

	public override string GetValue()
	{
		if (bindingDelegate != null)
		{
			object obj = bindingDelegate(dataContext);
			if (obj == null)
			{
				Log.Warning("[XUi] Binding '" + SourceText + "' returned null, should always return appropriate non-null value of same data type. Hierarchy: " + dataContext.GetXuiHierarchy());
				obj = "";
			}
			return obj.ToString();
		}
		string _value = "";
		dataContext.GetBindingValue(ref _value, fieldName);
		return _value;
	}
}
