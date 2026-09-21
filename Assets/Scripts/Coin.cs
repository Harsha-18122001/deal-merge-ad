//using Dreamteck.Splines;

using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] ColorType coinType;
    [SerializeField] MeshRenderer meshRenderer;
    Material material;
    //SplineFollower SplineFollower;
    public ColorType ColorType => coinType;

    public MeshRenderer MeshRenderer { get => meshRenderer; }

    [SerializeField] private List<Sprite> sprites = new();
    [SerializeField] private List<SpriteRenderer> spriteRenderers = new();
    protected virtual void Start()
    {
        //SplineFollower = GetComponent<SplineFollower>();
        SetColorType(coinType);
    }

    /*public void StartMoving(SplineComputer splineComputer, float time)
    {
        if (SplineFollower == null) SplineFollower = GetComponent<SplineFollower>();
        SplineFollower.spline = splineComputer;
        // Disable auto start position if it's enabled
        SplineFollower.autoStartPosition = false;

        // Set the spline
        SplineFollower.spline = splineComputer;

        // Set the start position along the spline
        SplineFollower.startPosition = time;
        SplineFollower.SetPercent(time);
        SplineFollower.motion.offset = new Vector2(0, CreativeSettings.Instance.CardHalfHeight);
        SplineFollower.enabled = true;
        GetComponent<BoxCollider>().enabled = true;
        //SplineFollower.motion.rotationOffset = new Vector3(0, Random.Range(0, 360), 0);
    }
    public void StopMoving()
    {
        if (SplineFollower == null)
            SplineFollower = GetComponent<SplineFollower>();
        SplineFollower.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
    }*/

    public virtual void SetColorType(ColorType colorType)
    {
        this.coinType = colorType;
        MeshRenderer.sharedMaterial = coinType.CoinMaterial;
        
        int index = CreativeSettings.Instance.GetColorIndex(colorType);

        Debug.Log($"ColorType: {colorType.name}");
        Debug.Log($"Index: {index}");

        if (index < 0)
        {
            Debug.LogError($"'{colorType.name}' was not found in CreativeSettings.colorTypes");
            return;
        }

        Debug.Log($"Sprite Assigned: {sprites[index].name}");

        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                //sr.sprite = sprites[index];
                sr.color = colorType.SpriteColor;
        }
        
        /*Color tint = Color.white;

        Material mat = colorType.MoneyMaterial;

        if (mat.HasProperty("_BaseColor"))
            tint = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color"))
            tint = mat.GetColor("_Color");

        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
                sr.color = tint;
        }*/

        
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.EditorUtility.SetDirty(MeshRenderer);
#endif
    }
    public void SetMaterial(Material material)
    {
        MeshRenderer.sharedMaterial = material;
    }

    internal void HighLight(bool highlight)
    {
        if (material == null)
        {
            material = MeshRenderer.material;
        }

        // Toony Shader Pro outline width control
        if (material.HasProperty("_OutlineWidth"))
        {
            material.SetFloat("_OutlineWidth", highlight ? 5f : 0.0f); // 1.5f is a typical visible width, adjust as needed
        }
        else
        {
            Debug.LogWarning("Material does not have an _OutlineWidth property.");
        }
    }

}
