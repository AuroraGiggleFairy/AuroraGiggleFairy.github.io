public class BindingItemCvar : BindingItem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string format;

	public BindingItemCvar(BindingInfo _parent, string _sourceText)
		: base(_sourceText)
	{
		fieldName = fieldName.Replace("cvar(", "").Replace(")", "");
		if (fieldName.IndexOf(':') >= 0)
		{
			string[] array = fieldName.Split(':');
			fieldName = array[0];
			format = array[1];
		}
		for (XUiController xUiController = _parent.View.Controller; xUiController != null; xUiController = xUiController.Parent)
		{
			if (xUiController.GetType() != typeof(XUiController))
			{
				xUiController.Bindings.AddBinding(_parent);
				break;
			}
		}
	}

	public override string GetValue()
	{
		return XUiM_Player.GetPlayer().GetCVar(fieldName).ToString(format);
	}
}
