using UnityEngine;
using System.Collections.Generic;

public class KeyInventory : MonoBehaviour
{
   [SerializeField] private List<int> keyIds = new List<int>();
   
   public static KeyInventory Instance { get; private set; }

   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
         DontDestroyOnLoad(gameObject);
      }
      else
      {
         
         Destroy(gameObject);
         
      }
   }

   public void AddKey(Key key)
   {
      if (!keyIds.Contains(key.id))
      {
         keyIds.Add(key.id);
         Debug.Log($"key added:{key.keyName} (ID:{key.id}");
      }
   }

   public bool Haskey(Key key)
   {
      return keyIds.Contains(key.id);
   }
}
