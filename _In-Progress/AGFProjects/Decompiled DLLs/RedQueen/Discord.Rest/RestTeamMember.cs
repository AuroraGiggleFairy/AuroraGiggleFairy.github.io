using System;
using Discord.API;

namespace Discord.Rest;

internal class RestTeamMember : ITeamMember
{
	public MembershipState MembershipState { get; }

	public string[] Permissions { get; }

	public ulong TeamId { get; }

	public IUser User { get; }

	internal RestTeamMember(BaseDiscordClient discord, TeamMember model)
	{
		MembershipState = model.MembershipState switch
		{
			Discord.API.MembershipState.Invited => MembershipState.Invited, 
			Discord.API.MembershipState.Accepted => MembershipState.Accepted, 
			_ => throw new InvalidOperationException("Invalid membership state"), 
		};
		Permissions = model.Permissions;
		TeamId = model.TeamId;
		User = RestUser.Create(discord, model.User);
	}
}
