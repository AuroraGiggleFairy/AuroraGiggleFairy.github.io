using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Discord.API;

namespace Discord.Rest;

internal class RestTeam : RestEntity<ulong>, ITeam
{
	private string _iconId;

	public string IconUrl
	{
		get
		{
			if (_iconId == null)
			{
				return null;
			}
			return CDN.GetTeamIconUrl(base.Id, _iconId);
		}
	}

	public IReadOnlyList<ITeamMember> TeamMembers { get; private set; }

	public string Name { get; private set; }

	public ulong OwnerUserId { get; private set; }

	internal RestTeam(BaseDiscordClient discord, ulong id)
		: base(discord, id)
	{
	}

	internal static RestTeam Create(BaseDiscordClient discord, Team model)
	{
		RestTeam restTeam = new RestTeam(discord, model.Id);
		restTeam.Update(model);
		return restTeam;
	}

	internal virtual void Update(Team model)
	{
		if (model.Icon.IsSpecified)
		{
			_iconId = model.Icon.Value;
		}
		Name = model.Name;
		OwnerUserId = model.OwnerUserId;
		TeamMembers = model.TeamMembers.Select((TeamMember x) => new RestTeamMember(base.Discord, x)).ToImmutableArray();
	}
}
