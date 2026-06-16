using Webserver.Permissions;

namespace Webserver.WebAPI;

public abstract class AbsWebAPI
{
	public readonly string Name;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly Web ParentWeb;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string CachedApiModuleName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsWebAPI(string _name = null)
		: this(null, _name)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AbsWebAPI(Web _parentWeb, string _name = null)
	{
		Name = _name ?? GetType().Name;
		ParentWeb = _parentWeb;
		CachedApiModuleName = "webapi." + Name;
		RegisterPermissions();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RegisterPermissions()
	{
		AdminWebModules.Instance.AddKnownModule(new AdminWebModules.WebModule(CachedApiModuleName, DefaultPermissionLevel(), _isDefault: true));
	}

	public abstract void HandleRequest(RequestContext _context);

	public virtual bool Authorized(RequestContext _context)
	{
		return AdminWebModules.Instance.GetModule(CachedApiModuleName).LevelGlobal >= _context.PermissionLevel;
	}

	public virtual int DefaultPermissionLevel()
	{
		return 0;
	}
}
