using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Bug Information", menuName = "Scriptables/Bug Information")]
public class bugInfo : ScriptableObject
{
    [SerializeField]
    private string bugName;
    
    [SerializeField]
    private float bugHP;

    public string GetBugName ()
    {
        return bugName;
    }

    public float GetBugHP ()
    {
        return bugHP;
    }
}
