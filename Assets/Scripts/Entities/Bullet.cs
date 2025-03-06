using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(TrailRenderer))]
public class Bullet : MonoBehaviour {
    const float lifetime = 5f;

    Character _attacker;
    Rigidbody _rigidbody;
    TrailRenderer _trail;
    bool _hasCollided = false;
    float _aliveFor = 0f;
    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _trail = GetComponent<TrailRenderer>();
    }

    private void Update() {
        if (!_hasCollided && _aliveFor >= lifetime) {
            ReturnToPool();
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (_hasCollided) return;

        Vector3 bulletDir = transform.forward;
        ContactPoint cp = collision.GetContact(0);
        ImpactManager.instance.SpawnImpactEffect(cp.point, cp.normal, collision.collider.sharedMaterial);
        DecalManager.instance.SpawnImpactDecal(cp.point - bulletDir * 0.5f, (bulletDir - cp.normal) / 2f, collision.collider.sharedMaterial, collision.collider.transform);

        if (collision.collider.CompareTag("Character")) {
            Character victim = collision.gameObject.GetComponentInParent<Character>();
            float damage = BattleUtils.CalculateDamage(_attacker, victim, 1.0f, true);
            BattleUtils.DoDamage(victim, damage);
            Debug.LogFormat("Inflicted {0} damage to {1}", damage, victim.name);
        }

        ReturnToPool();
    }

    private void ReturnToPool() {
        _hasCollided = true;
        _rigidbody.isKinematic = true;
        BulletManager.instance.Pool.Release(this);
    }

    public void Initialize(Character attacker, float speed, bool gravity) {
        _attacker = attacker;
        _rigidbody.velocity = transform.forward * speed;
        _rigidbody.useGravity = gravity;
        _rigidbody.isKinematic = false;
        _trail.Clear();
        _aliveFor = 0f;
        _hasCollided = false;
    }
}
