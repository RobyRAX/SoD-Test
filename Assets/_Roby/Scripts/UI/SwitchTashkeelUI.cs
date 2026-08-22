using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SwitchTashkeelUI : MonoBehaviour
{
    [SerializeField]
    Image unequipImg;

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

    public void OnPointerUp_Handler(BaseEventData eventData)
    {
        if (playerUnit == null || image == null)
            return;

        var pointer = eventData as PointerEventData;

        if (UiPointerHelper.IsFingerOverImage(unequipImg, pointer))
        {
            playerUnit.Unequip();
            Debug.Log("Unequip Tashkeel Triggered by UI (drop from Switch)");
            return;
        }

        if (UiPointerHelper.IsFingerOverImage(image, pointer))
        {
            playerUnit.SwitchToNextTashkeel();
            Debug.Log("Switch Tashkeel Triggered by UI");
        }
    }
}
