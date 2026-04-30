using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal interface ICommandInfo
{
	string Name { get; }

	string MethodName { get; }

	bool IgnoreGroupNames { get; }

	bool SupportsWildCards { get; }

	bool IsTopLevelCommand { get; }

	ModuleInfo Module { get; }

	InteractionService CommandService { get; }

	RunMode RunMode { get; }

	IReadOnlyCollection<Attribute> Attributes { get; }

	IReadOnlyCollection<PreconditionAttribute> Preconditions { get; }

	IReadOnlyCollection<IParameterInfo> Parameters { get; }

	Task<IResult> ExecuteAsync(IInteractionContext context, IServiceProvider services);

	Task<PreconditionResult> CheckPreconditionsAsync(IInteractionContext context, IServiceProvider services);
}
