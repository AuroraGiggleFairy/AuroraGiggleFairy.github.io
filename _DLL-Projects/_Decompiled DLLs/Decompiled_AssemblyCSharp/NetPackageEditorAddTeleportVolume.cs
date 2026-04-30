using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddTeleportVolume : NetPackageEditorAddSleeperVolume
{
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabVolumeManager.Instance.AddTeleportVolumeServer(startPos, size);
		}
	}
}
