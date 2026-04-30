using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IComponentInteraction : IDiscordInteraction, ISnowflakeEntity, IEntity<ulong>
{
	new IComponentInteractionData Data { get; }

	IUserMessage Message { get; }

	Task UpdateAsync(Action<MessageProperties> func, RequestOptions options = null);

	Task DeferLoadingAsync(bool ephemeral = false, RequestOptions options = null);
}
