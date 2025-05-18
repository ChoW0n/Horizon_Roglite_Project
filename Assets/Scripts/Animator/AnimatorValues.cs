namespace HRP.AnimatorCoder
{
    // �ִϸ��̼� ���� �̸�
    public enum Animations
    {
        // �÷��̾� ����
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
        // �� ����
        ENEMY_IDLE,
        ENEMY_WALK,
        ENEMY_STRIKE,
        ENEMY_FLY,
        ENEMY_HURT,
        ENEMY_DEAD,
        RESET
    }

    // �ִϸ��̼� �Ķ����
    public enum Parameters
    {
        GROUNDED,
        FALLING,
        STRIKING
    }
}