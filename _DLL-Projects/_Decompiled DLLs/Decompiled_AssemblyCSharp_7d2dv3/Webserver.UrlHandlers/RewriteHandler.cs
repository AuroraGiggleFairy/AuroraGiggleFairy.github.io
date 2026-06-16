namespace Webserver.UrlHandlers;

public class RewriteHandler : AbsHandler
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly string target;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool fixedTarget;

	public RewriteHandler(string _target, bool _fixedTarget = false)
		: base(null)
	{
		target = _target;
		fixedTarget = _fixedTarget;
	}

	public override void HandleRequest(RequestContext _context)
	{
		_context.RequestPath = (fixedTarget ? target : (target + _context.RequestPath.Remove(0, urlBasePath.Length)));
		parent.ApplyPathHandler(_context);
	}
}
