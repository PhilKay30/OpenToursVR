using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider slider;
    public GameObject OverlayMap;

    public void UpdateRotation()
    {
        OverlayMap.transform.rotation = Quaternion.Euler(0, slider.value * 360, 0);
    }
}
