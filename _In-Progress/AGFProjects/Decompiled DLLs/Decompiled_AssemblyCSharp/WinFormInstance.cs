using System;
using System.Threading;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Profiling;

public class WinFormInstance : IConsoleServer
{
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Thread windowThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopThread;

	[PublicizedFrom(EAccessModifier.Private)]
	public WinFormConnection form;

	public WinFormInstance()
	{
		try
		{
			windowThread = new Thread(windowThreadMain)
			{
				Name = "WinFormInstance"
			};
			windowThread.SetApartmentState(ApartmentState.STA);
			windowThread.Start();
			Thread.Sleep(250);
			Log.Out("Started Terminal Window");
		}
		catch (Exception ex)
		{
			Log.Out("Error in WinFormInstance.ctor: " + ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void windowThreadMain()
	{
		form = new WinFormConnection(this);
		Log.Out("WinThread started");
		System.Windows.Forms.Application.ThreadException += ApplicationOnThreadException;
		System.Windows.Forms.Application.Run(form);
		Profiler.EndThreadProfiling();
		form = null;
		Log.Out("WinThread ended");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplicationOnThreadException(object _sender, ThreadExceptionEventArgs _threadExceptionEventArgs)
	{
		Log.Error("TerminalWindow Exeption:");
		Log.Exception(_threadExceptionEventArgs.Exception);
	}

	public void Disconnect()
	{
		if (form != null)
		{
			Log.Out("Closing Terminal Window");
			WinFormConnection winFormConnection = form;
			form = null;
			winFormConnection.CloseTerminal();
			windowThread.Join();
			Log.Out("Ended Terminal Window");
		}
	}

	public void SendLine(string _line)
	{
		if (_line != null && form != null)
		{
			form.SendLine(_line);
		}
	}

	public void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
	{
		if (_formattedMessage != null && form != null)
		{
			form.SendLog(_formattedMessage, _plainMessage, _trace, _type, _timestamp, _uptime);
		}
	}
}
