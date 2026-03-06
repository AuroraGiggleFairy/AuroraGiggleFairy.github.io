using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class ComplexParameterAttribute : Attribute
{
	public Type[] PrioritizedCtorSignature { get; }

	public ComplexParameterAttribute()
	{
	}

	public ComplexParameterAttribute(Type[] types)
	{
		PrioritizedCtorSignature = types;
	}
}
