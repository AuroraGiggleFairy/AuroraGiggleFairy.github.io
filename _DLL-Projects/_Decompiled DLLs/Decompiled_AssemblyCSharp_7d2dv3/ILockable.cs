using System.Collections.Generic;

public interface ILockable
{
	bool IsLocked();

	void SetLocked(bool _isLocked);

	PlatformUserIdentifierAbs GetOwner();

	void SetOwner(PlatformUserIdentifierAbs _userIdentifier);

	bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier);

	List<PlatformUserIdentifierAbs> GetUsers();

	bool LocalPlayerIsOwner();

	bool IsOwner(PlatformUserIdentifierAbs _userIdentifier);

	bool HasPassword();

	bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier);

	bool CheckPasswordHash(string _password, PlatformUserIdentifierAbs _userIdentifier);

	string GetPasswordHash();

	string GetHashForPassword(string _password)
	{
		return _password.GetStableHashCode().ToString("X8");
	}
}
