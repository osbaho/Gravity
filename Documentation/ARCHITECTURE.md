# Arquitectura y Sistemas

Este documento explica la arquitectura del proyecto, el flujo de gameplay, y cómo interactúan los diferentes sistemas.

---

## 📐 Arquitectura General

### Patrón de Diseño

**Gravity Defenders** utiliza una arquitectura híbrida:

1. **Singleton Managers:** Sistemas centralizados de acceso global
2. **Event-Driven:** Comunicación desacoplada vía UnityEvents y eventos C#
3. **Component-Based:** GameObject composition para entidades (enemigos, torretas, nave)
4. **ScriptableObject Configuration:** Data-driven design para balance y configuración

### Jerarquía de Inicialización

```
SceneBootstrap (ExecutionOrder: -10000)
  ↓ Crea/valida Managers
  ↓
ResourceManager (Awake)
MetaProgressionManager (Awake, lee mejoras permanentes)
UpgradeManager (Awake)
MapManager (Awake)
MiningManager (Awake)
WaveManager (Awake)
GameManager (Awake)
  ↓
GameManager (Start)
  ├─ MetaProgressionManager.ReapplyPermanentUpgrades()
  ├─ MetaProgressionManager.ApplyRunModifiers(this)
  └─ UpgradeManager.ResetRunStateStatic()
  ↓
WaveManager.Start() → Inicia ciclo de oleadas
```

---

## 🎯 Managers Principales

### GameManager
**Responsabilidad:** Flow principal del juego, game over, victoria

**API Pública:**
```csharp
public static GameManager Instance { get; }
public Transform CentralShipTransform { get; }
public CentralShip CentralShip { get; }
public bool ShipPartCollectedThisRun { get; }

// Eventos C#
public event Action ShipPartCollected;
public event Action RunStarted;

// UnityEvents (Inspector)
public UnityEvent ShipPartCollectedEvent { get; }
public UnityEvent RunStartedEvent { get; }
public UnityEvent GameOverEvent { get; }
public UnityEvent WinEvent { get; }

// Métodos
public void SetInitialGameValues(int primaryResourceBonus, int shipHealthBonus);
public bool RegisterShipPartCollected();
public void GameOver();
public void WinGame();
```

**Flujo:**
1. `Start()`: Aplica mejoras permanentes y modifiers
2. Suscribe a `CentralShip.Health.OnDeath` para game over
3. `RegisterShipPartCollected()`: Llamado por `ShipPartPickup` al colectar
4. `GameOver()`: Pausa juego, otorga 10% de recursos invertidos como moneda permanente, reinicia escena tras 3s
5. `WinGame()`: Se invoca cuando se colectan todas las partes requeridas

**Dependencias:**
- `CentralShip` (opcional, busca con FindFirstObjectByType si no se asigna)
- `MetaProgressionManager`
- `ResourceManager`
- `UpgradeManager`

---

### ResourceManager
**Responsabilidad:** Gestión de recursos (primarios, minería A/B, moneda permanente)

**API Pública:**
```csharp
public static ResourceManager Instance { get; }
public int PrimaryResource { get; }
public int MiningResourceA { get; }
public int MiningResourceB { get; }
public int PermanentCurrency { get; }

public static float resourceGainOnKillMultiplier = 1f; // Modificable por upgrades

// Eventos
public event Action<int> PrimaryResourceChanged;
public event Action<int> MiningResourceAChanged;
public event Action<int> MiningResourceBChanged;
public event Action<int> PermanentCurrencyChanged;

// Métodos
public void AddResources(int amount); // Multiplica por resourceGainOnKillMultiplier
public void AddMiningResources(int amountA, int amountB);
public void AddPermanentCurrency(int amount);
public bool HasEnoughResources(int primary, int miningA, int miningB);
public bool HasEnoughPermanentCurrency(int amount);
public void SpendResources(int primary, int miningA, int miningB);
public void SpendPermanentCurrency(int amount);
public int GetTotalResourcesInvestedInRun();
public void ResetRunResources();
public void GrantStartingPrimaryResource(int amount);
```

**Flujo:**
- Enemigos llaman `AddResources()` al morir (vía `Health.TakeDamage()`)
- `MiningManager` llama `AddMiningResources()` al minar vetas
- `TurretBuilder` llama `SpendResources()` al construir torreta
- `MetaProgressionManager` llama `SpendPermanentCurrency()` al comprar upgrade permanente
- `GameOver()` otorga 10% de recursos invertidos como permanentes

