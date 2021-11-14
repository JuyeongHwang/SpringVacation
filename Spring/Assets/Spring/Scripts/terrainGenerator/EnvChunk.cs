using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvChunk : MonoBehaviour
{
    // ============================= 마우스 클릭 추가 ==========================================

    private void OnMouseDown ()
    {
        // 스크린에서 윌드 위치로
        RaycastHit hit;
        Vector3 pos = Vector3.zero;
        float clickDist = 100;
                
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, clickDist)) {
            pos = hit.point;
        }

        if (MyGameManager_Gameplay.Inst != null)
        {
            MyGameManager_Gameplay.Inst.ClickFromTerrain (pos);
        }
    }
}
