using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class SignCanvas : MonoBehaviour, ISignRenderingDataUpdateListener
{
	public enum SignBlendMode
	{
		Cutout,
		AlphaBlend
	}

	public class CanvasState
	{
		public GlobalSignId SignId = SignData.defaultSignDataID;

		public SignBlendMode BlendMode;

		public bool ShowOnImposter;

		public CanvasRotationMode CanvasRotation;

		public CanvasState Clone()
		{
			return new CanvasState
			{
				SignId = SignId,
				BlendMode = BlendMode,
				ShowOnImposter = ShowOnImposter,
				CanvasRotation = CanvasRotation
			};
		}

		public void Write(BinaryWriter _bw)
		{
			SignId.ToStream(_bw);
			_bw.Write((byte)BlendMode);
			_bw.Write((byte)CanvasRotation);
			_bw.Write(ShowOnImposter);
		}

		public static CanvasState Read(BinaryReader _br)
		{
			return new CanvasState
			{
				SignId = GlobalSignId.FromStream(_br),
				BlendMode = (SignBlendMode)_br.ReadByte(),
				CanvasRotation = (CanvasRotationMode)_br.ReadByte(),
				ShowOnImposter = _br.ReadBoolean()
			};
		}
	}

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public CanvasState _state = new CanvasState();

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool allowedOnImposter = true;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 canvasDimensions = Vector2.one;

	public List<SignRenderer> SignRenderers = new List<SignRenderer>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Texture bakedTexture;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public SignDataManager.RenderingDataPatcher _patchRenderingData;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _signDataAvailabilityDirty = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool _signDataAvailable = true;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 _localCenter = Vector3.zero;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public float _sizeSquared = 1f;

	public CanvasState State
	{
		get
		{
			return _state;
		}
		set
		{
			if (value != null)
			{
				_state = value;
				_signDataAvailabilityDirty = true;
			}
		}
	}

	public bool AllowedOnImposter => allowedOnImposter;

	public float CanvasAspect
	{
		get
		{
			if (canvasDimensions.y == 0f)
			{
				return 1f;
			}
			return canvasDimensions.x / canvasDimensions.y;
		}
		set
		{
			canvasDimensions = new Vector2(value, 1f);
		}
	}

	public Texture BakedTexture => bakedTexture;

	public float SizeSquared => _sizeSquared;

	public GlobalSignId SignId
	{
		get
		{
			return State.SignId;
		}
		set
		{
			if (!State.SignId.Equals(value))
			{
				State.SignId = value;
				_signDataAvailabilityDirty = true;
				SignDataManager.Instance.RegisterListener(DisplaySignId, this);
				SignTextureManager.Instance.Invalidate(this);
				RefreshRenderers();
				this.RenderingDataUpdated?.Invoke();
			}
		}
	}

	public GlobalSignId DisplaySignId
	{
		get
		{
			if (_signDataAvailabilityDirty)
			{
				_signDataAvailable = SignDataManager.Instance.TryGetSignData(State.SignId, out var _);
				_signDataAvailabilityDirty = false;
			}
			if (!_signDataAvailable)
			{
				return SignDataManager.ErrorSignId;
			}
			return State.SignId;
		}
	}

	public SignBlendMode BlendMode
	{
		get
		{
			return State.BlendMode;
		}
		set
		{
			if (State.BlendMode != value)
			{
				State.BlendMode = value;
				RefreshRenderers();
			}
		}
	}

	public bool ShowOnImposter
	{
		get
		{
			return State.ShowOnImposter;
		}
		set
		{
			State.ShowOnImposter = value;
		}
	}

	public CanvasRotationMode CanvasRotation
	{
		get
		{
			return State.CanvasRotation;
		}
		set
		{
			if (State.CanvasRotation != value)
			{
				State.CanvasRotation = value;
				RefreshRenderers();
			}
		}
	}

	public float CanvasRotationRadians => (float)(int)CanvasRotation * MathF.PI * 0.5f;

	public bool IsDecal
	{
		get
		{
			foreach (SignRenderer signRenderer in SignRenderers)
			{
				if (signRenderer.IsDecal)
				{
					return true;
				}
			}
			return false;
		}
	}

	public event Action RenderingDataUpdated;

	public void Initialize(SignDataManager.RenderingDataPatcher _alternativePatcher = null)
	{
		_patchRenderingData = _alternativePatcher ?? new SignDataManager.RenderingDataPatcher(PatchRenderingData);
		GetComponentsInChildren(includeInactive: true, SignRenderers);
		CacheBoundsMetrics();
		Register();
		RefreshRenderers();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CacheBoundsMetrics()
	{
		if (SignRenderers.Count == 0)
		{
			_localCenter = Vector3.zero;
			_sizeSquared = 1f;
			return;
		}
		Bounds bounds = SignRenderers[0].Renderer.bounds;
		for (int i = 1; i < SignRenderers.Count; i++)
		{
			bounds.Encapsulate(SignRenderers[i].Renderer.bounds);
		}
		_localCenter = base.transform.InverseTransformPoint(bounds.center);
		Vector3 size = bounds.size;
		float num = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
		_sizeSquared = ((num > 0f) ? (num * num) : 1f);
	}

	public void SetTexture(Texture rt)
	{
		if (!(bakedTexture == rt))
		{
			bakedTexture = rt;
			RefreshRenderers();
		}
	}

	public Vector3 GetWorldPosition()
	{
		return base.transform.TransformPoint(_localCenter) + Origin.position;
	}

	public void RefreshRenderers()
	{
		if (SignRenderers != null && SignRenderers.Count != 0)
		{
			SignDataManager.Instance.TryApplyRenderingData(DisplaySignId, _patchRenderingData, SignRenderers, State.BlendMode);
		}
	}

	public void PatchRenderingData(MaterialPropertyBlock mpb)
	{
		if (bakedTexture != null)
		{
			mpb.SetInteger(SignShaderIDs._UseTexture, 1);
			mpb.SetTexture(SignShaderIDs._BakedTexture, bakedTexture);
		}
		else
		{
			mpb.SetInteger(SignShaderIDs._UseTexture, 0);
			mpb.SetTexture(SignShaderIDs._BakedTexture, Texture2D.whiteTexture);
		}
		mpb.SetFloat(SignShaderIDs._CanvasRotation, CanvasRotationRadians);
	}

	public void HandleRenderingDataUpdate()
	{
		RefreshRenderers();
		SignTextureManager.Instance.Invalidate(this);
		this.RenderingDataUpdated?.Invoke();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Register()
	{
		SignTextureManager.Instance.Register(this);
		SignDataManager.Instance.RegisterListener(DisplaySignId, this);
	}

	public void Cleanup()
	{
		SignDataManager.Instance.DeregisterListener(this);
		SignTextureManager.Instance.Deregister(this);
		bakedTexture = null;
	}

	[Conditional("DEBUG_LOG_SIGN_CANVAS")]
	public static void DebugLog(string message)
	{
		Log.Out(message);
	}
}