---

### WaveManager
**Responsabilidad:** Spawn de enemigos, oleadas, dificultad escalable

**API Pública:**
```csharp
public static WaveManager Instance { get; }

// Eventos
public event Action<int> WaveStarted;
public event Action<int> WaveCompleted;
public event Action<UpgradeData[]> UpgradeOptionsGenerated;

public UnityEvent<int> WaveStartedEvent { get; }
public UnityEvent<int> WaveCompletedEvent { get; }
public UnityEvent<float> CalmPhaseEvent { get; } // Duración de calma
public UnityEvent<UpgradeData[]> UpgradeOptionsGeneratedEvent { get; }

// Métodos internos
internal void NotifyShipPartPickupRemoved(); // Llamado por ShipPartPickup
```

**Configuración (Inspector):**
```csharp
[SerializeField] private float calmPhaseDuration = 8f;
[SerializeField] private float spawnInterval = 0.4f;
[SerializeField] private int baseEnemiesPerWave = 8;
[SerializeField] private int enemiesPerWaveGrowth = 2;
[SerializeField] private int wavesPerUpgrade = 3;
[SerializeField] private GameObject shipPartPickupPrefab;
[SerializeField, Range(0f, 1f)] private float shipPartDropChance = 0.04f;
[SerializeField] private List<Transform> spawnPoints;
[SerializeField] private EnemyArchetype swarmerArchetype;
[SerializeField] private EnemyArchetype tankArchetype;
[SerializeField] private EnemyArchetype rangedArchetype;
```

**Flujo:**
1. `WaveCycle()` coroutine:
   - Calma → `CalmPhaseEvent(duration)`
   - Spawn oleada → `WaveStartedEvent(waveIndex)`
   - Espera a que enemigos mueran
   - `WaveCompletedEvent(waveIndex)`
   - Cada 3 oleadas: `TriggerUpgradeSelection()` → pausa juego
2. Composición de oleada por dificultad:
   - Oleadas 1-3: Solo Swarmers
   - Oleadas 4-7: Swarmers + Tanks
   - Oleadas 8+: Swarmers + Tanks + Ranged
3. Escalado de stats por oleada:
   - `healthGrowthPerWave` (default 10% por oleada)
   - `damageGrowthPerWave` (default 5% por oleada)
   - `speedGrowthPerWave` (default 2% por oleada)
4. Ship part drop:
   - Solo después de oleada 20+
   - Solo si no hay pickup en mundo
   - Solo si no se colectó parte este run
   - Chance configurable (4% default)

**Dependencias:**
- `GameManager` (para CentralShipTransform y ShipPartCollected event)
- `UpgradeManager` (para opciones de upgrade)
- `EnemyArchetype` ScriptableObjects

---

### MetaProgressionManager
**Responsabilidad:** Mejoras permanentes entre runs, progreso de partes de nave

**API Pública:**
```csharp
public static MetaProgressionManager Instance { get; }

public int ShipPartsCollected { get; }
public int ShipPartsRequiredForVictory { get; }
public int StartingPrimaryResourceBonus { get; }
public int StartingShipHealthBonus { get; }

// Eventos
public event Action<PermanentUpgradeData> PermanentUpgradePurchased;
public event Action<int, int> ShipPartsProgressChanged; // (collected, required)

// Métodos
public void PurchaseUpgrade(PermanentUpgradeData upgrade);
public void ReapplyPermanentUpgrades(); // Llamado al inicio de cada run
public void ApplyRunModifiers(GameManager gameManager);
public bool TryRecordShipPart();
public bool HasCollectedAllShipParts();
public IReadOnlyList<PermanentUpgradeData> GetAllPermanentUpgrades();
public IReadOnlyList<PermanentUpgradeData> GetPurchasedUpgrades();
public bool IsUpgradePurchased(PermanentUpgradeData upgrade);
```

**Mejoras Permanentes (PermanentUpgradeType):**
```csharp
StartingPrimaryResource       // +X recursos al inicio de run
StartingShipHealth           // +X vida de nave al inicio de run
GlobalTurretDamageBonus      // +X% daño de torretas (Projectile.damageMultiplier)
GlobalTurretFireRateBonus    // +X% fire rate (Turret.fireRateMultiplier)
GlobalTurretRangeBonus       // +X% rango (Turret.rangeMultiplier)
GlobalMiningYieldBonus       // +X% yield de minería (MiningManager.miningYieldMultiplier)
GlobalResourceGainOnKillBonus // +X% recursos por kill (ResourceManager.resourceGainOnKillMultiplier)
GlobalEnemySlowBonus         // +X slow global a enemigos (Enemy.globalSlowFactor)
GlobalEnemyHealthReductionBonus // -X% vida de enemigos (Health.globalHealthReductionFactor)
```

