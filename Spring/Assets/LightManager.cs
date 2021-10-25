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


    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
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

        if (DirectionLight != null)
        {
            DirectionLight.color = Preset.DirectionalColor.Evaluate(timePrecent);
            DirectionLight.transform.localRotation = Quaternion.Euler(new Vector3((timePrecent * 360f) - 90f, 170f, 0));
        }

    }
    private void OnValidate()
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

}
