using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace Discord.Net.Rest;

internal struct RestResponse
{
	public HttpStatusCode StatusCode
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public Dictionary<string, string> Headers
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public Stream Stream
	{
		[_003C565f4ed8_002D6c7c_002D496e_002D81c6_002D6ecd6b2b714c_003EIsReadOnly]
		get;
	}

	public RestResponse(HttpStatusCode statusCode, Dictionary<string, string> headers, Stream stream)
	{
		StatusCode = statusCode;
		Headers = headers;
		Stream = stream;
	}
}
