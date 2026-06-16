using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using SpaceWizards.HttpListener;

namespace Webserver;

public static class OpenID
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string STEAM_LOGIN = "https://steamcommunity.com/openid/login";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex steamIdUrlMatcher;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly X509Certificate2 caCert;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly X509Certificate2 caIntermediateCert;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool verboseSsl;

	public static bool debugOpenId;

	[PublicizedFrom(EAccessModifier.Private)]
	static OpenID()
	{
		steamIdUrlMatcher = new Regex("^https?:\\/\\/steamcommunity\\.com\\/openid\\/id\\/([0-9]{17,18})");
		caCert = new X509Certificate2(GameIO.GetGameDir("Data/Web") + "/steam-rootca.cer");
		caIntermediateCert = new X509Certificate2(GameIO.GetGameDir("Data/Web") + "/steam-intermediate.cer");
		verboseSsl = false;
		for (int i = 0; i < Environment.GetCommandLineArgs().Length; i++)
		{
			if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-debugopenid"))
			{
				debugOpenId = true;
			}
		}
		ServicePointManager.ServerCertificateValidationCallback = [PublicizedFrom(EAccessModifier.Internal)] (object _srvPoint, X509Certificate _certificate, X509Chain _chain, SslPolicyErrors _errors) =>
		{
			if (_errors == SslPolicyErrors.None)
			{
				if (verboseSsl)
				{
					Log.Out("[OpenID] Steam certificate: No error (1)");
				}
				return true;
			}
			X509Chain x509Chain = new X509Chain
			{
				ChainPolicy = 
				{
					RevocationMode = X509RevocationMode.NoCheck,
					ExtraStore = { caCert, caIntermediateCert }
				}
			};
			if (x509Chain.Build(new X509Certificate2(_certificate)))
			{
				x509Chain.Reset();
				if (verboseSsl)
				{
					Log.Out("Steam certificate: No error (2)");
				}
				return true;
			}
			if (x509Chain.ChainStatus.Length == 0)
			{
				x509Chain.Reset();
				if (verboseSsl)
				{
					Log.Out("Steam certificate: No error (3)");
				}
				return true;
			}
			X509ChainElementEnumerator enumerator = x509Chain.ChainElements.GetEnumerator();
			X509ChainStatus[] chainElementStatus;
			while (enumerator.MoveNext())
			{
				X509ChainElement current = enumerator.Current;
				if (verboseSsl)
				{
					Log.Out("Validating cert: " + current.Certificate.Subject);
				}
				chainElementStatus = current.ChainElementStatus;
				for (int j = 0; j < chainElementStatus.Length; j++)
				{
					X509ChainStatus x509ChainStatus = chainElementStatus[j];
					if (verboseSsl)
					{
						Log.Out($"   Status: {x509ChainStatus.Status}");
					}
					if (x509ChainStatus.Status != X509ChainStatusFlags.NoError && (x509ChainStatus.Status != X509ChainStatusFlags.UntrustedRoot || !current.Certificate.Equals(caCert)))
					{
						Log.Warning($"[OpenID] Steam certificate error: {current.Certificate.Subject} ### Error: {x509ChainStatus.Status}");
						x509Chain.Reset();
						return false;
					}
				}
			}
			chainElementStatus = x509Chain.ChainStatus;
			for (int j = 0; j < chainElementStatus.Length; j++)
			{
				X509ChainStatus x509ChainStatus2 = chainElementStatus[j];
				if (x509ChainStatus2.Status != X509ChainStatusFlags.NoError && x509ChainStatus2.Status != X509ChainStatusFlags.UntrustedRoot)
				{
					Log.Warning($"[OpenID] Steam certificate error: {x509ChainStatus2.Status}");
					x509Chain.Reset();
					return false;
				}
			}
			x509Chain.Reset();
			if (verboseSsl)
			{
				Log.Out("[OpenID] Steam certificate: No error (4)");
			}
			return true;
		};
	}

	public static string GetOpenIdLoginUrl(string _returnHost, string _returnUrl)
	{
		Dictionary<string, string> queryParams = new Dictionary<string, string>
		{
			{ "openid.ns", "http://specs.openid.net/auth/2.0" },
			{ "openid.mode", "checkid_setup" },
			{ "openid.return_to", _returnUrl },
			{ "openid.realm", _returnHost },
			{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
			{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
		};
		return "https://steamcommunity.com/openid/login?" + buildUrlParams(queryParams);
	}

	public static ulong Validate(SpaceWizards.HttpListener.HttpListenerRequest _req)
	{
		string value = getValue(_req, "openid.mode");
		if (value == "cancel")
		{
			Log.Warning("[OpenID] Steam OpenID login canceled");
			return 0uL;
		}
		if (value == "error")
		{
			Log.Warning("[OpenID] Steam OpenID login error: " + getValue(_req, "openid.error"));
			if (debugOpenId)
			{
				PrintOpenIdResponse(_req);
			}
			return 0uL;
		}
		string value2 = getValue(_req, "openid.claimed_id");
		Match match = steamIdUrlMatcher.Match(value2);
		if (match.Success)
		{
			ulong result = ulong.Parse(match.Groups[1].Value);
			Dictionary<string, string> dictionary = new Dictionary<string, string>
			{
				{ "openid.ns", "http://specs.openid.net/auth/2.0" },
				{
					"openid.assoc_handle",
					getValue(_req, "openid.assoc_handle")
				},
				{
					"openid.signed",
					getValue(_req, "openid.signed")
				},
				{
					"openid.sig",
					getValue(_req, "openid.sig")
				},
				{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" }
			};
			string[] array = getValue(_req, "openid.signed").Split(',');
			foreach (string text in array)
			{
				string text2 = "openid." + text;
				dictionary[text2] = getValue(_req, text2);
			}
			dictionary.Add("openid.mode", "check_authentication");
			byte[] bytes = Encoding.ASCII.GetBytes(buildUrlParams(dictionary));
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/openid/login");
			httpWebRequest.Method = "POST";
			httpWebRequest.ContentType = "application/x-www-form-urlencoded";
			httpWebRequest.ContentLength = bytes.Length;
			httpWebRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en");
			using (Stream stream = httpWebRequest.GetRequestStream())
			{
				stream.Write(bytes, 0, bytes.Length);
			}
			string text3;
			using (Stream stream2 = ((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream())
			{
				using StreamReader streamReader = new StreamReader(stream2);
				text3 = streamReader.ReadToEnd();
			}
			if (text3.ContainsCaseInsensitive("is_valid:true"))
			{
				return result;
			}
			Log.Warning("[OpenID] Steam OpenID login failed: " + text3);
			return 0uL;
		}
		Log.Warning("[OpenID] Steam OpenID login result did not give a valid SteamID");
		if (debugOpenId)
		{
			PrintOpenIdResponse(_req);
		}
		return 0uL;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string buildUrlParams(Dictionary<string, string> _queryParams)
	{
		string[] array = new string[_queryParams.Count];
		int num = 0;
		foreach (var (text3, stringToEscape) in _queryParams)
		{
			array[num++] = text3 + "=" + Uri.EscapeDataString(stringToEscape);
		}
		return string.Join("&", array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getValue(SpaceWizards.HttpListener.HttpListenerRequest _req, string _name)
	{
		NameValueCollection queryString = _req.QueryString;
		if (queryString[_name] == null)
		{
			throw new MissingMemberException("[OpenID] OpenID parameter \"" + _name + "\" missing");
		}
		return queryString[_name];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void PrintOpenIdResponse(SpaceWizards.HttpListener.HttpListenerRequest _req)
	{
		NameValueCollection queryString = _req.QueryString;
		for (int i = 0; i < queryString.Count; i++)
		{
			Log.Out("   " + queryString.GetKey(i) + " = " + queryString[i]);
		}
	}
}
