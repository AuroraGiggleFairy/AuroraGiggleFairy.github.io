namespace Webserver.UrlHandlers;

public class SimpleRedirectHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string target;

	public SimpleRedirectHandler(string _target)
		: base(null)
	{
		target = _target;
	}

	public override void HandleRequest(RequestContext _context)
	{
		_context.Response.Redirect(target);
	}
}
