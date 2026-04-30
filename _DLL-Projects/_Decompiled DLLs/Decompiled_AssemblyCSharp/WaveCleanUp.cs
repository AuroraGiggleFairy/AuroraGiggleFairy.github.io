using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WaveCleanUp : MonoBehaviour
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject PrefabWaveCleanUp;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] ChunkID = Encoding.ASCII.GetBytes("RIFF");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] Format = Encoding.ASCII.GetBytes("WAVE");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] Subchunk1ID = Encoding.ASCII.GetBytes("fmt ");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] Subchunk2ID = Encoding.ASCII.GetBytes("data");

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static short AudioFormat = 1;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int SampleRate = 44100;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static short Channels = 2;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static short BitsPerSample = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int ByteRate = SampleRate * Channels * BitsPerSample / 8;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static short BlockAlign = (short)(Channels * BitsPerSample / 8);

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public static int Subchunk1Size = 16;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsFinished;

	public string FilePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		StartCoroutine(FormatHeader());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FormatHeader()
	{
		yield return new WaitUntil([PublicizedFrom(EAccessModifier.Private)] () => FilePath != null);
		UnityWebRequest getAudioFile = UnityWebRequestMultimedia.GetAudioClip("file://" + FilePath, AudioType.WAV);
		getAudioFile.disposeDownloadHandlerOnDispose = true;
		yield return getAudioFile.SendWebRequest();
		if (getAudioFile.result == UnityWebRequest.Result.ConnectionError)
		{
			Debug.Log(getAudioFile.error);
		}
		else
		{
			AudioClip content = DownloadHandlerAudioClip.GetContent(getAudioFile);
			float[] array = new float[content.samples * content.channels];
			content.GetData(array, 0);
			byte[] array2 = PCMDataToByteArray(array);
			int num = content.samples * Channels * BitsPerSample / 8;
			int value = 36 + num;
			using Stream stream = SdFile.Open(FilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			stream.Write(ChunkID, 0, 4);
			stream.Write(BitConverter.GetBytes(value), 0, 4);
			stream.Write(Format, 0, 4);
			stream.Write(Subchunk1ID, 0, 4);
			stream.Write(BitConverter.GetBytes(Subchunk1Size), 0, 4);
			stream.Write(BitConverter.GetBytes(AudioFormat), 0, 2);
			stream.Write(BitConverter.GetBytes(Channels), 0, 2);
			stream.Write(BitConverter.GetBytes(SampleRate), 0, 4);
			stream.Write(BitConverter.GetBytes(ByteRate), 0, 4);
			stream.Write(BitConverter.GetBytes(BlockAlign), 0, 2);
			stream.Write(BitConverter.GetBytes(BitsPerSample), 0, 2);
			stream.Write(Subchunk2ID, 0, 4);
			stream.Write(BitConverter.GetBytes(num), 0, 4);
			stream.Write(array2, 0, array2.Length);
		}
		getAudioFile.Dispose();
		Log.Out("Cleaned up: " + FilePath);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] PCMDataToByteArray(float[] _pcmData)
	{
		byte[] array = new byte[2 * _pcmData.Length];
		for (int i = 0; i < _pcmData.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes((short)(_pcmData[i] * 32767f));
			for (int j = 0; j < 2; j++)
			{
				array[2 * i + j] = bytes[j];
			}
		}
		return array;
	}

	public static GameObject Create()
	{
		if (PrefabWaveCleanUp == null)
		{
			PrefabWaveCleanUp = Resources.Load<GameObject>("Prefabs/prefabDMSWaveCleanup");
		}
		GameObject obj = UnityEngine.Object.Instantiate(PrefabWaveCleanUp);
		obj.name = "WaveCleanUp";
		return obj;
	}
}
