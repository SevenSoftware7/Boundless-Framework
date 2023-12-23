

using System;

namespace LandlessSkies.Core;

public abstract class AttackAction : EntityAction {

	public new abstract class Info(Weapon weapon) : EntityAction.Info() {
		public readonly Weapon Weapon = weapon;

		protected abstract override AttackAction Build();
	}
}