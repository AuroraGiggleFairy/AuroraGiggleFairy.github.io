using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestMessageCommandData : RestCommandBaseData, IMessageCommandInteractionData, IApplicationCommandInteractionData, IDiscordInteractionData
{
	public RestMessage Message => ResolvableData?.Messages.FirstOrDefault().Value;

	public override IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	IMessage IMessageCommandInteractionData.Message => Message;

	internal RestMessageCommandData(DiscordRestClient client, ApplicationCommandInteractionData model)
		: base(client, model)
	{
	}

	internal new static async Task<RestMessageCommandData> CreateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		RestMessageCommandData entity = new RestMessageCommandData(client, model);
		await entity.UpdateAsync(client, model, guild, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}
}
