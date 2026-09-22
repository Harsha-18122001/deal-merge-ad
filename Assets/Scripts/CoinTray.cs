using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinTray : MonoBehaviour
{
    static List<CoinTray> allTrays = new();
    [SerializeField] List<ColorType> colorTypes;
    [SerializeField] List<Coin> coins;
    public List<Coin> Coins => coins;
    public List<ColorType> ColorTypes => colorTypes;
    public Transform StartPosition => startPosition;
    public Vector3 StackDirection => stackDirection;
    public Vector3 EulerRotation => eulerRotation;
    public float StackHeightOffset => stackHeightOffset;
    public int MaxCapacity => maxCapacity;
    [SerializeField] Transform startPosition;
    [SerializeField] Vector3 stackDirection, eulerRotation;
    [SerializeField] float stackHeightOffset = 0.1f;
    [SerializeField] float gizmoSphereRadius = 0.1f;
    [SerializeField] int maxCapacity = 12;
    [SerializeField] int minMergeCount = 6;
    [SerializeField] bool autoMerge = false;
    bool isMerging = false;
    [SerializeField] private SpriteRenderer lockSpriteRenderer;
    [SerializeField] private SpriteRenderer mergeOutline;

    [Header("Jump Animation Override")]
    [Tooltip("If checked, this tray will use its own jump settings instead of CreativeSettings")]
    [SerializeField] private bool overrideJumpSettings = false;

    [Header("Transfer Jump (Coins moving to this tray)")]
    [SerializeField] private float transferJumpHeight = 2f;
    [SerializeField] private int transferNumJumps = 1;
    [SerializeField] private bool useFixedTransferDuration = false;
    [SerializeField] private float fixedTransferDuration = 0.35f;
    [Tooltip("If useFixedTransferDuration is false, duration = Mathf.Sqrt(distance) / transferSpeedDivider")]
    [SerializeField] private float transferSpeedDivider = 10f;
    [SerializeField] private Ease transferJumpEase = Ease.Linear;
    [SerializeField] private float transferCoinInterval = 0.035f;
    [SerializeField] private bool enableTransferRotation = true;
    [SerializeField] private Vector3 transferRotation = new Vector3(180, 0, 180);
    [SerializeField] private RotateMode transferRotateMode = RotateMode.FastBeyond360;
    [SerializeField] private Ease transferRotationEase = Ease.InSine;

    [Header("Deal Jump (If this tray deals coins)")]
    [SerializeField] private float dealJumpHeight = 2f;
    [SerializeField] private float dealJumpDuration = 0.3f;
    [SerializeField] private int dealNumJumps = 1;
    [SerializeField] private Ease dealJumpEase = Ease.Linear;
    [SerializeField] private float dealCoinInterval = 0.05f;
    [SerializeField] private bool enableDealRotation = false;
    [SerializeField] private Vector3 dealRotation = Vector3.zero;
    [SerializeField] private RotateMode dealRotateMode = RotateMode.FastBeyond360;
    [SerializeField] private Ease dealRotationEase = Ease.Linear;

    public bool OverrideJumpSettings { get => overrideJumpSettings; set => overrideJumpSettings = value; }

    public float GetTransferJumpHeight() => overrideJumpSettings ? transferJumpHeight : CreativeSettings.Instance.CardJumpHeight;
    public int GetTransferNumJumps() => overrideJumpSettings ? transferNumJumps : CreativeSettings.Instance.CardJumpNumJumps;
    public Ease GetTransferJumpEase() => overrideJumpSettings ? transferJumpEase : CreativeSettings.Instance.CardJumpEase;
    public float GetTransferInterval() => overrideJumpSettings ? transferCoinInterval : CreativeSettings.Instance.CardJumpInterval;
    public bool GetEnableTransferRotation() => overrideJumpSettings ? enableTransferRotation : CreativeSettings.Instance.EnableCardJumpRotation;
    public Vector3 GetTransferRotation() => overrideJumpSettings ? transferRotation : CreativeSettings.Instance.CardJumpRotation;
    public RotateMode GetTransferRotateMode() => overrideJumpSettings ? transferRotateMode : CreativeSettings.Instance.CardJumpRotateMode;
    public Ease GetTransferRotationEase() => overrideJumpSettings ? transferRotationEase : CreativeSettings.Instance.CardJumpRotationEase;

    public float CalculateTransferDuration(float distance)
    {
        bool useFixed = overrideJumpSettings ? useFixedTransferDuration : CreativeSettings.Instance.UseFixedCardJumpDuration;
        if (useFixed)
        {
            return overrideJumpSettings ? fixedTransferDuration : CreativeSettings.Instance.FixedCardJumpDuration;
        }
        else
        {
            float speedDiv = overrideJumpSettings ? transferSpeedDivider : CreativeSettings.Instance.CardJumpDuration;
            if (speedDiv <= 0.0001f) speedDiv = 1f;
            return Mathf.Sqrt(distance) / speedDiv;
        }
    }

    public float GetDealJumpHeight() => overrideJumpSettings ? dealJumpHeight : CreativeSettings.Instance.JumpHeight;
    public float GetDealJumpDuration() => overrideJumpSettings ? dealJumpDuration : CreativeSettings.Instance.JumpDuration;
    public int GetDealNumJumps() => overrideJumpSettings ? dealNumJumps : CreativeSettings.Instance.DealJumpNumJumps;
    public Ease GetDealJumpEase() => overrideJumpSettings ? dealJumpEase : CreativeSettings.Instance.DealJumpEase;
    public float GetDealInterval() => overrideJumpSettings ? dealCoinInterval : CreativeSettings.Instance.DealJumpInterval;
    public bool GetEnableDealRotation() => overrideJumpSettings ? enableDealRotation : CreativeSettings.Instance.EnableDealJumpRotation;
    public Vector3 GetDealRotation() => overrideJumpSettings ? dealRotation : CreativeSettings.Instance.DealJumpRotation;
    public RotateMode GetDealRotateMode() => overrideJumpSettings ? dealRotateMode : CreativeSettings.Instance.DealJumpRotateMode;
    public Ease GetDealRotationEase() => overrideJumpSettings ? dealRotationEase : CreativeSettings.Instance.DealJumpRotationEase;

    private MeshRenderer trayMesh;
    private void Awake()
    {
        trayMesh = transform.GetChild(0).GetComponent<MeshRenderer>();
    }
    private void UpdateMergeOutline()
    {
        if (mergeOutline == null)
            return;

        mergeOutline.gameObject.SetActive(CanMerge());
    }
    public void SetLocked(bool locked)
    {
        trayMesh.enabled = !locked;

        if (lockSpriteRenderer != null)
            lockSpriteRenderer.enabled = locked;
    }

    public bool IsLocked => lockSpriteRenderer != null && lockSpriteRenderer.enabled;
    public void Merge()
    {
        MergeAll();
    }

    public static void MergeAll()
    {
        foreach (var tray in allTrays)
        {
            if (tray != null && tray.CanMerge())
            {
                tray.MergeAndSpawn();
            }
        }
    }

    private void OnDestroy()
    {
        allTrays.Remove(this);
    }

    private void Start()
    {
        allTrays.Add(this);
        RepositionCoins();
        UpdateMergeOutline();
    }
    [EditorButton]
    private void RepositionCoins()
    {
        for (int i = 0; i < coins.Count; i++)
        {
            coins[i].transform.position = startPosition.position.GetStackedPosition(i, stackDirection, eulerRotation, stackHeightOffset);
        }
    }

    public bool CanMerge()
    {
        if (isMerging) return false;
        if (coins.Count < minMergeCount) return false;
        ColorType colorType = coins[^1].ColorType;
        int count = 0;
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i].ColorType != colorType) break;
            count++;
        }
        return count >= minMergeCount;
    }
    void MergeAndSpawn()
    {
        if (isMerging) return;
        isMerging = true;
        ColorType colorType = coins[^1].ColorType;
        Vector3 lastPos = Vector3.zero;
        int J = 0;
        float duration = 0.5f;
        float delay = duration / 10;
        int lastIndex = coins.Count - 1;
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            if (coins[i].ColorType != colorType) break;
            lastIndex = i;
        }
        var pos = GetStackedPosition(lastIndex);

        Sequence masterSeq = DOTween.Sequence();
        Sequence HighlightSeq = DOTween.Sequence();
        Sequence mergeSeq = DOTween.Sequence();
        Sequence rotationSeq = DOTween.Sequence();
        mergeSeq.AppendInterval(delay);

        for (int i = coins.Count - 1; i >= 0; i--)
        {
            Coin coin = null;
            Sequence rotationsSeq = DOTween.Sequence();

            if (coins[i].ColorType != colorType) break;
            coin = coins[i];
            HighlightSeq.Insert(
                CreativeSettings.Instance.HighlightInterval * J,
                coin.transform.DOPunchScale(new Vector3(0.15f, 0.05f, 0f), .3f, vibrato: 1)
                    .SetEase(Ease.Linear)
                    .OnKill(() =>
                    {
                        coin.transform.localScale = Vector3.one;
                    })
            );


            mergeSeq.Join(coin.transform.DOMove(pos, duration)
                .SetEase(Ease.InSine));

            rotationsSeq.Join(coin.transform.DORotate(new Vector3(0, 0, -13f), 0.05f).SetEase(Ease.OutBack));
            rotationsSeq.Append(coin.transform.DORotate(new Vector3(0, 0, 13f), 0.1f).SetEase(Ease.OutBack));
            rotationsSeq.Append(coin.transform.DORotate(new Vector3(0, 0, 0), 0.3f));
            rotationsSeq.Append(coin.transform.DOScale(new Vector3(1.2f, .8f, 1f), 0.5f)/*);
            rotationsSeq.Append(coin.transform.DOScale(new Vector3(1.2f, .8f, 1f), 0.5f)*/
                .OnComplete(() => coin.gameObject.SetActive(false)));

            lastPos = coin.transform.position;
            coins.Remove(coin);
            J++;
            rotationSeq.Join(rotationsSeq);
        }
        masterSeq.Append(HighlightSeq);
        masterSeq.Append(mergeSeq);
        masterSeq.Append(rotationSeq);
        masterSeq.AppendCallback(() =>
        {
            UpdateMergeOutline();
            if (colorType.NextColor != null)
            {
                Sequence popSeq = DOTween.Sequence();
                popSeq.AppendInterval(0.1f);
                int startIndex = coins.Count;
                for (int i = 0; i < 2; i++)
                {
                    Coin coin = Instantiate(original: CreativeSettings.Instance.CoinPrefab,
                                   position: GetStackedPosition(startIndex + i),
                                   rotation: Quaternion.Euler(eulerRotation),
                                   parent: transform);
                    popSeq.Join(coin.transform.DOScale(1, .2f).From(0).SetEase(Ease.OutBack));
                    coin.transform.localScale = Vector3.zero;
                    coin.SetColorType(colorType.NextColor);
                    RecieveCoin(coin);
                    isMerging= false;
                }
                masterSeq.Append(popSeq);
            }
        });
        //mergeSeq.AppendCallback(() =>
        //{
        //    if (mergeFx != null)
        //    {
        //        mergeFx.transform.position = pos;
        //        mergeFx.Play();
        //    }
        //});
    }
    public void Highlight(bool active)
    {
        if (coins.Count == 0) return;
        ColorType colorType = coins[^1].ColorType;
        Sequence highlightTween = DOTween.Sequence();
        int multiplier = active ? 1 : 0;
        for (int i = coins.Count - 1; i >= 0; i--)
        {
            Coin coin = coins[i];
            if (coin.ColorType != colorType) break;
            int index = coins.Count - 1 - i;
            coin.transform.DOKill();
            coin.transform.DOMove(GetStackedPosition(i).AddY(CreativeSettings.Instance.HighlightOffset * multiplier), .1f)
                .SetEase(Ease.InBack)
                .SetDelay(CreativeSettings.Instance.HighlightInterval * index);
            coin.transform.DOPunchScale(new Vector3(0.15f, 0.05f, 0f), .3f, vibrato: 1)
                .SetEase(Ease.Linear)
                .SetDelay(CreativeSettings.Instance.HighlightInterval * index)
                .OnKill(() =>
                {
                    coin.transform.localScale = Vector3.one;
                });
        }
    }

    /*public static void Deal(Transform startPosition)
    {
        foreach (var tray in allTrays)
        {
            tray.DealCoint(startPosition);
        }
    }*/
    public void Deal(Transform spawnPoint)
    {
        DealAll(spawnPoint != null ? spawnPoint : startPosition);
    }

    public void Deal()
    {
        Deal(startPosition);
    }

    public static void DealAll(Transform spawnPoint)
    {
        foreach (var tray in allTrays)
        {
            if (tray != null && !tray.IsLocked)
            {
                tray.DealCoint(spawnPoint);
            }
        }
    }
    public void DealCoint(Transform startPosition)
    {
        if (IsLocked)
            return;
        if (startPosition == null)
            startPosition = this.startPosition;
        if (startPosition == null)
            return;
        int startIndex = coins.Count;
        float delay = GetDealInterval();

        // One random color for the entire deal
        ColorType color = CreativeSettings.Instance.GetRandomColorType();

        for (int i = 0; i < CreativeSettings.Instance.DealSpawnCount; i++)
        {
            if (!CanRecieve())
                break;

            Vector3 targetPosition = GetStackedPosition(startIndex + i);

            Coin coin = Instantiate(
                CreativeSettings.Instance.CoinPrefab,
                startPosition.position,
                Quaternion.Euler(eulerRotation),
                transform);

            coin.SetColorType(color);

            coin.transform.DOKill();
            coin.transform.DOJump(
                    targetPosition,
                    GetDealJumpHeight(),
                    GetDealNumJumps(),
                    GetDealJumpDuration())
                .SetEase(GetDealJumpEase())
                .SetDelay(delay * i);

            if (GetEnableDealRotation())
            {
                coin.transform.DORotate(GetDealRotation(), GetDealJumpDuration(), GetDealRotateMode())
                    .SetEase(GetDealRotationEase())
                    .SetDelay(delay * i)
                    .OnComplete(() => coin.transform.localRotation = Quaternion.Euler(eulerRotation));
            }

            coins.Add(coin);
        }
    }
    /*public void DealCoint(Transform startPosition)
    {
        int startIndex = coins.Count;
        float delay = 0.05f;
        for (int i = 0; i < CreativeSettings.Instance.DealSpawnCount; i++)
        {
            Vector3 targetPosition = GetStackedPosition(startIndex + i);

            if (CanRecieve())
            {
                Coin coin = Instantiate(original: CreativeSettings.Instance.CoinPrefab,
                               position: startPosition.position,
                               rotation: Quaternion.Euler(eulerRotation),
                               parent: transform);
                coin.SetColorType(CreativeSettings.Instance.GetRandomColorType());
                coin.transform.DOJump(
                endValue: targetPosition,
                jumpPower: CreativeSettings.Instance.JumpHeight,
                numJumps: 1,
                duration: CreativeSettings.Instance.JumpDuration).SetEase(Ease.Linear).SetDelay(delay * i);
                coins.Add(coin);
            }
            else
            {
                break;
            }
        }
    }*/
    bool CanRecieve()
    {
        return coins.Count < maxCapacity;
    }
    public bool CanRecieve(CoinTray otherTray)
    {
        return CanRecieve() && otherTray.coins.Count > 0 && (coins.Count == 0 || coins[^1].ColorType == otherTray.coins[^1].ColorType);
    }
    void RecieveCoin(Coin coin)
    {
        coins.Add(coin);
        UpdateMergeOutline();
    }
    public void RecieveCoins(CoinTray otherTray)
    {
        ColorType colorType = null;
        if (coins.Count == 0)
        {
            colorType = otherTray.coins[^1].ColorType;
        }
        else
        {
            colorType = coins[^1].ColorType;
        }
        int i = 0;
        while (CanRecieve(otherTray))
        {
            Coin coin = otherTray.coins[^1];
            otherTray.coins.Remove(coin);

            coin.transform.SetParent(transform);
            Vector3 targetPosition = startPosition.position.GetStackedPosition(coins.Count, stackDirection, eulerRotation, stackHeightOffset);

            float distance = (targetPosition - coin.transform.position).magnitude;
            float cardJumpDuration = CalculateTransferDuration(distance);

            float cardJumpInterval = GetTransferInterval();
            coin.transform.DOKill();
            float delay = cardJumpInterval * i;
            coin.transform.DOJump(
                endValue: targetPosition,
                jumpPower: GetTransferJumpHeight(),
                numJumps: GetTransferNumJumps(),
                duration: cardJumpDuration)
                .SetEase(GetTransferJumpEase())
                .SetDelay(delay)
                .OnKill(() =>
                {
                    coin.transform.position = targetPosition;
                    if (autoMerge)
                    {
                        Merge();
                    }
                });

            if (GetEnableTransferRotation())
            {
                coin.transform.DORotate(GetTransferRotation(), cardJumpDuration, GetTransferRotateMode())
                    .SetEase(GetTransferRotationEase())
                    .SetDelay(delay)
                    .OnComplete(() => coin.transform.localRotation = Quaternion.Euler(eulerRotation));
            }
            else
            {
                coin.transform.localRotation = Quaternion.Euler(eulerRotation);
            }

            RecieveCoin(coin);
            i++;
        }
        if (otherTray.coins.Count > 0 && otherTray.coins[^1].ColorType == colorType)
            otherTray.Highlight(false);
    }
    private Vector3 GetStackedPosition(int index)
    {
        return startPosition.position.GetStackedPosition(
                                           index: index,
                                           direction: stackDirection,
                                           eulerRotation: eulerRotation,
                                           slotOffset: stackHeightOffset);
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (coins.Count != colorTypes.Count)
        {
            //CreateStack();
            return;
        }
        for (int i = 0; i < coins.Count; i++)
        {
            Coin coin = coins[i];
            coin.SetMaterial(colorTypes[i].CoinMaterial);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    [EditorButton("Create Stack")]
    public void CreateStack()
    {
        ClearCoinObjects();

        // If colorTypes is empty, automatically spawn a random stack!
        if (colorTypes == null || colorTypes.Count == 0)
        {
            SpawnRandomStack();
            return;
        }

        if (startPosition == null)
        {
            Debug.LogError("[CoinTray] startPosition is not assigned!");
            return;
        }

        Coin prefab = CreativeSettings.Instance != null ? CreativeSettings.Instance.CoinPrefab : null;
        if (prefab == null)
        {
            Debug.LogError("[CoinTray] No CoinPrefab assigned in CreativeSettings!");
            return;
        }

        if (coins == null) coins = new System.Collections.Generic.List<Coin>();

        for (int i = 0; i < colorTypes.Count; i++)
        {
            if (i >= maxCapacity) break;
            if (colorTypes[i] == null) continue;

            Vector3 pos = startPosition.position.GetStackedPosition(i, stackDirection, eulerRotation, stackHeightOffset);
            Quaternion rot = Quaternion.Euler(eulerRotation);

            Coin newCoin;
            if (!Application.isPlaying)
            {
                newCoin = (Coin)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
                newCoin.transform.position = pos;
                newCoin.transform.rotation = rot;
                UnityEditor.Undo.RegisterCreatedObjectUndo(newCoin.gameObject, "Create Stack");
            }
            else
            {
                newCoin = Instantiate(prefab, pos, rot, transform);
            }

            newCoin.SetColorType(colorTypes[i]);
            coins.Add(newCoin);
        }

        UpdateMergeOutline();
        UnityEditor.EditorUtility.SetDirty(this);
    }
    [EditorButton("Randomize Colors")]
    public void RandomizeColors()
    {
        if (coins == null || coins.Count == 0) return;
        var randomizer = FindObjectOfType<CoinColorRandomizer>();
        var pool = (randomizer != null) ? randomizer.GetFilteredColors() : null;
        if (pool == null || pool.Count == 0)
        {
            if (CreativeSettings.Instance != null && CreativeSettings.Instance.AllColors != null && CreativeSettings.Instance.AllColors.Length > 0)
                pool = new System.Collections.Generic.List<ColorType>(CreativeSettings.Instance.AllColors);
        }
        if (pool == null || pool.Count == 0)
        {
#if UNITY_EDITOR
            pool = new System.Collections.Generic.List<ColorType>();
            for (int i = 1; i <= 8; i++)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ColorType>($"Assets/ScriptableObject/MoneySort/{i}.asset");
                if (asset != null) pool.Add(asset);
            }
#endif
        }

        int minAdj = (randomizer != null) ? randomizer.MinAdjacentCount : 2;
        int maxAdj = (randomizer != null) ? randomizer.MaxAdjacentCount : 3;

        var colorList = CoinColorRandomizer.GenerateAdjacentColors(coins.Count, pool, minAdj, maxAdj);

        for (int i = 0; i < coins.Count; i++)
        {
            if (coins[i] == null) continue;
            ColorType randomColor = (i < colorList.Count)
                ? colorList[i]
                : pool[UnityEngine.Random.Range(0, pool.Count)];

            UnityEditor.Undo.RecordObject(coins[i], "Randomize Coin Color");
            UnityEditor.Undo.RecordObject(this, "Randomize Tray Colors");
            coins[i].SetColorType(randomColor);
            if (colorTypes != null && i < colorTypes.Count)
            {
                colorTypes[i] = randomColor;
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [EditorButton("Spawn Random Stack")]
    public void SpawnRandomStack()
    {
        var spawner = FindObjectOfType<CoinSpawnRandomizer>();
        int minCount = spawner != null ? spawner.MinCoinsPerTray : 3;
        int maxCount = spawner != null ? spawner.MaxCoinsPerTray : Mathf.Min(6, maxCapacity);
        int coinCount = UnityEngine.Random.Range(Mathf.Min(minCount, maxCount), Mathf.Max(minCount, maxCount) + 1);

        var randomizer = FindObjectOfType<CoinColorRandomizer>();
        var pool = (randomizer != null) ? randomizer.GetFilteredColors() : null;
        if (pool == null || pool.Count == 0)
        {
            if (CreativeSettings.Instance != null && CreativeSettings.Instance.AllColors != null)
                pool = new System.Collections.Generic.List<ColorType>(CreativeSettings.Instance.AllColors);
        }

        int minAdj = (randomizer != null) ? randomizer.MinAdjacentCount : (CreativeSettings.Instance != null ? CreativeSettings.Instance.MinAdjacentCount : 2);
        int maxAdj = (randomizer != null) ? randomizer.MaxAdjacentCount : (CreativeSettings.Instance != null ? CreativeSettings.Instance.MaxAdjacentCount : 3);

        var colors = CoinColorRandomizer.GenerateAdjacentColors(coinCount, pool, minAdj, maxAdj);
        SpawnCoins(colors);
    }

    public void SpawnCoins(System.Collections.Generic.List<ColorType> newColors, Coin overridePrefab = null)
    {
        ClearCoinObjects();
        if (newColors == null || newColors.Count == 0 || startPosition == null) return;

        Coin prefab = overridePrefab != null ? overridePrefab : (CreativeSettings.Instance != null ? CreativeSettings.Instance.CoinPrefab : null);
        if (prefab == null)
        {
            Debug.LogError("[CoinTray] No CoinPrefab assigned in CreativeSettings!");
            return;
        }

        if (colorTypes == null) colorTypes = new System.Collections.Generic.List<ColorType>();
        colorTypes.Clear();

        for (int i = 0; i < newColors.Count; i++)
        {
            if (i >= maxCapacity) break;
            ColorType color = newColors[i];
            Vector3 pos = startPosition.position.GetStackedPosition(i, stackDirection, eulerRotation, stackHeightOffset);
            Quaternion rot = Quaternion.Euler(eulerRotation);

            Coin newCoin;
            if (!Application.isPlaying)
            {
                newCoin = (Coin)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
                newCoin.transform.position = pos;
                newCoin.transform.rotation = rot;
                UnityEditor.Undo.RegisterCreatedObjectUndo(newCoin.gameObject, "Spawn Coin");
            }
            else
            {
                newCoin = Instantiate(prefab, pos, rot, transform);
            }

            newCoin.SetColorType(color);
            coins.Add(newCoin);
            colorTypes.Add(color);
        }

        UpdateMergeOutline();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [EditorButton("Clear Stack")]
    public void ClearStack()
    {
        ClearCoinObjects();
        UpdateMergeOutline();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    public void ClearCoinObjects()
    {
        if (coins != null && coins.Count > 0)
        {
            foreach (var coin in coins)
            {
                if (coin != null)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(coin.gameObject);
                    else
                        Destroy(coin.gameObject);
                }
            }
            coins.Clear();
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < 12; i++)
        {
            Gizmos.DrawSphere(startPosition.position.GetStackedPosition(i, stackDirection, eulerRotation, stackHeightOffset), gizmoSphereRadius);
        }
    }
#endif

}
