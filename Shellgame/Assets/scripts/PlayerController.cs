
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;           // Asegura que no sea cinemático
        rb.useGravity = true;             // Activa gravedad
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Evita atravesar objetos
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Movimiento en plano XZ
        Vector3 movement = new Vector3(h, 0, v).normalized * moveSpeed * Time.fixedDeltaTime;

        // Movimiento físico
        rb.MovePosition(rb.position + movement);
    }
}


