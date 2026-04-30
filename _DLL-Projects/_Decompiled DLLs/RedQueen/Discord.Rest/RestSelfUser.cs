using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestSelfUser : RestUser, ISelfUser, IUser, ISnowflakeEntity, IEntity<ulong>, IMentionable, IPresence
{
	public string Email { get; private set; }

	public bool IsVerified { get; private set; }

	public bool IsMfaEnabled { get; private set; }

	public UserProperties Flags { get; private set; }

	public PremiumType PremiumType { get; private set; }

	public string Locale { get; private set; }

	internal RestSelfUser(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal new static RestSelfUser Create(BaseDiscordClient discord, User model)
	{
		RestSelfUser restSelfUser = new RestSelfUser(discord, model.Id);
		restSelfUser.Update(model);
		return restSelfUser;
	}

	internal override void Update(User model)
	{
		base.Update(model);
		if (model.Email.IsSpecified)
		{
			Email = model.Email.Value;
		}
		if (model.Verified.IsSpecified)
		{
			IsVerified = model.Verified.Value;
		}
		if (model.MfaEnabled.IsSpecified)
		{
			IsMfaEnabled = model.MfaEnabled.Value;
		}
		if (model.Flags.IsSpecified)
		{
			Flags = model.Flags.Value;
		}
		if (model.PremiumType.IsSpecified)
		{
			PremiumType = model.PremiumType.Value;
		}
		if (model.Locale.IsSpecified)
		{
			Locale = model.Locale.Value;
		}
	}

	public override async Task UpdateAsync(RequestOptions options = null)
	{
		User user = await base.Discord.ApiClient.GetMyUserAsync(options).ConfigureAwait(continueOnCapturedContext: false);
		if (user.Id != base.Id)
		{
			throw new InvalidOperationException("Unable to update this object using a different token.");
		}
		Update(user);
	}

	public async Task ModifyAsync(Action<SelfUserProperties> func, RequestOptions options = null)
	{
		if (base.Id != base.Discord.CurrentUser.Id)
		{
			throw new InvalidOperationException("Unable to modify this object using a different token.");
		}
		Update(await UserHelper.ModifyAsync(this, base.Discord, func, options).ConfigureAwait(continueOnCapturedContext: false));
	}
}
