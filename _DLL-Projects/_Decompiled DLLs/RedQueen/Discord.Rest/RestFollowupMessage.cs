using System;
using System.Net;
using System.Threading.Tasks;
using Discord.API;
using Discord.Net;

namespace Discord.Rest;

internal class RestFollowupMessage : RestUserMessage
{
	internal string Token { get; }

	internal RestFollowupMessage(BaseDiscordClient discord, ulong id, IUser author, string token, IMessageChannel channel)
		: base(discord, id, channel, author, MessageSource.Bot)
	{
		Token = token;
	}

	internal static RestFollowupMessage Create(BaseDiscordClient discord, Message model, string token, IMessageChannel channel)
	{
		ulong id = model.Id;
		IUser author;
		if (!model.Author.IsSpecified)
		{
			IUser currentUser = discord.CurrentUser;
			author = currentUser;
		}
		else
		{
			IUser currentUser = RestUser.Create(discord, model.Author.Value);
			author = currentUser;
		}
		RestFollowupMessage restFollowupMessage = new RestFollowupMessage(discord, id, author, token, channel);
		restFollowupMessage.Update(model);
		return restFollowupMessage;
	}

	internal new void Update(Message model)
	{
		base.Update(model);
	}

	public Task DeleteAsync()
	{
		return InteractionHelper.DeleteFollowupMessageAsync(base.Discord, this);
	}

	public new async Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		try
		{
			Update(await InteractionHelper.ModifyFollowupMessageAsync(base.Discord, this, func, options).ConfigureAwait(continueOnCapturedContext: false));
		}
		catch (HttpException ex)
		{
			if (ex.HttpCode == HttpStatusCode.NotFound)
			{
				throw new InvalidOperationException("The token of this message has expired!", ex);
			}
			throw;
		}
	}
}
