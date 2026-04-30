using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerInputRecordingSystem
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct SPosRot
	{
		public Vector3 pos;

		public Vector3 rot;

		public int ticks;

		public void Write(BinaryWriter _bw)
		{
			_bw.Write(ticks);
			StreamUtils.Write(_bw, pos);
			StreamUtils.Write(_bw, rot);
		}

		public void Read(BinaryReader _br)
		{
			ticks = _br.ReadInt32();
			pos = StreamUtils.ReadVector3(_br);
			rot = StreamUtils.ReadVector3(_br);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int CurrentSaveVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInputRecordingSystem mInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SPosRot> recording = new List<SPosRot>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong startTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong relativeStartTickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startFrameCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float startTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public string autoSaveFilename;

	public static PlayerInputRecordingSystem Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new PlayerInputRecordingSystem();
			}
			return mInstance;
		}
	}

	public void Record(MovementInput _movement, int _frameNr)
	{
	}

	public void Record(EntityPlayer _player, ulong _ticks)
	{
		if (startTickTime == 0L)
		{
			startTickTime = GameTimer.Instance.ticks;
		}
		recording.Add(new SPosRot
		{
			pos = _player.position,
			rot = _player.rotation,
			ticks = (int)(_ticks - startTickTime)
		});
	}

	public void Reset(bool _bClearRecordings = false)
	{
		index = 0;
		if (_bClearRecordings)
		{
			recording.Clear();
			startTickTime = 0uL;
		}
		relativeStartTickTime = 0uL;
		autoSaveFilename = null;
	}

	public bool Play(EntityPlayer _player, bool _bPlayRelativeToNow = false)
	{
		if (relativeStartTickTime == 0L)
		{
			if (_bPlayRelativeToNow)
			{
				relativeStartTickTime = GameTimer.Instance.ticks;
			}
			else
			{
				relativeStartTickTime = startTickTime;
			}
			startFrameCount = Time.frameCount;
			startTime = Time.time;
		}
		while (index < recording.Count)
		{
			int ticks = recording[index].ticks;
			if (GameTimer.Instance.ticks < (ulong)((long)relativeStartTickTime + (long)ticks))
			{
				break;
			}
			_player.SetPosition(recording[index].pos);
			_player.SetRotation(recording[index].rot);
			index++;
		}
		if (index == recording.Count)
		{
			Log.Out("Playing ended. Frames=" + (Time.frameCount - startFrameCount) + " avg fps=" + ((float)(Time.frameCount - startFrameCount) / (Time.time - startTime)).ToString("0.0"));
			GameManager.Instance.SetConsoleWindowVisible(_b: true);
			index++;
		}
		return index < recording.Count;
	}

	public void SetStartPosition(EntityPlayer _player)
	{
		if (recording.Count > 0)
		{
			_player.SetPosition(recording[0].pos);
			_player.SetRotation(recording[0].rot);
		}
	}

	public void Load(string _filename)
	{
		using BinaryReader binaryReader = new BinaryReader(SdFile.OpenRead(GameIO.GetSaveGameDir() + "/" + _filename + ".rec"));
		recording.Clear();
		binaryReader.ReadByte();
		startTickTime = binaryReader.ReadUInt64();
		int num = (int)binaryReader.ReadUInt32();
		for (int i = 0; i < num; i++)
		{
			SPosRot item = default(SPosRot);
			item.Read(binaryReader);
			recording.Add(item);
		}
	}

	public void SetAutoSaveTo(string _filename)
	{
		autoSaveFilename = _filename;
	}

	public bool AutoSave()
	{
		if (autoSaveFilename != null)
		{
			doSave(autoSaveFilename);
			autoSaveFilename = null;
			return true;
		}
		return false;
	}

	public void Save(string _filename)
	{
		doSave(GameIO.GetSaveGameDir() + "/" + _filename);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doSave(string _filename)
	{
		using BinaryWriter binaryWriter = new BinaryWriter(SdFile.Open(_filename + ".rec", FileMode.Create, FileAccess.Write, FileShare.Read));
		binaryWriter.Write((byte)1);
		binaryWriter.Write(startTickTime);
		binaryWriter.Write((uint)recording.Count);
		for (int i = 0; i < recording.Count; i++)
		{
			recording[i].Write(binaryWriter);
		}
	}
}
