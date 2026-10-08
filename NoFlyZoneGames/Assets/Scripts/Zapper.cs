using UnityEngine;

public class Zapper : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    [SerializeField]
    private float damage = 25f;

    private void OnTriggerEnter(Collider other)
    {
        if (!effectActive)
        {
            return;
        }
        if (other.CompareTag("Mosquito"))
        {
            MosquitoHealth health = other.GetComponent<MosquitoHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    public void PickUp(Transform holder)
    {
        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
    }

    public void Place(Vector3 position)
    {
        transform.SetParent(null);
        transform.position = position;
    }

    public void SetEffectActive(bool active)
    {
        if (effectActive == active)
        {
            return;
        }

        effectActive = active;
    }

}