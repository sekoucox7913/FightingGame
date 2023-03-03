using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeManager : MonoBehaviour
{
	public GameObject Panel_Fade;
	public static FadeManager _Instance;
	private void Awake()
	{
		if (_Instance == null)
		{
			_Instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
		Hide_Fade();
	}

	public void Show_Fade(string text = "")
	{
		if (routine_fade != null)
		{
			StopCoroutine(routine_fade);
		}
		routine_fade = StartCoroutine(process_fade(true));
		Panel_Fade.GetComponentInChildren<UnityEngine.UI.Text>().text = text;
	}

	public void Hide_Fade()
	{
		if (routine_fade != null)
		{
			StopCoroutine(routine_fade);
		}
		routine_fade = StartCoroutine(process_fade(false));
	}

	Coroutine routine_fade;
	private IEnumerator process_fade(bool isShow)
	{
		float alpha = Panel_Fade.GetComponent<CanvasGroup>().alpha;
		if (isShow)
		{
			while (alpha < 1f)
			{
				alpha += Time.deltaTime * 2f;
				Panel_Fade.GetComponent<CanvasGroup>().alpha = alpha;
				Panel_Fade.GetComponent<CanvasGroup>().interactable = true;
				Panel_Fade.GetComponent<CanvasGroup>().blocksRaycasts = true;
				yield return null;
			}
		}
		else
		{
			while (alpha > 0f)
			{
				alpha -= Time.deltaTime;
				Panel_Fade.GetComponent<CanvasGroup>().alpha = alpha;
				Panel_Fade.GetComponent<CanvasGroup>().interactable = true;
				Panel_Fade.GetComponent<CanvasGroup>().blocksRaycasts = true;
				yield return null;
			}
			Panel_Fade.GetComponent<CanvasGroup>().interactable = false;
			Panel_Fade.GetComponent<CanvasGroup>().blocksRaycasts = false;
		}
	}
}
