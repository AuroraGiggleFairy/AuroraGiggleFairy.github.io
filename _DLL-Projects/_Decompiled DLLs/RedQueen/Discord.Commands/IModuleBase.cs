using System.Threading.Tasks;
using Discord.Commands.Builders;

namespace Discord.Commands;

internal interface IModuleBase
{
	void SetContext(ICommandContext context);

	Task BeforeExecuteAsync(CommandInfo command);

	void BeforeExecute(CommandInfo command);

	Task AfterExecuteAsync(CommandInfo command);

	void AfterExecute(CommandInfo command);

	void OnModuleBuilding(CommandService commandService, ModuleBuilder builder);
}
