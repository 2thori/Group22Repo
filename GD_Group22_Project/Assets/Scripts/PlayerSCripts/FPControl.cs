using UnityEngine;

public class FPControl : MonoBehaviour
{
    public Rigidbody rbPlayer;

    private void Awake()
    {
        rbPlayer = GetComponent<Rigidbody>();

    }

    public void Jump()
    { 
        rbPlayer.AddForce(Vector3.up * 6f, ForceMode.Impulse);
     
    }

    
}
