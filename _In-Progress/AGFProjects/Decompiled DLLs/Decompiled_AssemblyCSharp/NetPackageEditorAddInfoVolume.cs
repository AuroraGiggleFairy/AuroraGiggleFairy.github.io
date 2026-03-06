using UnityEngine.Scripting;

[Preserve]
public class NetPackageEditorAddInfoVolume : NetPackageEditorAddSleeperVolume
{
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world != null && !_world.IsRemote())
		{
			PrefabVolumeManager.Instance.AddInfoVolumeServer(startPos, size);
		}
	}
}
