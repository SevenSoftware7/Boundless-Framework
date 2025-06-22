namespace SevenDev.Boundless;

using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SevenDev.Boundless.Injection;
using SevenDev.Boundless.Modding;
using SevenDev.Boundless.Persistence;

[Tool]
[GlobalClass]
public partial class MainNode : Node3D, ISerializationListener {
	public IInjectionNode InjectionNode { get; }
	private readonly ItemDataRegistry _registry = new(GD.Print);
	[Injector] private readonly CompositeItemDataRegistry _registries = new();


	public MainNode() : base() {
		InjectionNode = new GodotNodeInjectionNode(this);
	}


	public override void _Ready() {
		base._Ready();

		RegisterData();
		this.PropagateInjection();

		if (Engine.IsEditorHint()) return;


		IEnumerable<Mod> mods = ModLoader.LoadInternalModFolder("EndlessTwilight");
		List<IItemDataProvider> addedRegistries = [];
		foreach (Mod mod in mods) {
			mod.Start();
			foreach (IModInterface modInterface in mod.ModInterfaces) {
				if (modInterface.ItemDataProvider is IItemDataProvider itemDataProvider) {
					addedRegistries.Add(itemDataProvider);
					_registries.AddRegistry(itemDataProvider);
				}
			}
		}

		GD.Print($"Loaded {mods.Count()} mods with {addedRegistries.Count} item data providers.");

		foreach (IItemDataProvider itemDataProvider in addedRegistries) {
			_registries.RemoveRegistry(itemDataProvider);
		}

		foreach (Mod mod in mods) {
			mod.Stop();
		}
	}

	public void OnBeforeSerialize() { }

	public void OnAfterDeserialize() {
		Callable.From(_Ready).CallDeferred();
	}


	private void RegisterData() {
		_registry.RegisterData(GD.Load<EntitySceneData>("uid://djyqyaq7lwdkr")); // Eos
		_registry.RegisterData(GD.Load<EntityCostumeSceneData>("uid://dvcjw3gon7rak")); // Eos Base
		_registry.RegisterData(GD.Load<EntitySceneData>("uid://q4arq6wxs6ho")); // Bike
		_registry.RegisterData(GD.Load<EntityCostumeSceneData>("uid://bb3xo4pwx7g3y")); // Bike Base

		_registry.RegisterData(GD.Load<CompanionSceneData>("uid://7o3ch4vnd7ux")); // Erebos
		_registry.RegisterData(GD.Load<CompanionCostumeSceneData>("uid://caqyvip0olism")); // Erebos Base

		_registry.RegisterData(GD.Load<WeaponSceneData>("uid://7vx1dn4qvh34")); // Epiphron
		_registry.RegisterData(GD.Load<WeaponCostumeSceneData>("uid://c376t1le83xwr")); // Epiphron Base
		_registry.RegisterData(GD.Load<WeaponSceneData>("uid://ci6ahybitat4f")); // Pax
		_registry.RegisterData(GD.Load<WeaponCostumeSceneData>("uid://ctyrw08y4svjk")); // Pax Base
		_registry.RegisterData(GD.Load<WeaponSceneData>("uid://dwoudcgmk0aae")); // Eleos
		_registry.RegisterData(GD.Load<WeaponCostumeSceneData>("uid://q68mneifjek2")); // Eleos Base

		_registries.AddRegistry(_registry);
	}
}