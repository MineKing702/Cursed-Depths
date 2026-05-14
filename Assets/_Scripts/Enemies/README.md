# Enemy Controller Setup

Add `EnemyController` to enemy GameObjects instead of the input-driven Bandits Pixel Art demo `Bandit` script.

Required components:

- `Rigidbody2D`
- `Animator` using the LightBandit or HeavyBandit controller/override
- One or more `Collider2D` components
- `EnemyController`

Optional components and references:

- A child named `GroundSensor` with `Sensor_Bandit` for grounded and air-speed animator updates
- `Health` and `Coordinate` references if the CursedDepths core package supplies them as assignable Unity objects
- Patrol point transforms assigned to `patrolPoints`
- `playerTarget` assigned manually, or a player GameObject tagged `Player`
- `playerLayer` configured as a fallback lookup when no tagged/manual target is available

The controller drives Bandit-style animator parameters only when they exist, so missing optional parameters should not produce Animator errors.
