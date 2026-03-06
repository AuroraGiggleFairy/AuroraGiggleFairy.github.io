using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Commands;

internal static class CommandServiceExtensions
{
	public static async Task<IReadOnlyCollection<CommandInfo>> GetExecutableCommandsAsync(this ICollection<CommandInfo> commands, ICommandContext context, IServiceProvider provider)
	{
		List<CommandInfo> executableCommands = new List<CommandInfo>();
		var array = await Task.WhenAll(commands.Select(async (CommandInfo c) => new
		{
			Command = c,
			PreconditionResult = await c.CheckPreconditionsAsync(context, provider).ConfigureAwait(continueOnCapturedContext: false)
		})).ConfigureAwait(continueOnCapturedContext: false);
		foreach (var anon in array)
		{
			if (anon.PreconditionResult.IsSuccess)
			{
				executableCommands.Add(anon.Command);
			}
		}
		return executableCommands;
	}

	public static Task<IReadOnlyCollection<CommandInfo>> GetExecutableCommandsAsync(this CommandService commandService, ICommandContext context, IServiceProvider provider)
	{
		return commandService.Commands.ToArray().GetExecutableCommandsAsync(context, provider);
	}

	public static async Task<IReadOnlyCollection<CommandInfo>> GetExecutableCommandsAsync(this ModuleInfo module, ICommandContext context, IServiceProvider provider)
	{
		List<CommandInfo> executableCommands = new List<CommandInfo>();
		List<CommandInfo> list = executableCommands;
		list.AddRange(await module.Commands.ToArray().GetExecutableCommandsAsync(context, provider).ConfigureAwait(continueOnCapturedContext: false));
		executableCommands.AddRange((await Task.WhenAll(module.Submodules.Select(async (ModuleInfo s) => await s.GetExecutableCommandsAsync(context, provider).ConfigureAwait(continueOnCapturedContext: false))).ConfigureAwait(continueOnCapturedContext: false)).SelectMany((IReadOnlyCollection<CommandInfo> c) => c));
		return executableCommands;
	}
}
