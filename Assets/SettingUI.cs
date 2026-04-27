using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public Transform followTarget;
    public Vector3 localOffset = new Vector3(0.2f, -0.1f, 0.25f);
    public Vector3 localEuler = new Vector3(0f, 180f, 0f);
    public Button continuButton;
    public Button exitButton;
    
    private void Awake()
    {
        continuButton.onClick.AddListener(OnContinueClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnContinueClicked()
    {
        Time.timeScale = 1;  
        gameObject.transform.GetChild(0).gameObject.SetActive(!gameObject.transform.GetChild(0).gameObject.activeSelf);
    }

    private void OnExitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        UnityEngine.Application.Quit();
        #endif
    }

    private void Update()
    {
        if (ARAVRInput.GetDown(ARAVRInput.Button.Start, ARAVRInput.Controller.LTouch))
        {
            if (Time.timeScale == 1)
            {
                Time.timeScale = 0;
            }else
            {
                Time.timeScale = 1;       
            }
            gameObject.transform.GetChild(0).gameObject.SetActive(!gameObject.transform.GetChild(0).gameObject.activeSelf);
        }

        if (gameObject.transform.GetChild(0).gameObject.activeSelf)
        {
            UpdatePose();
        }

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
