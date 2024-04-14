namespace LandlessSkies.Core;

public interface ILoadable : IDestroyable, IEnablable {

	bool IsLoaded { get; set; }

	public sealed void SetLoaded(bool loaded) {
		IsLoaded = loaded;
	}
	public sealed bool Load() {
		if (IsLoaded)
			return false;

		return LoadBehaviour();
	}
	public sealed void Unload() {
		if (!IsLoaded)
			return;

		UnloadBehaviour();
	}

	public sealed void LoadUnload(bool loaded) {
		if (loaded) {
			Load();
		} else {
			Unload();
		}
	}


	/// <summary>
	/// Reloads the model by unloading it then loading it back again.
	/// </summary>
	virtual void ReloadModel(bool forceLoad = false) {
		bool wasLoaded = IsLoaded;
		Unload();

		if (wasLoaded || forceLoad) {
			Load();
		}
	}


	/// <summary>
	/// Loads the model immediately, without checking if it's already loaded.
	/// </summary>
	/// <returns>
	/// Returns true if the model was loaded, false if it wasn't.
	/// </returns>
	protected bool LoadBehaviour() => true;


	/// <summary>
	/// Unloads the model immediately, without checking if it's already unloaded.
	/// </summary>
	protected void UnloadBehaviour() { }
}