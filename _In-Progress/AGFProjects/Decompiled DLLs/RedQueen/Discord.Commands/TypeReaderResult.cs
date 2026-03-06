using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Discord.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal struct TypeReaderResult : IResult
{
	public IReadOnlyCollection<TypeReaderValue> Values
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public CommandError? Error
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public string ErrorReason
	{
		[_003Cc51ba3f1_002D43fc_002D4bc9_002D811d_002D97306e7eb83f_003EIsReadOnly]
		get;
	}

	public bool IsSuccess => !Error.HasValue;

	public object BestMatch
	{
		get
		{
			if (!IsSuccess)
			{
				throw new InvalidOperationException("TypeReaderResult was not successful.");
			}
			if (Values.Count != 1)
			{
				return Values.OrderByDescending((TypeReaderValue v) => v.Score).First().Value;
			}
			return Values.Single().Value;
		}
	}

	private string DebuggerDisplay
	{
		get
		{
			if (!IsSuccess)
			{
				return $"{Error}: {ErrorReason}";
			}
			return "Success (" + string.Join(", ", Values) + ")";
		}
	}

	private TypeReaderResult(IReadOnlyCollection<TypeReaderValue> values, CommandError? error, string errorReason)
	{
		Values = values;
		Error = error;
		ErrorReason = errorReason;
	}

	public static TypeReaderResult FromSuccess(object value)
	{
		return new TypeReaderResult(System.Collections.Immutable.ImmutableArray.Create(new TypeReaderValue(value, 1f)), null, null);
	}

	public static TypeReaderResult FromSuccess(TypeReaderValue value)
	{
		return new TypeReaderResult(System.Collections.Immutable.ImmutableArray.Create(value), null, null);
	}

	public static TypeReaderResult FromSuccess(IReadOnlyCollection<TypeReaderValue> values)
	{
		return new TypeReaderResult(values, null, null);
	}

	public static TypeReaderResult FromError(CommandError error, string reason)
	{
		return new TypeReaderResult(null, error, reason);
	}

	public static TypeReaderResult FromError(Exception ex)
	{
		return FromError(CommandError.Exception, ex.Message);
	}

	public static TypeReaderResult FromError(IResult result)
	{
		return new TypeReaderResult(null, result.Error, result.ErrorReason);
	}

	public override string ToString()
	{
		if (!IsSuccess)
		{
			return $"{Error}: {ErrorReason}";
		}
		return "Success";
	}
}
