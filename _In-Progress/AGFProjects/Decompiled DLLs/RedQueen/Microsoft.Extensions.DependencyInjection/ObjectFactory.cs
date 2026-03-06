using System;

namespace Microsoft.Extensions.DependencyInjection;

internal delegate object ObjectFactory(IServiceProvider serviceProvider, object?[]? arguments);
