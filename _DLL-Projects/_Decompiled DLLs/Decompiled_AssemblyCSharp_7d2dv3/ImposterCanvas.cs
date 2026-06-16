using System.IO;
using UnityEngine;

public class ImposterCanvas
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort Version = 2;

	public Vector3 WorldPosition;

	public Quaternion WorldRotation;

	public Vector2 Size;

	public float CanvasAspect;

	public bool IsDecal;

	public SignCanvas Canvas;

	[PublicizedFrom(EAccessModifier.Private)]
	public SignCanvas.CanvasState pendingCanvasState = new SignCanvas.CanvasState();

	public SignCanvas.CanvasState State
	{
		get
		{
			if (!(Canvas != null))
			{
				return pendingCanvasState;
			}
			return Canvas.State;
		}
		set
		{
			if (Canvas != null)
			{
				Canvas.State = value;
			}
			else
			{
				pendingCanvasState = value;
			}
		}
	}

	public ImposterCanvas Clone()
	{
		return new ImposterCanvas
		{
			WorldPosition = WorldPosition,
			WorldRotation = WorldRotation,
			Size = Size,
			CanvasAspect = CanvasAspect,
			IsDecal = IsDecal,
			pendingCanvasState = State?.Clone()
		};
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((ushort)2);
		_bw.Write(WorldPosition.x);
		_bw.Write(WorldPosition.y);
		_bw.Write(WorldPosition.z);
		_bw.Write(WorldRotation.x);
		_bw.Write(WorldRotation.y);
		_bw.Write(WorldRotation.z);
		_bw.Write(WorldRotation.w);
		_bw.Write(Size.x);
		_bw.Write(Size.y);
		_bw.Write(CanvasAspect);
		_bw.Write(IsDecal);
		State.Write(_bw);
	}

	public static ImposterCanvas Read(BinaryReader _br)
	{
		ImposterCanvas imposterCanvas = new ImposterCanvas();
		ushort num = _br.ReadUInt16();
		imposterCanvas.WorldPosition.x = _br.ReadSingle();
		imposterCanvas.WorldPosition.y = _br.ReadSingle();
		imposterCanvas.WorldPosition.z = _br.ReadSingle();
		imposterCanvas.WorldRotation.x = _br.ReadSingle();
		imposterCanvas.WorldRotation.y = _br.ReadSingle();
		imposterCanvas.WorldRotation.z = _br.ReadSingle();
		imposterCanvas.WorldRotation.w = _br.ReadSingle();
		imposterCanvas.Size.x = _br.ReadSingle();
		imposterCanvas.Size.y = _br.ReadSingle();
		imposterCanvas.CanvasAspect = _br.ReadSingle();
		if (num >= 2)
		{
			imposterCanvas.IsDecal = _br.ReadBoolean();
		}
		imposterCanvas.pendingCanvasState = SignCanvas.CanvasState.Read(_br);
		return imposterCanvas;
	}
}
