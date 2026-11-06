using TMPro;
using UnityEngine;

public class WaveTimer : MonoBehaviour
{
    [SerializeField]
    private PlayerHUDController textControl;

    private int waveNum = 0;

    float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveNum = 1;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        textControl.SetCountdown(timer);


        
    }

    public void BeginWave()
    {

    }
}
