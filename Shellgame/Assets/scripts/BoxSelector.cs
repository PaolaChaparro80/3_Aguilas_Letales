using UnityEngine;

public class BoxSelector : MonoBehaviour
{
    public int index;
    private ShellGameManager manager;

    void Start()
    {
        manager = FindObjectOfType<ShellGameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && manager != null)
        {
            manager.SelectBox(index);
        }
    }
}



