using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class CoinSpawnRandomizer : MonoBehaviour
{
    [Header("Coins Per Tray")]
    [Tooltip("Minimum number of coins to spawn in each tray.")]
    [SerializeField, Min(0)] private int minCoinsPerTray = 3;

    [Tooltip("Maximum number of coins to spawn in each tray (capped by tray capacity).")]
    [SerializeField, Min(1)] private int maxCoinsPerTray = 6;

    [Tooltip("Percentage chance (0 to 100) that an individual tray will be populated with coins. Set to 100 to fill every tray.")]
    [SerializeField, Range(0f, 100f)] private float fillTrayChance = 100f;

    [Tooltip("If true, trays with IsLocked == true will not be spawned.")]
    [SerializeField] private bool skipLockedTrays = true;

    [Header("Color Range (Min to Max)")]
    [Tooltip("Minimum color number (1 to 8).")]
    [SerializeField, Min(1)] private int minColor = 1;

    [Tooltip("Maximum color number (1 to 8).")]
    [SerializeField, Min(1)] private int maxColor = 8;

    [Header("Adjacent Grouping")]
    [Tooltip("Ensure spawned coins are grouped into batches of adjacent matching colors.")]
    [SerializeField] private bool groupAdjacentCoins = true;

    [Tooltip("Use Min/Max Adjacent Count configured in CreativeSettings.asset.")]
    [SerializeField] private bool useCreativeSettingsAdjacent = false;

    [Tooltip("Minimum adjacent coins of the same color.")]
    [SerializeField, Min(1)] private int minAdjacentCount = 2;

    [Tooltip("Maximum adjacent coins of the same color.")]
    [SerializeField, Min(1)] private int maxAdjacentCount = 3;

    [Header("Spawning Options")]
    [Tooltip("If true, clears existing coins from trays before spawning.")]
    [SerializeField] private bool clearBeforeSpawn = true;

    [Tooltip("If true, automatically spawns random coins into trays when entering Play Mode.")]
    [SerializeField] private bool spawnOnStart = false;

    [Tooltip("Optional custom prefab to spawn. If null, uses CreativeSettings.Instance.CoinPrefab.")]
    [SerializeField] private Coin customCoinPrefab = null;

    [Header("Color Pool")]
    [Tooltip("List of all available ColorType assets (ordered 1 to 8). Auto-loaded if empty.")]
    [SerializeField] private List<ColorType> colorPool = new();

    public int MinCoinsPerTray { get => minCoinsPerTray; set => minCoinsPerTray = Mathf.Max(0, value); }
    public int MaxCoinsPerTray { get => maxCoinsPerTray; set => maxCoinsPerTray = Mathf.Max(1, value); }
    public int MinColor { get => minColor; set => minColor = Mathf.Max(1, value); }
    public int MaxColor { get => maxColor; set => maxColor = Mathf.Max(1, value); }
    public int MinAdjacentCount
    {
        get => (useCreativeSettingsAdjacent && CreativeSettings.Instance != null) ? CreativeSettings.Instance.MinAdjacentCount : minAdjacentCount;
        set => minAdjacentCount = Mathf.Max(1, value);
    }
    public int MaxAdjacentCount
    {
        get => (useCreativeSettingsAdjacent && CreativeSettings.Instance != null) ? CreativeSettings.Instance.MaxAdjacentCount : maxAdjacentCount;
        set => maxAdjacentCount = Mathf.Max(1, value);
    }

    private void Reset()
    {
        LoadAllAvailableColors();
        if (CreativeSettings.Instance != null)
        {
            minAdjacentCount = CreativeSettings.Instance.MinAdjacentCount;
            maxAdjacentCount = CreativeSettings.Instance.MaxAdjacentCount;
        }
    }

    private void OnValidate()
    {
        if (minCoinsPerTray < 0) minCoinsPerTray = 0;
        if (maxCoinsPerTray < 1) maxCoinsPerTray = 1;
        if (minColor < 1) minColor = 1;
        if (maxColor < 1) maxColor = 1;
        if (minAdjacentCount < 1) minAdjacentCount = 1;
        if (maxAdjacentCount < 1) maxAdjacentCount = 1;

        if (useCreativeSettingsAdjacent && CreativeSettings.Instance != null)
        {
            minAdjacentCount = CreativeSettings.Instance.MinAdjacentCount;
            maxAdjacentCount = CreativeSettings.Instance.MaxAdjacentCount;
        }

        if (colorPool == null || colorPool.Count == 0)
        {
            LoadAllAvailableColors();
        }
    }

    private void Start()
    {
        if (Application.isPlaying && spawnOnStart)
        {
            SpawnRandomCoinsInAllTrays();
        }
    }

    public void LoadAllAvailableColors()
    {
#if UNITY_EDITOR
        colorPool.Clear();
        for (int i = 1; i <= 8; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath<ColorType>($"Assets/ScriptableObject/MoneySort/{i}.asset");
            if (asset != null)
            {
                colorPool.Add(asset);
            }
        }
#endif
        if (colorPool.Count == 0 && CreativeSettings.Instance != null && CreativeSettings.Instance.AllColors != null)
        {
            colorPool.AddRange(CreativeSettings.Instance.AllColors);
        }
    }

    public List<ColorType> GetFilteredColors()
    {
        if (colorPool == null || colorPool.Count == 0)
        {
            LoadAllAvailableColors();
        }

        List<ColorType> filtered = new();
        int min = Mathf.Min(minColor, maxColor);
        int max = Mathf.Max(minColor, maxColor);

        foreach (var color in colorPool)
        {
            if (color == null) continue;

            if (int.TryParse(color.name, out int num))
            {
                if (num >= min && num <= max)
                {
                    filtered.Add(color);
                }
            }
            else
            {
                int index = colorPool.IndexOf(color) + 1;
                if (index >= min && index <= max)
                {
                    filtered.Add(color);
                }
            }
        }

        return filtered;
    }

    [EditorButton("Spawn Random Coins In All Trays")]
    [ContextMenu("Spawn Random Coins In All Trays")]
    public void SpawnRandomCoinsInAllTrays()
    {
        var activeColors = GetFilteredColors();
        if (activeColors == null || activeColors.Count == 0)
        {
            Debug.LogWarning($"[CoinSpawnRandomizer] No colors found in range {minColor} to {maxColor}!");
            return;
        }

        Coin prefab = customCoinPrefab != null ? customCoinPrefab : (CreativeSettings.Instance != null ? CreativeSettings.Instance.CoinPrefab : null);
        if (prefab == null)
        {
            Debug.LogError("[CoinSpawnRandomizer] No CoinPrefab found in CreativeSettings or customCoinPrefab!");
            return;
        }

        var trays = FindObjectsOfType<CoinTray>(true);
        if (trays == null || trays.Length == 0)
        {
            Debug.LogWarning("[CoinSpawnRandomizer] No CoinTray components found in the scene!");
            return;
        }

        int effMinCoins = Mathf.Min(minCoinsPerTray, maxCoinsPerTray);
        int effMaxCoins = Mathf.Max(minCoinsPerTray, maxCoinsPerTray);
        int effMinAdj = Mathf.Min(MinAdjacentCount, MaxAdjacentCount);
        int effMaxAdj = Mathf.Max(MinAdjacentCount, MaxAdjacentCount);

        int totalSpawnedCoins = 0;
        int traysPopulated = 0;
        int traysSkipped = 0;

        foreach (var tray in trays)
        {
            if (tray == null) continue;

            if (skipLockedTrays && tray.IsLocked)
            {
                traysSkipped++;
                continue;
            }

            // Probability check
            if (fillTrayChance < 100f && UnityEngine.Random.Range(0f, 100f) > fillTrayChance)
            {
                if (clearBeforeSpawn)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(tray, "Clear Tray");
#endif
                    tray.ClearStack();
                }
                traysSkipped++;
                continue;
            }

            // Decide coin count for this tray
            int coinCount = UnityEngine.Random.Range(effMinCoins, effMaxCoins + 1);
            coinCount = Mathf.Min(coinCount, tray.MaxCapacity);

            if (coinCount <= 0)
            {
                if (clearBeforeSpawn)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(tray, "Clear Tray");
#endif
                    tray.ClearStack();
                }
                continue;
            }

            // Generate color sequence
            List<ColorType> trayColors = groupAdjacentCoins
                ? CoinColorRandomizer.GenerateAdjacentColors(coinCount, activeColors, effMinAdj, effMaxAdj)
                : new List<ColorType>();

            if (!groupAdjacentCoins)
            {
                for (int i = 0; i < coinCount; i++)
                {
                    trayColors.Add(activeColors[UnityEngine.Random.Range(0, activeColors.Count)]);
                }
            }

#if UNITY_EDITOR
            Undo.RecordObject(tray, "Spawn Random Coins In Tray");
#endif
            tray.SpawnCoins(trayColors, prefab);

            totalSpawnedCoins += coinCount;
            traysPopulated++;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
#endif

        string adjStr = groupAdjacentCoins ? $" (adjacent runs of {effMinAdj}-{effMaxAdj})" : "";
        Debug.Log($"[CoinSpawnRandomizer] Spawned {totalSpawnedCoins} coins{adjStr} across {traysPopulated} trays ({traysSkipped} skipped).");
    }

    [EditorButton("Clear All Coins In Trays")]
    [ContextMenu("Clear All Coins In Trays")]
    public void ClearAllCoinsInTrays()
    {
        var trays = FindObjectsOfType<CoinTray>(true);
        if (trays == null || trays.Length == 0) return;

        int count = 0;
        foreach (var tray in trays)
        {
            if (tray == null) continue;
#if UNITY_EDITOR
            Undo.RecordObject(tray, "Clear Coins In Tray");
#endif
            tray.ClearStack();
            count++;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
#endif

        Debug.Log($"[CoinSpawnRandomizer] Cleared all coins from {count} trays.");
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Deal Merge/Spawn Random Coins In Trays")]
    public static void MenuItemSpawnRandomCoins()
    {
        var existing = FindObjectOfType<CoinSpawnRandomizer>();
        if (existing != null)
        {
            existing.SpawnRandomCoinsInAllTrays();
            return;
        }

        var tempObj = new GameObject("__TempCoinSpawnRandomizer");
        var spawner = tempObj.AddComponent<CoinSpawnRandomizer>();
        spawner.LoadAllAvailableColors();
        spawner.SpawnRandomCoinsInAllTrays();
        DestroyImmediate(tempObj);
    }

    [MenuItem("Tools/Deal Merge/Clear All Coins In Trays")]
    public static void MenuItemClearAllCoins()
    {
        var existing = FindObjectOfType<CoinSpawnRandomizer>();
        if (existing != null)
        {
            existing.ClearAllCoinsInTrays();
            return;
        }

        var tempObj = new GameObject("__TempCoinSpawnRandomizer");
        var spawner = tempObj.AddComponent<CoinSpawnRandomizer>();
        spawner.ClearAllCoinsInTrays();
        DestroyImmediate(tempObj);
    }
#endif
}
