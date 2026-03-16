import * as THREE from "three";

export class Spell {
    constructor(
        id,
        caster,
        sprite,
        radius,
        damage,
        lifetime,
        position,
        direction,   // ✅ forward direction (Vector3 or {x,y,z})
        speed,        // ✅ units per second
        knockback
    ) {
        this.id = id;
        this.caster = caster;
        this.sprite = sprite;

        // Position MUST be a Vector3 on the server
        this.position = new THREE.Vector3(
            position.x,
            position.y,
            position.z
        );

        // Normalize direction to avoid speed scaling bugs
        this.direction = new THREE.Vector3(
            direction.x,
            direction.y,
            direction.z
        ).normalize();

        this.speed = speed;
        this.velocity = this.direction.clone().multiplyScalar(this.speed);

        this.radius = radius;
        this.damage = damage;
        this.knockback=knockback;
        this.time = lifetime; // ms
        this.dealtdamage = false;
    }

    update(delta) {
        // delta = seconds
        const step = this.velocity.clone().multiplyScalar(delta);
        this.position.add(step);

        this.time -= delta * 1000;
    }
}
