namespace Platform;

public interface IUserDataRoaming
{
	bool IsSupported { get; }

	SaveRoamingMode SaveRoamingMode { get; }

	bool SaveRoamingEnabled { get; }

	bool IsRoamingOptional { get; }

	UserDataStorageType DefaultSaveStorage { get; }

	void Init(IPlatform platform);

	void ValidateRoamingMode();
}
