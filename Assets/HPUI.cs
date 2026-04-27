using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour
{
    public Transform followTarget;
    public Image hpImage;
    public Vector3 localOffset = new Vector3(0.2f, -0.1f, 0.25f);
    public Vector3 localEuler = new Vector3(0f, 180f, 0f);
    private Tower tower;

    private void Awake()
    {
        tower = FindObjectOfType<Tower>();
    }

    private void Update()
    {
        if (ARAVRInput.GetDown(ARAVRInput.Button.Two, ARAVRInput.Controller.LTouch))
        {
            gameObject.transform.GetChild(0).gameObject.SetActive(!gameObject.transform.GetChild(0).gameObject.activeSelf);
        }

        if (gameObject.transform.GetChild(0).gameObject.activeSelf)
        {
            UpdatePose();
        }

        hpImage.fillAmount = tower.HP / tower.initialHP;
    }

    

    

    private void UpdatePose()
    {

        Transform target = followTarget != null ? followTarget : ARAVRInput.LHand;
        if (target == null)
        {
            return;
        }

        transform.SetParent(target, false);
        transform.localPosition = localOffset;
        transform.localEulerAngles = localEuler;
    }
}
