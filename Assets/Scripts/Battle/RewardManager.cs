using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardManager : MonoBehaviour
{
    // [수정 - S-01] 싱글톤 중복 체크 추가
    public static RewardManager Instance;

    [Header("보상 UI 연결")]
    public GameObject rewardPanel;
    public GameObject[] rewardButtons;
    public TextMeshProUGUI[] itemNameTexts;
    public TextMeshProUGUI[] itemDescTexts;

    [Header("아이템 데이터베이스")]
    public List<ItemData> allAvailableItems;

    private List<ItemData> currentItemPool = new List<ItemData>();
    private ItemData[] currentChoices = new ItemData[3];

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        rewardPanel.SetActive(false);
        currentItemPool = new List<ItemData>(allAvailableItems);
    }

    public void ShowRewardPopup()
    {
        if (currentItemPool.Count <= 0)
        {
            Debug.Log("더 이상 획득할 아이템이 없습니다!");
            return;
        }

        rewardPanel.SetActive(true);

        int countToShow = Mathf.Min(3, currentItemPool.Count);
        List<ItemData> tempPool = new List<ItemData>(currentItemPool);

        for (int i = 0; i < 3; i++)
        {
            if (i < countToShow)
            {
                int ri = Random.Range(0, tempPool.Count);
                currentChoices[i] = tempPool[ri];
                tempPool.RemoveAt(ri);

                rewardButtons[i].SetActive(true);
                itemNameTexts[i].text = currentChoices[i].itemName;
                itemDescTexts[i].text = currentChoices[i].description;
            }
            else
            {
                rewardButtons[i].SetActive(false);
            }
        }
    }

    public void SelectItem(int buttonIndex)
    {
        if (InventoryManager.Instance == null) { Debug.LogError("🚨 InventoryManager 없음!"); return; }
        if (GridManager.Instance == null) { Debug.LogError("🚨 GridManager 없음!"); return; }
        if (PlayerManager.Instance == null) { Debug.LogError("🚨 PlayerManager 없음!"); return; }

        ItemData selectedItem = currentChoices[buttonIndex];

        // 1. 인벤토리에 추가
        InventoryManager.Instance.AddItem(selectedItem);

        // 2. 획득한 아이템 풀에서 영구 제거
        currentItemPool.Remove(selectedItem);

        // 3. 팝업 닫기 + 보드 초기화
        rewardPanel.SetActive(false);
        GridManager.Instance.ClearAllTiles();

        // [수정 - B-03] DrawTiles(5) 호출 제거
        // 새 BattleScene 진입 시 HandManager.Start()에서 자동으로 DrawTiles(5)가 호출됩니다.

        // 4. 스탯 저장 후 맵으로 귀환 (내부에서 currentStage++ 처리)
        PlayerManager.Instance.SaveAndReturnToMap();
    }
}