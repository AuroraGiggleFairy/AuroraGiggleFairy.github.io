using System;

public class MeshTiming
{
	public double CopyVerts;

	public double CopyUv;

	public double CopyUv2;

	public double CopyUv3;

	public double CopyUv4;

	public double CopyColours;

	public double CopyTriangles;

	public double CopyNormals;

	public double CopyTangents;

	public double UploadMesh;

	public double NormalRecalc;

	public DateTime Start;

	public double time => GetTime();

	public string Details()
	{
		double num = CopyVerts + CopyUv + CopyUv2 + CopyUv3 + CopyUv4 + CopyColours + CopyTriangles + CopyNormals + CopyTangents + UploadMesh + NormalRecalc;
		return $"\r\nCopyVerts: {CopyVerts}\r\nCopyUv:{CopyUv}\r\nCopyUv2:{CopyUv2}\r\nCopyUv3:{CopyUv3}\r\nCopyUv4:{CopyUv4}\r\nCopyColours:{CopyColours}\r\nCopyTriangles:{CopyTriangles}\r\nCopyNormals:{CopyNormals}\r\nCopyTangents:{CopyTangents}\r\nUploadMesh:{UploadMesh}\r\nNormalRecalc:{NormalRecalc}\r\nTotal: {num}\r\n";
	}

	public double GetTime()
	{
		return (DateTime.Now - Start).TotalMilliseconds;
	}

	public void Reset()
	{
		Start = DateTime.Now;
	}
}
