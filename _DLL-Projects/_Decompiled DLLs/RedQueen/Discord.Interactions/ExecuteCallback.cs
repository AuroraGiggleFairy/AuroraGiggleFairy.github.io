using System;
using System.Threading.Tasks;

namespace Discord.Interactions;

internal delegate Task ExecuteCallback(IInteractionContext context, object[] args, IServiceProvider serviceProvider, ICommandInfo commandInfo);
