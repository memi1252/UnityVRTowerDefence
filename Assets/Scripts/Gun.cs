using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    //총알 파편 효과
    public Transform bulletImpact; 
    // 총알 파편 파티클 시스템
    private ParticleSystem bulletEffect; 
    // 총알 발사 사운드
    private AudioSource bulletAudio;
    // crosshair를 위한 속성
    public Transform crosshair;
    public Transform leftCrosshair;
    
    public float gunlinerWidth = 0.1f;
    private GrabObject _grabObject;
    private int _excludeLayerMask;
    public float rightDamge;
    public float leftDamge;
    
    
    void Start()
    {
        //총알 효과 파티클 시스템 컴포넌트 가져오기
        bulletEffect = bulletImpact.GetComponent<ParticleSystem>();
        //총알 효과 오디오 소스 컴퍼넌트 가져오기
        bulletAudio = bulletImpact.GetComponent<AudioSource>();
        _grabObject = GetComponent<GrabObject>();

        int playerLayer = 1 << LayerMask.NameToLayer("Player");
        int towerLayer = 1 << LayerMask.NameToLayer("Tower");
        int weaponLayer = 1 << LayerMask.NameToLayer("Weapon");
        int doorLayer = 1 << LayerMask.NameToLayer("Door");
        _excludeLayerMask = playerLayer | towerLayer | weaponLayer | doorLayer;
    }

    void Update()
    {
        DrawCrosshairs();

        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch))
        {
            TryShoot(ARAVRInput.Controller.RTouch);
        }

        if (ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.LTouch))
        {
            TryShoot(ARAVRInput.Controller.LTouch);
        }
    }

    private void DrawCrosshairs()
    {
        if (crosshair != null)
        {
            ARAVRInput.DrawCrosshair(crosshair, true, ARAVRInput.Controller.RTouch);
        }

        if (leftCrosshair != null)
        {
            ARAVRInput.DrawCrosshair(leftCrosshair, true, ARAVRInput.Controller.LTouch);
        }
    }

    private void TryShoot(ARAVRInput.Controller controller)
    {
        if (_grabObject == null)
        {
            return;
        }

        GameObject weaponObject = _grabObject.GetGrabbedObject(controller);
        if (weaponObject == null || weaponObject.layer != LayerMask.NameToLayer("Weapon"))
        {
            return;
        }

        Weapon weapon = weaponObject.GetComponentInChildren<Weapon>();
        if (weapon == null)
        {
            return;
        }

        if (!weapon.ConsumeAmmo())
        {
            return;
        }

        LineRenderer lr = weaponObject.GetComponent<LineRenderer>();
        if (lr == null)
        {
            return;
        }

        ARAVRInput.PlayVibration(controller);

        if (bulletAudio != null)
        {
            bulletAudio.Stop();
            bulletAudio.Play();
        }

        Ray ray = new Ray(GetHandPosition(controller), GetHandDirection(controller));
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 200f, ~_excludeLayerMask))
        {
            lr.startWidth = gunlinerWidth;
            lr.endWidth = gunlinerWidth;
            lr.SetPosition(0, ray.origin);
            lr.SetPosition(1, hitInfo.point);
            StartCoroutine(LineOff(lr));

            if (bulletEffect != null)
            {
                bulletEffect.Stop();
                bulletEffect.Play();
            }

            if (bulletImpact != null)
            {
                bulletImpact.position = hitInfo.point;
                bulletImpact.forward = hitInfo.normal;
            }

            DroneAI drone = hitInfo.transform.GetComponentInParent<DroneAI>();
            if (drone != null)
            {
                if (controller == ARAVRInput.Controller.RTouch)
                {
                   
                    drone.OnDamageProcess(rightDamge);
                }
                else
                {
                    
                    drone.OnDamageProcess(leftDamge);
                }
            }
        }
    }

    private Vector3 GetHandPosition(ARAVRInput.Controller controller)
    {
        if (controller == ARAVRInput.Controller.RTouch)
        {
            return ARAVRInput.RHandPosition;
        }

        return ARAVRInput.LHandPosition;
    }

    private Vector3 GetHandDirection(ARAVRInput.Controller controller)
    {
        if (controller == ARAVRInput.Controller.RTouch)
        {
            return ARAVRInput.RHandDirection;
        }

        return ARAVRInput.LHandDirection;
    }

    IEnumerator LineOff(LineRenderer lr)
    {
        while (lr.endWidth > 0)
        {
            lr.startWidth = Mathf.Lerp(lr.startWidth, 0, Time.deltaTime *2);
            lr.endWidth = Mathf.Lerp(lr.endWidth, 0, Time.deltaTime * 2);
            yield return null;
        }
    }
}
