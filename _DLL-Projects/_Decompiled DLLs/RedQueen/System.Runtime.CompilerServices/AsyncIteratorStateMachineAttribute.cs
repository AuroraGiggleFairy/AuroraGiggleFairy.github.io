namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
internal sealed class AsyncIteratorStateMachineAttribute : StateMachineAttribute
{
	public AsyncIteratorStateMachineAttribute(Type stateMachineType)
		: base(stateMachineType)
	{
	}
}
