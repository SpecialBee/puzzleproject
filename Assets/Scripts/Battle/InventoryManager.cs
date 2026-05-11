using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // [���� - S-01] �̱��� �ߺ� üũ �߰�
    public static InventoryManager Instance;

    [Header("�� ����")]
    public List<ItemData> acquiredItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    public void AddItem(ItemData newItem)
    {
        if (newItem == null)
        {
            Debug.LogError("InventoryManager.AddItem: 전달된 아이템이 null입니다. 아이템 선택 UI를 확인하세요.");
            return;
        }

        acquiredItems.Add(newItem);
        Debug.Log($"[�κ��丮] '{newItem.itemName}' ȹ��!");

        if (newItem.itemType == ItemData.ItemType.Passive)
        {
            ApplyPassiveItem(newItem);
        }
        else if (newItem.itemType == ItemData.ItemType.Active)
        {
            Debug.Log("��Ƽ�� �������� ���濡 �����˴ϴ�. (���� ��� UI ���� ����)");
        }
    }

    private void ApplyPassiveItem(ItemData item)
    {
        PlayerManager pm = PlayerManager.Instance;
        if (pm == null) return;

        // [���� - B-02] hp�� maxHp�� �Բ� �������� "110/100" ������ ǥ�� ����
        if (item.valueHP > 0)
        {
            pm.maxHp += item.valueHP;
            pm.hp += item.valueHP;
            Debug.Log($"�нú� �ߵ�! �ִ� ü�� +{item.valueHP} �� {pm.hp}/{pm.maxHp}");
        }

        // [���� - B-04] valueAttack�� ���� ���ݷ�(baseAttack)�� �ݿ�
        // DataManager.baseAttack�� �����ؾ� �� ���� �� ResetTurnStats()���� ������
        if (item.valueAttack > 0)
        {
            pm.attack += item.valueAttack;
            if (DataManager.Instance != null)
                DataManager.Instance.baseAttack += item.valueAttack;
            Debug.Log($"�нú� �ߵ�! ���� ���ݷ� +{item.valueAttack} �� {pm.attack}");
        }

        // [���� - B-04/��] valueDefense�� ���� ����(baseDefense)�� �ݿ�
        // DataManager.baseDefense�� �����ؾ� �� ���� �� ResetTurnStats()���� ������
        if (item.valueDefense > 0)
        {
            pm.defense += item.valueDefense;
            if (DataManager.Instance != null)
                DataManager.Instance.baseDefense += item.valueDefense;
            Debug.Log($"�нú� �ߵ�! ���� ���� +{item.valueDefense} �� {pm.defense}");
        }

        pm.UpdateUI();
    }
}