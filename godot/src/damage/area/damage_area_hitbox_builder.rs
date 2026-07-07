use godot::{classes::{Area3D, BoxShape3D, CapsuleShape3D, CollisionShape3D, CylinderShape3D, Shape3D, SphereShape3D}, prelude::*};

use crate::DamageAreaHitboxBehaviour;

#[derive(GodotConvert, Var, Export, Default, Clone)]
#[godot(via = i64)]
pub enum HitboxShapeKind {
	#[default]
	Box = 0,
	Sphere = 1,
	Capsule = 2,
	Cylinder = 3,
}

#[derive(GodotClass)]
#[class(base=Resource, init, tool)]
pub struct DamageAreaHitboxBuilder {
	#[init(val = HitboxShapeKind::Box)]
	#[export]
	pub shape_kind: HitboxShapeKind,

	#[init(val = Vector3::ONE)]
	#[export]
	pub size: Vector3,

	#[export]
	pub behaviour: Option<Gd<DamageAreaHitboxBehaviour>>,
}

impl DamageAreaHitboxBuilder {
	fn build_shape(&self) -> Gd<Shape3D> {
		match self.shape_kind {
			HitboxShapeKind::Sphere => {
				let mut shape = SphereShape3D::new_gd();
				shape.set_radius(self.size.x.max(0.01));
				shape.upcast()
			}
			HitboxShapeKind::Capsule => {
				let mut shape = CapsuleShape3D::new_gd();
				shape.set_radius(self.size.x.max(0.01));
				shape.set_height(self.size.y.max(0.01));
				shape.upcast()
			}
			HitboxShapeKind::Cylinder => {
				let mut shape = CylinderShape3D::new_gd();
				shape.set_radius(self.size.x.max(0.01));
				shape.set_height(self.size.y.max(0.01));
				shape.upcast()
			}
			HitboxShapeKind::Box => {
				let mut shape = BoxShape3D::new_gd();
				shape.set_size(self.size);
				shape.upcast()
			}
		}
	}

	pub fn add_to(&self, damage_area: &mut Gd<Area3D>) -> Gd<CollisionShape3D> {
		let mut collision = CollisionShape3D::new_alloc();
		collision.set_shape(&self.build_shape());
		damage_area.add_child(&collision.clone().upcast::<Node>());

		if let Some(mut behaviour) = self.behaviour.clone() {
			behaviour
				.bind_mut()
				.setup_hitbox(Some(damage_area.clone()), Some(collision.clone()));
		}

		collision
	}
}
