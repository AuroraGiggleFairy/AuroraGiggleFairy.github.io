using System;
using System.Reflection;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal class MessageCommandAttribute : ContextCommandAttribute
{
	public MessageCommandAttribute(string name)
		: base(name, ApplicationCommandType.Message)
	{
	}

	internal override void CheckMethodDefinition(MethodInfo methodInfo)
	{
		ParameterInfo[] parameters = methodInfo.GetParameters();
		if (parameters.Length != 1 || !typeof(IMessage).IsAssignableFrom(parameters[0].ParameterType))
		{
			throw new InvalidOperationException("Message Commands must have only one parameter that is a type of IMessage");
		}
	}
}
