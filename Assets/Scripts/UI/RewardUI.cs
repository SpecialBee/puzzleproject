using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardUI : MonoBehaviour
{
    public static RewardUI Instance;

    [Header("Reward UI")]
    public GameObject rewardButtonPrefab;
    public Transform buttonContainer;
    public GameObject rewardPanel;

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

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    public void Show()
    {
        if (RewardManager.Instance == null)
        {
            Debug.LogError("RewardUI.Show: RewardManager 인스턴스가 없습니다.");
            return;
        }

        if (rewardButtonPrefab == null)
        {
            Debug.LogError("RewardUI.Show: rewardButtonPrefab이 할당되지 않았습니다.");
            return;
        }

        if (buttonContainer == null)
        {
            Debug.LogError("RewardUI.Show: buttonContainer가 할당되지 않았습니다.");
            return;
        }

        if (rewardPanel == null)
        {
            Debug.LogError("RewardUI.Show: rewardPanel이 할당되지 않았습니다.");
            return;
        }

        List<ItemData> choices = RewardManager.Instance.DrawChoices();
        ClearButtons();

        if (choices.Count == 0)
        {
            Debug.Log("RewardUI: 선택 가능한 보상 아이템이 없습니다. 즉시 맵으로 이동합니다.");
            rewardPanel.SetActive(false);
            if (PlayerManager.Instance != null)
                PlayerManager.Instance.SaveAndReturnToMap();
            return;
        }

        foreach (ItemData choice in choices)
        {
            GameObject buttonObject = Instantiate(rewardButtonPrefab, buttonContainer);
            buttonObject.transform.localScale = Vector3.one;

            Button button = buttonObject.GetComponentInChildren<Button>(true);
            if (button == null)
            {
                Debug.LogError("RewardUI.Show: rewardButtonPrefab에 Button 컴포넌트가 없습니다.");
                continue;
            }

            TextMeshProUGUI[] texts = buttonObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (texts.Length >= 2)
            {
                texts[0].text = choice.itemName;
                texts[1].text = choice.description;
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{choice.itemName}\n{choice.description}";
            }

            ItemData selectedItem = choice;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => RewardManager.Instance.ConfirmSelection(selectedItem));
        }

        rewardPanel.SetActive(true);
    }

    private void ClearButtons()
    {
        if (buttonContainer == null)
            return;

        for (int i = buttonContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonContainer.GetChild(i).gameObject);
        }
    }
}
