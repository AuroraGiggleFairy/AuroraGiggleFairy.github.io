using System;
using UnityEngine;

public class AnimationTestSceneTools : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Animator anim;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int layerIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float oneHandMeleeTargetWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float oneHandPistolTargetWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int maxLayers;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float layerWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float turnRate = 200f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int totalModels;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int currentModel = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int weaponPrefabIndex;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int weaponJointChildrenCount;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currOneHandMeleeWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float currOneHandPistolWeight;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCrouching;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int weaponPrefabCount;

	public GameObject[] weaponPrefabs;

	public int[] weaponHoldTypes;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float[] locomotionSpeeds = new float[3] { 0f, 2.08f, 4.2f };

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int locomotionState;

	public Transform weaponJoint;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject existingChild;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject newWeapon;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mousePosXRatio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mousePosYRatio;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mousePosX;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float mousePosY;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float forward;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float forwardGoal;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float strafe;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float YLook;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float horizontalMax = 4.2f;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float verticalMax = 4.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		anim = GetComponent<Animator>();
		weaponPrefabCount = weaponPrefabs.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.LeftControl))
		{
			if (isCrouching)
			{
				isCrouching = false;
			}
			else
			{
				isCrouching = true;
			}
			anim.SetBool("IsCrouching", isCrouching);
		}
		if (Input.GetKeyUp(KeyCode.Space))
		{
			locomotionState++;
			if (locomotionState >= locomotionSpeeds.Length)
			{
				locomotionState = 0;
			}
			forwardGoal = locomotionSpeeds[locomotionState];
		}
		if (Input.GetKey(KeyCode.A))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime * -1f, 0f);
		}
		if (Input.GetKey(KeyCode.D))
		{
			base.transform.Rotate(0f, turnRate * Time.deltaTime, 0f);
		}
		if (Input.GetKeyUp(KeyCode.W))
		{
			weaponPrefabIndex++;
			anim.SetTrigger("ItemHasChangedTrigger");
			if (weaponPrefabIndex >= weaponPrefabCount)
			{
				weaponPrefabIndex = 0;
			}
			attachWeapon(weaponPrefabIndex);
		}
		if (Input.GetKeyUp(KeyCode.S))
		{
			weaponPrefabIndex--;
			anim.SetTrigger("ItemHasChangedTrigger");
			if (weaponPrefabIndex < 0)
			{
				weaponPrefabIndex = weaponPrefabCount;
			}
			attachWeapon(weaponPrefabIndex);
		}
		if (Input.GetKeyUp(KeyCode.R))
		{
			anim.SetTrigger("Reload");
		}
		if (Input.GetMouseButtonDown(0))
		{
			anim.SetTrigger("WeaponFire");
		}
		if (Input.GetMouseButtonUp(0))
		{
			anim.ResetTrigger("WeaponFire");
		}
		if (Input.GetMouseButtonDown(1))
		{
			anim.SetTrigger("IsAiming");
		}
		if (Input.GetMouseButtonUp(1))
		{
			anim.ResetTrigger("IsAiming");
		}
		if (Input.GetKeyUp(KeyCode.Q))
		{
			anim.SetTrigger("PowerAttack");
		}
		if (Input.GetKeyUp(KeyCode.E))
		{
			anim.SetTrigger("UseItem");
		}
		updateYLook();
		forward = Mathf.Lerp(forward, forwardGoal, 0.01f);
		anim.SetFloat("Forward", forward);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void attachWeapon(int weaponPrefabIndex)
	{
		removeAllWeapons();
		if (weaponPrefabs[weaponPrefabIndex] != null)
		{
			newWeapon = UnityEngine.Object.Instantiate(weaponPrefabs[weaponPrefabIndex]);
			newWeapon.transform.parent = weaponJoint.transform;
			newWeapon.transform.localPosition = Vector3.zero;
			newWeapon.transform.localEulerAngles = Vector3.zero;
		}
		Debug.Log(weaponPrefabIndex);
		anim.SetInteger("WeaponHoldType", weaponHoldTypes[weaponPrefabIndex]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeAllWeapons()
	{
		weaponJointChildrenCount = weaponJoint.childCount;
		if (weaponJointChildrenCount > 0)
		{
			for (int i = 0; i < weaponJointChildrenCount; i++)
			{
				existingChild = weaponJoint.GetChild(i).gameObject;
				UnityEngine.Object.Destroy(existingChild);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateYLook()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosXRatio = mousePosition.x / (float)Screen.width;
		mousePosYRatio = mousePosition.y / (float)Screen.height;
		mousePosX = (mousePosXRatio - 0.5f) * 2f;
		mousePosY = (mousePosYRatio - 0.5f) * -2f;
		anim.SetFloat("YLook", mousePosY);
	}
}
