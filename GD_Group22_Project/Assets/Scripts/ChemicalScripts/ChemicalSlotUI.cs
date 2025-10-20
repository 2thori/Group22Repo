using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChemicalSlotUI : MonoBehaviour
{
    [SerializeField] private Image chemicalIcon;
    [SerializeField] private TMP_Text chemicalNameText;
    [SerializeField] private TMP_Text quantityText;
    
    public void SetChemical(ChemicalItem chemical, int quantity)
    {
        if (chemicalIcon != null && chemical != null)
        {
            chemicalIcon.sprite = chemical.icon;
            chemicalIcon.color = chemical.liquidColor;
        }
        
        if (chemicalNameText != null)
        {
            chemicalNameText.text = chemical.chemicalName;
        }
        
        if (quantityText != null)
        {
            quantityText.text = $"x{quantity}";
        }
    }
}