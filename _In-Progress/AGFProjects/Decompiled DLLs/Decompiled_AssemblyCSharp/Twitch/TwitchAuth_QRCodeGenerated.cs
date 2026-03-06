using UnityEngine;

namespace Twitch;

public delegate void TwitchAuth_QRCodeGenerated(Texture2D qrCodeTex, string userCode, string url);
