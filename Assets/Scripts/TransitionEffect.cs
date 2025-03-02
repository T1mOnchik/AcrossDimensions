using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionEffect : MonoBehaviour
{
    
    [SerializeField]private Image whitePanel;
    
    [Header("Настройки анимации")]
    [SerializeField] private float closeDuration = 1f;  // Время закрытия
    [SerializeField] private float openDuration = 1f;   // Время открытия
    [SerializeField] private float holdTime = 0.2f;

    private void Awake()
    {
        // Убедимся, что тип изображения правильно выставлен (на случай, если забыли в инспекторе)
        if (whitePanel)
        {
            whitePanel.type = Image.Type.Filled;
            whitePanel.fillMethod = Image.FillMethod.Vertical;
            whitePanel.fillOrigin = 0;       // 0 = снизу вверх
            whitePanel.fillAmount = 0f;      // Изначально полностью «прозрачно» (нет белого)
        }
    }
    
    public void StartTransition()
    {
        StartCoroutine(TransitionCoroutine());
    }
    
    
    
    private IEnumerator TransitionCoroutine()
    {
        if (!whitePanel)
        {
            yield break;
        }
    
        // 1. Закрываем экран (fillAmount: 0 -> 1)
        float time = 0f;
        while (time < closeDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / closeDuration);
            whitePanel.fillAmount = progress; 
            yield return null;
        }
        whitePanel.fillAmount = 1f;  // гарантируем, что дошли до конца
    
        // 2. Небольшая пауза «полного белого»
        yield return new WaitForSeconds(holdTime);
    
        // 3. Открываем экран обратно (fillAmount: 1 -> 0)
        time = 0f;
        while (time < openDuration)
        {
            time += Time.deltaTime;
            float progress = Mathf.Clamp01(time / openDuration);
            whitePanel.fillAmount = 1f - progress; 
            yield return null;
        }
        whitePanel.fillAmount = 0f;  // всё открыто
    }
}
