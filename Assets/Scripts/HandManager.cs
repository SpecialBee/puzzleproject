using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
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
        Debug.Log("턴 종료! 남은 패를 버리고 새로운 5장을 드로우합니다.");

        // (나중에 이곳에 몬스터가 공격하는 로직이나, 턴 종료 시 발동하는 효과 등을 넣을 수 있습니다.)

        // 남은 패를 비우고 5장을 새로 뽑는 함수를 호출합니다.
        DrawTiles(5);
    }
}