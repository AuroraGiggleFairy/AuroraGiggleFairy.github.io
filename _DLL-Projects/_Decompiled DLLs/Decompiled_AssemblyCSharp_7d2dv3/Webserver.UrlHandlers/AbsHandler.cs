using Webserver.Permissions;

namespace Webserver.UrlHandlers;

public abstract class AbsHandler
{
	public readonly string ModuleName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string urlBasePath;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Web parent;

	public string UrlBasePath => urlBasePath;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsHandler(string _moduleName, int _defaultPermissionLevel = 0)
	{
		ModuleName = _moduleName;
		AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(_moduleName, _defaultPermissionLevel, _isDefault: true));
	}

	public abstract void HandleRequest(RequestContext _context);

	public virtual bool IsAuthorizedForHandler(RequestContext _context)
	{
		if (ModuleName != null)
		{
			return AdminWebModules.Instance.ModuleAllowedWithLevel(ModuleName, _context.PermissionLevel);
		}
		return true;
	}

	public virtual void Shutdown()
	{
	}

	public virtual void SetBasePathAndParent(Web _parent, string _relativePath)
	{
		parent = _parent;
		urlBasePath = _relativePath;
		parent.OpenApiHelpers.LoadOpenApiSpec(this);
	}
}
