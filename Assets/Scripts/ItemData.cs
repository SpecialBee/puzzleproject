using UnityEngine;

// 타일 데이터를 만들 때처럼, 우클릭 메뉴에서 쉽게 아이템을 찍어낼 수 있게 해줍니다.
[CreateAssetMenu(fileName = "New Item Data", menuName = "BingoGame/Item Data")]
public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        Passive, // 영구 적용 (예: 최대 체력 증가, 매 턴 공격력 추가)
        Active   // 일회성 사용 (예: 즉시 체력 회복, 특정 타일 파괴 등)
    }

    [Header("아이템 기본 정보")]
    public string itemName; // 아이템 이름
    [TextArea]
    public string description; // 아이템 설명
    public ItemType itemType; // 아이템 종류

    // [수정된 부분] Sprite 대신 GameObject(프리팹)를 받도록 변경했습니다.
    [Tooltip("화면에 보여질 아이템 UI 프리팹을 넣는 곳입니다.")]
    public GameObject itemPrefab;

    [Header("아이템 효과 수치")]
    [Tooltip("패시브일 경우 최대 체력 증가량, 액티브일 경우 즉시 회복량 등으로 사용")]
    public int valueHP;
    [Tooltip("기본 공격력을 영구적으로 올려주는 수치 등")]
    public int valueAttack;
    public int valueDefense;
}