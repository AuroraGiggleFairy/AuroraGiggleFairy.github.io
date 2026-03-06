using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Discord.Commands.Builders;

namespace Discord.Commands;

[DebuggerDisplay("{Name,nq}")]
internal class CommandInfo
{
	private static readonly MethodInfo _convertParamsMethod = typeof(CommandInfo).GetTypeInfo().GetDeclaredMethod("ConvertParamsList");

	private static readonly ConcurrentDictionary<Type, Func<IEnumerable<object>, object>> _arrayConverters = new ConcurrentDictionary<Type, Func<IEnumerable<object>, object>>();

	private readonly CommandService _commandService;

	private readonly Func<ICommandContext, object[], IServiceProvider, CommandInfo, Task> _action;

	public ModuleInfo Module { get; }

	public string Name { get; }

	public string Summary { get; }

	public string Remarks { get; }

	public int Priority { get; }

	public bool HasVarArgs { get; }

	public bool IgnoreExtraArgs { get; }

	public RunMode RunMode { get; }

	public IReadOnlyList<string> Aliases { get; }

	public IReadOnlyList<ParameterInfo> Parameters { get; }

	public IReadOnlyList<PreconditionAttribute> Preconditions { get; }

	public IReadOnlyList<Attribute> Attributes { get; }

	internal CommandInfo(CommandBuilder builder, ModuleInfo module, CommandService service)
	{
		CommandInfo info = this;
		Module = module;
		Name = builder.Name;
		Summary = builder.Summary;
		Remarks = builder.Remarks;
		RunMode = ((builder.RunMode == RunMode.Default) ? service._defaultRunMode : builder.RunMode);
		Priority = builder.Priority;
		Aliases = (from x in module.Aliases.Permutate(builder.Aliases, delegate(string first, string second)
			{
				if (first == "")
				{
					return second;
				}
				return (second == "") ? first : (first + service._separatorChar + second);
			})
			select (!service._caseSensitive) ? x.ToLowerInvariant() : x).ToImmutableArray();
		Preconditions = builder.Preconditions.ToImmutableArray();
		Attributes = builder.Attributes.ToImmutableArray();
		Parameters = builder.Parameters.Select((ParameterBuilder x) => x.Build(info)).ToImmutableArray();
		HasVarArgs = builder.Parameters.Count > 0 && builder.Parameters[builder.Parameters.Count - 1].IsMultiple;
		IgnoreExtraArgs = builder.IgnoreExtraArgs;
		_action = builder.Callback;
		_commandService = service;
	}

