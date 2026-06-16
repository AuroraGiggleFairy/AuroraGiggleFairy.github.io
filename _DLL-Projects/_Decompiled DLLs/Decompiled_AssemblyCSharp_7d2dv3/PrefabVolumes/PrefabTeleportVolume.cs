namespace PrefabVolumes;

public class PrefabTeleportVolume : PrefabVolumeAbs<PrefabTeleportVolume>
{
	public override EVolumeType VolumeType => EVolumeType.Teleport;

	public override int SerializedSize => 25;
}
