using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Google.Protobuf.WellKnownTypes;

public class CameraControls : MonoBehaviour
{
    Transform cam;
    [SerializeField] private CinemachineFreeLook cineCam;
    [SerializeField] [Range(0.5f, 5)] float cameraZoomMultiplier = 1;

    public void CCStart()
    {
        UISettings.Singleton.onMouseSensValueChanged += ChangeMouseSensitivity;
        
        cineCam = FindObjectOfType<CinemachineFreeLook>();
        MouseSensOnStart();

        cam = Camera.main.transform;

    }

    public void SwitchCameraMovementControl()
    {
        cineCam.m_Orbits[0].m_Height = Mathf.Clamp((cineCam.m_Orbits[0].m_Height - Input.mouseScrollDelta.y), 2, 20 * cameraZoomMultiplier);
        cineCam.m_Orbits[1].m_Radius = Mathf.Clamp((cineCam.m_Orbits[1].m_Radius - Input.mouseScrollDelta.y), 2, 21 * cameraZoomMultiplier);
        cineCam.m_Orbits[2].m_Height = Mathf.Clamp((cineCam.m_Orbits[2].m_Height + Input.mouseScrollDelta.y), -20 * cameraZoomMultiplier, -2);
        //cam.m_Orbits[2].m_Radius -= Input.mouseScrollDelta.y / 2;

        if (Input.GetMouseButton(1))
        {
            cineCam.m_YAxis.m_InputAxisName = "Mouse Y";
            cineCam.m_XAxis.m_InputAxisName = "Mouse X";

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            cineCam.m_YAxis.m_InputAxisName = string.Empty;
            cineCam.m_XAxis.m_InputAxisName = string.Empty;

            cineCam.m_YAxis.m_InputAxisValue = 0;
            cineCam.m_XAxis.m_InputAxisValue = 0;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void MouseSensOnStart()
    {
        float value = PlayerPrefs.GetFloat("Mouse Sensitivity");

        cineCam.m_YAxis.m_MaxSpeed = value * 10f;
        cineCam.m_XAxis.m_MaxSpeed = value * 1500f;
    }

    void ChangeMouseSensitivity(object sender, MouseSenSettings e)
    {
        cineCam.m_YAxis.m_MaxSpeed = e.sensValue * 10f;
        cineCam.m_XAxis.m_MaxSpeed = e.sensValue * 1500f;
    }
}
