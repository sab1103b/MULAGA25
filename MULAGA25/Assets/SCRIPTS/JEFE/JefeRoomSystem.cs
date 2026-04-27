using UnityEngine;

public class JefeRoomSystem : MonoBehaviour
{
    [Header("Orbes")]
    public Transform orbe1;
    public Transform orbe2;
    public Transform orbe3;

    [Header("Llaves")]
    public Transform llave1;
    public Transform llave2;
    public Transform llave3;

    [Header("Puerta")]
    public Transform puerta;
    public float doorMoveDistance = 5f;
    public float doorSpeed = 2f;

    [Header("Detección")]
    public float activationDistance = 0.5f;

    private Vector3 puertaInitialPos;
    private Vector3 puertaOpenPos;

    private bool isOpen = false;
    private bool permanentlyLocked = false;

    void Start()
    {
        puertaInitialPos = puerta.position;
        puertaOpenPos = puertaInitialPos + Vector3.down * doorMoveDistance;
    }

    void Update()
    {
        // SOLO puede abrir si no está bloqueado permanentemente
        if (!isOpen && !permanentlyLocked)
        {
            bool orbe1Correct = Vector3.Distance(orbe1.position, llave1.position) < activationDistance;
            bool orbe2Correct = Vector3.Distance(orbe2.position, llave2.position) < activationDistance;
            bool orbe3Correct = Vector3.Distance(orbe3.position, llave3.position) < activationDistance;

            if (orbe1Correct && orbe2Correct && orbe3Correct)
            {
                isOpen = true;
            }
        }

        if (isOpen)
        {
            puerta.position = Vector3.Lerp(
                puerta.position,
                puertaOpenPos,
                doorSpeed * Time.deltaTime
            );
        }
        else
        {
            puerta.position = Vector3.Lerp(
                puerta.position,
                puertaInitialPos,
                doorSpeed * Time.deltaTime
            );
        }
    }

    public void CerrarPuerta()
    {
        isOpen = false;
        permanentlyLocked = true;
    }
}