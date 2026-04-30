using System;

namespace Microsoft.Extensions.DependencyInjection;

internal interface IServiceScope : IDisposable
{
	IServiceProvider ServiceProvider { get; }
}
