using System;

namespace Discord.Commands;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal class RemainderAttribute : Attribute
{
}
