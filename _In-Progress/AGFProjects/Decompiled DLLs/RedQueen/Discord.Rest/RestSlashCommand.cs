using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestSlashCommand : RestCommandBase, ISlashCommandInteraction, IApplicationCommandInteraction, IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	public new RestSlashCommandData Data { get; private set; }

	IApplicationCommandInteractionData ISlashCommandInteraction.Data => Data;

	IApplicationCommandInteractionData IApplicationCommandInteraction.Data => Data;

	internal RestSlashCommand(DiscordRestClient client, Interaction model)
		: base(client, model)
	{
	}

	internal new static async Task<RestSlashCommand> CreateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		RestSlashCommand entity = new RestSlashCommand(client, model);
		await entity.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}

	internal override async Task UpdateAsync(DiscordRestClient client, Interaction model, bool doApiCall)
	{
		await base.UpdateAsync(client, model, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		ApplicationCommandInteractionData model2 = (model.Data.IsSpecified ? ((ApplicationCommandInteractionData)model.Data.Value) : null);
		Data = await RestSlashCommandData.CreateAsync(client, model2, base.Guild, base.Channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
	}
}
