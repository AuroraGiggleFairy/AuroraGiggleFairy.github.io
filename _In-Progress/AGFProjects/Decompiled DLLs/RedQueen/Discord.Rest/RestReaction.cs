using Discord.API;

namespace Discord.Rest;

internal class RestReaction : IReaction
{
	public IEmote Emote { get; }

	public int Count { get; }

	public bool Me { get; }

	internal RestReaction(IEmote emote, int count, bool me)
	{
		Emote = emote;
		Count = count;
		Me = me;
	}

	internal static RestReaction Create(Reaction model)
	{
		IEmote emote = ((!model.Emoji.Id.HasValue) ? ((IEmote)new Emoji(model.Emoji.Name)) : ((IEmote)new Emote(model.Emoji.Id.Value, model.Emoji.Name, model.Emoji.Animated == true)));
		return new RestReaction(emote, model.Count, model.Me);
	}
}
