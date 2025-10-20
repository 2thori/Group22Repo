using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ApparatusSlotUI : MonoBehaviour
{
    [SerializeField] private Image apparatusIcon;
    [SerializeField] private TMP_Text apparatusNameText;
    [SerializeField] private Image backgroundImage;
    
    public void SetApparatus(ApparatusItem apparatus)
    {
        if (apparatusIcon != null && apparatus != null)
        {
            apparatusIcon.sprite = apparatus.icon;
        }
        
        if (apparatusNameText != null)
        {
            apparatusNameText.text = apparatus.apparatusName;
        }
        
        // Optional: Change background color when collected
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.green;
        }
    }
}