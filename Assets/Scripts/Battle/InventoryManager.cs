using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
    public static InventoryManager Instance;

    [Header("내 가방")]
    public List<ItemData> acquiredItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    public void AddItem(ItemData newItem)
    {
        acquiredItems.Add(newItem);
        Debug.Log($"[인벤토리] '{newItem.itemName}' 획득!");

        if (newItem.itemType == ItemData.ItemType.Passive)
        {
            ApplyPassiveItem(newItem);
        }
        else if (newItem.itemType == ItemData.ItemType.Active)
        {
            Debug.Log("액티브 아이템은 가방에 보관됩니다. (추후 사용 UI 구현 예정)");
        }
    }

    private void ApplyPassiveItem(ItemData item)
    {
        PlayerManager pm = PlayerManager.Instance;
        if (pm == null) return;

        // [수정 - B-02] hp와 maxHp를 함께 증가시켜 "110/100" 비정상 표시 방지
        if (item.valueHP > 0)
        {
            pm.maxHp += item.valueHP;
            pm.hp += item.valueHP;
            Debug.Log($"패시브 발동! 최대 체력 +{item.valueHP} → {pm.hp}/{pm.maxHp}");
        }

        // [수정 - B-04] valueAttack을 영구 공격력(baseAttack)에 반영
        // DataManager.baseAttack에 누적해야 턴 종료 후 ResetTurnStats()에서 유지됨
        if (item.valueAttack > 0)
        {
            pm.attack += item.valueAttack;
            if (DataManager.Instance != null)
                DataManager.Instance.baseAttack += item.valueAttack;
            Debug.Log($"패시브 발동! 영구 공격력 +{item.valueAttack} → {pm.attack}");
        }

        // [수정 - B-04/①] valueDefense를 영구 방어력(baseDefense)에 반영
        // DataManager.baseDefense에 누적해야 턴 종료 후 ResetTurnStats()에서 복원됨
        if (item.valueDefense > 0)
        {
            pm.defense += item.valueDefense;
            if (DataManager.Instance != null)
                DataManager.Instance.baseDefense += item.valueDefense;
            Debug.Log($"패시브 발동! 영구 방어력 +{item.valueDefense} → {pm.defense}");
        }

        pm.UpdateUI();
    }
}