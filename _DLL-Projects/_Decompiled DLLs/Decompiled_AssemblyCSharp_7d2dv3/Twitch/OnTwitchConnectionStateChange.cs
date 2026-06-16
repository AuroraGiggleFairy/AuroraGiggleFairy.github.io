namespace Twitch;

public delegate void OnTwitchConnectionStateChange(TwitchManager.InitStates oldState, TwitchManager.InitStates newState);
