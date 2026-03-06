using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Discord.Interactions.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions;

internal abstract class CommandInfo<TParameter> : ICommandInfo where TParameter : class, IParameterInfo
{
	private readonly ExecuteCallback _action;

	private readonly ILookup<string, PreconditionAttribute> _groupedPreconditions;

	internal IReadOnlyDictionary<string, TParameter> _parameterDictionary { get; }

	public ModuleInfo Module { get; }

	public InteractionService CommandService { get; }

	public string Name { get; }

	public string MethodName { get; }

	public virtual bool IgnoreGroupNames { get; }

	public abstract bool SupportsWildCards { get; }

	public bool IsTopLevelCommand { get; }

	public RunMode RunMode { get; }

	public IReadOnlyCollection<Attribute> Attributes { get; }

	public IReadOnlyCollection<PreconditionAttribute> Preconditions { get; }

	public abstract IReadOnlyCollection<TParameter> Parameters { get; }

	IReadOnlyCollection<IParameterInfo> ICommandInfo.Parameters => Parameters;

	internal CommandInfo(ICommandBuilder builder, ModuleInfo module, InteractionService commandService)
	{
		CommandService = commandService;
		Module = module;
		Name = builder.Name;
		MethodName = builder.MethodName;
		IgnoreGroupNames = builder.IgnoreGroupNames;
		IsTopLevelCommand = IgnoreGroupNames || CheckTopLevel(Module);
		RunMode = ((builder.RunMode != RunMode.Default) ? builder.RunMode : commandService._runMode);
		Attributes = builder.Attributes.ToImmutableArray();
		Preconditions = builder.Preconditions.ToImmutableArray();
		_action = builder.Callback;
		_groupedPreconditions = builder.Preconditions.ToLookup((PreconditionAttribute x) => x.Group, (PreconditionAttribute x) => x, StringComparer.Ordinal);
		_parameterDictionary = Parameters?.ToDictionary((TParameter x) => x.Name, (TParameter x) => x).ToImmutableDictionary();
	}