**Flujo:**
1. `Start()`: Aplica mejoras compradas (de PlayerPrefs en futuro)
2. `GameManager.Start()` llama `ReapplyPermanentUpgrades()` y `ApplyRunModifiers()`
3. UI llama `PurchaseUpgrade()` al comprar con moneda permanente
4. `TryRecordShipPart()`: Incrementa contador, verifica victoria

**Dependencias:**
- `ResourceManager` (para gastar moneda permanente)
- Multipliers estáticos en: `Projectile`, `Turret`, `MiningManager`, `ResourceManager`, `Enemy`, `Health`

---

### UpgradeManager
**Responsabilidad:** Mejoras temporales durante el run (cada 3 oleadas)

**API Pública:**
```csharp
public static UpgradeManager Instance { get; }

// Eventos
public event Action<UpgradeData[]> UpgradeOptionsGenerated;
public event Action<UpgradeData> UpgradeApplied;

// Métodos
public List<UpgradeData> GetRandomUpgrades(int count);
public void ApplyUpgrade(UpgradeData upgrade);
public void ResetRunState(); // Limpia último upgrade seleccionado
public static void ResetRunStateStatic();
```

**Tipos de Mejora (UpgradeType):**
```csharp
TurretDamage              // +X% Projectile.damageMultiplier
TurretFireRate            // +X% Turret.fireRateMultiplier
TurretRange               // +X% Turret.rangeMultiplier
MiningYield               // +X% MiningManager.miningYieldMultiplier
ResourceGainOnKill        // +X% ResourceManager.resourceGainOnKillMultiplier
GlobalEnemySlow           // +X Enemy.globalSlowFactor
GlobalEnemyHealthReduction // +X Health.globalHealthReductionFactor
```

**Flujo:**
1. `WaveManager` invoca `GetRandomUpgrades(3)` cada 3 oleadas
2. UI muestra opciones y llama `ApplyUpgrade(selectedUpgrade)`
3. Upgrade incrementa multiplier correspondiente
4. `ResetRunState()` al inicio de nuevo run (evita repetir último upgrade)

**Anti-repetición:** Evita ofrecer el último upgrade seleccionado en las opciones actuales

---

### MapManager
**Responsabilidad:** Ondas gravitacionales, expansión de escudo a zonas

**API Pública:**
```csharp
public static MapManager Instance { get; }

public TurretCost ShieldExpansionCost { get; }

// Eventos
public event Action<float> GravitationalWaveTimerTick; // Tiempo restante
public event Action GravitationalWaveTriggered;

// Métodos
public bool TryExpandShieldToZone(MapZone zoneToShield);
```

**Configuración:**
```csharp
[SerializeField] private float timeBetweenWaves = 60f;
[SerializeField] private TurretCost shieldExpansionCost;
```

**Flujo:**
1. `Update()`: Decrementa timer, invoca `GravitationalWaveTimerTick(remainingTime)`
2. Al llegar a 0: `TriggerGravitationalWave()`
   - Obtiene zonas no protegidas
   - Shuffle de posiciones
   - `MapZone.TransformTo(newPosition)` para cada zona
3. `TryExpandShieldToZone()`:
   - Verifica recursos
   - Gasta recursos
   - `zone.SetShielded(true)`
   - Puede triggear spawn de veta de recursos

**Dependencias:**
- `MapZone` components en escena
- `ResourceManager`

---

### MiningManager
**Responsabilidad:** Input de minería, raycast a vetas de recursos

**API Pública:**
```csharp
public static MiningManager Instance { get; }
public static float miningYieldMultiplier = 1f;

// Eventos
public UnityEvent<int, MiningResourceType> ResourcesMinedEvent { get; }
```

**Configuración:**
```csharp
[SerializeField] private int miningAmountPerClick = 50;
[SerializeField] private LayerMask miningLayerMask;
```

**Flujo:**
1. `Update()`: Detecta `Mouse.current.leftButton.wasPressedThisFrame`
2. Raycast desde mouse position
3. Si hit en `ResourceVein`:
   - `vein.Mine(amount * miningYieldMultiplier)`
   - `ResourceManager.AddMiningResources()`
   - `ResourcesMinedEvent(amount, type)`

