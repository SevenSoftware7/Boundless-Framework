use std::{collections::{HashMap, HashSet}};

use boundless::{damage::{Damage, DamageDealer, DamageModifier, Damageable}, sync::BdlsPtr};
use godot::{
	classes::{Area3D, IArea3D}, prelude::*
};
use itertools::Itertools;

use crate::{DamageAreaHitboxBuilder, DamageModifierWrapper, GodotDamage, GodotDamageDealer, GodotDamageable};


#[derive(GodotClass)]
#[class(base=Area3D, init, tool)]
pub struct DamageArea {
	#[base]
	pub base: Base<Area3D>,

	#[init(val = 0)]
	#[export]
	pub flags: i64,

	#[init(val = 0.0)]
	#[export]
	pub life_time: f32,

	#[init(val = 0)]
	#[export]
	pub max_impacts: i32,

	#[init(val = 1.0)]
	#[export]
	pub damage_multiplier: f32,

	#[export]
	pub damage_builders: Array<Gd<GodotDamage>>,

	#[export]
	pub damage_dealer: Option<DynGd<Node, dyn DamageDealer>>,

	impacts_history: HashMap<DynGd<Node, dyn Damageable>, i32>,
	impacts_buffer: HashSet<DynGd<Node, dyn Damageable>>,

	buffered_impacts_flush: bool,
}

#[godot_api]
impl DamageArea {
	#[must_use]
	pub fn new(
		base: Base<Area3D>,
		flags: i64,
		life_time: f32,
		max_impacts: i32,
		damage_multiplier: f32,
	) -> Self {
		Self {
			base,
			flags,
			life_time,
			max_impacts,
			damage_multiplier,
			damage_builders: Array::default(),
			damage_dealer: None,
			impacts_history: HashMap::default(),
			impacts_buffer: HashSet::default(),
			buffered_impacts_flush: false,
		}
	}

	#[must_use]
	pub fn with_dealer(mut self, dealer: DynGd<Node, dyn DamageDealer>) -> Self {
		self.damage_dealer = Some(dealer);
		self
	}

	#[must_use]
	pub fn with_hitbox(mut self, hitbox_builder: &Gd<DamageAreaHitboxBuilder>) -> Self {
		hitbox_builder.bind().add_to(&mut self.base_mut());
		self
	}

	#[must_use]
	pub fn with_hitboxes(mut self, hitbox_builders: impl IntoIterator<Item = Gd<DamageAreaHitboxBuilder>>) -> Self {
		for hitbox_builder in hitbox_builders {
			hitbox_builder.bind().add_to(&mut self.base_mut());
		}

		self
	}

	pub fn build_damages(&self) -> Vec<Damage> {
		let mut damages: Vec<Damage> = self.damage_builders.iter_shared().map(|builder| {
			let builder_ref = builder.bind();
			Damage::new(
				builder_ref.amount,
				builder_ref.modifiers.iter_shared()
					.flatten()
					.unique()
					.map(|modifier| BdlsPtr::<DamageModifierWrapper>::new(modifier.into()) as BdlsPtr<dyn DamageModifier>),
			)
		}).collect();

		for damage in &mut damages {
			damage.amount *= self.damage_multiplier;
		}

		damages
	}

	pub fn inflict(
		&mut self,
		target: DynGd<Node, dyn Damageable>,
	) {
		let mut damages = self.build_damages();

		let godot_target = GodotDamageable::from(target);
		let damage_dealer = self.damage_dealer.clone().map(GodotDamageDealer::from);

		for damage in &mut damages {
			damage.inflict(godot_target.clone(), damage_dealer.clone());
		}
	}

	pub fn get_impact_count(&self, target: &DynGd<Node, dyn Damageable>) -> i32 {
		self.impacts_history.get(target).copied().unwrap_or(0)
	}

	fn buffer_impact(&mut self, target: DynGd<Node, dyn Damageable>) {
		if self.max_impacts >= 0 && self.get_impact_count(&target) >= self.max_impacts {
			return;
		}

		self.impacts_buffer.insert(target);
		if ! self.buffered_impacts_flush {
			self.run_deferred(Self::flush_buffered_impacts);
			self.buffered_impacts_flush = true;
		}
	}

	pub fn flush_buffered_impacts(&mut self) {
		let buffered = std::mem::take(&mut self.impacts_buffer);
		for target in buffered {
			self.inflict(target.clone());
			*self.impacts_history.entry(target).or_insert(0) += 1;
		}

		self.buffered_impacts_flush = false;
		self.reset_impacts();
	}

	pub fn reset_impacts(&mut self) {
		self.impacts_buffer.clear();
	}


	#[func]
	pub fn on_area_entered(&mut self, area: Gd<Area3D>) {
		if let Ok(damageable) = area.upcast::<Node3D>().try_dynify::<dyn Damageable>() {
			self.buffer_impact(damageable.upcast::<Node>());
		}
	}

	#[func]
	pub fn on_body_entered(&mut self, body: Gd<Node3D>) {
		if let Ok(damageable) = body.try_dynify::<dyn Damageable>() {
			self.buffer_impact(damageable.upcast::<Node>());
		}
	}
}

#[godot_api]
impl IArea3D for DamageArea {
	fn ready(&mut self) {
		// We use untyped signals to avoid crashes when hot-reloading
		let mut gd_self = self.to_gd();

		let body_entered_callable = gd_self.callable("on_body_entered");
		gd_self.connect("body_entered", &body_entered_callable);

		let area_entered_callable = gd_self.callable("on_area_entered");
		gd_self.connect("area_entered", &area_entered_callable);


		// self.base()
		// 	.signals()
		// 	.body_entered()
		// 	.connect_other(self, DamageArea::on_body_entered);

		// self.base()
		// 	.signals()
		// 	.area_entered()
		// 	.connect_other(self, DamageArea::on_area_entered);
	}
}
