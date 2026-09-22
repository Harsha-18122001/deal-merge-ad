using UnityEngine;
using DG.Tweening;

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

    [Header("General")]
    [SerializeField] Coin coinPrefab;
    [SerializeField] float mergeDuration = 2;
    [SerializeField] private float highlightOffset = 0.1f;
    [SerializeField] private float highlightInterval = .05f;
    [SerializeField] private float cardHalfHeight = 0.1f;

    [Header("Deal Jump Animation (Spawning from Deal Tray)")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = .8f;
    [SerializeField] private int dealJumpNumJumps = 1;
    [SerializeField] private Ease dealJumpEase = Ease.Linear;
    [SerializeField] private float dealJumpInterval = 0.05f;
    [SerializeField] private bool enableDealJumpRotation = false;
    [SerializeField] private Vector3 dealJumpRotation = Vector3.zero;
    [SerializeField] private RotateMode dealJumpRotateMode = RotateMode.FastBeyond360;
    [SerializeField] private Ease dealJumpRotationEase = Ease.Linear;
    [SerializeField] private int dealSpawnCount = 3;

    [Header("Transfer Jump Animation (Between Trays)")]
    [SerializeField] private float cardJumpHeight = 2f;
    [SerializeField] private int cardJumpNumJumps = 1;
    [SerializeField] private bool useFixedCardJumpDuration = false;
    [SerializeField] private float fixedCardJumpDuration = 0.35f;
    [Tooltip("If useFixedCardJumpDuration is false, jump duration = Mathf.Sqrt(distance) / cardJumpDuration")]
    [SerializeField] float cardJumpDuration = 10f;
    [SerializeField] private Ease cardJumpEase = Ease.Linear;
    [SerializeField] float cardJumpInterval = 0.035f;
    [SerializeField] private bool enableCardJumpRotation = true;
    [SerializeField] private Vector3 cardJumpRotation = new Vector3(180, 0, 180);
    [SerializeField] private RotateMode cardJumpRotateMode = RotateMode.FastBeyond360;
    [SerializeField] private Ease cardJumpRotationEase = Ease.InSine;

    [Header("Colors & Spawning")]
    [SerializeField] ColorType[] colorTypes;
    [SerializeField] private int minAdjacentCount = 2;
    [SerializeField] private int maxAdjacentCount = 3;
    [SerializeField] private ColorType[] spawnableColors;
    [SerializeField] private ColorType[] allColors;

    // Public Getters
    public Coin CoinPrefab => coinPrefab;
    public float MergeDuration => mergeDuration;
    public float HighlightOffset => highlightOffset;
    public float HighlightInterval => highlightInterval;
    public float CardHalfHeight => cardHalfHeight;

    // Deal Jump Getters
    public float JumpHeight => jumpHeight;
    public float JumpDuration => jumpDuration;
    public int DealJumpNumJumps => dealJumpNumJumps;
    public Ease DealJumpEase => dealJumpEase;
    public float DealJumpInterval => dealJumpInterval;
    public bool EnableDealJumpRotation => enableDealJumpRotation;
    public Vector3 DealJumpRotation => dealJumpRotation;
    public RotateMode DealJumpRotateMode => dealJumpRotateMode;
    public Ease DealJumpRotationEase => dealJumpRotationEase;
    public int DealSpawnCount => dealSpawnCount;

    // Transfer Jump Getters
    public float CardJumpHeight => cardJumpHeight;
    public int CardJumpNumJumps => cardJumpNumJumps;
    public bool UseFixedCardJumpDuration => useFixedCardJumpDuration;
    public float FixedCardJumpDuration => fixedCardJumpDuration;
    public float CardJumpDuration => cardJumpDuration;
    public Ease CardJumpEase => cardJumpEase;
    public float CardJumpInterval => cardJumpInterval;
    public bool EnableCardJumpRotation => enableCardJumpRotation;
    public Vector3 CardJumpRotation => cardJumpRotation;
    public RotateMode CardJumpRotateMode => cardJumpRotateMode;
    public Ease CardJumpRotationEase => cardJumpRotationEase;

    // Colors Getters
    public ColorType[] ColorTypes => colorTypes;
    public ColorType[] SpawnableColors => spawnableColors;
    public ColorType[] AllColors => allColors;
    public int MinAdjacentCount => minAdjacentCount;
    public int MaxAdjacentCount => maxAdjacentCount;

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
}
