using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal static class IDiscordInteractionExtentions
{
	public static async Task RespondWithModalAsync<T>(this IDiscordInteraction interaction, string customId, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		if (!ModalUtils.TryGet<T>(out var modalInfo))
		{
			throw new ArgumentException($"{typeof(T).FullName} isn't referenced by any registered Modal Interaction Command and doesn't have a cached {typeof(ModalInfo)}");
		}
		await SendModalResponseAsync(interaction, customId, modalInfo, options, modifyModal);
	}

	public static async Task RespondWithModalAsync<T>(this IDiscordInteraction interaction, string customId, InteractionService interactionService, RequestOptions options = null, Action<ModalBuilder> modifyModal = null) where T : class, IModal
	{
		ModalInfo orAdd = ModalUtils.GetOrAdd<T>(interactionService);
		await SendModalResponseAsync(interaction, customId, orAdd, options, modifyModal);
	}

	private static async Task SendModalResponseAsync(IDiscordInteraction interaction, string customId, ModalInfo modalInfo, RequestOptions options = null, Action<ModalBuilder> modifyModal = null)
	{
		ModalBuilder modalBuilder = new ModalBuilder(modalInfo.Title, customId);
		foreach (InputComponentInfo component in modalInfo.Components)
		{
			if (component is TextInputComponentInfo textInputComponentInfo)
			{
				modalBuilder.AddTextInput(textInputComponentInfo.Label, textInputComponentInfo.CustomId, textInputComponentInfo.Style, textInputComponentInfo.Placeholder, textInputComponentInfo.IsRequired ? new int?(textInputComponentInfo.MinLength) : ((int?)null), textInputComponentInfo.MaxLength, textInputComponentInfo.IsRequired, textInputComponentInfo.InitialValue);
				continue;
			}
			throw new InvalidOperationException(component.GetType().FullName + " isn't a valid component info class");
		}
		modifyModal?.Invoke(modalBuilder);
		await interaction.RespondWithModalAsync(modalBuilder.Build(), options).ConfigureAwait(continueOnCapturedContext: false);
	}
}
