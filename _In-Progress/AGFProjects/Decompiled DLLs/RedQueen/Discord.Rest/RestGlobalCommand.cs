using System;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestGlobalCommand : RestApplicationCommand
{
	internal RestGlobalCommand(BaseDiscordClient client, ulong id)
		: base(client, id)
	{
	}

	internal static RestGlobalCommand Create(BaseDiscordClient client, ApplicationCommand model)
	{
		RestGlobalCommand restGlobalCommand = new RestGlobalCommand(client, model.Id);
		restGlobalCommand.Update(model);
		return restGlobalCommand;
	}

	public override async Task DeleteAsync(RequestOptions options = null)
	{
		await InteractionHelper.DeleteGlobalCommandAsync(base.Discord, this).ConfigureAwait(continueOnCapturedContext: false);
	}

	public override async Task ModifyAsync<TArg>(Action<TArg> func, RequestOptions options = null)
	{
		Update(await InteractionHelper.ModifyGlobalCommandAsync(base.Discord, this, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}
}
