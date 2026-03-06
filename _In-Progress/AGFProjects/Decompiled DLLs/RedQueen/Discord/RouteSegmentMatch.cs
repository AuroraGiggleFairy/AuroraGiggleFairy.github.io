using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Discord;

internal record RouteSegmentMatch : IRouteSegmentMatch
{
	[_003C9e0c2a9e_002Dcd98_002D48b2_002Dac89_002Dcfbfe504abd4_003ENullable(1)]
	[CompilerGenerated]
	protected virtual Type EqualityContract
	{
		[CompilerGenerated]
		[_003C564e5dee_002D9b2b_002D48a1_002D9d93_002D36b5c6e6b10a_003ENullableContext(1)]
		get
		{
			return typeof(RouteSegmentMatch);
		}
	}

	public string Value { get; }

	public RouteSegmentMatch(string value)
	{
		Value = value;
	}

	[_003C564e5dee_002D9b2b_002D48a1_002D9d93_002D36b5c6e6b10a_003ENullableContext(1)]
	[CompilerGenerated]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("RouteSegmentMatch");
		stringBuilder.Append(" { ");
		if (PrintMembers(stringBuilder))
		{
			stringBuilder.Append(' ');
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

	[CompilerGenerated]
	[_003C564e5dee_002D9b2b_002D48a1_002D9d93_002D36b5c6e6b10a_003ENullableContext(1)]
	protected virtual bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Value = ");
		builder.Append((object)Value);
		return true;
	}

	[CompilerGenerated]
	[_003C564e5dee_002D9b2b_002D48a1_002D9d93_002D36b5c6e6b10a_003ENullableContext(2)]
	public virtual bool Equals(RouteSegmentMatch other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract)
			{
				return EqualityComparer<string>.Default.Equals(Value, other.Value);
			}
			return false;
		}
		return true;
	}
}
