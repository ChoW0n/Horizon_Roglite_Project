namespace HRP.AnimatorCoder
{
    // 애니메이션 상태 이름
    public enum Animations
    {
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
        RESET
    }

    // 애니메이션 파라미터
    public enum Parameters
    {
        GROUNDED,
        FALLING
    }
}