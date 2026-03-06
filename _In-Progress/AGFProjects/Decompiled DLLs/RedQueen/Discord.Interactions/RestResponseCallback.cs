using System.Threading.Tasks;

namespace Discord.Interactions;

internal delegate Task RestResponseCallback(IInteractionContext context, string responseBody);
