using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShellGameManager : MonoBehaviour
{
    [Header("Cajas")]
    public List<Transform> boxes;

    [Header("Monedas (10)")]
    public List<GameObject> coins;

    [Header("Shuffle Settings")]
    public float shuffleSpeed = 3f;
    public int shuffleCount = 5;

    [Header("Intentos")]
    public int maxAttempts = 3;
    private int attemptsLeft;

    [Header("Jugador")]
    public Transform player;
    private Vector3 playerStartPos;
    private Quaternion playerStartRot;
    private Rigidbody playerRb;

    [Header("Sonidos")]
    public AudioClip winSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    [Header("UI")]
    public TMPro.TextMeshProUGUI resultText;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI hintText;

    [Header("Botones Opciones")]
    public GameObject btnContinue;
    public GameObject btnRestart;
    public GameObject btnExit;

    private bool canSelect = false;
    private int winningBoxIndex;
    private int score = 0;
    private int totalWins = 0;

    void Awake()
    {
        if (player != null)
        {
            playerStartPos = player.position;
            playerStartRot = player.rotation;
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    void Start()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // -------------------------------------------------------------
    // Inicialización cuando se activa GamePanel
    // -------------------------------------------------------------
    public void InitializeGame()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateScoreUI();
        StopAllCoroutines();
        StartCoroutine(StartGame());
    }

    // -------------------------------------------------------------
    // INICIO DEL JUEGO / RONDA COMPLETA (con engaño de caja)
    // -------------------------------------------------------------
    IEnumerator StartGame()
    {
        HideOptions();
        ClearResultUI();
        canSelect = false;
        attemptsLeft = maxAttempts;

        // Primero devuelve al jugador al inicio
        yield return StartCoroutine(ResetPlayerSmooth());

        // Step 1: Elegir una caja aleatoria donde se mostrarán inicialmente las monedas
        int initialBox = Random.Range(0, boxes.Count);
        Transform initialTargetBox = boxes[initialBox];

        // Step 2: Animar monedas hacia esa caja “visible”
        yield return StartCoroutine(AnimateCoinsToBox(initialTargetBox));
        yield return StartCoroutine(RevealCoins(2f));
        SetCoinsVisible(false);

        // Step 3: Decidir la caja ganadora real (diferente de la inicial)
        do
        {
            winningBoxIndex = Random.Range(0, boxes.Count);
        } while (winningBoxIndex == initialBox);

        // Step 4: Barajar las cajas
        yield return StartCoroutine(ShuffleBoxes());

        canSelect = true;
        ShowHint($"Intentos: {attemptsLeft}. Elige una caja...");
    }

    // ✅ FUNCIÓN PARA REINICIAR POSICIÓN DEL JUGADOR
    IEnumerator ResetPlayerSmooth()
    {
        Collider col = player != null ? player.GetComponent<Collider>() : null;
        if (col != null) col.enabled = false;

        if (player != null)
        {
            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
                playerRb.MovePosition(playerStartPos);
                playerRb.MoveRotation(playerStartRot);
            }
            else
            {
                player.position = playerStartPos;
                player.rotation = playerStartRot;
            }
        }

        yield return new WaitForSeconds(0.1f);
        if (col != null) col.enabled = true;
    }

    // -------------------------------------------------------------
    // ANIMACIÓN DE MONEDAS
    // -------------------------------------------------------------
    IEnumerator AnimateCoinsToBox(Transform target)
    {
        foreach (GameObject coin in coins)
        {
            coin.SetActive(true);
            coin.transform.position = new Vector3(Random.Range(-2f, 2f), Random.Range(2f, 4f), Random.Range(-2f, 2f));
            coin.transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
        }

        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            foreach (GameObject coin in coins)
            {
                Vector3 targetPos = target.position + Vector3.up * 0.8f;
                coin.transform.position = Vector3.Lerp(coin.transform.position, targetPos, t);
                coin.transform.Rotate(Vector3.up * 360f * Time.deltaTime);
            }

            yield return null;
        }

        ArrangeCoinsOnBox(target);
    }

    void ArrangeCoinsOnBox(Transform target)
    {
        float baseY = 0.95f;
        int gridCols = 4;
        float gridStep = 0.16f;
        float jitterXY = 0.06f;

        for (int i = 0; i < coins.Count; i++)
        {
            GameObject coin = coins[i];
            coin.transform.SetParent(target);

            int row = i / gridCols;
            int col = i % gridCols;

            Vector3 offsetGrid = new Vector3((col - (gridCols - 1) * 0.5f) * gridStep, 0f, (row - 1) * gridStep);
            Vector3 jitter = new Vector3(Random.Range(-jitterXY, jitterXY), Random.Range(0f, 0.03f), Random.Range(-jitterXY, jitterXY));

            coin.transform.localPosition = new Vector3(0, baseY, 0) + offsetGrid + jitter;
            coin.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb == null) rb = coin.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            Collider colliderComponent = coin.GetComponent<Collider>();
            if (colliderComponent == null) colliderComponent = coin.AddComponent<SphereCollider>();
            colliderComponent.isTrigger = false;
        }
    }

    IEnumerator RevealCoins(float revealTime)
    {
        SetCoinsVisible(true);
        ShowHint("Observa dónde están las monedas...");
        yield return new WaitForSeconds(revealTime);
        ClearResultUI();
    }

    void SetCoinsVisible(bool visible)
    {
        foreach (GameObject coin in coins)
        {
            coin.SetActive(visible);
        }
    }

    IEnumerator ShuffleBoxes()
    {
        for (int i = 0; i < shuffleCount; i++)
        {
            int a = Random.Range(0, boxes.Count);
            int b = Random.Range(0, boxes.Count);
            while (b == a) b = Random.Range(0, boxes.Count);

            Transform boxA = boxes[a];
            Transform boxB = boxes[b];

            Vector3 posA = boxA.position;
            Vector3 posB = boxB.position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * shuffleSpeed;
                boxA.position = Vector3.Lerp(posA, posB, t);
                boxB.position = Vector3.Lerp(posB, posA, t);
                yield return null;
            }

            (boxes[a], boxes[b]) = (boxes[b], boxes[a]);

            if (winningBoxIndex == a) winningBoxIndex = b;
            else if (winningBoxIndex == b) winningBoxIndex = a;

            yield return new WaitForSeconds(0.2f);
        }
    }

    // -------------------------------------------------------------
    // SELECCIONAR CAJA
    // -------------------------------------------------------------
    public void SelectBox(int index)
    {
        if (!canSelect) return;
        canSelect = false;

        bool won = (index == winningBoxIndex);
        if (won)
        {
            totalWins++;
            score += coins.Count;
            UpdateScoreUI();
            ShowResult(true);
            ShowHint($"¡Ganaste! Aciertos totales: {totalWins}");
            PlaySound(winSound);
            ShowOptions(false);
        }
        else
        {
            attemptsLeft--;
            if (attemptsLeft > 0)
            {
                ShowHint($"Fallaste. Intentos restantes: {attemptsLeft}. Presiona 'Continuar' para intentar de nuevo.");
                ShowOptions(true);
            }
            else
            {
                ShowResult(false);
                ShowHint($"Perdiste. Aciertos totales: {totalWins}");
                PlaySound(loseSound);
                ShowOptions(false);
            }
        }
    }

    // -------------------------------------------------------------
    // BOTONES
    // -------------------------------------------------------------
    public void OnContinueClicked()
    {
        HideOptions();

        if (attemptsLeft > 0 && !canSelect)
        {
            StartCoroutine(ResetPlayerSmooth());
            canSelect = true;
            ShowHint($"Intentos restantes: {attemptsLeft}. Elige una caja...");
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(StartGame());
        }
    }

    public void OnRestartClicked()
    {
        HideOptions();
        totalWins = 0;
        score = 0;
        UpdateScoreUI();
        StopAllCoroutines();
        StartCoroutine(StartGame());
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Salir del juego (Editor)...");
#else
        Application.Quit();
#endif
    }

    // -------------------------------------------------------------
    // UI y utilidades
    // -------------------------------------------------------------
    void ShowOptions(bool canContinue)
    {
        if (btnContinue != null) btnContinue.SetActive(canContinue);
        if (btnRestart != null) btnRestart.SetActive(true);
        if (btnExit != null) btnExit.SetActive(true);
    }

    void HideOptions()
    {
        if (btnContinue != null) btnContinue.SetActive(false);
        if (btnRestart != null) btnRestart.SetActive(false);
        if (btnExit != null) btnExit.SetActive(false);
    }

    void ShowHint(string message)
    {
        if (hintText != null) hintText.text = message;
    }

    void ShowResult(bool won)
    {
        if (resultText != null)
        {
            resultText.text = won ? "¡Correcto!" : "Incorrecto";
            resultText.color = won ? Color.green : Color.red;
        }
    }

    void ClearResultUI()
    {
        if (resultText != null) resultText.text = "";
        if (hintText != null) hintText.text = "";
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"Puntaje: {score}";
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.PlayOneShot(clip);
        }
    }
}
