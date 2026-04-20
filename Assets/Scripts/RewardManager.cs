using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("보상 UI 연결")]
    public GameObject rewardPanel;
    public GameObject[] rewardButtons; // 버튼 게임오브젝트 자체를 연결 (켜고 끄기 위함)
    public TextMeshProUGUI[] itemNameTexts;
    public TextMeshProUGUI[] itemDescTexts;

    [Header("아이템 데이터베이스")]
    public List<ItemData> allAvailableItems; // 전체 아이템 리스트 (원본)

    // [추가] 현재 게임에서 획득 가능한 아이템들 (뽑을 때마다 줄어듭니다)
    private List<ItemData> currentItemPool = new List<ItemData>();
    private ItemData[] currentChoices = new ItemData[3];

    void Awake()
    {
        Instance = this;
        rewardPanel.SetActive(false);

        // 시작할 때 원본 리스트에서 복사본 풀을 만듭니다.
        currentItemPool = new List<ItemData>(allAvailableItems);
    }

    public void ShowRewardPopup()
    {
        // 만약 풀이 완전히 비어있다면 팝업을 띄우지 않습니다.
        if (currentItemPool.Count <= 0)
        {
            Debug.Log("더 이상 획득할 아이템이 없습니다!");
            return;
        }

        rewardPanel.SetActive(true);

        // 이번에 보여줄 아이템 개수 결정 (최대 3개, 남은 게 적으면 그만큼만)
        int countToShow = Mathf.Min(3, currentItemPool.Count);

        // 임시 리스트를 만들어 중복 없이 무작위로 뽑습니다.
        List<ItemData> tempPool = new List<ItemData>(currentItemPool);

        for (int i = 0; i < 3; i++)
        {
            if (i < countToShow)
            {
                // 뽑기 로직
                int randomIndex = Random.Range(0, tempPool.Count);
                currentChoices[i] = tempPool[randomIndex];
                tempPool.RemoveAt(randomIndex); // 이번 팝업 내 중복 방지

                // UI 갱신 및 활성화
                rewardButtons[i].SetActive(true);
                itemNameTexts[i].text = currentChoices[i].itemName;
                itemDescTexts[i].text = currentChoices[i].description;
            }
            else
            {
                // 남은 아이템이 3개보다 적다면 나머지 버튼은 숨깁니다.
                rewardButtons[i].SetActive(false);
            }
        }
    }

    public void SelectItem(int buttonIndex)
    {
        ItemData selectedItem = currentChoices[buttonIndex];

        // 1. 인벤토리에 추가
        InventoryManager.Instance.AddItem(selectedItem);

        // 2. [핵심] 획득한 아이템을 전체 풀에서 영구 제거합니다.
        currentItemPool.Remove(selectedItem);

        // 3. 팝업창 닫기 및 다음 스테이지 준비
        rewardPanel.SetActive(false);
        GridManager.Instance.ClearAllTiles();
        MonsterManager.Instance.SetupNextStage();
        HandManager.Instance.DrawTiles(5);
        PlayerManager.Instance.ResetTurnStats();
    }
}