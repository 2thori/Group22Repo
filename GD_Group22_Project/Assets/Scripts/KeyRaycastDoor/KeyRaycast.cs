using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KeySystem
{
    public class KeyRaycast : MonoBehaviour
    {
        [SerializeField] private int rayLength = 5;
        [SerializeField] private LayerMask layerMaskInteract;
        [SerializeField] private string excluseLayerName = null;
        
        //private KeyItemController raycastedObject;
    }


}