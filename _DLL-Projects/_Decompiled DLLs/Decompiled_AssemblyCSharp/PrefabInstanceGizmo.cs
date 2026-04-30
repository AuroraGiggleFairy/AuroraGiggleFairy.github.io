using UnityEngine;

public class PrefabInstanceGizmo : MonoBehaviour
{
	public static PrefabInstance Selected;

	public PrefabInstance pi;

	public Vector3 pos;

	public int rot;

	public bool bSelected;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmos()
	{
		if (pi != null)
		{
			Gizmos.color = ((!bSelected) ? Color.green : Color.yellow);
			if (!bSelected && base.transform.hasChanged)
			{
				Gizmos.color = Color.cyan;
			}
			Vector3 vector = pi.boundingBoxSize.ToVector3();
			Gizmos.DrawSphere(base.transform.position + new Vector3(0f, vector.y + 1f, 0f), 1.5f);
			Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, (vector.y - (float)pi.prefab.yOffset) / 2f, 0f), pi.boundingBoxSize.ToVector3() + new Vector3(0f, pi.prefab.yOffset, 0f));
			if (pi.prefab.yOffset != 0)
			{
				Gizmos.color = ((!bSelected) ? new Color(0f, 0.5f, 0f, 0.5f) : new Color(0.7f, 0.7f, 0f, 0.5f));
				if (!bSelected && base.transform.hasChanged)
				{
					Gizmos.color = Color.cyan;
				}
				Gizmos.DrawCube(base.transform.position + new Vector3(0f, (float)(-1 * pi.prefab.yOffset) / 2f, 0f), new Vector3(pi.boundingBoxSize.x, (float)(-1 * pi.prefab.yOffset) - 0.1f, pi.boundingBoxSize.z));
				Gizmos.color = ((!bSelected) ? Color.green : Color.yellow);
				if (!bSelected && base.transform.hasChanged)
				{
					Gizmos.color = Color.cyan;
				}
				Gizmos.DrawWireCube(base.transform.position + new Vector3(0f, (float)(-1 * pi.prefab.yOffset) / 2f, 0f), new Vector3(pi.boundingBoxSize.x, (float)(-1 * pi.prefab.yOffset) - 0.1f, pi.boundingBoxSize.z));
			}
			pos = base.transform.position - new Vector3((float)pi.boundingBoxSize.x * 0.5f, 0f, (float)pi.boundingBoxSize.z * 0.5f);
			Vector3 vector2 = Vector3.zero;
			switch (pi.rotation)
			{
			case 0:
				vector2 = new Vector3(-0.5f, 0f, -0.5f);
				break;
			case 1:
				vector2 = new Vector3(-0.5f, 0f, 0.5f);
				break;
			case 2:
				vector2 = new Vector3(0.5f, 0f, 0.5f);
				break;
			case 3:
				vector2 = new Vector3(0.5f, 0f, -0.5f);
				break;
			}
			if (Utils.FastAbs(pos.x - (float)(int)pos.x) > 0.001f)
			{
				pos.x -= vector2.x;
			}
			if (Utils.FastAbs(pos.z - (float)(int)pos.z) > 0.001f)
			{
				pos.z -= vector2.z;
			}
			rot = pi.rotation;
			if (Selected == pi)
			{
				pi.boundingBoxPosition = World.worldToBlockPos(pos);
				pi.rotation = (byte)rot;
			}
		}
		bSelected = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDrawGizmosSelected()
	{
		bSelected = true;
		Selected = pi;
	}
}
