using System;
using System.Text;
using UnityEngine.Scripting;

namespace Webserver.WebAPI;

[Preserve]
public class Null : AbsWebAPI
{
	public Null(string _name)
		: base(_name)
	{
	}

	public override void HandleRequest(RequestContext _context)
	{
		_context.Response.ContentLength64 = 0L;
		_context.Response.ContentType = "text/plain";
		_context.Response.ContentEncoding = Encoding.ASCII;
		_context.Response.OutputStream.Write(Array.Empty<byte>(), 0, 0);
	}
}
