using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeySystem
{
    public class KeyItemController : MonoBehaviour
    {
        [SerializeField] private bool orangeDoor = false;
        [SerializeField] private bool orangeKey = false;

        [SerializeField] private KeyInventory _keyInventory = null;

        private KeyDoorController doorObject;

        private void start()
        {
            if (orangeDoor)
            {
                doorObject = GetComponent<KeyDoorController>();
            }
        }

        public void ObjectInteraction()
        {
            if (orangeDoor)
            {
                doorObject.PlayAnimation();
            }
            else if (orangeKey)
            {
                _keyInventory.hasOrangeKey = true;
                gameObject.SetActive(false);
            }
        }
    }
}