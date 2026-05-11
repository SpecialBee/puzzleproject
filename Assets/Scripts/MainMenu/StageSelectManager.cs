using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectManager : MonoBehaviour
{
    [Header("Stage Button Prefab")]
    [Tooltip("StageNode가 포함된 버튼 프리팹을 연결하세요.")]
    public GameObject stageButtonPrefab;

    [Header("Button Container")]
    [Tooltip("생성된 스테이지 버튼을 배치할 부모 컨테이너를 연결하세요.")]
    public Transform buttonContainer;

    [Header("Stage Settings")]
    [Tooltip("생성할 스테이지 버튼 개수입니다.")]
    public int stageCount = 10;

    [Tooltip("스테이지 번호 시작 값입니다. (예: 1이면 STAGE 1, STAGE 2, ... 생성)")]
    public int startStageNumber = 1;

    [Tooltip("전투 씬 이름입니다.")]
    public string battleSceneName = "BattleScene";

    [Tooltip("최소 잠금 해제 스테이지를 지정합니다. DataManager가 없으면 이 값이 사용됩니다.")]
    public int defaultUnlockedStage = 1;

    private void Start()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.totalStageCount = startStageNumber + stageCount - 1;
        }
        CreateStageButtons();
    }

    public void CreateStageButtons()
    {
        if (stageButtonPrefab == null || buttonContainer == null)
        {
            Debug.LogError("StageSelectManager: stageButtonPrefab 또는 buttonContainer가 설정되어 있지 않습니다.");
            return;
        }

        int unlockedStage = GetUnlockedStage();

        // 기존 버튼 정리
        List<Transform> existingChildren = new List<Transform>();
        foreach (Transform child in buttonContainer)
            existingChildren.Add(child);
        foreach (Transform child in existingChildren)
            Destroy(child.gameObject);

        for (int i = 0; i < stageCount; i++)
        {
            int stage = startStageNumber + i;
            GameObject buttonObject = Instantiate(stageButtonPrefab);
            buttonObject.transform.SetParent(buttonContainer, false);
            StageNode stageNode = buttonObject.GetComponent<StageNode>();
            if (stageNode == null)
            {
                Debug.LogError("StageSelectManager: stageButtonPrefab에 StageNode 컴포넌트가 없습니다.");
                Destroy(buttonObject);
                continue;
            }

            bool unlocked = stage == unlockedStage;
            stageNode.SetStage(stage, battleSceneName, unlocked);

            // StageNode가 Button과 연결되어 있지 않으면 자동 연결 시도
            if (stageNode.stageButton == null)
            {
                Button button = buttonObject.GetComponent<Button>();
                if (button != null)
                    stageNode.stageButton = button;
            }

            // 버튼 클릭이 StageNode.OnClickNode를 호출하도록 보장
            if (stageNode.stageButton != null)
            {
                stageNode.stageButton.onClick.RemoveAllListeners();
                stageNode.stageButton.onClick.AddListener(stageNode.OnClickNode);
            }
        }
    }

    private int GetUnlockedStage()
    {
        if (DataManager.Instance != null)
            return Mathf.Max(DataManager.Instance.maxUnlockedStage, defaultUnlockedStage);
        return defaultUnlockedStage;
    }
}
