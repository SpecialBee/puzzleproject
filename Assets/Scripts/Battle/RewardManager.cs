using System.Collections.Generic;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("보상 아이템 풀")]
    [Tooltip("전체 보상 아이템을 Inspector에서 등록하세요.")]
    public List<ItemData> itemPool = new List<ItemData>();

    private List<ItemData> currentChoices = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (itemPool == null)
            itemPool = new List<ItemData>();
    }

    public List<ItemData> DrawChoices()
    {
        currentChoices.Clear();

        if (itemPool == null || itemPool.Count == 0)
            return new List<ItemData>();

        List<ItemData> tempPool = new List<ItemData>(itemPool);
        int drawCount = Mathf.Min(3, tempPool.Count);

        for (int i = 0; i < drawCount; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            currentChoices.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }

        return new List<ItemData>(currentChoices);
    }

    public void ConfirmSelection(ItemData selected)
    {
        if (selected == null)
        {
            Debug.LogError("RewardManager.ConfirmSelection: 선택된 아이템이 null입니다.");
            return;
        }

        if (!currentChoices.Contains(selected))
        {
            Debug.LogError($"RewardManager.ConfirmSelection: 선택된 아이템 '{selected.itemName}'이 현재 선택지에 포함되어 있지 않습니다.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("RewardManager.ConfirmSelection: InventoryManager 없음!");
            return;
        }

        if (GridManager.Instance == null)
        {
            Debug.LogError("RewardManager.ConfirmSelection: GridManager 없음!");
            return;
        }

        if (PlayerManager.Instance == null)
        {
            Debug.LogError("RewardManager.ConfirmSelection: PlayerManager 없음!");
            return;
        }

        InventoryManager.Instance.AddItem(selected);
        itemPool.Remove(selected);
        currentChoices.Clear();

        GridManager.Instance.ClearAllTiles();
        PlayerManager.Instance.SaveAndReturnToMap();
    }
}
