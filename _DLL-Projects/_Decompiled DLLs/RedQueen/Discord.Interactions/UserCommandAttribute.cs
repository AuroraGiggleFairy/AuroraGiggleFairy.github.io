using System;
using System.Reflection;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class UserCommandAttribute : ContextCommandAttribute
{
	public UserCommandAttribute(string name)
		: base(name, ApplicationCommandType.User)
	{
	}

	internal override void CheckMethodDefinition(MethodInfo methodInfo)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length != 1 || !typeof(IUser).IsAssignableFrom(parameters[0].ParameterType))
		{
			throw new InvalidOperationException("User Commands must have only one parameter that is a type of IUser");
		}
	}
}
