using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// ジェネリックを隠すために継承してしまう
/// [System.Serializable]を書くのを忘れない
/// </summary>
[System.Serializable]
public class RotationTable : Serialize.TableBase<GameObject, bool, RotationPair>
{


}

/// <summary>
/// ジェネリックを隠すために継承してしまう
/// [System.Serializable]を書くのを忘れない
/// </summary>
[System.Serializable]
public class RotationPair : Serialize.KeyAndValue<GameObject, bool>
{

    public RotationPair(GameObject key, bool value) : base(key, value)
    {

    }
}

public class FingerForce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RotationTable servos;
    public float angleRate = 90f;
    private Slider slider;
    private bool zeroForce = false;
    private bool changeable = false;


    void Start()
    {
        slider = GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (changeable)
        {
            foreach (KeyValuePair<GameObject, bool> pair in servos.GetTable())
            {
                GameObject itemObject = pair.Key;
                if (pair.Value)
                {
                    itemObject.transform.eulerAngles = new Vector3(0f, 0f, -1 * slider.value * angleRate);
                }
                else
                {
                    itemObject.transform.eulerAngles = new Vector3(0f, 0f, slider.value * angleRate);
                }
            }
        }

        if (zeroForce)
        {
            float currentValue = slider.value;
            if(changeable) slider.value = Mathf.Lerp(currentValue, 0f, 0.25f);
            if ((slider.value <= 0.001f) && (slider.value >= -0.001f))
            {
                slider.value = 0f;
                changeable = false;
                zeroForce = false;
            }
        }
    }
    // ドラックが開始したとき呼ばれる.
    public void OnPointerDown(PointerEventData eventData)
    {
        changeable = true;
        zeroForce = false;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        zeroForce = true;
    }
}
