using UnityEngine;

public class UVTestMosquito : MonoBehaviour
{
    bool isExternallyControlled;
    float speed = 1f;
    Rigidbody mosquitoBody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mosquitoBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isExternallyControlled)
        {
            return;
        }
        Vector3 nextPos = mosquitoBody.position;
        Vector3 movement = transform.forward * speed * Time.fixedDeltaTime;
        nextPos = mosquitoBody.position + movement;
        mosquitoBody.MovePosition(nextPos);
    }

    public void BeginExternalControl()
    {
        isExternallyControlled = true;
    }

    public void EndExternalControl()
    {
        isExternallyControlled = false;
    }

}
