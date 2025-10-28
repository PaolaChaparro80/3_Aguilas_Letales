using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShellGameManager : MonoBehaviour
{
    [Header("Cajas")]
    public List<Transform> boxes;  // Las cajas del juego

    [Header("Monedas")]
    public List<GameObject> coins; // Todas las monedas (10)

    [Header("Configuración de Shuffle")]
    public float shuffleSpeed = 3f;
    public int shuffleCount = 5;

    [Header("Sonidos")]
    public AudioClip winSound;
    public AudioClip loseSound;
    private AudioSource audioSource;

    private bool canSelect = false;
    private int winningBoxIndex;  // Índice de la caja ganadora

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(StartGame());
    }

    void AssignCoinsToBoxes()
    {
        // Seleccionar la caja ganadora de forma aleatoria
        winningBoxIndex = Random.Range(0, boxes.Count); // Índice de la caja ganadora
        Transform targetBox = boxes[winningBoxIndex];

        // Asegurarse de que las monedas estén activas antes de comenzar el juego
        foreach (GameObject coin in coins)
        {
            coin.SetActive(true);  // Activar las monedas

            coin.transform.SetParent(targetBox);
            coin.transform.localPosition = Vector3.zero; // Colocar las monedas en el centro de la caja

            // Agregar Rigidbody y Collider si no existen
            if (coin.GetComponent<Rigidbody>() == null)
                coin.AddComponent<Rigidbody>().isKinematic = true;

            if (coin.GetComponent<Collider>() == null)
                coin.AddComponent<SphereCollider>();
        }

        Debug.Log("Las monedas han sido asignadas a las cajas.");
    }

    IEnumerator StartGame()
    {
        AssignCoinsToBoxes(); // Asignar las monedas antes de la mezcla
        yield return new WaitForSeconds(1f);  // Esperar antes de comenzar la mezcla
        yield return StartCoroutine(ShuffleBoxes());  // Comienza la mezcla
        canSelect = true;
        Debug.Log("¡Elige una caja!");
    }

    IEnumerator ShuffleBoxes()
    {
        for (int i = 0; i < shuffleCount; i++)
        {
            int a = Random.Range(0, boxes.Count);
            int b = Random.Range(0, boxes.Count);
            while (b == a) b = Random.Range(0, boxes.Count);

            Vector3 posA = boxes[a].position;
            Vector3 posB = boxes[b].position;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * shuffleSpeed;
                boxes[a].position = Vector3.Lerp(posA, posB, t);
                boxes[b].position = Vector3.Lerp(posB, posA, t);
                yield return null;
            }

            // Intercambiar las cajas (referencias)
            (boxes[a], boxes[b]) = (boxes[b], boxes[a]);
            yield return new WaitForSeconds(0.5f);  // Esperar entre mezclas
        }
    }

    public void SelectBox(int index)
    {
        if (!canSelect) return;
        canSelect = false;

        bool won = (index == winningBoxIndex);  // Verificar si el jugador seleccionó la caja ganadora

        for (int i = 0; i < boxes.Count; i++)
        {
            foreach (Transform coin in boxes[i])
            {
                bool isSelected = (i == index);
                coin.gameObject.SetActive(isSelected);  // Mostrar monedas solo si la caja es seleccionada

                // Si la caja tiene monedas y es seleccionada, lanzarlas
                if (isSelected && won)
                {
                    Rigidbody rb = coin.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = false;  // Activar física para lanzar
                        Vector3 randomDir = new Vector3(
                            Random.Range(-1f, 1f),
                            Random.Range(1.5f, 2.5f),
                            Random.Range(-1f, 1f)
                        ).normalized;
                        rb.AddForce(randomDir * Random.Range(3f, 6f), ForceMode.Impulse);
                        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
                    }
                }
                else
                {
                    // Si no se ha ganado, desactivar las monedas de la caja no seleccionada
                    coin.gameObject.SetActive(false);
                }
            }
        }

        // Reproducir sonido y mostrar el mensaje según el resultado
        if (won)
        {
            Debug.Log("🎉 ¡Ganaste! Las monedas vuelan 🎉");
            if (winSound != null) audioSource.PlayOneShot(winSound);  // Reproducir sonido de victoria
        }
        else
        {
            Debug.Log("❌ Caja vacía.");
            if (loseSound != null) audioSource.PlayOneShot(loseSound);  // Reproducir sonido de derrota
        }

        // Reiniciar el juego después de un breve momento
        StartCoroutine(RestartGame());
    }

    public IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2f);  // Esperar un momento antes de reiniciar el juego

        // Desactivar monedas y restablecer físicas
        foreach (GameObject coin in coins)
        {
            coin.SetActive(false);
            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }

        // Volver a asignar las monedas a las cajas antes de la próxima ronda
        StartCoroutine(StartGame());
    }
}













