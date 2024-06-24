namespace LandlessSkies.Core;

public interface ISaveable {
	ISaveData Save();
}

public interface ISaveable<out T> : ISaveable {
	ISaveData ISaveable.Save() => Save();
	new ISaveData<T> Save();
}