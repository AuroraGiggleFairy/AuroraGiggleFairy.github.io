using System.Net;
using UnityEngine.Scripting;

namespace Webserver.WebAPI.APIs;

[Preserve]
public class OpenAPI : AbsRestApi
{
	public OpenAPI(Web _parentWeb)
		: base(_parentWeb)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleRestGet(RequestContext _context)
	{
		string requestPath = _context.RequestPath;
		if (!ParentWeb.OpenApiHelpers.TryGetOpenApiSpec(requestPath, out var _specText))
		{
			WebUtils.WriteText(_context.Response, "Spec for " + requestPath + " not found", HttpStatusCode.NotFound);
		}
		else
		{
			WebUtils.WriteText(_context.Response, _specText, HttpStatusCode.OK, "text/x-yaml");
		}
	}

	public override int DefaultPermissionLevel()
	{
		return 2000;
	}
}
