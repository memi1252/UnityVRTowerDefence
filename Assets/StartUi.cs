using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUi : MonoBehaviour
{
    public Text text;

    private float time = 5;
    private void Update()
    {
        time -= Time.deltaTime;
        text.text = $"{(int)time +1}초후 자동시작";
        if (time < 0)
        {
            gameObject.SetActive(false);
        }
    }
}
