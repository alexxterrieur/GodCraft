using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mapGenWindow;
    [SerializeField] private GameObject gameUi;
    public GameObject closeWindowButton;

    void Start()
    {
        mapGenWindow.SetActive(true);
        gameUi.SetActive(false);
        closeWindowButton.SetActive(false);
    }

    public void EnableGameUi()
    {
        mapGenWindow.SetActive(false);
        gameUi.SetActive(true);
    }
    
    public void EnableMapGenWindow()
    {
        gameUi.SetActive(false);
        mapGenWindow.SetActive(true);
    }
}
