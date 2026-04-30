using System;

namespace Microsoft.Extensions.DependencyInjection;

internal interface ISupportRequiredService
{
	object GetRequiredService(Type serviceType);
}
