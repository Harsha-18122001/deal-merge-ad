using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class CoinColorRandomizer : MonoBehaviour
{
    public enum RandomizeMode
    {
        PerCoin,        // Coins randomized in adjacent clusters of matching colors
        PerTray         // All coins in the same tray get the same random color
    }

    [Header("Range Configuration")]
    [Tooltip("Minimum color number (e.g. 1).")]
    [SerializeField, Range(1, 8)] private int minColor = 1;

    [Tooltip("Maximum color number (e.g. 8).")]
    [SerializeField, Range(1, 8)] private int maxColor = 8;

    [Header("Adjacent Grouping")]
    [Tooltip("Ensure at least 2 to 3 adjacent coins in each stack have the same color.")]
    [SerializeField] private bool groupAdjacentCoins = true;

    [Tooltip("Minimum adjacent coins of the same color.")]
    [SerializeField, Range(1, 6)] private int minAdjacentCount = 2;

    [Tooltip("Maximum adjacent coins of the same color.")]
    [SerializeField, Range(1, 6)] private int maxAdjacentCount = 3;

    [Header("Mode & Options")]
    [Tooltip("How colors are assigned to coins and trays.")]
    [SerializeField] private RandomizeMode mode = RandomizeMode.PerCoin;

    [Tooltip("If true, automatically randomizes all scene coins when entering Play Mode.")]
    [SerializeField] private bool randomizeOnStart = false;

    [Tooltip("Only randomize coins that are currently stored in CoinTrays.")]
    [SerializeField] private bool onlyCoinsInTrays = false;

    [Header("Color Pool")]
    [Tooltip("List of all available ColorType assets (ordered 1 to 8).")]
    [SerializeField] private List<ColorType> colorPool = new();

    public int MinColor { get => minColor; set => minColor = Mathf.Clamp(value, 1, 8); }
    public int MaxColor { get => maxColor; set => maxColor = Mathf.Clamp(value, 1, 8); }
    public int MinAdjacentCount { get => minAdjacentCount; set => minAdjacentCount = Mathf.Clamp(value, 1, 6); }
    public int MaxAdjacentCount { get => maxAdjacentCount; set => maxAdjacentCount = Mathf.Clamp(value, 1, 6); }
    public bool GroupAdjacentCoins { get => groupAdjacentCoins; set => groupAdjacentCoins = value; }

    private void Reset()
    {
        LoadAllAvailableColors();
    }

    private void OnValidate()
    {
        if (minColor > maxColor)
        {
            minColor = maxColor;
        }

        if (minAdjacentCount > maxAdjacentCount)
        {
            minAdjacentCount = maxAdjacentCount;
        }

        if (colorPool == null || colorPool.Count == 0)
        {
            LoadAllAvailableColors();
        }
    }

    private void Start()
    {
        if (Application.isPlaying && randomizeOnStart)
        {
            RandomizeAllSceneCoins();
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

    /// <summary>
    /// Returns the subset of colors matching the [minColor, maxColor] range.
    /// </summary>
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

    /// <summary>
    /// Generates a list of colors where consecutive items are grouped in batches of adjacent matching colors.
    /// </summary>
    public static List<ColorType> GenerateAdjacentColors(int totalCount, List<ColorType> pool, int minAdjacent = 2, int maxAdjacent = 3)
    {
        List<ColorType> result = new();
        if (totalCount <= 0 || pool == null || pool.Count == 0) return result;

        List<int> clusters = GetClusterSizes(totalCount, minAdjacent, maxAdjacent);
        ColorType previousColor = null;

        foreach (int size in clusters)
        {
            ColorType chosenColor = PickColorDifferentFrom(pool, previousColor);
            previousColor = chosenColor;

            for (int i = 0; i < size; i++)
            {
                result.Add(chosenColor);
            }
        }

        return result;
    }

    private static ColorType PickColorDifferentFrom(List<ColorType> pool, ColorType previous)
    {
        if (pool.Count == 1) return pool[0];
        ColorType pick;
        int safety = 25;
        do
        {
            pick = pool[UnityEngine.Random.Range(0, pool.Count)];
            safety--;
        } while (pick == previous && safety > 0);

        return pick;
    }

    public static List<int> GetClusterSizes(int total, int minSize = 2, int maxSize = 3)
    {
        List<int> clusters = new();
        int rem = total;
        while (rem > 0)
        {
            if (rem < minSize)
            {
                if (clusters.Count > 0)
                    clusters[clusters.Count - 1] += rem;
                else
                    clusters.Add(rem);
                break;
            }

            int s = UnityEngine.Random.Range(minSize, maxSize + 1);
            if (rem - s < 0)
            {
                s = rem;
            }
            else if (rem - s > 0 && rem - s < minSize)
            {
                if (s == maxSize)
                    s = minSize;
                else
                    s = rem;
            }

            clusters.Add(s);
            rem -= s;
        }
        return clusters;
    }

    [EditorButton("Randomize All Scene Coins")]
    [ContextMenu("Randomize All Scene Coins")]
    public void RandomizeAllSceneCoins()
    {
        var activeColors = GetFilteredColors();
        if (activeColors == null || activeColors.Count == 0)
        {
            Debug.LogWarning($"[CoinColorRandomizer] No colors found in range {minColor} to {maxColor}!");
            return;
        }

        int min = Mathf.Min(minColor, maxColor);
        int max = Mathf.Max(minColor, maxColor);

        // 1. Process all CoinTrays in the scene
        var trays = FindObjectsOfType<CoinTray>(true);
        HashSet<Coin> coinsInTrays = new();

        int traysModified = 0;
        int coinsModified = 0;

        foreach (var tray in trays)
        {
            if (tray == null) continue;

#if UNITY_EDITOR
            Undo.RecordObject(tray, "Randomize Tray Colors");
#endif
            var coins = tray.Coins;
            var trayColorTypes = tray.ColorTypes;

            if (coins == null || coins.Count == 0) continue;

            List<ColorType> trayColors;
            if (mode == RandomizeMode.PerTray)
            {
                ColorType uniformColor = activeColors[UnityEngine.Random.Range(0, activeColors.Count)];
                trayColors = new List<ColorType>();
                for (int i = 0; i < coins.Count; i++) trayColors.Add(uniformColor);
            }
            else if (groupAdjacentCoins)
            {
                trayColors = GenerateAdjacentColors(coins.Count, activeColors, minAdjacentCount, maxAdjacentCount);
            }
            else
            {
                trayColors = new List<ColorType>();
                for (int i = 0; i < coins.Count; i++)
                    trayColors.Add(activeColors[UnityEngine.Random.Range(0, activeColors.Count)]);
            }

            for (int i = 0; i < coins.Count; i++)
            {
                Coin coin = coins[i];
                if (coin == null) continue;

                coinsInTrays.Add(coin);
                ColorType selectedColor = trayColors[i];

#if UNITY_EDITOR
                Undo.RecordObject(coin, "Randomize Coin Color");
                if (coin.MeshRenderer != null) Undo.RecordObject(coin.MeshRenderer, "Randomize Coin Material");
#endif
                coin.SetColorType(selectedColor);

                if (trayColorTypes != null && i < trayColorTypes.Count)
                {
                    trayColorTypes[i] = selectedColor;
                }

                coinsModified++;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(tray);
#endif
            traysModified++;
        }

        // 2. Process standalone coins not in any tray
        if (!onlyCoinsInTrays)
        {
            var allCoins = FindObjectsOfType<Coin>(true);
            List<Coin> standaloneCoins = new();
            foreach (var coin in allCoins)
            {
                if (coin != null && !coinsInTrays.Contains(coin))
                {
                    standaloneCoins.Add(coin);
                }
            }

            if (standaloneCoins.Count > 0)
            {
                List<ColorType> standaloneColors = groupAdjacentCoins
                    ? GenerateAdjacentColors(standaloneCoins.Count, activeColors, minAdjacentCount, maxAdjacentCount)
                    : null;

                for (int i = 0; i < standaloneCoins.Count; i++)
                {
                    ColorType selectedColor = (standaloneColors != null)
                        ? standaloneColors[i]
                        : activeColors[UnityEngine.Random.Range(0, activeColors.Count)];

#if UNITY_EDITOR
                    Undo.RecordObject(standaloneCoins[i], "Randomize Coin Color");
                    if (standaloneCoins[i].MeshRenderer != null) Undo.RecordObject(standaloneCoins[i].MeshRenderer, "Randomize Coin Material");
#endif
                    standaloneCoins[i].SetColorType(selectedColor);
                    coinsModified++;
                }
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
#endif

        string groupingMsg = groupAdjacentCoins ? $" (clusters of {minAdjacentCount}-{maxAdjacentCount} adjacent)" : "";
        Debug.Log($"[CoinColorRandomizer] Randomized {coinsModified} coins{groupingMsg} (range {min}-{max}) across {traysModified} trays.");
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Deal Merge/Randomize All Scene Coins")]
    public static void MenuItemRandomizeAllSceneCoins()
    {
        var existingRandomizer = FindObjectOfType<CoinColorRandomizer>();
        if (existingRandomizer != null)
        {
            existingRandomizer.RandomizeAllSceneCoins();
            return;
        }

        var tempObj = new GameObject("__TempCoinColorRandomizer");
        var randomizer = tempObj.AddComponent<CoinColorRandomizer>();
        randomizer.LoadAllAvailableColors();
        randomizer.RandomizeAllSceneCoins();
        DestroyImmediate(tempObj);
    }
#endif
}
