using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public bool triggerOnHoldAction = false;
    public bool isDown;

    internal Action SpinAction;
    internal Action AutoSpinACtion;
    public float holdTime = 0f;
    [SerializeField] internal Button btn;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!btn.interactable)
            return;
        isDown = true;
        StartCoroutine(HoldCheckRoutine());
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        if (!btn.interactable)
            return;
        isDown = false;
        if (triggerOnHoldAction)
        {
            triggerOnHoldAction=false;
            return;
        }
            SpinAction.Invoke();
        Debug.Log("up");


    }

    IEnumerator HoldCheckRoutine()
    {
        holdTime=0;
        while (isDown)
        {
            holdTime += Time.deltaTime;
            if (holdTime >= 2f)
            {
                AutoSpinACtion.Invoke();
                Debug.Log("sdsdsds");
                triggerOnHoldAction = true;
                yield break;
            }
            yield return null;
        }
    }

}
