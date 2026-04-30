namespace Microsoft.Extensions.DependencyInjection;

internal interface IServiceScopeFactory
{
	IServiceScope CreateScope();
}
