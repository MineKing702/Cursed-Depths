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
- Keep `Avoid Ledges` enabled and set `Ground Layer` to your platforms/terrain so the enemy raycasts down from the current and next leading-foot positions before each horizontal move
- Tune `Ledge Raycast Distance`, `Ledge Look Ahead Distance`, and horizontal/vertical offsets if the rays start too close to, or too far from, the enemy collider
- Keep `Can Jump Over Walls` enabled for enemies that should hop up small ledges or short walls, and set `Wall Layer` to the same layer(s) as climbable terrain
- Tune `Wall Clearance Height`, `Wall Jump Horizontal Speed`, and `Wall Jump Force` per enemy prefab so the jump clears the ledge without launching too far

Animation setup:

- By default, `EnemyController` does **not** assume the player's Bandit animator state values.
- Map your enemy Animator parameters in the `Animation` section (`EnemyState`, `Speed`, `IsMoving`, `Grounded`, `VerticalSpeed`, and attack/hurt/death triggers), or leave fields blank for parameters the controller should not drive.
- Enable `Use Bandit Animator Parameters` only for enemies that intentionally use the Bandit demo controller (`AnimState`, `Grounded`, `AirSpeed`, `Attack`, `Hurt`, `Death`).
- Disable `Drive Animator` if an enemy prefab has its own animation script and should only use this component for AI movement/combat decisions.

The controller checks whether each configured parameter exists before setting it, so missing optional parameters should not produce Animator errors.
