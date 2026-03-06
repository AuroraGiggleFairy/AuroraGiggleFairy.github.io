using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_FPPlayerEventHandler : vp_PlayerEventHandler
{
	public vp_Message<vp_DamageInfo> HUDDamageFlash;

	public vp_Message<string> HUDText;

	public vp_Value<Texture> Crosshair;

	public vp_Value<Texture2D> CurrentAmmoIcon;

	public vp_Value<Vector2> InputSmoothLook;

	public vp_Value<Vector2> InputRawLook;

	public vp_Message<string, bool> InputGetButton;

	public vp_Message<string, bool> InputGetButtonUp;

	public vp_Message<string, bool> InputGetButtonDown;

	public vp_Value<bool> InputAllowGameplay;

	public vp_Value<bool> Pause;

	public vp_Value<Vector3> CameraLookDirection;

	public vp_Message CameraToggle3rdPerson;

	public vp_Message<float> CameraGroundStomp;

	public vp_Message<float> CameraBombShake;

	public vp_Value<Vector3> CameraEarthQuakeForce;

	public vp_Activity<Vector3> CameraEarthQuake;

	public vp_Value<vp_Interactable> Interactable;

	public vp_Value<bool> CanInteract;

	public vp_Value<string> CurrentWeaponClipType;

	public vp_Attempt<object> AddAmmo;

	public vp_Attempt RemoveClip;
}
