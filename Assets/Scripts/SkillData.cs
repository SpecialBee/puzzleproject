using UnityEngine;

// 유니티 우클릭 메뉴에서 스킬 데이터를 쉽게 생성하게 해줍니다.
[CreateAssetMenu(fileName = "New Skill", menuName = "BingoGame/Skill Data")]
public class SkillData : ScriptableObject
{
    public enum SkillEffect
    {
        // 제안 1: 퍼즐 조작계
        RerollHand,     // 손패 리롤
        DestroyTarget,  // 지정 타일 파괴
        MakeJoker,      // 조커 타일 변환

        // 제안 2: 생존/전투계 (추후 구현)
        ManaShield,
        Heal,
        DirectDamage,

        // 제안 3: 시스템 연출계 (추후 구현)
        SkipMonsterTurn,
        CleanseBoard
    }

    [Header("스킬 기본 정보")]
    public string skillName;
    [TextArea] public string description;
    public int manaCost;          // 소모 마나량

    [Header("스킬 효과 설정")]
    public SkillEffect effect;    // 어떤 효과를 낼 것인가?
    public int value;             // 범용 수치 (회복량, 데미지량 등)

    [Tooltip("체크하면 스킬 사용 시 보드판이나 손패를 클릭해야 발동합니다.")]
    public bool requiresTarget;   // 타겟팅(클릭)이 필요한 스킬인가?
}