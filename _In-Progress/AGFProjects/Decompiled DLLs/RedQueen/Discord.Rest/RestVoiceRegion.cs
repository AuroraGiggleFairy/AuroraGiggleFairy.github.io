using System.Diagnostics;
using Discord.API;

namespace Discord.Rest;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class RestVoiceRegion : RestEntity<string>, IVoiceRegion
{
	public string Name { get; private set; }

	public bool IsVip { get; private set; }

	public bool IsOptimal { get; private set; }

	public bool IsDeprecated { get; private set; }

	public bool IsCustom { get; private set; }

	private string DebuggerDisplay => Name + " (" + base.Id + (IsVip ? ", VIP" : "") + (IsOptimal ? ", Optimal" : "") + ")";

	internal RestVoiceRegion(BaseDiscordClient client, string id)
		: base(client, id)
	{
	}

	internal static RestVoiceRegion Create(BaseDiscordClient client, VoiceRegion model)
	{
		RestVoiceRegion restVoiceRegion = new RestVoiceRegion(client, model.Id);
		restVoiceRegion.Update(model);
		return restVoiceRegion;
	}

	internal void Update(VoiceRegion model)
	{
		Name = model.Name;
		IsVip = model.IsVip;
		IsOptimal = model.IsOptimal;
		IsDeprecated = model.IsDeprecated;
		IsCustom = model.IsCustom;
	}

	public override string ToString()
	{
		return Name;
	}
}
