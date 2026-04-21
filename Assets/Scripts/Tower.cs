using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    
    
    
    // 타워의 최초 Hp
    public float initialHP = 50;
    private float _hp = 0;
    public Image damageImage;

    public float HP
    {
        get
        {
            return _hp;
        }
        set
        {
            _hp = value;
            // hp가 0이하이면 제거
            if (_hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    private void Awake()
    {
        
    }

    void Start()
    {
        if (initialHP <= 0f)
        {
            initialHP = 1f;
        }

        _hp = initialHP;
        // 카메라의 nearClipPlane값을 기억해 둔다
        float z = Camera.main.nearClipPlane + 0.01f;
    }

    void Update()
    {
        if (damageImage != null)
        {
            damageImage.fillAmount = HP / initialHP;
        }
    }
}
