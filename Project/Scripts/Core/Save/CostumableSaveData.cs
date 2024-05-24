namespace LandlessSkies.Core;

using Godot;

public abstract class CostumableSaveData<T, TCostumable, TCostume>(TCostumable data) : SceneSaveData<T>(data) where T : Node where TCostumable : T, ICostumable<TCostume> where TCostume : Costume {
	public string? CostumePath = data.Costume?.ResourcePath;

	public override TCostumable? Load() {
		if (base.Load() is not TCostumable data) return null;

		if (CostumePath is not null) {
			TCostume? costume = ResourceLoader.Load<TCostume>(CostumePath);
			data.Costume = costume;
		}

		return data;
	}
}