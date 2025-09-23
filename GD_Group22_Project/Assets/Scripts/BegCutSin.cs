using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BegCutSin : MonoBehaviour
{
     void OnEnable()
    {
        //only specifying the scene name will load the scene with the single mode 
        SceneManager.LoadScene("GreyboxScene", LoadSceneMode.Single);

    }
}
