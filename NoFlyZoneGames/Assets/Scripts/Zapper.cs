using UnityEngine;
using System.Collections;

public class Zapper : MonoBehaviour, IItemEffect
{
    private bool effectActive = true;
    [SerializeField]
    private float damage = 25f;

    [SerializeField]
    private float cooldown;

    [SerializeField]
    private TrapAudioFeedback audioFeedback;

    private MosquitoHealth health;

    private Coroutine zapTime;

    private void Awake()
    {
        if (audioFeedback == null)
            audioFeedback = GetComponent<TrapAudioFeedback>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!effectActive)
        {
            return;
        }
        MosquitoHealth mosquitoHealth = other.GetComponentInParent<MosquitoHealth>();
        if (mosquitoHealth != null && mosquitoHealth.CompareTag("Mosquito"))
        {
            health = mosquitoHealth;
            
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
        if (other.GetComponentInParent<MosquitoHealth>() == health)
        {
            StopZapping();
        }
    }

    private void OnDisable()
    {
        StopZapping();
    }

    private void StopZapping()
    {
        if (zapTime != null) StopCoroutine(zapTime);
        zapTime = null;
        health = null;
    }

    public IEnumerator ApplyZap(float cool)
    {
        MosquitoHealth target = health;

        while (target != null &&
            target.gameObject.activeInHierarchy &&
            !target.IsDead &&
            effectActive)
        {
            if (audioFeedback != null)
                audioFeedback.PlayOneShot();

            target.TakeDamage(damage);

            if (target.IsDead ||
                !target.gameObject.activeInHierarchy)
                break;

            yield return new WaitForSeconds(cool);
        }

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
        if (!active) StopZapping();
    }

}
