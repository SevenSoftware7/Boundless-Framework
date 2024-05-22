using Godot;

namespace LandlessSkies.Core;

public interface ISaveData {
	Node? Load();
}

public interface ISaveData<T> : ISaveData where T : Node {
	Node? ISaveData.Load() => Load();
	new T? Load();
}