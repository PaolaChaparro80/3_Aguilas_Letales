using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels (NO Canvas)")]
    public GameObject mainMenuPanel;  // Panel de la portada (Jugar/Salir)
    public GameObject gamePanel;      // Panel del juego (HUD, botones, etc.)

    [Header("Game Manager")]
    public ShellGameManager shellGameManager; // Referencia al manager del juego

    void Start()
    {
        // Al iniciar: mostrar menú y ocultar juego
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public void OnPlayClicked()
    {
        Debug.Log("Desactivando MainMenuPanel...");

        // Ocultar el panel del menú
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        // Activar el panel del juego
        if (gamePanel != null) gamePanel.SetActive(true);

        // Inicializar el juego manualmente
        if (shellGameManager != null)
        {
            shellGameManager.InitializeGame();
        }
        else
        {
            Debug.LogError("ShellGameManager no está asignado en el Inspector!");
        }

        Debug.Log("Juego iniciado");
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Salir del juego (Editor)...");
#else
        Application.Quit(); // En WebGL no hace nada, pero en PC sí
#endif
    }
}