using UnityEngine;
using UnityEngine.UI;

public class DamageFlashUI : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashAlpha = 0.35f;
    [SerializeField] private float fadeSpeed = 6f;

    private float current;

    void Update()
    {
        current = Mathf.MoveTowards(current, 0f, Time.deltaTime * fadeSpeed);

        if (flashImage != null)
        {
            var c = flashImage.color;
            c.a = current;
            flashImage.color = c;
        }
    }

    public void Trigger()
    {
        current = flashAlpha;
    }
}
