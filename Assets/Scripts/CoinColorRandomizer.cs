using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class CoinColorRandomizer : MonoBehaviour
{
    public enum RandomizeMode
    {
        PerCoin,        // Each coin gets an individual random color
        PerTray         // All coins in the same tray get the same random color
    }

    [Header("Settings")]
    [Tooltip("If true, picks colors from CreativeSettings.Instance (spawnableColors).")]
    [SerializeField] private bool useCreativeSettings = true;

    [Tooltip("Custom colors to pick from when useCreativeSettings is false or empty.")]
    [SerializeField] private List<ColorType> customColors = new();

    [Tooltip("How colors are assigned to coins and trays.")]
    [SerializeField] private RandomizeMode mode = RandomizeMode.PerCoin;

    [Tooltip("If true, automatically randomizes all scene coins when the game starts.")]
    [SerializeField] private bool randomizeOnStart = false;

    [Tooltip("Only randomize coins that are currently in CoinTrays.")]
    [SerializeField] private bool onlyCoinsInTrays = false;

    private void Start()
    {
        if (randomizeOnStart)
        {
            RandomizeAllSceneCoins();
        }
    }

    [EditorButton("Randomize All Scene Coins")]
    [ContextMenu("Randomize All Scene Coins")]
    public void RandomizeAllSceneCoins()
    {
        var colors = GetAvailableColors();
        if (colors == null || colors.Count == 0)
        {
            Debug.LogWarning("[CoinColorRandomizer] No colors available to assign! Please check CreativeSettings or assign customColors.");
            return;
        }

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

            ColorType trayUniformColor = GetRandomColor(colors);

            for (int i = 0; i < coins.Count; i++)
            {
                Coin coin = coins[i];
                if (coin == null) continue;

                coinsInTrays.Add(coin);
                ColorType selectedColor = (mode == RandomizeMode.PerTray) ? trayUniformColor : GetRandomColor(colors);

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

        // 2. Process standalone coins not in any tray (if onlyCoinsInTrays is false)
        if (!onlyCoinsInTrays)
        {
            var allCoins = FindObjectsOfType<Coin>(true);
            foreach (var coin in allCoins)
            {
                if (coin == null || coinsInTrays.Contains(coin)) continue;

                ColorType selectedColor = GetRandomColor(colors);
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

        Debug.Log($"[CoinColorRandomizer] Successfully randomized {coinsModified} coins across {traysModified} trays.");
    }

    private List<ColorType> GetAvailableColors()
    {
        if (useCreativeSettings && CreativeSettings.Instance != null)
        {
            var spawnable = CreativeSettings.Instance.SpawnableColors;
            if (spawnable != null && spawnable.Length > 0)
            {
                return new List<ColorType>(spawnable);
            }

            var fallbackAll = CreativeSettings.Instance.ColorTypes;
            if (fallbackAll != null && fallbackAll.Length > 0)
            {
                return new List<ColorType>(fallbackAll);
            }
        }

        return customColors;
    }

    private ColorType GetRandomColor(List<ColorType> pool)
    {
        return pool[Random.Range(0, pool.Count)];
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
        randomizer.RandomizeAllSceneCoins();
        DestroyImmediate(tempObj);
    }
#endif
}
