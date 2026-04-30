using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestUserCommand : RestCommandBase, IUserCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new RestUserCommandData Data { get; private set; }

	IUserCommandInteractionData IUserCommandInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal RestUserCommand(DiscordRestClient client, Interaction model)
		: base(client, model)
	{
	}

	internal new static async Task<RestUserCommand> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestUserCommand entity = new RestUserCommand(client, model);
		await entity.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal override async Task UpdateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		await base.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		Data = await RestUserCommandData.CreateAsync(client, model2, base.Guild, base.Channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
	}
}
