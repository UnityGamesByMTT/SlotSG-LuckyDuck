using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using System;

public class ManageLineButtons : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler, IPointerUpHandler,IPointerDownHandler
{

	internal int num;
	[SerializeField] internal TMP_Text num_text;

	internal Action<int> GenerateLine;
	internal Action DestroyLine;


	public void OnPointerEnter(PointerEventData eventData)
	{

			GenerateLine?.Invoke(num);
			// slotManager.GenerateStaticLine(num_text);
	}
	public void OnPointerExit(PointerEventData eventData)
	{

			DestroyLine?.Invoke();
			// slotManager.DestroyStaticLine();
	}
	public void OnPointerDown(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			this.gameObject.GetComponent<Button>().Select();
			// slotManager.GenerateStaticLine(num_text);
			GenerateLine?.Invoke(num);

		}
	}
	public void OnPointerUp(PointerEventData eventData)
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && Application.isMobilePlatform)
		{
			//Debug.Log("run on pointer up");
			// slotManager.DestroyStaticLine();
			DestroyLine?.Invoke();

			DOVirtual.DelayedCall(0.1f, () =>
			{
				this.gameObject.GetComponent<Button>().spriteState = default;
				EventSystem.current.SetSelectedGameObject(null);
			 });
		}
	}
}
