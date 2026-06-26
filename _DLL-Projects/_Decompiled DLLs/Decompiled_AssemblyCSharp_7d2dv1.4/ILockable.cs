using System.Collections.Generic;

public interface ILockable
{
	int EntityId { get; set; }

	bool IsLocked();

	void SetLocked(bool _isLocked);

	PlatformUserIdentifierAbs GetOwner();

	void SetOwner(PlatformUserIdentifierAbs _userIdentifier);

	bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier);

	List<PlatformUserIdentifierAbs> GetUsers();

	bool LocalPlayerIsOwner();

	bool IsOwner(PlatformUserIdentifierAbs _userIdentifier);

	bool HasPassword();

	bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed);

	string GetPassword();
}
