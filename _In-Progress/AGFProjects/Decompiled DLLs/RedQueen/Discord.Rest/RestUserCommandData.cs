using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

internal class RestUserCommandData : RestCommandBaseData, IUserCommandInteractionData, IApplicationCommandInteractionData, IDiscordInteractionData
{
	public RestUser Member => ResolvableData.GuildMembers.Values.FirstOrDefault() ?? ResolvableData.Users.Values.FirstOrDefault();

	public override IReadOnlyCollection<IApplicationCommandInteractionDataOption> Options
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	IUser IUserCommandInteractionData.User => Member;

	internal RestUserCommandData(DiscordRestClient client, ApplicationCommandInteractionData model)
		: base(client, model)
	{
	}

	internal new static async Task<RestUserCommandData> CreateAsync(DiscordRestClient client, ApplicationCommandInteractionData model, RestGuild guild, IRestMessageChannel channel, bool doApiCall)
	{
		RestUserCommandData entity = new RestUserCommandData(client, model);
		await entity.UpdateAsync(client, model, guild, channel, doApiCall).ConfigureAwait(continueOnCapturedContext: false);
		return entity;
	}
}
