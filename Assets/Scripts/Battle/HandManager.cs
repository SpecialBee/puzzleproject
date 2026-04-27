using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("타일 뽑기 설정")]
    [Tooltip("게임에 등장할 모든 타일 데이터(ScriptableObject)를 넣어주세요.")]
    public List<TileData> availableTiles;

    [Tooltip("뽑힌 타일들이 생성될 화면 하단의 투명 상자 (HandArea)")]
    public Transform handArea;

    // [추가] 턴이 진행되는 동안 버튼 중복 클릭을 막기 위한 자물쇠입니다.
    private bool isTurnProcessing = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 게임이 시작되면 첫 턴을 위해 5장을 뽑습니다.
        DrawTiles(5);
    }

    public void DrawTiles(int amount)
    {
        // 1. 하단에 남아있는 기존 타일들을 모두 파괴(버린 패 처리)합니다.
        foreach (Transform child in handArea)
        {
            Destroy(child.gameObject);
        }

        // 2. 지정된 개수(amount)만큼 새로운 타일을 무작위로 뽑아 생성합니다.
        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, availableTiles.Count);
            TileData selectedData = availableTiles[randomIndex];

            GameObject newTile = Instantiate(selectedData.tilePrefab, handArea);
            newTile.transform.localScale = Vector3.one;

            // 3. 프리팹은 껍데기일 뿐이므로, 방금 당첨된 진짜 데이터(selectedData)를 주입해 줍니다.
            newTile.GetComponent<TileDrag>().myTileData = selectedData;
        }
    }

    // UI 버튼(턴 종료 버튼)을 클릭했을 때 유니티가 실행해 줄 함수입니다.
    public void OnEndTurnButtonClicked()
    {
        // 이미 턴이 진행 중이라면 버튼을 눌러도 무시합니다.
        if (isTurnProcessing) return;

        // HandArea에 자식(타일)이 남아있는지 확인합니다.
        if (handArea.childCount > 0)
        {
            Debug.LogWarning("모든 타일을 판에 배치해야 턴을 마칠 수 있습니다!");
            return;
        }

        // 순차적인 턴 흐름(코루틴)을 시작합니다!
        StartCoroutine(TurnPhaseSequence());
    }

    // ----------------------------------------------------
    // [핵심] 시간의 흐름을 두고 턴을 순차적으로 진행하는 모듈
    // ----------------------------------------------------
    private IEnumerator TurnPhaseSequence()
    {
        isTurnProcessing = true; // 턴 진행 잠금

        // --- 페이즈 1: 플레이어 턴 ---
        Debug.Log("--- [페이즈 1: 플레이어 턴] ---");
        if (MonsterManager.Instance != null && PlayerManager.Instance != null)
        {
            MonsterManager.Instance.TakeDamage(PlayerManager.Instance.attack);
        }

        // 몬스터가 죽었다면 보상 팝업이 뜨므로, 여기서 턴 시퀀스를 즉시 중단합니다.
        // (나머지 초기화 및 드로우는 보상 선택 후 RewardManager가 알아서 합니다)
        if (MonsterManager.Instance.currentHp <= 0)
        {
            isTurnProcessing = false;
            yield break;
        }

        yield return new WaitForSeconds(0.5f); // 0.5초 대기 (타격감 및 흐름 분리)

        // --- 페이즈 2: 몬스터 턴 ---
        Debug.Log("--- [페이즈 2: 몬스터 턴] ---");
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.PerformMonsterTurn(); // 데미지 + 기믹 발동
        }

        // 몬스터 공격으로 플레이어가 죽었다면(게임오버) 역시 중단합니다.
        if (PlayerManager.Instance.hp <= 0)
        {
            isTurnProcessing = false;
            yield break;
        }

        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        // --- 페이즈 3: 턴 종료 및 결산 ---
        Debug.Log("--- [페이즈 3: 턴 결산 및 종료] ---");

        PlayerManager.Instance.ResetTurnStats(); // 내 공/방 스탯 일회성 초기화
        GridManager.Instance.ProcessTurnEndForGrid(); // 보드판 기믹 턴수 차감 (예: 잠금 지속시간 -1)
        DrawTiles(5); // 새로운 패 지급

        isTurnProcessing = false; // 턴 진행 잠금 해제, 이제 다시 타일을 놓을 수 있습니다.
    }
}