using UnityEngine;

[CreateAssetMenu(fileName = "CreativeSettings")]
public class CreativeSettings : ScriptableObject
{
    private static CreativeSettings instance;
    public static CreativeSettings Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<CreativeSettings>("CreativeSettings");
            return instance;
        }
    }

    [SerializeField] Coin coinPrefab;
    [SerializeField] float mergeDuration = 2;
    [SerializeField] private float highlightOffset = 0.1f;
    [SerializeField] private float highlightInterval = .05f;
    [SerializeField] private float cardHalfHeight = 0.1f;
    [SerializeField] private float jumpDuration = .8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int dealSpawnCount = 3;
    [SerializeField] ColorType[] colorTypes;
    [SerializeField] private float cardJumpHeight = 2f;
    [SerializeField] float cardJumpDuration = 0.3f;
    [SerializeField] float cardJumpInterval = 0.1f;
    public ColorType[] ColorTypes => colorTypes;
    public float MergeDuration { get => mergeDuration; }
    public Coin CoinPrefab { get => coinPrefab; }
    public float HighlightOffset { get => highlightOffset; }
    public float HighlightInterval { get => highlightInterval; }
    public float CardHalfHeight { get => cardHalfHeight; }
    public float JumpDuration { get => jumpDuration; }
    public float JumpHeight { get => jumpHeight; }
    public int DealSpawnCount { get => dealSpawnCount; }
    public float CardJumpHeight { get => cardJumpHeight; }
    public float CardJumpDuration { get => cardJumpDuration; }
    public float CardJumpInterval { get => cardJumpInterval; }
    public ColorType[] SpawnableColors => spawnableColors;
    public ColorType[] AllColors => allColors;
    public int MinAdjacentCount => minAdjacentCount;
    public int MaxAdjacentCount => maxAdjacentCount;
    [SerializeField] private int minAdjacentCount = 2;
    [SerializeField] private int maxAdjacentCount = 3;
    [SerializeField] private ColorType[] spawnableColors;
    [SerializeField] private ColorType[] allColors;
    public int GetColorIndex(ColorType colorType)
    {
        for (int i = 0; i < allColors.Length; i++)
        {
            if (allColors[i] == colorType)
                return i;
        }

        return -1;
    }
    public ColorType GetRandomColorType()
    {
        return spawnableColors[Random.Range(0, spawnableColors.Length)];
    }
    /*public ColorType GetRandomColorType()
    {
        return colorTypes[Random.Range(0, colorTypes.Length)];
    }*/
    
    /*public int GetColorIndex(ColorType colorType)
    {
        for (int i = 0; i < colorTypes.Length; i++)
        {
            if (colorTypes[i] == colorType)
                return i;
        }

        return -1;
    }*/
}