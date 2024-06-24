namespace LandlessSkies.Core;

using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using SevenDev.Utility;

[Serializable]
public class CostumableSaveData<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T, TCostume>(T data) : SceneSaveData<T>(data) where T : Node, ICostumable where TCostume : Costume {
	public string? CostumePath = data.CostumeHolder?.Costume?.ResourcePath;

	public override T? Load() {
		if (base.Load() is not T data) return null;

		if (CostumePath is not null) {
			TCostume? costume = ResourceLoader.Load<TCostume>(CostumePath);

			data.CostumeHolder ??= new CostumeHolder().ParentTo(data);
			data.CostumeHolder.SetCostume(costume);
		}

		return data;
	}
}