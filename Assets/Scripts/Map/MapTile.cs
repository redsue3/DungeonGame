public enum TileType
{
    Empty,
    NormalEnemy,   // 일반 조우 (적 1마리)
    GroupEnemy,    // 집단 조우 (적 2~3마리)
    EliteEnemy,    // 엘리트 조우
    Boss,          // 보스
    Rest,          // 휴식
    Shop,          // 상점
    Shrine,        // 성소 - 캐릭터 이벤트, 카드 제작
}
