using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Bug Information", menuName = "Scriptables/Bug Information")]
public class bugInfo : ScriptableObject
{
    [Header ("곤충 기본 설정")]
    [SerializeField]
    private string bugName;
    
    [SerializeField]
    private float bugHP;

    [Header ("곤충 이동 설정")]
    [SerializeField]
    private float bugMoveSpeed_min;
    [SerializeField]
    private float bugMoveSpeed_max;
    
    [SerializeField]
    private float bugMoveDist_min;
    [SerializeField]
    private float bugMoveDist_max;

    [SerializeField]
    private float bugMoveDelay_min;
    [SerializeField]
    private float bugMoveDelay_max;

    [SerializeField]
    private float bugRotSpeed = 0.5f;

    [SerializeField]
    private float bugFlyingDistFromGround = 0.75f;

    public string GetBugName ()
    {
        return bugName;
    }

    public float GetBugHP ()
    {
        return bugHP;
    }

    public float GetBugMoveSpeed ()
    {
        return Random.Range (bugMoveSpeed_min, bugMoveSpeed_max);
    }

    public float GetBugMoveDistance ()
    {
        return Random.Range (bugMoveDist_min, bugMoveDist_max);
    }

    public float GetBugMoveDelay ()
    {
        return Random.Range (bugMoveDelay_min, bugMoveDelay_max);
    }

    public float GetBugRotationSpeed ()
    {
        return bugRotSpeed;
    }

    public float GetBugFlyingDistanceFromGround ()
    {
        return bugFlyingDistFromGround;
    }
}
