using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("내 가방")]
    public List<ItemData> acquiredItems = new List<ItemData>();

    void Awake()
    {
        Instance = this;
    }

    // 보상 창에서 아이템을 선택하면 이 함수가 실행됩니다.
    public void AddItem(ItemData newItem)
    {
        acquiredItems.Add(newItem);
        Debug.Log($"[인벤토리] '{newItem.itemName}' 획득!");

        // 패시브 아이템일 경우 즉시 효과(예: 체력 증가)를 적용합니다.
        if (newItem.itemType == ItemData.ItemType.Passive)
        {
            PlayerManager.Instance.hp += newItem.valueHP;
            PlayerManager.Instance.UpdateUI();
            Debug.Log($"패시브 발동! 최대 체력이 {newItem.valueHP}만큼 증가했습니다.");
        }
        else if (newItem.itemType == ItemData.ItemType.Active)
        {
            Debug.Log("액티브 아이템은 가방에 보관됩니다. (추후 사용 UI 구현 예정)");
        }
    }
}