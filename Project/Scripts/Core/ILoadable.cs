namespace LandlessSkies.Core;

public interface ILoadable : IEnablable {

	bool IsLoaded { get; set; }

	sealed void SetLoaded(bool loaded) {
		IsLoaded = loaded;
	}

	sealed bool Load() {
		if (IsLoaded)
			return false;

		return LoadBehaviour();
	}
	sealed void Unload() {
		if (!IsLoaded) return;

		UnloadBehaviour();
	}


	/// <summary>
	/// Reloads the model by unloading it then loading it back again.
	/// </summary>
	void Reload(bool forceLoad = false) {
		bool wasLoaded = IsLoaded;
		Unload();

		if (wasLoaded || forceLoad) {
			Load();
		}
	}


	/// <summary>
	/// Utility method to Load or Unload, depending on the given "doLoad" value.
	/// </summary>
	internal sealed void LoadUnload(bool doLoad) {
		if (doLoad) {
			Load();
		}
		else {
			Unload();
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