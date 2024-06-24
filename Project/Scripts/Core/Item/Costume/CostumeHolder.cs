namespace LandlessSkies.Core;

using Godot;
using SevenDev.Utility;

[Tool]
[GlobalClass]
public partial class CostumeHolder : Node3D {
	[Export] public Costume? Costume {
		get => _costume;
		set => SetCostume(value);
	}
	private Costume? _costume;

	[Export] public Model? Model { get; private set; }


	public CostumeHolder() { }
	public CostumeHolder(Costume? costume) : this() {
		SetCostume(costume);
	}


	public void SetCostume(Costume? newCostume) {
		Costume? oldCostume = _costume;
		if (newCostume == oldCostume) return;

		_costume = newCostume;

		Load(true);
	}

	public void Load(bool forceReload = false) {
		if (Model is not null && ! forceReload) return;

		Unload();

		Model = Costume?.Instantiate()?.ParentTo(this);
	}
	public void Unload() {
		Model?.QueueFree();
		Model = null;
	}
}