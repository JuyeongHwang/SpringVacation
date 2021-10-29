using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct bugInfo
{
    public string bugName;//tag
    public float hp;
    public bugInfo(string name, float _hp)
    {
        bugName = name;
        hp = _hp;
    }
}
public class bugController : MonoBehaviour
{
    [HideInInspector]
    public bugInfo bug;

    public Slider hpBar;

    // Start is called before the first frame update
    void Start()
    {
        bug.bugName = "butterfly";
        bug.hp = 100;

        hpBar.value = bug.hp / 100;
    }

    private void Update()
    {
        hpBar.value = bug.hp / 100;
    }
}
