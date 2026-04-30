using System;

namespace Microsoft.Extensions.DependencyInjection;

internal interface IServiceProviderFactory<TContainerBuilder> where TContainerBuilder : notnull
{
	TContainerBuilder CreateBuilder(IServiceCollection services);

	IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder);
}
