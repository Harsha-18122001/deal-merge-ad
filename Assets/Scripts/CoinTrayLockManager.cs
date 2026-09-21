using System.Collections.Generic;
using UnityEngine;

public class CoinTrayLockManager : MonoBehaviour
{
    [SerializeField] private List<CoinTray> trays = new();

    [SerializeField] private int initiallyUnlocked = 1;

    private int unlockedCount;

    private void Start()
    {
        unlockedCount = Mathf.Clamp(initiallyUnlocked, 0, trays.Count);

        for (int i = 0; i < trays.Count; i++)
        {
            trays[i].SetLocked(i >= unlockedCount);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UnlockNext();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LockPrevious();
        }
    }

    public void UnlockNext()
    {
        if (unlockedCount >= trays.Count)
            return;

        trays[unlockedCount].SetLocked(false);
        unlockedCount++;
    }

    public void LockPrevious()
    {
        if (unlockedCount <= 0)
            return;

        unlockedCount--;
        trays[unlockedCount].SetLocked(true);
    }
}