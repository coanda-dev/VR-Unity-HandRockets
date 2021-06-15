using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


/// <summary>
/// The XR Rig component is typically attached to the base object of the XR Rig,
/// and stores the <see cref="GameObject"/> that will be manipulated via locomotion.
/// It is also used for offsetting the camera.
///
/// This extended rig offers additional functionality, such as access to the motion
/// controller references.
/// </summary>
[AddComponentMenu("XR/XR Extended Rig")]
[DisallowMultipleComponent]
public class ExtendedXRRig : XRRig
{
    [SerializeField] private GameObject m_LeftHandController;

    public GameObject LeftHandController
    {
        get => m_LeftHandController;
        set => m_LeftHandController = value;
    }

    [SerializeField] private GameObject m_RightHandController;

    public GameObject RightHandController
    {
        get => m_RightHandController;
        set => m_RightHandController = value;
    }
}