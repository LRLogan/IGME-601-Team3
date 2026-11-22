using System;
using UnityEngine;

public class Window : MonoBehaviour
{
    public event Action OnMosquitoPassed;
    public event Action<Window, MosquitoAgent> OnMosquitoExited;

    public void OnTriggerEnter(Collider collider)
    {
        MosquitoAgent mosquito = collider.GetComponentInParent<MosquitoAgent>();
        if (mosquito == null || !mosquito.CompareTag("Mosquito") || !mosquito.gameObject.activeInHierarchy)
            return;

        MosquitoHealth health = mosquito.GetComponent<MosquitoHealth>();
        if (health != null)
            health.Despawn();
        else
        {
            mosquito.ResetAgent();
            mosquito.gameObject.SetActive(false);
        }

        OnMosquitoExited?.Invoke(this, mosquito);
        OnMosquitoPassed?.Invoke();
    }
}
