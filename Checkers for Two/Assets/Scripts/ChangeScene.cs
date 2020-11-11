using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    void OnMouseUp()
    {
        Console.WriteLine("Test");
        SceneManager.LoadScene("MainGameScreen");
    }
}
