using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class MinScript
{
	[PublicizedFrom(EAccessModifier.Private)]
	public struct CmdLine
	{
		public ushort command;

		public string parameters;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte cVersion = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char cLineSepChar = '^';

	[PublicizedFrom(EAccessModifier.Private)]
	public const string cLineSepStr = "^";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<CmdLine> commandList = new List<CmdLine>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int curIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public float sleep;

	[PublicizedFrom(EAccessModifier.Private)]
	public int loopCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int loopToIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdNop = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLog = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLabel = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdLoop = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSleep = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSound = 40;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdSpawn = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdWaitAlive = 51;

	[PublicizedFrom(EAccessModifier.Private)]
	public const ushort cCmdTrigger = 52;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, ushort> nameToCmds = new Dictionary<string, ushort>
	{
		{ "log", 1 },
		{ "label", 2 },
		{ "loop", 3 },
		{ "sleep", 4 },
		{ "sound", 40 },
		{ "spawn", 50 },
		{ "trigger", 52 },
		{ "waitalive", 51 }
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] tempBytes = new byte[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public static char[] tempChars = new char[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer player;

	[PublicizedFrom(EAccessModifier.Private)]
	public float countScale = 1f;

	public static string ConvertFromUIText(string _text)
	{
		return _text.Replace("\n", "^");
	}

	public static string ConvertToUIText(string _text)
	{
		if (_text == null)
		{
			return string.Empty;
		}
		return _text.Replace("^", "\n");
	}

	public void SetText(string _text)
	{
		int num = 0;
		int length = _text.Length;
		int i = 0;
		CmdLine item = default(CmdLine);
		while (i < length)
		{
			int num2 = _text.IndexOf('^', i, length - i);
			if (num2 < 0)
			{
				num2 = length;
			}
			for (; i < length && _text[i] == ' '; i++)
			{
			}
			int num3 = num2 - i;
			if (num3 > 0 && _text[i] != '/')
			{
				int num4 = _text.IndexOf(' ', i, num3);
				if (num4 < 0)
				{
					num4 = num2;
				}
				string key = _text.Substring(i, num4 - i);
				if (nameToCmds.TryGetValue(key, out item.command))
				{
					num4++;
					item.parameters = null;
					int num5 = num2 - num4;
					if (num5 > 0)
					{
						item.parameters = _text.Substring(num4, num5);
					}
					commandList.Add(item);
				}
			}
			num++;
			i = num2 + 1;
		}
	}

	public void Reset()
	{
		curIndex = -1;
	}

	public void Restart()
	{
		curIndex = 0;
		sleep = 0f;
	}

	public void Run(SleeperVolume _sv, EntityPlayer _player, float _countScale)
	{
		if (commandList != null)
		{
			player = _player;
			countScale = _countScale;
			curIndex = 0;
			sleep = 0f;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsRunning()
	{
		return curIndex >= 0;
	}

	public void Tick(SleeperVolume _sv)
	{
		if (curIndex < 0)
		{
			return;
		}
		if (sleep > 0f)
		{
			sleep -= 0.05f;
			if (sleep > 0f)
			{
				return;
			}
		}
		do
		{
			CmdLine cmdLine = commandList[curIndex];
			switch (cmdLine.command)
			{
			case 1:
				Log.Out("MinScript " + cmdLine.parameters);
				break;
			case 4:
				sleep = float.Parse(cmdLine.parameters ?? "1");
				break;
			case 51:
			{
				int num = int.Parse(cmdLine.parameters ?? "0");
				if (_sv.GetAliveCount() > num)
				{
					return;
				}
				break;
			}
			case 3:
				if (loopCount <= 0)
				{
					string[] array = cmdLine.parameters.Split(' ');
					if (array.Length == 2)
					{
						loopToIndex = FindLabel(array[0]);
						if (loopToIndex < 0)
						{
							Log.Warning("MinScript loop label {0} missing: {1}", array[0], _sv);
						}
						else
						{
							loopCount = int.Parse(array[1]);
						}
					}
					else
					{
						Log.Warning("MinScript loop needs 2 params: {0}", _sv);
					}
				}
				if (--loopCount > 0)
				{
					curIndex = loopToIndex;
				}
				break;
			case 50:
			{
				if (cmdLine.parameters == null)
				{
					break;
				}
				string[] array2 = cmdLine.parameters.Split(' ');
				if (array2.Length < 1)
				{
					break;
				}
				float num2 = 1f;
				float num3 = 1f;
				if (array2.Length >= 2)
				{
					num2 = float.Parse(array2[1]);
					num3 = num2;
					if (array2.Length >= 3)
					{
						num3 = float.Parse(array2[2]);
					}
				}
				_sv.AddSpawnCount(array2[0], num2 * countScale, num3 * countScale);
				break;
			}
			case 40:
				GameManager.Instance.PlaySoundAtPositionServer(_sv.Center, cmdLine.parameters, AudioRolloffMode.Linear, 100, _sv.GetPlayerTouchedToUpdateId());
				break;
			case 52:
				if ((bool)player)
				{
					if (cmdLine.parameters != null)
					{
						byte trigger = (byte)int.Parse(cmdLine.parameters);
						player.world.triggerManager.Trigger(player, _sv.PrefabInstance, trigger);
					}
					else
					{
						Log.Warning("MinScript trigger !param {0}", _sv);
					}
				}
				break;
			}
			if (++curIndex >= commandList.Count)
			{
				curIndex = -1;
				break;
			}
		}
		while (sleep <= 0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindLabel(string _name)
	{
		int count = commandList.Count;
		for (int i = 0; i < count; i++)
		{
			CmdLine cmdLine = commandList[i];
			if (cmdLine.command == 2 && cmdLine.parameters == _name)
			{
				return i;
			}
		}
		return -1;
	}

	public static MinScript Read(BinaryReader _br)
	{
		_br.ReadByte();
		MinScript minScript = new MinScript();
		minScript.curIndex = _br.ReadInt16();
		if (minScript.curIndex >= 0)
		{
			minScript.sleep = _br.ReadSingle();
		}
		int num = _br.ReadUInt16();
		CmdLine item = default(CmdLine);
		for (int i = 0; i < num; i++)
		{
			item.command = _br.ReadUInt16();
			item.parameters = null;
			int num2 = _br.ReadByte();
			if (num2 > 0)
			{
				_br.Read(tempBytes, 0, num2);
				int chars = Encoding.UTF8.GetChars(tempBytes, 0, num2, tempChars, 0);
				item.parameters = new string(tempChars, 0, chars);
			}
			minScript.commandList.Add(item);
		}
		return minScript;
	}

	public bool HasData()
	{
		return commandList.Count > 0;
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((byte)1);
		_bw.Write((short)curIndex);
		if (curIndex >= 0)
		{
			_bw.Write(sleep);
		}
		int num = commandList.Count;
		if (num >= 1000)
		{
			num = 1;
			Log.Error("MinScript Write error: {0}", this);
		}
		_bw.Write((ushort)num);
		for (int i = 0; i < num; i++)
		{
			CmdLine cmdLine = commandList[i];
			_bw.Write(cmdLine.command);
			if (cmdLine.parameters != null && cmdLine.parameters.Length > 0)
			{
				for (int j = 0; j < cmdLine.parameters.Length; j++)
				{
					tempChars[j] = cmdLine.parameters[j];
				}
				byte b = (byte)Encoding.UTF8.GetBytes(tempChars, 0, cmdLine.parameters.Length, tempBytes, 0);
				_bw.Write(b);
				_bw.Write(tempBytes, 0, b);
			}
			else
			{
				_bw.Write((byte)0);
			}
		}
	}

	public override string ToString()
	{
		return $"cmds {commandList.Count}, index {curIndex}, sleep {sleep}";
	}

	[Conditional("DEBUG_MINSCRIPTLOG")]
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogMS(string format, params object[] args)
	{
		format = $"{GameManager.frameTime.ToCultureInvariantString()} {GameManager.frameCount} MinScript {format}";
		Log.Out(format, args);
	}
}
