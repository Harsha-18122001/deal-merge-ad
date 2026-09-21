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
        PerCoin,        // Each coin gets an individual random color within the range
        PerTray         // All coins in the same tray get the same random color within the range
    }

    [Header("Range Configuration")]
    [Tooltip("Minimum color number (e.g. 1).")]
    [SerializeField, Range(1, 8)] private int minColor = 1;

    [Tooltip("Maximum color number (e.g. 8).")]
    [SerializeField, Range(1, 8)] private int maxColor = 8;

    [Tooltip("How colors are assigned to coins and trays.")]
    [SerializeField] private RandomizeMode mode = RandomizeMode.PerCoin;

    [Header("Color Pool")]
    [Tooltip("List of all available ColorType assets (ordered 1 to 8).")]
    [SerializeField] private List<ColorType> colorPool = new();

    [Header("Options")]
    [Tooltip("If true, automatically randomizes all scene coins when entering Play Mode.")]
    [SerializeField] private bool randomizeOnStart = false;

    [Tooltip("Only randomize coins that are currently stored in CoinTrays.")]
    [SerializeField] private bool onlyCoinsInTrays = false;

    public int MinColor { get => minColor; set => minColor = Mathf.Clamp(value, 1, 8); }
    public int MaxColor { get => maxColor; set => maxColor = Mathf.Clamp(value, 1, 8); }

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

            // Try to parse number from name (e.g. "1", "2", "3")
            if (int.TryParse(color.name, out int num))
            {
                if (num >= min && num <= max)
                {
                    filtered.Add(color);
                }
            }
            else
            {
                // Fallback by list index (1-based)
                int index = colorPool.IndexOf(color) + 1;
                if (index >= min && index <= max)
                {
                    filtered.Add(color);
                }
            }
        }

        return filtered;
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

            ColorType trayUniformColor = activeColors[UnityEngine.Random.Range(0, activeColors.Count)];

            for (int i = 0; i < coins.Count; i++)
            {
                Coin coin = coins[i];
                if (coin == null) continue;

                coinsInTrays.Add(coin);
                ColorType selectedColor = (mode == RandomizeMode.PerTray)
                    ? trayUniformColor
                    : activeColors[UnityEngine.Random.Range(0, activeColors.Count)];

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
            foreach (var coin in allCoins)
            {
                if (coin == null || coinsInTrays.Contains(coin)) continue;

                ColorType selectedColor = activeColors[UnityEngine.Random.Range(0, activeColors.Count)];
#if UNITY_EDITOR
                Undo.RecordObject(coin, "Randomize Coin Color");
                if (coin.MeshRenderer != null) Undo.RecordObject(coin.MeshRenderer, "Randomize Coin Material");
#endif
                coin.SetColorType(selectedColor);
                coinsModified++;
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
#endif

        Debug.Log($"[CoinColorRandomizer] Randomized {coinsModified} coins (range {min}-{max}) across {traysModified} trays.");
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
