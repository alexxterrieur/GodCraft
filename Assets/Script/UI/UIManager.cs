using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject mapGenWindow;
    [SerializeField] private GameObject gameUi;
    [SerializeField] private GameObject currentWindowOpened;
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

    public void OpenWindow(GameObject windowToOpen)
    {
        currentWindowOpened.SetActive(false);
        windowToOpen.SetActive(true);
        currentWindowOpened = windowToOpen;
    }
}
