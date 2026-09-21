using TMPro;
using UnityEngine;

public class Money : Coin
{
    [SerializeField] TextMeshProUGUI numberUI;
    [SerializeField] TextMeshProUGUI numberUI1;
    public override void SetColorType(ColorType colorType)
    {
        base.SetColorType(colorType);

        MeshRenderer.sharedMaterial = colorType.MoneyMaterial;
        numberUI.text = colorType.name;
        numberUI1.text = colorType.name;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(MeshRenderer);
        UnityEditor.EditorUtility.SetDirty(numberUI);
        UnityEditor.EditorUtility.SetDirty(numberUI1);
#endif
    }
    
    
}
