using System;
using Discord.Interactions;

namespace Discord.Rest;

internal static class RestExtensions
{
	public static string RespondWithModal<T>(this RestInteraction interaction, string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		if (!ModalUtils.TryGet<T>(out var modalInfo))
		{
			throw new ArgumentException($"{typeof(T).FullName} isn't referenced by any registered Modal Interaction Command and doesn't have a cached {typeof(ModalInfo)}");
		}
		Modal modal = modalInfo.ToModal(customId, modifyModal);
		return interaction.RespondWithModal(modal, options);
	}
}
