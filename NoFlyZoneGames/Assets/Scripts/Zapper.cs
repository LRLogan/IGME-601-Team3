using UnityEngine;
using System.Collections;

public class Zapper : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    [SerializeField]
    private float damage = 25f;

    [SerializeField]
    private float cooldown;

    private MosquitoHealth health;

    private Coroutine zapTime;

    private void OnTriggerEnter(Collider other)
    {
        if (!effectActive)
        {
            return;
        }
        if (other.CompareTag("Mosquito"))
        {
            health = other.GetComponent<MosquitoHealth>();
            
            //Start coroutine
            if(zapTime == null)
            {
                zapTime = StartCoroutine(ApplyZap(cooldown));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Good for our purposes now BUT will pose a problem with multiple mosquitoes
        if(other.CompareTag("Mosquito"))
        {
            StopCoroutine(zapTime);

            //make sure to set the coroutine to null to prevent multiple threads
            zapTime = null;
        }
    }

    public IEnumerator ApplyZap(float cool)
    {
        //Applies damage until it hits zero
        while (health != null)
        {
            health.TakeDamage(damage);

            yield return new WaitForSeconds(cool);
        }
        //make sure to set the coroutine to null to prevent multiple threads
        zapTime = null;
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