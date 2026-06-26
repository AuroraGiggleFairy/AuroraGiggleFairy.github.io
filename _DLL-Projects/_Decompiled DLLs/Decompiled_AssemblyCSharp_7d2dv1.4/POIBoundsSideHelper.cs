using UnityEngine;

public class POIBoundsSideHelper : MonoBehaviour
{
	public enum SideTypes
	{
		PositiveX,
		NegativeX,
		PositiveZ,
		NegativeZ
	}

	public SideTypes SideType;

	public POIBoundsHelper Owner;

	public MeshRenderer MeshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
	}

	public void Setup()
	{
		MeshRenderer = GetComponent<MeshRenderer>();
	}

	public void SetSize(Vector3 size)
	{
		switch (SideType)
		{
		case SideTypes.PositiveX:
			base.transform.localPosition = new Vector3(size.z * 0.5f, 0f, 0f);
			base.transform.localScale = new Vector3(size.x, 100f, 6f);
			break;
		case SideTypes.NegativeX:
			base.transform.localPosition = new Vector3(size.z * -0.5f, 0f, 0f);
			base.transform.localScale = new Vector3(size.x, 100f, 6f);
			break;
		case SideTypes.PositiveZ:
			base.transform.localPosition = new Vector3(0f, 0f, size.x * 0.5f);
			base.transform.localScale = new Vector3(size.z, 100f, 6f);
			break;
		case SideTypes.NegativeZ:
			base.transform.localPosition = new Vector3(0f, 0f, size.x * -0.5f);
			base.transform.localScale = new Vector3(size.z, 100f, 6f);
			break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.layer == 24)
		{
			Owner.AddSideEntered(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == 24)
		{
			Owner.RemoveSideEntered(this);
		}
	}
}
