using UnityEngine;
using LightSide;
using System.Collections;

/// <summary>
/// Компонент отдельного уведомления. Отвечает за отображение и авто-удаление.
/// </summary>
public class NotificationItem : MonoBehaviour
{
    [SerializeField] private UniText _messageText;
    [SerializeField] private RectTransform _background;

    private float _duration;
    private float _elapsedTime;

    public void Initialize(string message, Color color, float duration)
    {
        _duration = duration;
        _elapsedTime = 0f;

        if (_messageText != null)
        {
            _messageText.Text = message;
            _messageText.color = color;
        }

        if (_background != null)
        {
            // Полупрозрачный фон
            Color bgColor = new Color(color.r, color.g, color.b, 0.8f);
            _background.GetComponent<UnityEngine.UI.Image>().color = bgColor;
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        // Ждем основное время отображения
        yield return new WaitForSeconds(_duration - 0.5f);

        // Плавное исчезновение за 0.5 секунды
        float fadeDuration = 0.5f;
        float elapsed = 0f;

        if (_background != null)
        {
            UnityEngine.UI.Image bgImage = _background.GetComponent<UnityEngine.UI.Image>();
            Color startColor = bgImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                bgImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                
                if (_messageText != null)
                {
                    Color textColor = _messageText.color;
                    _messageText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        Destroy(gameObject);
    }
}
