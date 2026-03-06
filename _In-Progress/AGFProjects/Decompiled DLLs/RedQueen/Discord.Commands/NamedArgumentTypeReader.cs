using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Discord.Commands;

internal sealed class NamedArgumentTypeReader<T> : TypeReader where T : class, new()
{
	private enum ReadState
	{
		LookingForParameter,
		InParameter,
		LookingForArgument,
		InArgument,
		InQuotedArgument,
		End
	}

	private static readonly IReadOnlyDictionary<string, PropertyInfo> _tProps = typeof(T).GetTypeInfo().DeclaredProperties.Where((PropertyInfo p) => p.SetMethod != null && p.SetMethod.IsPublic && !p.SetMethod.IsStatic).ToImmutableDictionary((PropertyInfo p) => p.Name, StringComparer.OrdinalIgnoreCase);

	private readonly CommandService _commands;

	private static readonly MethodInfo _readMultipleMethod = typeof(NamedArgumentTypeReader<T>).GetTypeInfo().DeclaredMethods.Single((MethodInfo m) => m.IsPrivate && m.IsStatic && m.Name == "ReadMultiple");

	public NamedArgumentTypeReader(CommandService commands)
	{
		_commands = commands;
	}

	public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
	{
		T result = new T();
		ReadState state = ReadState.LookingForParameter;
		int beginRead = 0;
		int currentRead = 0;
		while (state != ReadState.End)
		{
			try
			{
				string arg;
				PropertyInfo prop = Read(out arg);
				object obj = await ReadArgumentAsync(prop, arg).ConfigureAwait(continueOnCapturedContext: false);
				if (obj != null)
				{
					prop.SetMethod.Invoke(result, new object[1] { obj });
					continue;
				}
				return TypeReaderResult.FromError(CommandError.ParseFailed, $"Could not parse the argument for the parameter '{prop.Name}' as type '{prop.PropertyType}'.");
			}
			catch (Exception ex)
			{
				return TypeReaderResult.FromError(ex);
			}
		}
		return TypeReaderResult.FromSuccess(result);
		PropertyInfo GetPropAndValue(out string argv)
		{
			bool flag = state == ReadState.InQuotedArgument;
			state = ((currentRead == (flag ? (input.Length - 1) : input.Length)) ? ReadState.End : ReadState.LookingForParameter);
			if (flag)
			{
				argv = input.Substring(beginRead + 1, currentRead - beginRead - 1).Trim();
				currentRead++;
			}
			else
			{
				argv = input.Substring(beginRead, currentRead - beginRead);
			}
			return _tProps[P_1.currentParam];
		}
		PropertyInfo Read(out string argv)
		{
			string currentParam = null;
			char value = '\0';
			for (; currentRead < input.Length; currentRead++)
			{
				char c = input[currentRead];
				switch (state)
				{
				case ReadState.LookingForParameter:
					if (!char.IsWhiteSpace(c))
					{
						beginRead = currentRead;
						state = ReadState.InParameter;
					}
					break;
				case ReadState.InParameter:
					if (c == ':')
					{
						currentParam = input.Substring(beginRead, currentRead - beginRead);
						state = ReadState.LookingForArgument;
					}
					break;
				case ReadState.LookingForArgument:
					if (!char.IsWhiteSpace(c))
					{
						beginRead = currentRead;
						state = (QuotationAliasUtils.GetDefaultAliasMap.TryGetValue(c, out value) ? ReadState.InQuotedArgument : ReadState.InArgument);
					}
					break;
				case ReadState.InArgument:
					if (char.IsWhiteSpace(c))
					{
						return GetPropAndValue(out argv);
					}
					break;
				case ReadState.InQuotedArgument:
					if (c == value)
					{
						return GetPropAndValue(out argv);
					}
					break;
				}
			}
			if (currentParam == null)
			{
				throw new InvalidOperationException("No parameter name was read.");
			}
			return GetPropAndValue(out argv);
		}
		async Task<object> ReadArgumentAsync(PropertyInfo propertyInfo, string text)
		{
			Type type = propertyInfo.PropertyType;
			bool flag = false;
			if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				type = propertyInfo.PropertyType.GenericTypeArguments[0];
				flag = true;
			}
			OverrideTypeReaderAttribute customAttribute = propertyInfo.GetCustomAttribute<OverrideTypeReaderAttribute>();
			TypeReader typeReader = ((customAttribute != null) ? ModuleClassBuilder.GetTypeReader(_commands, type, customAttribute.TypeReader, services) : (_commands.GetDefaultTypeReader(type) ?? _commands.GetTypeReaders(type).FirstOrDefault().Value));
			if (typeReader != null)
			{
				if (flag)
				{
					return await ((Task<IEnumerable>)_readMultipleMethod.MakeGenericMethod(type).Invoke(null, new object[4]
					{
						typeReader,
						context,
						text.Split(','),
						services
					})).ConfigureAwait(continueOnCapturedContext: false);
				}
				return await ReadSingle(typeReader, context, text, services).ConfigureAwait(continueOnCapturedContext: false);
			}
			return null;
		}
	}

	private static async Task<object> ReadSingle(TypeReader reader, ICommandContext context, string arg, IServiceProvider services)
	{
		TypeReaderResult typeReaderResult = await reader.ReadAsync(context, arg, services).ConfigureAwait(continueOnCapturedContext: false);
		return typeReaderResult.IsSuccess ? typeReaderResult.BestMatch : null;
	}

	private static async Task<IEnumerable> ReadMultiple<TObj>(TypeReader reader, ICommandContext context, IEnumerable<string> args, IServiceProvider services)
	{
		List<TObj> objs = new List<TObj>();
		foreach (string arg in args)
		{
			object obj = await ReadSingle(reader, context, arg.Trim(), services).ConfigureAwait(continueOnCapturedContext: false);
			if (obj != null)
			{
				objs.Add((TObj)obj);
			}
		}
		return objs.ToImmutableArray();
	}
}
