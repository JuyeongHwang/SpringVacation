using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvObjectType
{
    TREE, FLOWER
}

public class EnvObject : MonoBehaviour
{
    public EnvObjectType envObjectType;

    void Start ()
    {
        if (EnvManager.Inst != null)
        {
            EnvManager.Inst.AddEnvObject (this);
        }
    }

    public EnvObjectType GetEnvObjectType ()
    {
        return envObjectType;
    }
}