	public abstract Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services);

	protected abstract Task InvokeModuleEvent(IInteractionContext context, IResult result);

	protected abstract string GetLogString(IInteractionContext context);

	public async Task<PreconditionResult> CheckPreconditionsAsync(IInteractionContext context, IServiceProvider services)
	{
		PreconditionResult preconditionResult = await CheckGroups(Module.GroupedPreconditions, "Module").ConfigureAwait(continueOnCapturedContext: false);
		if (!preconditionResult.IsSuccess)
		{
			return preconditionResult;
		}
		PreconditionResult preconditionResult2 = await CheckGroups(_groupedPreconditions, "Command").ConfigureAwait(continueOnCapturedContext: false);
		return (!preconditionResult2.IsSuccess) ? preconditionResult2 : PreconditionResult.FromSuccess();
		async Task<PreconditionResult> CheckGroups(ILookup<string, PreconditionAttribute> preconditions, string type)
		{
			foreach (IGrouping<string, PreconditionAttribute> preconditionGroup in preconditions)
			{
				if (preconditionGroup.Key == null)
				{
					foreach (PreconditionAttribute item in preconditionGroup)
					{
						PreconditionResult preconditionResult3 = await item.CheckRequirementsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false);
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
						list.Add(await item2.CheckRequirementsAsync(context, this, services).ConfigureAwait(continueOnCapturedContext: false));
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

	protected async Task<IResult> RunAsync(IInteractionContext context, object[] args, IServiceProvider services)
	{
		switch (RunMode)
		{
		case RunMode.Sync:
			if (CommandService._autoServiceScopes)
			{
				using (IServiceScope scope = services?.CreateScope())
				{
					return await ExecuteInternalAsync(context, args, scope?.ServiceProvider ?? EmptyServiceProvider.Instance).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			return await ExecuteInternalAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
		case RunMode.Async:
			Task.Run(async delegate
			{
				if (CommandService._autoServiceScopes)
				{
					using IServiceScope scope2 = services?.CreateScope();
					await ExecuteInternalAsync(context, args, scope2?.ServiceProvider ?? EmptyServiceProvider.Instance).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await ExecuteInternalAsync(context, args, services).ConfigureAwait(continueOnCapturedContext: false);
				}
			});
			return ExecuteResult.FromSuccess();
		default:
			throw new InvalidOperationException($"RunMode {RunMode} is not supported.");
		}
	}

	private async Task<IResult> ExecuteInternalAsync(IInteractionContext context, object[] args, IServiceProvider services)
	{
		await CommandService._cmdLogger.DebugAsync("Executing " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		IResult result;
		try
		{
			PreconditionResult preconditionResult = await CheckPreconditionsAsync(context, services).ConfigureAwait(continueOnCapturedContext: false);
			if (!preconditionResult.IsSuccess)
			{
				result = await InvokeEventAndReturn(context, preconditionResult).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				int index = 0;
				foreach (TParameter parameter in Parameters)
				{
					PreconditionResult preconditionResult2 = await parameter.CheckPreconditionsAsync(context, args[index++], services).ConfigureAwait(continueOnCapturedContext: false);
					if (preconditionResult2.IsSuccess)
					{
						continue;
					}
					result = await InvokeEventAndReturn(context, preconditionResult2).ConfigureAwait(continueOnCapturedContext: false);
					goto end_IL_0141;
				}
				Task task = _action(context, args, services, this);
				if (task is Task<IResult> task2)
				{
					IResult result2 = await task2.ConfigureAwait(continueOnCapturedContext: false);
					await InvokeModuleEvent(context, result2).ConfigureAwait(continueOnCapturedContext: false);
					result = ((!(result2 is RuntimeResult) && !(result2 is ExecuteResult)) ? (await InvokeEventAndReturn(context, ExecuteResult.FromError(InteractionCommandError.Unsuccessful, "Command execution failed for an unknown reason")).ConfigureAwait(continueOnCapturedContext: false)) : result2);
				}
				else
				{
					await task.ConfigureAwait(continueOnCapturedContext: false);
					result = await InvokeEventAndReturn(context, ExecuteResult.FromSuccess()).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			end_IL_0141:;
		}
		catch (Exception innerException)
		{
			Exception originalEx = innerException;
			while (innerException is TargetInvocationException)
			{
				innerException = innerException.InnerException;
			}
			await Module.CommandService._cmdLogger.ErrorAsync(innerException).ConfigureAwait(continueOnCapturedContext: false);
			ExecuteResult result3 = ExecuteResult.FromError(innerException);
			await InvokeModuleEvent(context, result3).ConfigureAwait(continueOnCapturedContext: false);
			if (Module.CommandService._throwOnError)
			{
				if (innerException == originalEx)
				{
					throw;
				}
				ExceptionDispatchInfo.Capture(innerException).Throw();
			}
			result = result3;
		}
		finally
		{
			await CommandService._cmdLogger.VerboseAsync("Executed " + GetLogString(context)).ConfigureAwait(continueOnCapturedContext: false);
		}
		return result;
	}

	protected async ValueTask<IResult> InvokeEventAndReturn(IInteractionContext context, IResult result)
	{
		await InvokeModuleEvent(context, result).ConfigureAwait(continueOnCapturedContext: false);
		return result;
	}

	private static bool CheckTopLevel(ModuleInfo parent)
	{
		for (ModuleInfo moduleInfo = parent; moduleInfo != null; moduleInfo = moduleInfo.Parent)
		{
			if (moduleInfo.IsSlashGroup)
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		for (ModuleInfo moduleInfo = Module; moduleInfo != null; moduleInfo = moduleInfo.Parent)
		{
			if (moduleInfo.IsSlashGroup)
			{
				list.Add(moduleInfo.SlashGroupName);
			}
		}
		list.Reverse();
		list.Add(Name);
		return string.Join(" ", list);
	}
}
