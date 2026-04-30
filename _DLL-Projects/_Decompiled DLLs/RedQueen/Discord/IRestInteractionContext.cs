using System;
using System.Threading.Tasks;

namespace Discord;

internal interface IRestInteractionContext : IInteractionContext
{
	Func<string, Task> InteractionResponseCallback { get; }
}
