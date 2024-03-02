namespace LandlessSkies.Core;

public interface ISaveData<T> {
	T Load();
}