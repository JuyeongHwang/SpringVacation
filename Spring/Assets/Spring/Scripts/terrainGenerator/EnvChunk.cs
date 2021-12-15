using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvChunk : MonoBehaviour
{
    private myDel_Terrain myDel;
    // ============================= 마우스 클릭 추가 ==========================================

    public void SetMyDelTerrain (myDel_Terrain mdt)
    {
        myDel = mdt;
    }

    private void OnMouseDown ()
    {
        // 스크린에서 윌드 위치로
        RaycastHit hit;
        Vector3 pos = Vector3.zero;
        float clickDist = 100;
                
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, clickDist)) {
            pos = hit.point;
        }

        // 키보드 인풋에 따라 다른 동작
        if (Input.GetKey (KeyCode.A))
        {
            if (myDel != null)
            {
                //myDel.UpDownTerrain(false, hit);

                if (MyGameManager_Gameplay.Inst != null)
                {
                    MyGameManager_Gameplay.Inst.InstantiateEffectByIndex (3, hit.point, Quaternion.identity);
                }
            }
        }
        else if (Input.GetKey (KeyCode.D))
        {
            if (myDel != null)
            {
                //myDel.UpDownTerrain(true, hit);

                if (MyGameManager_Gameplay.Inst != null)
                {
                    MyGameManager_Gameplay.Inst.InstantiateEffectByIndex (3, hit.point, Quaternion.identity);
                }
            }
        }
        else
        {
            if (MyGameManager_Gameplay.Inst != null)
            {
                MyGameManager_Gameplay.Inst.ClickFromTerrain (pos);

                // 이펙트
                MyGameManager_Gameplay.Inst.InstantiateEffectByIndex (0, hit.point, Quaternion.identity);
            }
        }
    }
}
