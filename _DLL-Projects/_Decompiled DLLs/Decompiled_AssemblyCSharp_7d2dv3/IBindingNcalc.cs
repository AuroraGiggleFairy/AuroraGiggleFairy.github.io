public interface IBindingNcalc
{
	string SourceText { get; }

	IXUiElement TargetElement { get; }

	bool IsInitializing { get; }

	void SetIndeterministic();

	bool FindParameter(string _name, out object _value);

	bool RegisterVariable(BindingInfoNcalc.VariableStateAbs _var);
}
