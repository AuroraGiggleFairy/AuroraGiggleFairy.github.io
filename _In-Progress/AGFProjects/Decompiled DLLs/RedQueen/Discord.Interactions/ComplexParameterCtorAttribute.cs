using System;

namespace Discord.Interactions;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
internal class ComplexParameterCtorAttribute : Attribute
{
}
