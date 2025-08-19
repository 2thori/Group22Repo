using UnityEngine;

[CreateAssetMenu(fileName = "Key", menuName = "Scriptable Object/Key")]
public class Key : ScriptableObject
{
    public string keyName;
    public int id;
    public Sprite keySprite;
}
