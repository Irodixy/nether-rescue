using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicialScript : MonoBehaviour
{
    public void Jogar()
    {
        //SceneTransitionController.Instance.LoadScene("TestingTutorialScene");
        SceneManager.LoadScene("TutorialScene");
    }

    public void Sair()
    {
        #if UNITY_EDITOR
            //Application.Quit() não funciona no editor por isso alteramos o
            // valor de Unity.isPlaying para falso para interromper a execução do jogo 
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
