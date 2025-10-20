using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Chemical Recipe", menuName = "Gravity Gun/Chemical Recipe")]
public class ChemicalRecipe : ScriptableObject
{
    [Serializable]
    public class ChemicalRequirement
    {
        public ChemicalItem chemical;
        public int amount = 1;
    }
    
    [Header("Recipe Requirements")]
    public ChemicalRequirement[] requiredChemicals;
    public ApparatusItem[] requiredApparatus;
    
    [Header("Mixing Process")]
    public float mixTime = 5f;
    public Color reactionColor = Color.blue;
    
    [Header("Output")]
    public string resultingChemicalName;
}