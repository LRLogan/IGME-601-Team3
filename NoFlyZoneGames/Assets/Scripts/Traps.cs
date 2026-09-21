using UnityEngine;

[CreateAssetMenu(fileName = "Traps", menuName = "Scriptable Objects/Traps")]
public class Traps : ScriptableObject
{
    [SerializeField]
    private string name;
    [SerializeField]
    private float radius;
}
