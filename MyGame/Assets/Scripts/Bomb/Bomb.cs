using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 此类用于控制炸弹。
/// </summary>
public class Bomb : MonoBehaviour
{
    private float _startTime;
    public float waitTime;

    private Animator _animator;

    public float explosionRadius;
    public LayerMask targetLayer;
    public float bombForce;
    private static readonly int Explosion = Animator.StringToHash("explosion");

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _startTime = Time.time;
    }

    void Update()
    {
        if (_startTime + waitTime <= Time.time)
        {
            _animator.SetTrigger(Explosion);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,explosionRadius);
    }

    public void Explode()
    {
        Collider2D[] aroundObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius,targetLayer);
        foreach (var item in aroundObjects)
        {
            Vector3 forceDirection = item.transform.position - transform.position;
            item.GetComponent<Rigidbody2D>().AddForce(forceDirection * bombForce,ForceMode2D.Impulse);
        }
    }

    public void Exploded()
    {
        gameObject.SetActive(false);
    }
}
