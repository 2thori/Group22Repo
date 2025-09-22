using UnityEngine;

[CreateAssetMenu(fileName = "GravityGunPart", menuName = "Scriptable Object/Gravity Gun Part")]
public class GravityGunPart : ScriptableObject
{
    public string partName;
    public int partId;
    public Sprite partSprite;
    public string description;
}