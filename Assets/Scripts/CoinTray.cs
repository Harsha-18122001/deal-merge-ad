using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinTray : MonoBehaviour
{
    static List<CoinTray> allTrays = new();
    [SerializeField] List<ColorType> colorTypes;
    [SerializeField] List<Coin> coins;
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
    public static void Merge()
    {
        foreach (var tray in allTrays)
        {
            if (tray.CanMerge())
            {
                tray.MergeAndSpawn();
            }
        }
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
    public static void Deal(Transform startPosition)
    {
        foreach (var tray in allTrays)
        {
            if (!tray.IsLocked)
            {
                tray.DealCoint(startPosition);
            }
        }
    }
    public void DealCoint(Transform startPosition)
    {
        if (IsLocked)
            return;
        int startIndex = coins.Count;
        float delay = 0.05f;

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

            coin.transform.DOJump(
                    targetPosition,
                    CreativeSettings.Instance.JumpHeight,
                    1,
                    CreativeSettings.Instance.JumpDuration)
                .SetEase(Ease.Linear)
                .SetDelay(delay * i);

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
            float adjustedDistance = Mathf.Sqrt(distance); // magic line
            float cardJumpDuration = adjustedDistance / CreativeSettings.Instance.CardJumpDuration;

            float cardJumpInterval = CreativeSettings.Instance.CardJumpInterval;
            coin.transform.DOKill();
            float delay = cardJumpInterval * i;
            coin.transform.DOJump(
                endValue: targetPosition,
                jumpPower: CreativeSettings.Instance.CardJumpHeight,
                numJumps: 1,
                duration: cardJumpDuration)
                .SetEase(Ease.Linear)
                .SetDelay(delay)
                .OnKill(() =>
                {
                    coin.transform.position = targetPosition;
                    if (autoMerge)
                    {
                        Merge();
                    }
                });

            coin.transform.DORotate(new Vector3(180, 0, 180), cardJumpDuration, RotateMode.FastBeyond360)
                 .SetEase(Ease.InSine)
                .SetDelay(delay)
                .OnComplete(() => coin.transform.rotation = Quaternion.identity);

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
        ClearStack();
        for (int i = 0; i < colorTypes.Count; i++)
        {
            Coin newCoin = (Coin)UnityEditor.PrefabUtility.InstantiatePrefab(CreativeSettings.Instance.CoinPrefab, transform);
            newCoin.transform.position = startPosition.position.GetStackedPosition(i, stackDirection, eulerRotation, stackHeightOffset);
            newCoin.SetColorType(colorTypes[i]);
            coins.Add(newCoin);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
    [EditorButton("Clear Stack")]
    private void ClearStack()
    {
        if (coins.Count > 0)
        {
            foreach (var coin in coins)
            {
                if (coin != null)
                    DestroyImmediate(coin.gameObject);
            }
            coins.Clear();
        }
        UnityEditor.EditorUtility.SetDirty(this);
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
