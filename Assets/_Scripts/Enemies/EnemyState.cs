/// <summary>
/// Represents the high-level behavior states used by EnemyController's enum-based AI state machine.
/// </summary>
public enum EnemyState
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Fleeing,
    Hurt,
    Dead
}