	public async Task<PreconditionResult> CheckPreconditionsAsync(ICommandContext context, IServiceProvider services = null)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		PreconditionResult preconditionResult = await CheckGroups(Module.Preconditions, "Module").ConfigureAwait(continueOnCapturedContext: false);
		if (!preconditionResult.IsSuccess)
		{
			return preconditionResult;
		}
		PreconditionResult preconditionResult2 = await CheckGroups(Preconditions, "Command").ConfigureAwait(continueOnCapturedContext: false);
		if (!preconditionResult2.IsSuccess)
		{
			return preconditionResult2;
		}
		return PreconditionResult.FromSuccess();
		async Task<PreconditionResult> CheckGroups(IEnumerable<PreconditionAttribute> preconditions, string type)
		{
			foreach (IGrouping<string, PreconditionAttribute> preconditionGroup in preconditions.GroupBy((PreconditionAttribute p) => p.Group, StringComparer.Ordinal))
			{
				if (preconditionGroup.Key == null)
				{
					foreach (PreconditionAttribute item in preconditionGroup)
					{
						PreconditionResult preconditionResult3 = await item.CheckPermissionsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false);
						if (!preconditionResult3.IsSuccess)
						{
							return preconditionResult3;
						}
					}
				}
				else
				{
					List<PreconditionResult> results = new List<PreconditionResult>();
					foreach (PreconditionAttribute item2 in preconditionGroup)
					{
						List<PreconditionResult> list = results;
						list.Add(await item2.CheckPermissionsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false));
					}
					if (!results.Any((PreconditionResult p) => p.IsSuccess))
					{
						return PreconditionGroupResult.FromError(type + " precondition group " + preconditionGroup.Key + " failed.", results);
					}
				}
			}
			return PreconditionGroupResult.FromSuccess();
		}
	}

	public async Task<ParseResult> ParseAsync(ICommandContext context, int startIndex, SearchResult searchResult, PreconditionResult preconditionResult = null, IServiceProvider services = null)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		if (!searchResult.IsSuccess)
		{
			return ParseResult.FromError(searchResult);
		}
		if (preconditionResult != null && !preconditionResult.IsSuccess)
		{
			return ParseResult.FromError(preconditionResult);
		}
		string input = searchResult.Text.Substring(startIndex);
		return await CommandParser.ParseArgsAsync(this, context, _commandService._ignoreExtraArgs, services, input, 0, _commandService._quotationMarkAliasMap).ConfigureAwait(continueOnCapturedContext: false);
	}

	public Task<IResult> ExecuteAsync(ICommandContext context, ParseResult parseResult, IServiceProvider services)
	{
		if (!parseResult.IsSuccess)
		{
			return Task.FromResult((IResult)ExecuteResult.FromError(parseResult));
		}
		object[] array = new object[parseResult.ArgValues.Count];
		for (int i = 0; i < parseResult.ArgValues.Count; i++)
		{
			if (!parseResult.ArgValues[i].IsSuccess)
			{
				return Task.FromResult((IResult)ExecuteResult.FromError(parseResult.ArgValues[i]));
			}
			array[i] = parseResult.ArgValues[i].Values.First().Value;
		}
		object[] array2 = new object[parseResult.ParamValues.Count];
		for (int j = 0; j < parseResult.ParamValues.Count; j++)
		{
			if (!parseResult.ParamValues[j].IsSuccess)
			{
				return Task.FromResult((IResult)ExecuteResult.FromError(parseResult.ParamValues[j]));
			}
			array2[j] = parseResult.ParamValues[j].Values.First().Value;
		}
		return ExecuteAsync(context, array, array2, services);
	}

	public async Task<IResult> ExecuteAsync(ICommandContext context, IEnumerable<object> argList, IEnumerable<object> paramList, IServiceProvider services)
	{
		if (services == null)
		{
			services = EmptyServiceProvider.Instance;
		}
		try
		{
			object[] args = GenerateArgs(argList, paramList);
			for (int position = 0; position < Parameters.Count; position++)
			{
				PreconditionResult result = await Parameters[position].CheckPreconditionsAsync(arg: args[position], context: context, services: services).ConfigureAwait(continueOnCapturedContext: false);
				if (!result.IsSuccess)
				{
					await Module.Service._commandExecutedEvent.InvokeAsync(this, context, result).ConfigureAwait(continueOnCapturedContext: false);
					return ExecuteResult.FromError(result);
				}
			}
			switch (RunMode)
			{
			case RunMode.Sync:
				return await ExecuteInternalAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
			case RunMode.Async:
				Task.Run(async delegate
				{
					await ExecuteInternalAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
				});
				break;
			}
			return ExecuteResult.FromSuccess();
		}
		catch (Exception ex)
		{
			return ExecuteResult.FromError(ex);
		}
	}

	private async Task<IResult> ExecuteInternalAsync(ICommandContext context, object[] args, IServiceProvider services)
	{
		await Module.Service._cmdLogger.DebugAsync("Executing " + GetLogText(context)).ConfigureAwait(continueOnCapturedContext: false);
		IResult result2;
		try
		{
			Task task = _action(context, args, services, this);
			if (task is Task<IResult> task2)
			{
				IResult result = await task2.ConfigureAwait(continueOnCapturedContext: false);
				await Module.Service._commandExecutedEvent.InvokeAsync(this, context, result).ConfigureAwait(continueOnCapturedContext: false);
				if (!(result is RuntimeResult runtimeResult))
				{
					goto IL_047e;
				}
				result2 = runtimeResult;
			}
			else
			{
				if (!(task is Task<ExecuteResult> task3))
				{
					await task.ConfigureAwait(continueOnCapturedContext: false);
					ExecuteResult executeResult = ExecuteResult.FromSuccess();
					await Module.Service._commandExecutedEvent.InvokeAsync(this, context, executeResult).ConfigureAwait(continueOnCapturedContext: false);
					goto IL_047e;
				}
				ExecuteResult result3 = await task3.ConfigureAwait(continueOnCapturedContext: false);
				await Module.Service._commandExecutedEvent.InvokeAsync(this, context, result3).ConfigureAwait(continueOnCapturedContext: false);
				result2 = result3;
			}
			goto end_IL_0122;
			IL_047e:
			ExecuteResult executeResult2 = ExecuteResult.FromSuccess();
			result2 = executeResult2;
			end_IL_0122:;
		}
		catch (Exception innerException)
		{
			Exception originalEx = innerException;
			while (innerException is TargetInvocationException)
			{
				innerException = innerException.InnerException;
			}
			CommandException exception = new CommandException(this, context, innerException);
			await Module.Service._cmdLogger.ErrorAsync(exception).ConfigureAwait(continueOnCapturedContext: false);
			ExecuteResult result3 = ExecuteResult.FromError(innerException);
			await Module.Service._commandExecutedEvent.InvokeAsync(this, context, result3).ConfigureAwait(continueOnCapturedContext: false);
			if (Module.Service._throwOnError)
			{
				if (innerException == originalEx)
				{
					throw;
				}
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			result2 = result3;
		}
		finally
		{
			await Module.Service._cmdLogger.VerboseAsync("Executed " + GetLogText(context)).ConfigureAwait(continueOnCapturedContext: false);
		}
		return result2;
	}

	private object[] GenerateArgs(IEnumerable<object> argList, IEnumerable<object> paramsList)
	{
		int num = Parameters.Count;
		object[] array = new object[Parameters.Count];
		if (HasVarArgs)
		{
			num--;
		}
		int num2 = 0;
		foreach (object arg in argList)
		{
			if (num2 == num)
			{
				throw new InvalidOperationException("Command was invoked with too many parameters.");
			}
			array[num2++] = arg;
		}
		if (num2 < num)
		{
			throw new InvalidOperationException("Command was invoked with too few parameters.");
		}
		if (HasVarArgs)
		{
			Func<IEnumerable<object>, object> orAdd = _arrayConverters.GetOrAdd(Parameters[Parameters.Count - 1].Type, (Type t) => (Func<IEnumerable<object>, object>)_convertParamsMethod.MakeGenericMethod(t).CreateDelegate(typeof(Func<IEnumerable<object>, object>)));
			array[num2] = orAdd(paramsList);
		}
		return array;
	}

	private static T[] ConvertParamsList<T>(IEnumerable<object> paramsList)
	{
		return paramsList.Cast<T>().ToArray();
	}

	internal string GetLogText(ICommandContext context)
	{
		if (context.Guild != null)
		{
			return $"\"{Name}\" for {context.User} in {context.Guild}/{context.Channel}";
		}
		return $"\"{Name}\" for {context.User} in {context.Channel}";
	}
}
