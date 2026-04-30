using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal sealed class EnumReader<T> : TypeReader<T> where T : struct, Enum
{
	public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, string option, IServiceProvider services)
	{
		T result;
		return Task.FromResult(Enum.TryParse<T>(option, out result) ? TypeConverterResult.FromSuccess(result) : TypeConverterResult.FromError(InteractionCommandError.ConvertFailed, "Value " + option + " cannot be converted to T"));
	}

	public override Task<string> SerializeAsync(object obj, IServiceProvider services)
	{
		return Task.FromResult(Enum.GetName(typeof(T), obj) ?? throw new ArgumentException($"Enum name cannot be parsed from {obj}"));
	}
}
