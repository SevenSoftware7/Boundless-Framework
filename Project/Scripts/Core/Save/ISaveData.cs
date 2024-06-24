using Godot;

namespace LandlessSkies.Core;

public interface ISaveData {
	object? Load();
}

public interface ISaveData<T> : ISaveData {
	object? ISaveData.Load() => Load();
	new T? Load();
}