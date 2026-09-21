using UnityEngine;

[CreateAssetMenu]
public class ColorType : ScriptableObject
{
    [SerializeField] Material coinMaterial;
    [SerializeField] Material moneyMaterial;
    [SerializeField] Material targetMaterial;
    [SerializeField] Material fxMaterial;
    [SerializeField] Material targetHighlight;
    public Material TargetMaterial { get => targetMaterial; }
    public Material CoinMaterial { get => coinMaterial; }
    public Material FxMaterial { get => fxMaterial; }
    public Material TargetHighlight { get => targetHighlight; }
    [field: SerializeField] public ColorType NextColor { get; internal set; }
    public Material MoneyMaterial { get => moneyMaterial; }
    
    [SerializeField] private Color spriteColor = Color.white;
    public Color SpriteColor => spriteColor;
}
