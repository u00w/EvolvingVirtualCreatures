EvolvingVirtualCreatures
=================

Unity implementation of Karl Sims' Evolved Virtual Creatures using a genetic algorithm and neural-network-controlled limb segments.

## Sources

- Evolved Virtual Creatures — http://www.karlsims.com/evolved-virtual-creatures.html

---

## Getting started

1. Clone the repository and open the folder as a Unity project (Unity 2020 LTS or newer recommended).
2. Open the scene: `Assets/EvolvingVirtualCreatures/Scenes/Demo.unity`
3. Press **Play**. Creatures will spawn in a grid and evolve over generations.

The `ProjectSettings/TagManager.asset` file is included in the repo, so the **Creature** layer (index 3) is already defined — no manual layer setup is needed.

---

## How it works

| Component | Role |
|---|---|
| `Factory.cs` | Spawns `count` creatures in an 8-column grid at Start; runs the genetic algorithm loop; handles reproduction every `automaticInterval` seconds |
| `SampleCreature.cs` | Wraps a `Segment` body; captures per-creature `origin`/`forward`/`target` at spawn; evaluates fitness (forward progress + upright reward) |
| `Segment.cs` | One body part; acts as both a sensor and an effector; drives its anchor rotation via a neural network output |
| `Population.cs` | Manages the creature list; runs fitness-proportionate selection and crossover/mutation |
| `DNA.cs` | Gene array encoding neural-network weights; supports crossover and Gaussian mutation |

### Creature–creature collision

All segment GameObjects are on the **Creature** layer (3). `Factory.Start()` calls:

```csharp
Physics.IgnoreLayerCollision(creatureLayer, creatureLayer, true);
```

so creatures pass through each other instead of colliding.

### Spawn-position fix (Unity 2020+)

`Physics.autoSyncTransforms` defaults to `false` in Unity 2020+.  
After instantiating a prefab and calling `AddComponent<Rigidbody>()`, the physics body is registered at the origin. Setting only `transform.position` does **not** update the physics body — the first `FixedUpdate` sync would otherwise snap every creature back to `(0, 0, 0)`.

`Factory.CreateCreature` therefore also sets the Rigidbody's position directly:

```csharp
body.transform.position = position;
var rb = body.GetComponent<Rigidbody>();
if (rb != null)
{
    rb.position = position;
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
}
```