**Nota:** Requiere `ENABLE_INPUT_SYSTEM` definido (falla compilación si no)

---

## 🎮 Flujo de Gameplay

### Inicio de Run

```
SceneBootstrap.Awake()
  → Crea managers faltantes
  ↓
GameManager.Start()
  → MetaProgressionManager.ReapplyPermanentUpgrades()
      - Resetea multipliers a defaults
      - Aplica upgrades permanentes comprados
  → MetaProgressionManager.ApplyRunModifiers(this)
      - SetInitialGameValues(bonusResources, bonusHealth)
  → UpgradeManager.ResetRunStateStatic()
  → Suscribe a CentralShip.Health.OnDeath
  → ResourceManager.GrantStartingPrimaryResource(bonus)
  → CentralShip.Health.AddHealth(bonus)
  → RunStarted event
  ↓
WaveManager.Start()
  → WaveCycle() coroutine inicia
```

### Ciclo de Oleada

```
Calm Phase (8s default)
  → CalmPhaseEvent(duration)
  ↓
SpawnWave()
  → WaveStarted event
  → BuildCompositionForWave(waveIndex)
      - Determina enemigos según dificultad
  → Spawn enemigos con interval
      - ApplyDifficultyScaling(enemy)
      - enemy.SetTarget(CentralShip)
  ↓
Wait enemiesAlive == 0
  ↓
WaveCompleted event
  ↓
Si waveIndex % 3 == 0:
  → TriggerUpgradeSelection()
      - UpgradeManager.GetRandomUpgrades(3)
      - UpgradeOptionsGenerated event
      - Time.timeScale = 0
      - Espera selección
  ↓
Si waveIndex >= 20 y !shipPartInWorld:
  → TryDropShipPart()
      - Random.value < shipPartDropChance
      - Instantiate(shipPartPickupPrefab)
  ↓
Repetir ciclo
```

### Combat

```
Turret.Update()
  → ScanForTarget() cada targetScanInterval
      - FindGameObjectsWithTag(enemyTag)
      - SelectTarget(enemies) según TurretTargetingMode
  → RotateTowardsTarget()
  → fireTimer -= Time.deltaTime
  → Si fireTimer <= 0:
      - PerformAttack()
          - Projectile: FireProjectile()
          - SlowPulse: ApplySlow() + optional damage
          - DirectDamage: TakeDamage()
      - fireTimer = 1 / (fireRate * fireRateMultiplier)
```

```
Projectile.Update()
  → Si target == null: Destroy(gameObject)
  → Mover hacia target
  → Si llegó:
      - HitTarget()
          - Si explosionRadius > 0: OverlapSphere() → AoE damage
          - Si no: damage directo a target
          - damage *= Projectile.damageMultiplier
      - Destroy(gameObject)
```

```
Enemy.Update()
  → UpdateMovement()
      - Si en rango de ataque y StopWhenInRange: no mover
      - Si no: mover hacia target
      - Speed *= (1 - globalSlowFactor) * (1 - temporarySlowFactor)
  → HandleAttack()
      - Si en rango y attackTimer <= 0:
          - Melee: TakeDamage(target)
          - Ranged: Instantiate(projectilePrefab)
          - attackTimer = attackCooldown
```

### Minería

```
MiningManager.Update()
  → Si Mouse.leftButton.wasPressedThisFrame:
      - TryMineAtMousePosition()
          - Raycast desde mouse
          - Si hit ResourceVein:
              - vein.Mine(amount * miningYieldMultiplier)
              - ResourceManager.AddMiningResources()
              - ResourcesMinedEvent(amount, type)
          - Si vein depleted: Destroy(vein)
```

### Construcción de Torreta

```
TurretBuildSlot.OnMouseDown()
  → SlotSelectedEvent(this)
  ↓
TurretBuildMenu.OpenMenu(slot)
  → ShowBlueprints
  ↓
Usuario selecciona blueprint
  → TurretBuildMenu.SelectBlueprint(blueprint)
      - BlueprintSelectedEvent(blueprint)
  ↓
TurretBuildPresenter.ApplySelection(blueprint)
  → TurretBuilder.TryBuildTurret(blueprint, slot)
      - Verifica HasEnoughResources()
      - SpendResources()
      - SpawnTurretForBlueprint()
      - turret.Initialize(archetype)
      - slot.AssignTurret(turret, blueprint)
      - TurretBuiltEvent(blueprint, slot)
```

