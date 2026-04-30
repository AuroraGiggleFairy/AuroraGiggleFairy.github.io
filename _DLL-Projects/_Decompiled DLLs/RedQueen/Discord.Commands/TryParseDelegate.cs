namespace Discord.Commands;

internal delegate bool TryParseDelegate<T>(string str, out T value);
