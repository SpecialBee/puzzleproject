using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    // [이 부분 추가!] 다른 스크립트에서 HandManager.Instance 로 부를 수 있게 합니다.
    public static HandManager Instance;

    [Header("타일 뽑기 설정")]
    [Tooltip("게임에 등장할 모든 타일 데이터(ScriptableObject)를 넣어주세요.")]
    public List<TileData> availableTiles;

    [Tooltip("뽑힌 타일들이 생성될 화면 하단의 투명 상자 (HandArea)")]
    public Transform handArea;

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

    // [추가된 부분] UI 버튼(턴 종료 버튼)을 클릭했을 때 유니티가 실행해 줄 함수입니다.
    public void OnEndTurnButtonClicked()
    {
        // [추가] HandArea에 자식(타일)이 남아있는지 확인합니다.
        if (handArea.childCount > 0)
        {
            Debug.LogWarning("모든 타일을 판에 배치해야 턴을 마칠 수 있습니다!");
            // 여기서 유저에게 알림 팝업을 띄우거나 버튼을 흔드는 연출을 추가하면 더 좋습니다.
            return;
        }

        Debug.Log("--- 턴 종료! 전투 페이즈 시작 ---");

        if (MonsterManager.Instance != null && PlayerManager.Instance != null)
        {
            // 1. 플레이어가 모아둔 공격력으로 몬스터를 먼저 때립니다!
            MonsterManager.Instance.TakeDamage(PlayerManager.Instance.attack);

            // 2. 몬스터가 살아있다면 플레이어에게 반격합니다!
            if (MonsterManager.Instance.currentHp > 0)
            {
                PlayerManager.Instance.TakeDamage(MonsterManager.Instance.attackDamage);
            }

            // 3. 전투가 한 차례 끝났으니 플레이어의 일회성 스탯(공/방)을 0으로 초기화합니다.
            PlayerManager.Instance.ResetTurnStats();
        }

        // 4. 남은 패를 비우고 5장을 새로 뽑습니다.
        DrawTiles(5);
    }
}