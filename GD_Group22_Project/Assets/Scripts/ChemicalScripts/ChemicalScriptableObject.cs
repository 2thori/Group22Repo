using UnityEngine;

[CreateAssetMenu(fileName = "New Chemical", menuName = "Gravity Gun/Chemical")]
public class ChemicalItem : ScriptableObject
{
    public string chemicalName;
    public Sprite icon;
    public Color liquidColor = Color.white;
    public GameObject physicalItemPrefab;
}

[CreateAssetMenu(fileName = "New Apparatus", menuName = "Gravity Gun/Apparatus")]
public class ApparatusItem : ScriptableObject
{
    public string apparatusName;
    public Sprite icon;
    public GameObject physicalItemPrefab;
}
