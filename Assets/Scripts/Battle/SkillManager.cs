using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    [Header("타겟팅 상태")]
    public SkillData currentTargetingSkill = null; // 현재 타겟팅 대기 중인 스킬

    void Awake()
    {
        Instance = this;
    }

    // UI 스킬 버튼을 누르면 이 함수가 실행됩니다.
    public void CastSkill(SkillData skill)
    {
        if (PlayerManager.Instance.mana < skill.manaCost)
        {
            Debug.LogWarning("💧 마나가 부족합니다!");
            return;
        }

        if (skill.requiresTarget)
        {
            // 타겟팅 모드로 진입 (보드판 클릭을 기다림)
            currentTargetingSkill = skill;
            Debug.Log($"🎯 [{skill.skillName}] 발동 대기! 파괴할 타일을 클릭하세요.");
            // (추후 여기에 마우스 커서 모양을 바꾸는 코드를 넣으면 좋습니다)
        }
        else
        {
            // 타겟팅이 필요 없는 즉발 스킬(리롤 등)은 마나를 빼고 즉시 실행!
            if (PlayerManager.Instance.UseMana(skill.manaCost))
            {
                ExecuteSkill(skill);
            }
        }
    }

    // 실제 스킬의 효과가 처리되는 곳입니다. (확장성이 핵심!)
    public void ExecuteSkill(SkillData skill, TileSlot targetSlot = null)
    {
        switch (skill.effect)
        {
            // --- [제안 1] 스킬 구현 ---
            case SkillData.SkillEffect.RerollHand:
                HandManager.Instance.DrawTiles(5); // 기존 패를 싹 날리고 5장 새로 드로우
                break;

            case SkillData.SkillEffect.DestroyTarget:
                if (targetSlot != null && targetSlot.transform.childCount > 0)
                {
                    Destroy(targetSlot.transform.GetChild(0).gameObject); // UI 파괴
                    GridManager.Instance.boardData[targetSlot.gridX, targetSlot.gridY] = null; // 데이터 파괴
                    targetSlot.SetLock(0); // 혹시 잠긴 칸이라면 잠금도 해제
                }
                break;

                // --- [제안 2, 3] 추후 여기에 case만 추가하면 무한 확장 가능! ---
                // case SkillData.SkillEffect.Heal: ...
                // case SkillData.SkillEffect.SkipMonsterTurn: ...
        }

        Debug.Log($"🌟 스킬 발동 성공! [{skill.skillName}]");
        currentTargetingSkill = null; // 타겟팅 상태 해제
    }
}