### Game Over

```
CentralShip.Health.TakeDamage()
  → currentHealth <= 0
      - OnDeath.Invoke()
  ↓
GameManager.GameOver()
  → Time.timeScale = 0
  → investedResources = ResourceManager.GetTotalResourcesInvestedInRun()
  → permanentGain = investedResources * 0.10
  → ResourceManager.AddPermanentCurrency(permanentGain)
  → ResourceManager.ResetRunResources()
  → GameOverEvent.Invoke()
  → Coroutine: Wait 3s realtime
      - Time.timeScale = 1
      - SceneManager.LoadScene(current)
```

### Victoria

```
ShipPartPickup.Collect()
  → GameManager.RegisterShipPartCollected()
      - ShipPartCollectedThisRun = true
      - MetaProgressionManager.TryRecordShipPart()
          - shipPartsCollected++
          - ShipPartsProgressChanged event
      - Si HasCollectedAllShipParts():
          → GameManager.WinGame()
              - Time.timeScale = 0
              - WinEvent.Invoke()
```

---

## 🔗 Comunicación Entre Sistemas

### Event-Driven Architecture

**Tipo 1: Eventos C# (Action)**
```csharp
// Suscripción
GameManager.Instance.RunStarted += OnRunStarted;

// Invocación
RunStarted?.Invoke();
```

**Tipo 2: UnityEvents (Inspector-assignable)**
```csharp
// Definición
[SerializeField] private UnityEvent<int> waveStartedEvent;

// Exposición
public UnityEvent<int> WaveStartedEvent => waveStartedEvent;

// Invocación
waveStartedEvent.Invoke(waveIndex);
```

### Cadenas de Eventos Comunes

**Muerte de Enemigo:**
```
Enemy.Health.TakeDamage(lethal)
  → Health.OnDeath.Invoke()
      → Enemy.NotifyDeath()
          → Enemy.Defeated?.Invoke(this)
              → WaveManager.HandleEnemyDefeated(enemy)
                  → enemiesAlive--
                  → Si oleada completa: WaveCompleted event
  → ResourceManager.AddResources(enemy.resourceDropAmount)
      → ResourceManager.PrimaryResourceChanged?.Invoke(newAmount)
```

**Colectar Parte de Nave:**
```
ShipPartPickup.Collect()
  → GameManager.RegisterShipPartCollected()
      → ShipPartCollectedThisRun = true
      → MetaProgressionManager.TryRecordShipPart()
          → shipPartsCollected++
          → ShipPartsProgressChanged?.Invoke(collected, required)
      → ShipPartCollected?.Invoke()
      → shipPartCollectedEvent.Invoke()
  → WaveManager.NotifyShipPartPickupRemoved()
      → shipPartPresentInWorld = false
```

**Selección de Upgrade:**
```
WaveManager: wavesCleared % wavesPerUpgrade == 0
  → UpgradeManager.GetRandomUpgrades(3)
      → UpgradeOptionsGenerated?.Invoke(options)
  ↓
UpgradePanelUI.ShowUpgrades(options)
  → Time.timeScale = 0
  → Muestra botones
  ↓
Usuario click en UpgradeButton
  → UpgradePanelUI.OnUpgradeSelected(upgrade)
      → Time.timeScale = 1
      → upgradeSelectedEvent.Invoke(upgrade)
  ↓
UpgradePanelPresenter.ApplyUpgrade(upgrade)
  → UpgradeManager.ApplyUpgrade(upgrade)
      → Modifica multiplier correspondiente
      → UpgradeApplied?.Invoke(upgrade)
```

---

## 🧩 Dependencias Entre Sistemas

### Diagrama de Dependencias

```
SceneBootstrap
  ├─ ResourceManager (independiente)
  ├─ MetaProgressionManager
  │   └─ depende de: ResourceManager, Projectile, Turret, MiningManager, Enemy, Health
  ├─ UpgradeManager
  │   └─ depende de: Projectile, Turret, MiningManager, ResourceManager, Enemy, Health
  ├─ MapManager
  │   └─ depende de: ResourceManager, MapZone
  ├─ MiningManager
  │   └─ depende de: ResourceManager, ResourceVein
  ├─ WaveManager
  │   └─ depende de: GameManager, UpgradeManager, EnemyArchetype, Health, Enemy
  └─ GameManager
      └─ depende de: CentralShip, MetaProgressionManager, ResourceManager, UpgradeManager
```

