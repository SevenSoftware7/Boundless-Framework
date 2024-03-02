namespace LandlessSkies.Core;

public interface ISaveable<T> {
	ISaveData<T> Save();
}