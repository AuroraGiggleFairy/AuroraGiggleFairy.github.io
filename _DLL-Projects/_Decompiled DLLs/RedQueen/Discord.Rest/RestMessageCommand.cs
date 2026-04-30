using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestMessageCommand : RestCommandBase, IMessageCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new RestMessageCommandData Data { get; private set; }

	IMessageCommandInteractionData IMessageCommandInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal RestMessageCommand(DiscordRestClient client, Interaction model)
		: base(client, model)
	{
	}

	internal new static async Task<RestMessageCommand> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestMessageCommand entity = new RestMessageCommand(client, model);
		await entity.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal override async Task UpdateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		await base.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		Data = await RestMessageCommandData.CreateAsync(client, model2, base.Guild, base.Channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
	}
}
