// namespace LandlessSkies.Core;

// public interface ICostumable {
// 	Costume? Costume { get; set; }

// 	abstract bool IsLoaded { get; }

// 	void Reload(bool forceReload = false) {
// 		bool wasLoaded = IsLoaded;
// 		Unload();
// 		if (wasLoaded || forceReload) {
// 			Load(forceReload);
// 		}
// 	}
// 	void Load(bool forceReload = false);
// 	void Unload();
// }

// public interface ICostumable<T> : ICostumable where T : Costume {
// 	Costume? ICostumable.Costume {
// 		get => Costume;
// 		set => Costume = value as T;
// 	}
// 	new T? Costume { get; set; }
// }