using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddWallVolume : NetPackageEditorAddSleeperVolume
{
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabVolumeManager.Instance.AddWallVolumeServer(startPos, size);
		}
	}
}
