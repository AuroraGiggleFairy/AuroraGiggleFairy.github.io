using System.Collections.Specialized;
using SpaceWizards.HttpListener;

namespace Webserver;

public class RequestContext
{
	public string RequestPath;

	public readonly ERequestMethod Method;

	public readonly HttpListenerRequest Request;

	[PublicizedFrom(EAccessModifier.Private)]
	public NameValueCollection queryParameters;

	public readonly HttpListenerResponse Response;

	public readonly WebConnection Connection;

	public readonly int PermissionLevel;

	public NameValueCollection QueryParameters => queryParameters ?? (queryParameters = Request.QueryString);

	public RequestContext(string _requestPath, HttpListenerRequest _request, HttpListenerResponse _response, WebConnection _connection, int _permissionLevel)
	{
		RequestPath = _requestPath;
		Request = _request;
		Response = _response;
		Connection = _connection;
		PermissionLevel = _permissionLevel;
		Method = _request.HttpMethod switch
		{
			"GET" => ERequestMethod.GET, 
			"PUT" => ERequestMethod.PUT, 
			"POST" => ERequestMethod.POST, 
			"DELETE" => ERequestMethod.DELETE, 
			"HEAD" => ERequestMethod.HEAD, 
			"OPTIONS" => ERequestMethod.OPTIONS, 
			_ => ERequestMethod.Other, 
		};
	}
}