### Multipliers Globales (Shared State)

Estos campos estáticos son modificados por upgrades:

```csharp
// Torretas
Projectile.damageMultiplier (float)
Turret.fireRateMultiplier (float)
Turret.rangeMultiplier (float)

// Recursos
MiningManager.miningYieldMultiplier (float)
ResourceManager.resourceGainOnKillMultiplier (float)

// Enemigos
Enemy.globalSlowFactor (float)
Health.globalHealthReductionFactor (float)
```

**Reset:** `MetaProgressionManager.ResetPersistentEffects()` los vuelve a defaults al inicio de cada run.

---

## 🛠️ Extensibilidad

### Añadir Nuevo Manager

1. Crear clase con patrón singleton:
```csharp
public class NewManager : MonoBehaviour
{
    public static NewManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
```

2. Añadir a `SceneBootstrap.EnsureManagers()`:
```csharp
EnsureSingleton<NewManager>();
```

3. Documentar dependencias y orden de inicialización

### Añadir Nuevo Tipo de Upgrade

1. Añadir enum a `UpgradeType` o `PermanentUpgradeType`
2. Implementar case en `UpgradeManager.ApplyUpgrade()` o `MetaProgressionManager.ApplyUpgradeEffect()`
3. Crear multiplier estático si es necesario
4. Añadir reset en `MetaProgressionManager.ResetPersistentEffects()`

### Añadir Nuevo Tipo de Torreta

1. Crear `TurretArchetype` ScriptableObject
2. Configurar `TurretBlueprint` con archetype y costo
3. Si requiere lógica especial:
   - Extender `Turret` o crear subclase
   - Añadir `TurretAttackType` enum si es ataque nuevo
   - Implementar en `Turret.PerformAttack()`

### Añadir Nuevo Tipo de Enemigo

1. Crear `EnemyArchetype` ScriptableObject
2. Configurar visual prefab y stats
3. Si requiere lógica especial:
   - Extender `Enemy` o crear subclase
   - Añadir `EnemyAttackMode` enum si es ataque nuevo
   - Implementar en `Enemy.HandleAttack()`
4. Añadir a composición de oleada en `WaveManager.BuildCompositionForWave()`

---

## 📊 Performance Considerations

### Singleton Access
- Todos los singletons se cachean en Awake
- Evitar buscar `Instance` en loops (cachear localmente)

### FindObjectsByType
- `SceneBootstrap` usa `FindFirstObjectByType` y `FindObjectsByType` solo en Start
- `GameManager` usa `FindFirstObjectByType<CentralShip>` solo si referencia faltante
- `Turret` usa `FindGameObjectsWithTag` solo en `ScanForTarget()` (configurable interval)

### Coroutines
- `WaveManager.WaveCycle()` corre una sola coroutine infinita
- `GameManager.RestartGameAfterDelay()` usa `WaitForSecondsRealtime` (no afectada por timeScale)
- `MapZone.RepositionRoutine()` una por zona, solo al reposicionar

### Physics
- `MiningManager` usa Raycast una vez por click
- `Projectile.HitTarget()` usa `OverlapSphere` solo si AoE
- Evitar Physics en FixedUpdate a menos que necesario

---

## 🔐 Thread Safety

**Nota:** Unity no permite multithreading con GameObjects/Components. Todo corre en main thread.

- Evitar Jobs/Burst en este proyecto (scope de game jam)
- Si se añade async/await en futuro, usar `UniTask` o similar
- UnityEvents son thread-unsafe por defecto (no invocar desde threads)

---

## 🧪 Testing

### Unit Testing (Opcional)
- Extraer lógica pura a clases estáticas/structs
- Testear cálculos (ej: `WaveComposition.GenerateOrder()`)

### Integration Testing
- Crear escenas de prueba con `SceneBootstrap`
- Verificar que no haya NullReferenceException en consola
- Probar flows completos (oleada → upgrade → game over)

### Manual Testing Checklist
- [ ] Oleadas spawn correctamente
- [ ] Torretas atacan enemigos
- [ ] Minería funciona en vetas
- [ ] Upgrades se aplican visualmente (damage/fire rate aumenta)
- [ ] Game over reinicia escena
- [ ] Ship parts se colectan y avanzan progreso
- [ ] Ondas gravitacionales mueven zonas
- [ ] Expansión de escudo consume recursos

---

**Próximos pasos:** Ver [Setup de Escenas](./SCENE_SETUP.md) para crear escenas funcionales.
