using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Discord.Interactions;

internal static class InteractionUtility
{
	public static async Task<SocketInteraction> WaitForInteractionAsync(BaseSocketClient client, TimeSpan timeout, Predicate<SocketInteraction> predicate, CancellationToken cancellationToken = default(CancellationToken))
	{
		TaskCompletionSource<SocketInteraction> tcs = new TaskCompletionSource<SocketInteraction>();
		CancellationTokenSource waitCancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		Task.Delay(timeout, waitCancelSource.Token).ContinueWith(delegate(Task t)
		{
			if (!t.IsCanceled)
			{
				tcs.SetResult(null);
			}
		});
		cancellationToken.Register(delegate
		{
			tcs.SetCanceled();
		});
		client.InteractionCreated += HandleInteraction;
		SocketInteraction result = await tcs.Task.ConfigureAwait(continueOnCapturedContext: false);
		client.InteractionCreated -= HandleInteraction;
		return result;
		Task HandleInteraction(SocketInteraction interaction)
		{
			if (predicate(interaction))
			{
				waitCancelSource.Cancel();
				tcs.SetResult(interaction);
			}
			return Task.CompletedTask;
		}
	}

	public static Task<SocketInteraction> WaitForMessageComponentAsync(BaseSocketClient client, IUserMessage fromMessage, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken))
	{
		return WaitForInteractionAsync(client, timeout, Predicate, cancellationToken);
		bool Predicate(SocketInteraction interaction)
		{
			if (interaction is SocketMessageComponent socketMessageComponent)
			{
				return socketMessageComponent.Message.Id == fromMessage.Id;
			}
			return false;
		}
	}

	public static async Task<bool> ConfirmAsync(BaseSocketClient client, IMessageChannel channel, TimeSpan timeout, string message = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (message == null)
		{
			message = "Would you like to continue?";
		}
		string confirmId = "confirm";
		string customId = "decline";
		MessageComponent components = new ComponentBuilder().WithButton("Confirm", confirmId, ButtonStyle.Success).WithButton("Cancel", customId, ButtonStyle.Danger).Build();
		IUserMessage prompt = await channel.SendMessageAsync(message, isTTS: false, null, null, null, null, components).ConfigureAwait(continueOnCapturedContext: false);
		SocketMessageComponent response = (await WaitForMessageComponentAsync(client, prompt, timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) as SocketMessageComponent;
		await prompt.DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (response != null && response.Data.CustomId == confirmId)
		{
			return true;
		}
		return false;
	}
}
