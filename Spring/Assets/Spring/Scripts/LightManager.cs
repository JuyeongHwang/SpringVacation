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
    public Material skyMat;
    protected float dayDuration;

    //[Header ("컬러")]
    //public Color lightColor_white;
    //public Color lightColor_red;

    public bool lightActive = true;

    void Start ()
    {
        if (lightActive == true)
        {
            // 초기화
            TimeOfDay = 0f;
        }
        
        if (DataManager.Inst != null)
        {
            dayDuration =  DataManager.Inst.GetDataPreset ()
            .DATAINFORMATIONS [DataManager.Inst.GetLevelIndex ()].DAYDURATION;
        }
    }

    private void Update()
    {
        if (Preset == null)
            return;

        if (lightActive == false)
        {
            UpdateLighting(TimeOfDay / 24f);
            return;
        }
            
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime * 24f / dayDuration;
            TimeOfDay = Mathf.Min (TimeOfDay, 24); //Clamp between 0-24
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

        }

        if (riverMat != null)
        {
            riverMat.SetFloat ("Vector1_ff3c4a1f6b0941508044adf34c334e48", 1 - Mathf.Sin (Mathf.PI * timePrecent));
        }

        if (skyMat != null)
        {
            skyMat.SetFloat ("Vector1_9d7170f9fdcf4156b7056caa61dbcca7", timePrecent);
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
