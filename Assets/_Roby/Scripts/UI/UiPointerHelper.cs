using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UiPointerHelper
{
    public static bool IsFingerOverImage(Image image, PointerEventData eventData)
    {
        if (image == null || eventData == null)
            return false;

        Canvas canvas = image.canvas;
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            image.rectTransform,
            eventData.position,
            cam);
    }
}
