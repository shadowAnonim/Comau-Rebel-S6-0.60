using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public RobotControl robot;

    [SerializeField]
    private GameObject errorText;
    [SerializeField]
    private TMP_InputField[] coordsInputs;

    public void BtnClick()
    {
        var pos = new Vector3(
            Convert.ToSingle(coordsInputs[1].text) * 0.001f,
            Convert.ToSingle(coordsInputs[2].text) * 0.001f,
            Convert.ToSingle(coordsInputs[0].text) * 0.001f);
        if (robot.CheckLimits(pos))
        {
            robot.TargetPosition = pos;
            errorText.SetActive(false);
        }
        else
        {
            errorText.SetActive(true);
        }
    }
}
