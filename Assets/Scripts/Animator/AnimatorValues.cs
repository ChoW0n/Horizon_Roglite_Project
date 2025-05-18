namespace HRP.AnimatorCoder
{
    // 애니메이션 상태 이름
    public enum Animations
    {
        // 플레이어 관련
        IDLE,
        WALK,
        RUN,
        DASH,
        DASH_Enhancement,
        ATTACK,
        DOWN_ATTACK,
        UP_ATTACK,
        JUMP,
        FALL,
        LAND,
        HIT,
        WALLJUMP,
        WALLSLIDE,
        DEAD,
        RESPAWN,
        // 적 관련
        ENEMY_IDLE,
        ENEMY_WALK,
        ENEMY_STRIKE,
        ENEMY_FLY,
        ENEMY_HURT,
        ENEMY_DEAD,
        RESET
    }

    // 애니메이션 파라미터
    public enum Parameters
    {
        GROUNDED,
        FALLING,
        STRIKING
    }
}