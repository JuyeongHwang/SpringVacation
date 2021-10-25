using UnityEngine;

[ExecuteAlways]
public class LightManager : MonoBehaviour
{
    [SerializeField]
    private Light DirectionLight;

    [SerializeField]
    private LightPreset Preset;

    [SerializeField, Range(0, 24)]
    private float TimeOfDay;

    public Material riverMat;


    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * 24f / MyGameManager_Gameplay.Inst.gameplayDuration;
            TimeOfDay %= 24; //Clamp between 0-24
            UpdateLighting(TimeOfDay / 24f);
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    private void UpdateLighting(float timePrecent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePrecent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePrecent);

        float startX = -0f;
        float endX = 200f;

        if (DirectionLight != null)
        {
            float curtX = Mathf.Lerp (startX, endX, timePrecent);

            DirectionLight.color = Preset.DirectionalColor.Evaluate(timePrecent);
            DirectionLight.transform.localRotation = Quaternion.Euler(new Vector3(curtX, 170f, 0));

            if (riverMat != null)
            {
                riverMat.SetFloat ("Vector1_ff3c4a1f6b0941508044adf34c334e48", 1 - Mathf.Sin (Mathf.PI * timePrecent));
            }
        }

    }
    /*private void OnValidate()
    {
        if (DirectionLight != null)
            return;
        if(RenderSettings.sun != null)
        {
            DirectionLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light light in lights)
            {
                if(light.type == LightType.Directional)
                {
                    DirectionLight = light;
                    return;
                }
            }
        }

    }
*/
}
