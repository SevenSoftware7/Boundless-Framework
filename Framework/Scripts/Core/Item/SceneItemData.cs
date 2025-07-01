namespace SevenDev.Boundless;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public abstract partial class SceneItemData<[MustBeVariant] T> : Resource, IItemData<T> where T : Node, IItem<T> {

	[Export] private string ItemKeyString {
		get => ItemKey?.String ?? string.Empty;
		set => ItemKey = string.IsNullOrWhiteSpace(value) ? null : new ItemKey(value);
	}
	public ItemKey? ItemKey = new();
	ItemKey? IItemData.ItemKey => ItemKey;

	[Export] public string DisplayName { get; private set; } = string.Empty;
	[Export] public Texture2D? DisplayPortrait { get; private set; } = null;


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