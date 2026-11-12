using UnityEngine;
using UnityEngine.UI;

// Fits the authored Image to the canvas without stretching the supplied artwork.
[ExecuteAlways, RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
public sealed class MenuBackground : MonoBehaviour
{
    private Image image;
    private AspectRatioFitter fitter;
    private Sprite previousSprite;

    private void OnEnable()
    {
        image = GetComponent<Image>();
        fitter = GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        Refresh();
    }
    private void Update() { if (image != null && image.sprite != previousSprite) Refresh(); }
    private void Refresh()
    {
        previousSprite = image.sprite;
        fitter.aspectRatio = previousSprite == null ? 16f / 9f : previousSprite.rect.width / Mathf.Max(1f, previousSprite.rect.height);
    }
}
