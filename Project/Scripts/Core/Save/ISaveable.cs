namespace LandlessSkies.Core;

using Godot;

public interface ISaveable {
	ISaveData Save();
}

public interface ISaveable<T> : ISaveable where T : Node {
	ISaveData ISaveable.Save() => Save();
	new ISaveData<T> Save();
}