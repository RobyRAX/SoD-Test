using UnityEngine;
using UnityEngine.UI;

public class SwitchTashkeelUI : MonoBehaviour
{
    PlayerUnitController playerUnit;
    Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void Setup(PlayerUnitController playerUnit)
    {
        this.playerUnit = playerUnit;
    }

    public void OnPointerUp_Handler()
    {
        if (//finger on top of image)
            // switch to next tashkeel
    }
}
