using System.Threading;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;
    private GameObject heldObj;
    public float pickUpRange = 5f;
    private float rotationSensitivity = 1f;
    private Rigidbody heldObjRb;

    private bool canThrow = true;
    private int LayerNumber;

  

    private void Start()
    {
        LayerNumber = LayerMask.NameToLayer("holdLayer");
        
    }

    private void Update()
    {
        
    }
}
