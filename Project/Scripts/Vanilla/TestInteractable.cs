namespace LandlessSkies.Vanilla;

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using KGySoft.Serialization.Binary;
using LandlessSkies.Core;
using SevenDev.Utility;
using static LandlessSkies.Core.Entity;

[Tool]
[GlobalClass]
public partial class TestInteractable : Interactable {
	public override string InteractLabel => "Interact";
	public override float? MinLookIncidence => 0f;

	public override void Interact(Entity entity, Player? player = null, int shapeIndex = 0) {
		GD.Print($"Entity {entity.Name} interacted with {Name}, shape {GetShape3D(shapeIndex)?.Name} (index {shapeIndex})");

		if (player is null) return;

		EntitySaveData? savedEntity = entity.Save();


		// string path = @$"{OS.GetUserDataDir()}/SaveData1.dat";
		// BinarySerializationFormatter formatter = new(BinarySerializationOptions.CompactSerializationOfStructures);

		// using (FileStream stream = new(path, FileMode.Create)) {
        //     formatter.SerializeToStream(stream, savedEntity);
		// }
		// using (FileStream stream = new(path, FileMode.Open)) {
        //     savedEntity = formatter.DeserializeFromStream<EntitySaveData?>(stream);
        // }


		Entity? clonedEntity = savedEntity?.Load()?.SetOwnerAndParent(this);
		if (clonedEntity is not null) {
			clonedEntity.GlobalTransform = GlobalTransform;

			player.Entity = clonedEntity;
		}
	}

	public override bool IsInteractable(Entity entity) {
		return true;
	}
}