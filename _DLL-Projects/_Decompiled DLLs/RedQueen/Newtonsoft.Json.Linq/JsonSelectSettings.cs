using System;

namespace Newtonsoft.Json.Linq;

internal class JsonSelectSettings
{
	public TimeSpan? RegexMatchTimeout { get; set; }

	public bool ErrorWhenNoMatch { get; set; }
}
