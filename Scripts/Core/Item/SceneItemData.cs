namespace LandlessSkies.Core;

using Godot;
using SevenDev.Boundless.Persistence;

[Tool]
public abstract partial class SceneItemData<[MustBeVariant] T> : Resource, IItemData<T> where T : Node, IItem<T> {
	IDataKeyProvider<T> IItemData<T>.KeyProvider => KeyProvider;
	[Export] private GenericResourceDataKey<T> KeyProvider {
		get => _keyProvider;
		set {
			if (value is null) return;
			_keyProvider = value;
		}
	}
	private GenericResourceDataKey<T> _keyProvider = new();

	[Export] public ItemUIData? UIData { get; private set; } = new();
	public string DisplayName => UIData?.DisplayName ?? string.Empty;
	public Texture2D? DisplayPortrait => UIData?.DisplayPortrait;


	[Export] public PackedScene? Scene {
		get => _scene;
		private set {
			if (value is null) {
				_scene = null;
				return;
			}

			Node? instance = value?.Instantiate();

			if (instance is T) {
				_scene = value;
			}
			else {
				GD.PushError($"SceneItemData<{typeof(T).Name}>: Scene must be a PackedScene of type {typeof(T).Name}");
			}

			instance?.QueueFree();
		}
	}

	private PackedScene? _scene = null;


	public SceneItemData(PackedScene? scene) : base() {
		_scene = scene;
	}
	public SceneItemData(string path) : this(ResourceLoader.Load<PackedScene>(path)) { }
	public SceneItemData() : this(scene: null) { }


	public T Instantiate() => Scene?.InstantiateOrNull<T>() ?? throw new System.NullReferenceException();
}