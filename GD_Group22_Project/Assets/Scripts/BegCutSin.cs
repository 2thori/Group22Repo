using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BegCutSin : MonoBehaviour
{
    void OnEnable()
    {
        //specifying scene name will loa the scene with the single mode
        SceneManager.LoadScene("GrayBoxScene", LoadSceneMode.Single);
    }
}
