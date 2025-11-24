
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuCanvas; // Canvas de la portada
    public GameObject gameCanvas;     // Canvas del juego (UI del ShellGame)

    void Start()
    {
        // Mostrar portada y ocultar el juego al inicio
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (gameCanvas != null) gameCanvas.SetActive(false);
    }

    public void OnPlayClicked()
    {
        // Oculta portada y muestra el juego
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (gameCanvas != null) gameCanvas.SetActive(true);
    }

    public void OnExitClicked()
    {
        Application.Quit();
        Debug.Log("Salir del juego...");
    }
}

