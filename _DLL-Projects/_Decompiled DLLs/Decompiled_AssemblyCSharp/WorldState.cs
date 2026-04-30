using System;
using System.IO;

public class WorldState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static int CurrentSaveVersion = 22;

	[PublicizedFrom(EAccessModifier.Private)]
	public uint version;

	public string gameVersionString = "";

	public VersionInformation gameVersion;

	[PublicizedFrom(EAccessModifier.Private)]
	public float waterLevel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeX;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeY;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkSizeZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkCount;

	public MemoryStream dynamicSpawnerState;

	public MemoryStream aiDirectorState;

	public int activeGameMode;

	public EnumChunkProviderId providerId;

	public int seed;

	public ulong worldTime;

	public ulong timeInTicks;

	public int nextEntityID;

	public long saveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPointList playerSpawnPoints;

	public MemoryStream sleeperVolumeState;

	public MemoryStream triggerVolumeState;

	public MemoryStream wallVolumeState;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public string Guid
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public WorldState()
	{
		providerId = EnumChunkProviderId.Disc;
		saveDataLimit = -1L;
		playerSpawnPoints = new SpawnPointList();
		GenerateNewGuid();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SaveLoad(string _filename, bool _load, bool _warnOnDifferentVersion, bool _infOnDiferentVersion)
	{
		lock (this)
		{
			Stream stream = null;
			try
			{
				if (_load)
				{
					try
					{
						stream = SdFile.OpenRead(_filename);
					}
					catch (Exception arg)
					{
						Log.Error($"Opening saved game: {arg}");
					}
				}
				else
				{
					try
					{
						stream = new BufferedStream(SdFile.Open(_filename, FileMode.Create, FileAccess.Write, FileShare.Read));
					}
					catch (Exception arg2)
					{
						Log.Error($"Opening buffer to save game: {arg2}");
					}
				}
				return stream != null && SaveLoad(stream, _load, _warnOnDifferentVersion, _infOnDiferentVersion);
			}
			catch (Exception arg3)
			{
				Log.Error($"Exception reading world header at pos {stream?.Position ?? 0}: {arg3}");
				return false;
			}
			finally
			{
				stream?.Dispose();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool SaveLoad(Stream _stream, bool _load, bool _warnOnDifferentVersion, bool _infOnDiferentVersion)
	{
		lock (this)
		{
			PooledBinaryWriter pooledBinaryWriter = null;
			PooledBinaryReader pooledBinaryReader = null;
			try
			{
				IBinaryReaderOrWriter binaryReaderOrWriter;
				if (_load)
				{
					chunkSizeX = (chunkSizeY = (chunkSizeZ = (chunkCount = 0)));
					pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
					pooledBinaryReader.SetBaseStream(_stream);
					binaryReaderOrWriter = pooledBinaryReader;
					char num = binaryReaderOrWriter.ReadWrite(' ');
					char c = binaryReaderOrWriter.ReadWrite(' ');
					char c2 = binaryReaderOrWriter.ReadWrite(' ');
					byte b = binaryReaderOrWriter.ReadWrite((byte)1);
					if (num != 't' || c != 't' || c2 != 'w' || b != 0)
					{
						Log.Error("Invalid magic bytes in world header");
						return false;
					}
				}
				else
				{
					pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
					pooledBinaryWriter.SetBaseStream(_stream);
					binaryReaderOrWriter = pooledBinaryWriter;
					binaryReaderOrWriter.ReadWrite('t');
					binaryReaderOrWriter.ReadWrite('t');
					binaryReaderOrWriter.ReadWrite('w');
					binaryReaderOrWriter.ReadWrite((byte)0);
				}
				version = binaryReaderOrWriter.ReadWrite((uint)CurrentSaveVersion);
				if (_load)
				{
					if (version > CurrentSaveVersion)
					{
						return true;
					}
					if (version > 11)
					{
						gameVersionString = binaryReaderOrWriter.ReadWrite("");
						if (gameVersionString != Constants.cVersionInformation.LongString)
						{
							if (_warnOnDifferentVersion)
							{
								Log.Warning("Loaded world file from different version: '{0}'", gameVersionString);
							}
							else if (_infOnDiferentVersion)
							{
								Log.Out("Loaded world file from different version: '{0}'", gameVersionString);
							}
						}
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite(Constants.cVersionInformation.LongString);
				}
				if (_load)
				{
					if (version > 14)
					{
						VersionInformation.EGameReleaseType releaseType = (VersionInformation.EGameReleaseType)binaryReaderOrWriter.ReadWrite(1);
						int major = binaryReaderOrWriter.ReadWrite(2);
						int minor = binaryReaderOrWriter.ReadWrite(5);
						int build = binaryReaderOrWriter.ReadWrite(32);
						gameVersion = new VersionInformation(releaseType, major, minor, build);
					}
					else
					{
						VersionInformation.TryParseLegacyString(gameVersionString, out gameVersion);
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite(1);
					binaryReaderOrWriter.ReadWrite(2);
					binaryReaderOrWriter.ReadWrite(5);
					binaryReaderOrWriter.ReadWrite(32);
				}
				binaryReaderOrWriter.ReadWrite(0u);
				if (version > 6)
				{
					activeGameMode = binaryReaderOrWriter.ReadWrite(activeGameMode);
				}
				binaryReaderOrWriter.ReadWrite(0u);
				waterLevel = binaryReaderOrWriter.ReadWrite(waterLevel);
				chunkSizeX = binaryReaderOrWriter.ReadWrite(chunkSizeX);
				chunkSizeZ = binaryReaderOrWriter.ReadWrite(chunkSizeY);
				chunkSizeY = binaryReaderOrWriter.ReadWrite(chunkSizeZ);
				chunkCount = binaryReaderOrWriter.ReadWrite(chunkCount);
				providerId = (EnumChunkProviderId)binaryReaderOrWriter.ReadWrite((int)providerId);
				seed = binaryReaderOrWriter.ReadWrite(seed);
				worldTime = binaryReaderOrWriter.ReadWrite(worldTime);
				if (version > 8)
				{
					timeInTicks = binaryReaderOrWriter.ReadWrite(timeInTicks);
				}
				if (_load)
				{
					if (version == 10)
					{
						binaryReaderOrWriter.ReadWrite(0uL);
					}
					if (version > 1 && version < 7)
					{
						binaryReaderOrWriter.ReadWrite(_value: false);
					}
					if (version > 4 && version < 7)
					{
						binaryReaderOrWriter.ReadWrite(_value: false);
						binaryReaderOrWriter.ReadWrite(_value: false);
					}
					if (version > 5)
					{
						playerSpawnPoints.Read(binaryReaderOrWriter);
					}
					else if (version > 2)
					{
						playerSpawnPoints.Clear();
						int num2 = binaryReaderOrWriter.ReadWrite(0);
						for (int i = 0; i < num2; i++)
						{
							playerSpawnPoints.Add(new SpawnPoint(new Vector3i(binaryReaderOrWriter.ReadWrite(0), binaryReaderOrWriter.ReadWrite(0), binaryReaderOrWriter.ReadWrite(0))));
						}
					}
				}
				else
				{
					binaryReaderOrWriter.ReadWrite((byte)0);
					binaryReaderOrWriter.ReadWrite(0);
				}
				if (version > 3)
				{
					nextEntityID = binaryReaderOrWriter.ReadWrite(nextEntityID);
				}
				if (_load)
				{
					nextEntityID = Utils.FastMax(nextEntityID, 171);
				}
				if (version >= 21)
				{
					saveDataLimit = binaryReaderOrWriter.ReadWrite(saveDataLimit);
				}
				else
				{
					saveDataLimit = -1L;
				}
				if (version > 7)
				{
					int num3 = binaryReaderOrWriter.ReadWrite((int)((dynamicSpawnerState != null) ? dynamicSpawnerState.Length : 0));
					if (_load)
					{
						if (num3 > 0)
						{
							dynamicSpawnerState = new MemoryStream(num3);
							dynamicSpawnerState.SetLength(num3);
							binaryReaderOrWriter.ReadWrite(dynamicSpawnerState.GetBuffer(), 0, num3);
							dynamicSpawnerState.Position = 0L;
						}
					}
					else if (dynamicSpawnerState != null)
					{
						dynamicSpawnerState.Position = 0L;
						StreamUtils.StreamCopy(dynamicSpawnerState, binaryReaderOrWriter.BaseStream);
					}
				}
				if (version > 10)
				{
					int num4 = binaryReaderOrWriter.ReadWrite((int)((aiDirectorState != null) ? aiDirectorState.Length : 0));
					if (_load)
					{
						if (num4 > 0)
						{
							aiDirectorState = new MemoryStream(num4);
							aiDirectorState.SetLength(num4);
							binaryReaderOrWriter.ReadWrite(aiDirectorState.GetBuffer(), 0, num4);
							aiDirectorState.Position = 0L;
						}
					}
					else if (aiDirectorState != null)
					{
						aiDirectorState.Position = 0L;
						StreamUtils.StreamCopy(aiDirectorState, binaryReaderOrWriter.BaseStream);
					}
				}
				if (version > 12)
				{
					int num5 = binaryReaderOrWriter.ReadWrite((int)((sleeperVolumeState != null) ? sleeperVolumeState.Length : 0));
					if (_load)
					{
						if (num5 > 0)
						{
							sleeperVolumeState = new MemoryStream(num5);
							sleeperVolumeState.SetLength(num5);
							binaryReaderOrWriter.ReadWrite(sleeperVolumeState.GetBuffer(), 0, num5);
							sleeperVolumeState.Position = 0L;
						}
					}
					else if (sleeperVolumeState != null)
					{
						sleeperVolumeState.Position = 0L;
						StreamUtils.StreamCopy(sleeperVolumeState, binaryReaderOrWriter.BaseStream);
					}
				}
				if (version >= 19)
				{
					int num6 = binaryReaderOrWriter.ReadWrite((int)((triggerVolumeState != null) ? triggerVolumeState.Length : 0));
					if (_load)
					{
						if (num6 > 0)
						{
							triggerVolumeState = new MemoryStream(num6);
							triggerVolumeState.SetLength(num6);
							binaryReaderOrWriter.ReadWrite(triggerVolumeState.GetBuffer(), 0, num6);
							triggerVolumeState.Position = 0L;
						}
					}
					else if (triggerVolumeState != null)
					{
						triggerVolumeState.Position = 0L;
						StreamUtils.StreamCopy(triggerVolumeState, binaryReaderOrWriter.BaseStream);
					}
				}
				if (version >= 20)
				{
					int num7 = binaryReaderOrWriter.ReadWrite((int)((wallVolumeState != null) ? wallVolumeState.Length : 0));
					if (_load)
					{
						if (num7 > 0)
						{
							wallVolumeState = new MemoryStream(num7);
							wallVolumeState.SetLength(num7);
							binaryReaderOrWriter.ReadWrite(wallVolumeState.GetBuffer(), 0, num7);
							wallVolumeState.Position = 0L;
						}
					}
					else if (wallVolumeState != null)
					{
						wallVolumeState.Position = 0L;
						StreamUtils.StreamCopy(wallVolumeState, binaryReaderOrWriter.BaseStream);
					}
				}
				bool flag = false;
				if (version > 11)
				{
					long position = binaryReaderOrWriter.BaseStream.Position;
					int num8 = 0;
					if (version > 15)
					{
						num8 = binaryReaderOrWriter.ReadWrite(0);
					}
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && version >= 22)
					{
						if (_load)
						{
							int num9 = num8 - 4;
							if (num9 > 0)
							{
								flag = true;
								WeatherManager.Load(binaryReaderOrWriter, num9);
							}
						}
						else
						{
							WeatherManager.Save(binaryReaderOrWriter);
						}
					}
					if (version > 15)
					{
						if (_load)
						{
							if (binaryReaderOrWriter.BaseStream.Position != position + num8)
							{
								if (flag)
								{
									Log.Out("Failed reading weather data from world header");
								}
								binaryReaderOrWriter.BaseStream.Position = position + num8;
							}
						}
						else
						{
							num8 = (int)(binaryReaderOrWriter.BaseStream.Position - position);
							binaryReaderOrWriter.BaseStream.Position = position;
							binaryReaderOrWriter.ReadWrite(num8);
							binaryReaderOrWriter.BaseStream.Seek(0L, SeekOrigin.End);
						}
					}
				}
				if (version > 13 && (flag || version > 15))
				{
					Guid = binaryReaderOrWriter.ReadWrite(Guid);
				}
				if (_load && string.IsNullOrEmpty(Guid))
				{
					GenerateNewGuid();
				}
				return true;
			}
			catch (Exception e)
			{
				Log.Error("Exception reading world header at pos {0}:", _stream.Position);
				Log.Exception(e);
				return false;
			}
			finally
			{
				if (pooledBinaryReader != null)
				{
					MemoryPools.poolBinaryReader.FreeSync(pooledBinaryReader);
				}
				if (pooledBinaryWriter != null)
				{
					MemoryPools.poolBinaryWriter.FreeSync(pooledBinaryWriter);
				}
			}
		}
	}

	public bool Load(string _filename, bool _warnOnDifferentVersion = true, bool _infOnDiferentVersion = false, bool _makeExtraBackupOnSuccess = false)
	{
		if (SaveLoad(_filename, _load: true, _warnOnDifferentVersion, _infOnDiferentVersion))
		{
			DoExtraBackup(_filename);
			return true;
		}
		Log.Warning("Failed loading world header file: " + _filename);
		SdFile.Copy(_filename, _filename + ".loadFailed", overwrite: true);
		string text = _filename + ".bak";
		if (SdFile.Exists(text))
		{
			Log.Out("Trying backup header: " + text);
			if (SaveLoad(text, _load: true, _warnOnDifferentVersion, _infOnDiferentVersion))
			{
				DoExtraBackup(text);
				return true;
			}
			SdFile.Copy(text, text + ".loadFailed", overwrite: true);
			Log.Error("Failed loading backup header file!");
		}
		else
		{
			Log.Out("No backup header!");
		}
		string text2 = _filename + ".ext.bak";
		if (SdFile.Exists(text2))
		{
			Log.Out("Trying extra backup header (from last successful load): " + text2);
			if (SaveLoad(text2, _load: true, _warnOnDifferentVersion, _infOnDiferentVersion))
			{
				return true;
			}
			SdFile.Copy(text2, text2 + ".loadFailed", overwrite: true);
			Log.Error("Failed loading extra backup header file!");
		}
		else
		{
			Log.Out("No extra backup header!");
		}
		return false;
		[PublicizedFrom(EAccessModifier.Internal)]
		void DoExtraBackup(string sourceFilename)
		{
			if (!_makeExtraBackupOnSuccess)
			{
				return;
			}
			string text3 = _filename + ".ext.bak";
			try
			{
				SdFile.Copy(sourceFilename, text3, overwrite: true);
			}
			catch (Exception arg)
			{
				Log.Error($"Failed to make extra backup (due to successfully loading) by copying '{sourceFilename}' to '{text3}': {arg}");
			}
		}
	}

	public bool Save(string _filename)
	{
		if (SdFile.Exists(_filename) && GameIO.FileSize(_filename) > 0)
		{
			SdFile.Copy(_filename, _filename + ".bak", overwrite: true);
		}
		return SaveLoad(_filename, _load: false, _warnOnDifferentVersion: false, _infOnDiferentVersion: false);
	}

	public bool Save(Stream _stream)
	{
		return SaveLoad(_stream, _load: false, _warnOnDifferentVersion: false, _infOnDiferentVersion: false);
	}

	public void SetFrom(World _world, EnumChunkProviderId _chunkProviderId)
	{
		waterLevel = WorldConstants.WaterLevel;
		chunkSizeX = 16;
		chunkSizeY = 16;
		chunkSizeZ = 256;
		chunkCount = 0;
		providerId = _chunkProviderId;
		seed = _world.Seed;
		worldTime = _world.worldTime;
		timeInTicks = GameTimer.Instance.ticks;
		sleeperVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter.SetBaseStream(sleeperVolumeState);
			_world.WriteSleeperVolumes(pooledBinaryWriter);
		}
		triggerVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter2 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter2.SetBaseStream(triggerVolumeState);
			_world.WriteTriggerVolumes(pooledBinaryWriter2);
		}
		wallVolumeState = new MemoryStream();
		using (PooledBinaryWriter pooledBinaryWriter3 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
		{
			pooledBinaryWriter3.SetBaseStream(wallVolumeState);
			_world.WriteWallVolumes(pooledBinaryWriter3);
		}
		nextEntityID = EntityFactory.nextEntityID;
		activeGameMode = _world.GetGameMode();
		dynamicSpawnerState = new MemoryStream();
		if (_world.GetDynamiceSpawnManager() != null)
		{
			using PooledBinaryWriter pooledBinaryWriter4 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
			pooledBinaryWriter4.SetBaseStream(dynamicSpawnerState);
			_world.GetDynamiceSpawnManager().Write(pooledBinaryWriter4);
		}
		aiDirectorState = new MemoryStream();
		if (_world.aiDirector != null)
		{
			using (PooledBinaryWriter pooledBinaryWriter5 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter5.SetBaseStream(aiDirectorState);
				_world.aiDirector.Save(pooledBinaryWriter5);
				return;
			}
		}
		using PooledBinaryWriter pooledBinaryWriter6 = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false);
		pooledBinaryWriter6.SetBaseStream(aiDirectorState);
		new AIDirector(_world).Save(pooledBinaryWriter6);
	}

	public void ResetDynamicData()
	{
		worldTime = 0uL;
		timeInTicks = 0uL;
	}

	public void GenerateNewGuid()
	{
		Guid = Utils.GenerateGuid();
	}
}
