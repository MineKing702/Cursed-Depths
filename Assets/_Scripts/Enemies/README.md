# Enemy Controller Setup

Add `EnemyController` to enemy GameObjects instead of the input-driven Bandits Pixel Art demo `Bandit` script.

Required components:

- `Rigidbody2D`
- An enemy-specific `Animator`
- One or more `Collider2D` components
- `EnemyController`

Optional components and references:

- A child named `GroundSensor` with `Sensor_Bandit` when the enemy animator needs grounded/vertical-speed parameters
- `Health` and `Coordinate` references if the CursedDepths core package supplies them as assignable Unity objects
- Patrol point transforms assigned to `patrolPoints`
- `playerTarget` assigned manually, or a player GameObject tagged `Player`
- `playerLayer` configured as a fallback lookup when no tagged/manual target is available
- Keep `Lock Rotation` enabled for normal walking enemies so Rigidbody2D collisions do not make them spin while chasing or fleeing

Animation setup:

- By default, `EnemyController` does **not** assume the player's Bandit animator state values.
- Map your enemy Animator parameters in the `Animation` section (`EnemyState`, `Speed`, `IsMoving`, `Grounded`, `VerticalSpeed`, and attack/hurt/death triggers), or leave fields blank for parameters the controller should not drive.
- Enable `Use Bandit Animator Parameters` only for enemies that intentionally use the Bandit demo controller (`AnimState`, `Grounded`, `AirSpeed`, `Attack`, `Hurt`, `Death`).
- Disable `Drive Animator` if an enemy prefab has its own animation script and should only use this component for AI movement/combat decisions.

The controller checks whether each configured parameter exists before setting it, so missing optional parameters should not produce Animator errors.
