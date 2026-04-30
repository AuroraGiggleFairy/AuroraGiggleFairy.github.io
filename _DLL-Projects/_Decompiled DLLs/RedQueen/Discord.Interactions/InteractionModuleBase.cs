using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Interactions.Builders;

namespace Discord.Interactions;

internal abstract class InteractionModuleBase<T> : IInteractionModuleBase where T : class, IInteractionContext
{
	public T Context { get; private set; }

	public virtual void AfterExecute(ICommandInfo command)
	{
	}

	public virtual void BeforeExecute(ICommandInfo command)
	{
	}

	public virtual Task BeforeExecuteAsync(ICommandInfo command)
	{
		return Task.CompletedTask;
	}

	public virtual Task AfterExecuteAsync(ICommandInfo command)
	{
		return Task.CompletedTask;
	}

	public virtual void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
	{
	}

	public virtual void Construct(ModuleBuilder builder, InteractionService commandService)
	{
	}

	internal void SetContext(IInteractionContext context)
	{
		T val = context as T;
		Context = val ?? throw new InvalidOperationException("Invalid context type. Expected " + typeof(T).Name + ", got " + context.GetType().Name + ".");
	}

	protected virtual async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
	{
		await Context.Interaction.DeferAsync(ephemeral, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
	{
		await Context.Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual Task RespondWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.RespondWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task RespondWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.RespondWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task RespondWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.RespondWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual async Task<IUserMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
	{
		return await Context.Interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.FollowupWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task<IUserMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual Task<IUserMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
	{
		return Context.Interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
	}

	protected virtual async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null)
	{
		return await Context.Channel.SendMessageAsync(text, isTTS: false, embed, options, allowedMentions, messageReference, components).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual Task<IUserMessage> GetOriginalResponseAsync(RequestOptions options = null)
	{
		return Context.Interaction.GetOriginalResponseAsync(options);
	}

	protected virtual Task<IUserMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options = null)
	{
		return Context.Interaction.ModifyOriginalResponseAsync(func, options);
	}

	protected virtual async Task DeleteOriginalResponseAsync()
	{
		await (await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(continueOnCapturedContext: false)).DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	protected virtual async Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
	{
		await Context.Interaction.RespondWithModalAsync(modal);
	}

	protected virtual async Task RespondWithModalAsync<TModal>(string customId, RequestOptions options = null) where TModal : class, IModal
	{
		await Context.Interaction.RespondWithModalAsync<TModal>(customId, options);
	}

	void IInteractionModuleBase.SetContext(IInteractionContext context)
	{
		SetContext(context);
	}
}
internal abstract class InteractionModuleBase : InteractionModuleBase<IInteractionContext>
{
}
