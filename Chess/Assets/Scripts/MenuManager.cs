using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void Play(bool botMode)
    {
        BoardManager.botMode = botMode;
        SceneManager.LoadScene(1);
    }
}
