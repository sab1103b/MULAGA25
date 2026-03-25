using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerModel model;

    bool canTakeDamage = true;

    public float interactRange = 2f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // El enemigo SIEMPRE muere
            EnemyLife enemy = other.GetComponent<EnemyLife>();
            if (enemy != null)
            {
                enemy.Die();
            }

            // El jugador solo recibe daño si puede
            if (canTakeDamage)
            {
                canTakeDamage = false;

                Debug.Log("El jugador fue golpeado");

                model.TakeDamage(1);

                Invoke("ResetDamage", 3f);
            }
        }
    }

    void ResetDamage()
    {
        canTakeDamage = true;
    }
    /*
    void Update()
    {
        // Botón del control VR (trigger derecho o tecla E para pruebas)
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("BOTON PRESIONADO");
            TryInteract();
        }
    }

    void TryInteract()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange);

        foreach (Collider hit in hits)
        {
            PosterFragment fragment = hit.GetComponent<PosterFragment>();

            if (fragment != null)
            {
                fragment.TryCollect();
                return;
            }
        }
    }
    */
}