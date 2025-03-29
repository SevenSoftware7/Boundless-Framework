namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public abstract partial class SceneItemData<[MustBeVariant] T> : Resource, IItemData<T> where T : Node, IItem<T> {
	ItemKey? IItemData.ItemKey => KeyProvider.ItemKey;
	[Export] public ResourceItemKey KeyProvider {
		get;
		private set {
			if (value is null) return;
			field = value;
		}
	} = new();

	[Export] public ItemUIData? UIData { get; private set; } = new();
	public string DisplayName => UIData?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UIData?.DisplayPortrait;


	// We export a scene path instead of a PackedScene because otherwise Godot has issues with cyclical resource loading.
	[Export(PropertyHint.File, "*.tscn")]
	public string? ScenePath {
		get;
		private set {
			field = value;
			Scene = null;
		}
	}


	protected PackedScene? Scene {
		get {
			if (field is null && ScenePath is not null) field = ResourceLoader.Load<PackedScene>(ScenePath);
			return field;
		}
		private set;
	}


	public SceneItemData(string? scenePath) : base() {
		ScenePath = scenePath;
	}
	public SceneItemData() : this(scenePath: null) { }


	public T Instantiate() {
		if (Scene is null) throw new System.InvalidOperationException($"Trying to Instantiate Scene Item without a valid Scene Path - {nameof(ScenePath)}");

		return Scene.Instantiate<T>();
	}
}