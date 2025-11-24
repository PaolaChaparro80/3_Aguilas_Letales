
using UnityEngine;

public class BoxSelector : MonoBehaviour
{
    public int index; // Índice único para cada caja
    private ShellGameManager manager;

    void Start()
    {
        manager = FindObjectOfType<ShellGameManager>();
    }

    // Usamos colisión física en lugar de trigger
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && manager != null)
        {
            manager.SelectBox(index);
        }
    }
}

