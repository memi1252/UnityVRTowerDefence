using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bomb : MonoBehaviour
{
    private Transform explosion;

    private ParticleSystem expEffect;

    private AudioSource expAudio;
    public bool isbomb = false;
    
    // 폭발 범위
    public float range = 5f;
    
    // Start is called before the first frame update
    void Start()
    {
        explosion = GameObject.Find("Explosion").transform;
        expEffect = explosion.GetComponent<ParticleSystem>();
        expAudio = explosion.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isbomb) return;
        // 레이어 마스크 가져오기
        int layerMask = 1 << LayerMask.NameToLayer("Drone");
        Collider[] drons = Physics.OverlapSphere(transform.position, range, layerMask);
        foreach (Collider drone in drons)
        {
            drone.GetComponentInParent<DroneAI>().OnDamageProcess(999);
        }
        
        explosion.position = transform.position;
        expEffect.Play();
        expAudio.Play();
        GetComponent<DroneAI>().OnDamageProcess(999);
    }
}
