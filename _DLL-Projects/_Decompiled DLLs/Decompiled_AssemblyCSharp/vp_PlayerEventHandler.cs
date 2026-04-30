using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_PlayerEventHandler : vp_StateEventHandler
{
	public vp_Value<bool> IsFirstPerson;

	public vp_Value<bool> IsLocal;

	public vp_Value<bool> IsAI;

	public vp_Value<float> Health;

	public vp_Value<float> MaxHealth;

	public vp_Value<Vector3> Position;

	public vp_Value<Vector2> Rotation;

	public vp_Value<float> BodyYaw;

	public vp_Value<Vector3> LookPoint;

	public vp_Value<Vector3> HeadLookDirection;

	public vp_Value<Vector3> AimDirection;

	public vp_Value<Vector3> MotorThrottle;

	public vp_Value<bool> MotorJumpDone;

	public vp_Value<Vector2> InputMoveVector;

	public vp_Value<float> InputClimbVector;

	public vp_Activity Dead;

	public vp_Activity Run;

	public vp_Activity Walk;

	public vp_Activity Jump;

	public vp_Activity Crouch;

	public vp_Activity CrouchWalk;

	public vp_Activity CrouchRun;

	public vp_Activity Zoom;

	public vp_Activity Attack;

	public vp_Activity Reload;

	public vp_Activity Climb;

	public vp_Activity Interact;

	public vp_Activity<int> SetWeapon;

	public vp_Activity OutOfControl;

	public vp_Activity Driving;

	public vp_Value<bool> CameraRelativeMovement3P;

	public vp_Value<Transform> CameraTransform;

	public vp_Message<int> Wield;

	public vp_Message Unwield;

	public vp_Attempt Fire;

	public vp_Message DryFire;

	public vp_Attempt SetPrevWeapon;

	public vp_Attempt SetNextWeapon;

	public vp_Attempt<string> SetWeaponByName;

	[Obsolete("Please use the 'CurrentWeaponIndex' vp_Value instead.")]
	public vp_Value<int> CurrentWeaponID;

	public vp_Value<int> CurrentWeaponIndex;

	public vp_Value<string> CurrentWeaponName;

	public vp_Value<bool> CurrentWeaponWielded;

	public vp_Attempt AutoReload;

	public vp_Value<float> CurrentWeaponReloadDuration;

	public vp_Message<string, int> GetItemCount;

	public vp_Attempt RefillCurrentWeapon;

	public vp_Value<int> CurrentWeaponAmmoCount;

	public vp_Value<int> CurrentWeaponMaxAmmoCount;

	public vp_Value<int> CurrentWeaponClipCount;

	public vp_Value<int> CurrentWeaponType;

	public vp_Value<int> CurrentWeaponGrip;

	public vp_Attempt<object> AddItem;

	public vp_Attempt<object> RemoveItem;

	public vp_Attempt DepleteAmmo;

	public vp_Message<Vector3> Move;

	public vp_Value<Vector3> Velocity;

	public vp_Value<float> SlopeLimit;

	public vp_Value<float> StepOffset;

	public vp_Value<float> Radius;

	public vp_Value<float> Height;

	public vp_Value<float> FallSpeed;

	public vp_Message<float> FallImpact;

	public vp_Message<float> FallImpact2;

	public vp_Message<float> HeadImpact;

	public vp_Message<Vector3> ForceImpact;

	public vp_Message Stop;

	public vp_Value<Transform> Platform;

	public vp_Value<Texture> GroundTexture;

	public vp_Value<vp_SurfaceIdentifier> SurfaceType;

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		BindStateToActivity(Run);
		BindStateToActivity(Walk);
		BindStateToActivity(Jump);
		BindStateToActivity(Crouch);
		BindStateToActivity(CrouchWalk);
		BindStateToActivity(CrouchRun);
		BindStateToActivity(Zoom);
		BindStateToActivity(Reload);
		BindStateToActivity(Dead);
		BindStateToActivity(Climb);
		BindStateToActivity(OutOfControl);
		BindStateToActivity(Driving);
		BindStateToActivityOnStart(Attack);
		SetWeapon.AutoDuration = 1f;
		Reload.AutoDuration = 1f;
		Zoom.MinDuration = 0.2f;
		Crouch.MinDuration = 0.1f;
		Jump.MinPause = 0f;
		SetWeapon.MinPause = 0.2f;
	}
}
