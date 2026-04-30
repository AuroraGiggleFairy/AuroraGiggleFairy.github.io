using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal interface IInteractionModuleBase
{
	void SetContext(IInteractionContext context);

	Task BeforeExecuteAsync(ICommandInfo command);

	void BeforeExecute(ICommandInfo command);

	Task AfterExecuteAsync(ICommandInfo command);

	void AfterExecute(ICommandInfo command);

	void OnModuleBuilding(InteractionService commandService, ModuleInfo module);

	void Construct(ModuleBuilder builder, InteractionService commandService);
}
