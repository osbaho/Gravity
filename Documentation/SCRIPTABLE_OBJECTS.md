# ScriptableObjects y Configuración

Guía completa para crear y configurar todos los ScriptableObjects del proyecto: arquetipos de torretas y enemigos, blueprints, upgrades y costos.

---

## 📋 Índice

1. [¿Qué son ScriptableObjects?](#qué-son-scriptableobjects)
2. [Enemy Archetypes](#enemy-archetypes)
3. [Turret Archetypes](#turret-archetypes)
4. [Turret Blueprints](#turret-blueprints)
5. [Upgrade Data (Temporal)](#upgrade-data-temporal)
6. [Permanent Upgrade Data](#permanent-upgrade-data)
7. [Turret Cost (Struct)](#turret-cost-struct)
8. [Balanceo y Valores Recomendados](#balanceo-y-valores-recomendados)

---

## 🎯 ¿Qué son ScriptableObjects?

**ScriptableObjects** son assets de Unity que almacenan datos fuera de escenas. Permiten:

- **Configuración centralizada:** Un archetype define stats para todos los enemigos de ese tipo
- **Data-driven design:** Diseñadores pueden balancear sin tocar código
- **Reutilización:** Un archetype puede usarse en múltiples enemigos/torretas
- **Performance:** Se instancian una vez, shared entre instancias

### Crear ScriptableObject en Unity

```
Project Window > Click derecho > Create > Gravity Defenders > [Tipo]
```

O desde menú:
```
Assets > Create > Gravity Defenders > [Tipo]
```

---

## 👾 Enemy Archetypes

**Propósito:** Define stats y comportamiento base de un tipo de enemigo.

### Crear Enemy Archetype

```
1. Project > Create > Gravity Defenders > Enemy Archetype
2. Renombrar: Enemy_[Nombre] (ej: Enemy_Swarmer)
3. Configurar parámetros (ver abajo)
```

### Parámetros Completos

#### Identity
```
Display Name: "Swarmer"
  - Nombre visible en UI/debug
```

#### Base Stats
```
Base Health: 100
  - Vida inicial (escalará con dificultad)
  - Afectado por Health.globalHealthReductionFactor
  
Base Move Speed: 5.0
  - Unidades/segundo
  - Afectado por Enemy.globalSlowFactor
  
Base Damage: 10
  - Daño por ataque
  
Base Resource Drop: 20
  - Recursos que otorga al morir
  - Afectado por ResourceManager.resourceGainOnKillMultiplier
```

#### Attack Configuration
```
Attack Mode: [None, Melee, Ranged]
  - None: No ataca (solo kamikaze/contact)
  - Melee: Ataque cuerpo a cuerpo cuando en rango
  - Ranged: Dispara proyectiles

Attack Range: 1.5
  - Distancia para activar ataque
  - Melee: típico 1.0 - 2.0
  - Ranged: típico 8.0 - 15.0

Attack Cooldown: 1.0
  - Segundos entre ataques
  - Melee: típico 0.5 - 1.5
  - Ranged: típico 1.5 - 3.0

Stop When In Range: true
  - Si true: Se detiene al entrar en rango de ataque
  - Si false: Sigue acercándose mientras ataca
  - Recomendado true para Ranged, false para Melee
```

#### Ranged Attack (solo si Attack Mode = Ranged)
```
Projectile Prefab: [GameObject con Projectile component]
  - Prefab del proyectil a disparar
  - Debe tener Projectile.cs
  
Projectile Spawn Offset: (0, 1, 0)
  - Offset local desde posición del enemigo
  - Ajustar según altura del modelo
```

#### Visual
```
Visual Prefab: [GameObject opcional]
  - Modelo 3D/sprite del enemigo
  - Si null: Enemigo usa GameObject base
  - Si tiene Enemy component: Se usa directo
  - Si no: Se instancia como hijo del enemigo
```

### Ejemplos de Configuración

#### Swarmer (Básico)
```yaml
Display Name: "Swarmer"
Base Health: 100
Base Move Speed: 5.0
Base Damage: 10
Base Resource Drop: 20
Attack Mode: Melee
Attack Range: 1.5
Attack Cooldown: 1.0
Stop When In Range: false
Visual Prefab: Cube (placeholder)
```

#### Tank (Tanque)
```yaml
Display Name: "Tank"
Base Health: 300
Base Move Speed: 3.0
Base Damage: 25
Base Resource Drop: 50
Attack Mode: Melee
Attack Range: 2.0
Attack Cooldown: 1.5
Stop When In Range: false
Visual Prefab: Capsule (placeholder)
```

#### Ranged (Tirador)
```yaml
Display Name: "Ranged"
Base Health: 80
Base Move Speed: 4.0
Base Damage: 15
Base Resource Drop: 30
Attack Mode: Ranged
Attack Range: 12.0
Attack Cooldown: 2.0
Stop When In Range: true
Projectile Prefab: EnemyProjectile
Projectile Spawn Offset: (0, 1, 0)
Visual Prefab: Sphere (placeholder)
```

---

## 🗼 Turret Archetypes

**Propósito:** Define comportamiento y stats base de un tipo de torreta.

### Crear Turret Archetype

```
1. Project > Create > Gravity Defenders > Turret Archetype
2. Renombrar: Turret_[Nombre] (ej: Turret_Basic)
3. Configurar parámetros
```

### Parámetros Completos

#### Identity
```
Display Name: "Basic Turret"
  - Nombre visible
```

#### Core Stats
```
Range: 15.0
  - Radio de detección de enemigos
  - Afectado por Turret.rangeMultiplier
  
Min Range: 0.0
  - Radio mínimo (dead zone)
  - Útil para torretas de largo alcance
  
Turn Speed: 10.0
  - Velocidad de rotación hacia target
  - Mayor = tracking más rápido
  
Fire Rate: 1.5
  - Ataques por segundo
  - Afectado por Turret.fireRateMultiplier
  
Target Scan Interval: 0.25
  - Segundos entre búsquedas de target
  - Menor = tracking más preciso, más cost
```

#### Targeting
```
Enemy Tag: "Enemy"
  - Tag de GameObjects a atacar
  - Debe coincidir con tag de enemigos
  
Targeting Mode: [ClosestToTurret, ClosestToShip, FarthestFromShip, HighestHealth]
  - ClosestToTurret: Prioriza cercanos a esta torreta
  - ClosestToShip: Prioriza cercanos a nave central
  - FarthestFromShip: Prioriza lejanos (interceptar temprano)
  - HighestHealth: Prioriza tanques
```

#### Attack
```
Attack Type: [Projectile, SlowPulse, DirectDamage]
  - Projectile: Dispara proyectil físico
  - SlowPulse: Aplica slow + daño opcional instantáneo
  - DirectDamage: Daño instantáneo (laser/beam)
  
Damage Per Shot: 25
  - Daño base por ataque
  - Afectado por Projectile.damageMultiplier si Apply Global = true
```

#### Projectile Settings (si Attack Type = Projectile)
```
Projectile Prefab: [GameObject con Projectile.cs]
  - Prefab del proyectil
  - Si null: Se crea GameObject básico con Projectile
  
Projectile Speed: 70.0
  - Velocidad del proyectil
  - Mayor = más rápido, menos arc
  
Projectile Explosion Radius: 0.0
  - Radio de AoE
  - 0 = single target
  - >0 = daño en área
  
Muzzle Offset: (0, 1, 0)
  - Offset local del spawn del proyectil
  - Ajustar según modelo de torreta
```

#### Slow Pulse Settings (si Attack Type = SlowPulse)
```
Slow Factor: 0.5 (rango 0-1)
  - Reducción de velocidad (0.5 = 50% slower)
  - Se aplica temporalmente al target
  
Slow Duration: 2.0
  - Segundos que dura el slow
```

#### Direct Damage Settings (para DirectDamage o SlowPulse con daño)
```
Apply Global Damage Multiplier: true
  - Si true: Aplica Projectile.damageMultiplier
  - Si false: Daño fijo sin upgrades
```

### Ejemplos de Configuración

#### Basic Turret (Proyectil Estándar)
```yaml
Display Name: "Basic Turret"
Range: 15.0
Min Range: 0.0
Turn Speed: 10.0
Fire Rate: 1.5
Target Scan Interval: 0.25
Enemy Tag: "Enemy"
Targeting Mode: ClosestToTurret
Attack Type: Projectile
Damage Per Shot: 25
Projectile Speed: 70.0
Projectile Explosion Radius: 0.0
Muzzle Offset: (0, 1, 0)
Apply Global Damage Multiplier: true
```

#### AoE Turret (Explosivo)
```yaml
Display Name: "Cannon"
Range: 12.0
Fire Rate: 0.8
Damage Per Shot: 40
Attack Type: Projectile
Projectile Speed: 50.0
Projectile Explosion Radius: 3.0
Targeting Mode: ClosestToShip
```

#### Slow Turret (Control de Multitudes)
```yaml
Display Name: "Frost Tower"
Range: 18.0
Fire Rate: 1.0
Damage Per Shot: 5
Attack Type: SlowPulse
Slow Factor: 0.7
Slow Duration: 3.0
Apply Global Damage Multiplier: true
Targeting Mode: FarthestFromShip
```

#### Sniper Turret (Largo Alcance)
```yaml
Display Name: "Sniper"
Range: 25.0
Min Range: 5.0
Fire Rate: 0.5
Damage Per Shot: 100
Attack Type: Projectile
Projectile Speed: 120.0
Targeting Mode: HighestHealth
```

---

## 📐 Turret Blueprints

**Propósito:** Envuelve un Turret Archetype con info de UI, costo y prefab custom.

### Crear Turret Blueprint

```
1. Project > Create > Gravity Defenders > Turret Blueprint
2. Renombrar: Blueprint_[Nombre] (ej: Blueprint_Basic)
3. Configurar parámetros
```

### Parámetros

#### Display
```
Turret Name: "Basic Turret"
  - Nombre para UI de construcción
  
Description: "Balanced damage and fire rate. Good all-rounder."
  - Descripción para tooltip/UI
  - Multiline
  
Icon: [Sprite]
  - Icono para botones de UI
  - Tamaño recomendado: 128x128
```

#### Setup
```
Archetype: [Turret Archetype]
  - Referencia al archetype que define comportamiento
  - REQUERIDO
  
Custom Prefab: [GameObject con Turret.cs] (opcional)
  - Prefab completo de la torreta con modelo
  - Si null: Se crea GameObject básico
  
Visual Prefab: [GameObject] (opcional)
  - Modelo 3D/sprite si Custom Prefab es null
  - Se instancia como hijo de la torreta
  
Visual Offset: (0, 0, 0)
  - Offset del visual si no se usa Custom Prefab
```

#### Cost
```
Cost > Primary Resource: 100
  - Costo en recurso primario (drops de enemigos)
  
Cost > Mining Resource A: 0
  - Costo en recurso de minería tipo A
  
Cost > Mining Resource B: 0
  - Costo en recurso de minería tipo B
```

### Ejemplo Completo

```yaml
# Blueprint_Basic.asset
Turret Name: "Basic Turret"
Description: "Balanced turret with moderate damage and fire rate. Effective against swarmers."
Icon: icon_turret_basic.png

Archetype: Turret_Basic
Custom Prefab: null
Visual Prefab: TurretModel_Basic
Visual Offset: (0, 0.5, 0)

Cost:
  Primary Resource: 100
  Mining Resource A: 0
  Mining Resource B: 0
```

```yaml
# Blueprint_Advanced.asset
Turret Name: "Advanced Turret"
Description: "High damage turret requiring mining resources. Excellent against tanks."
Icon: icon_turret_advanced.png

Archetype: Turret_Advanced
Custom Prefab: Turret_Advanced_Prefab
Visual Offset: (0, 0, 0)

Cost:
  Primary Resource: 150
  Mining Resource A: 50
  Mining Resource B: 0
```

---

## 📈 Upgrade Data (Temporal)

**Propósito:** Mejoras que se aplican durante el run actual (cada 3 oleadas).

### Crear Upgrade Data

```
1. Project > Create > Gravity Defenders > Upgrade Data
2. Renombrar: Upgrade_[Nombre] (ej: Upgrade_TurretDamage)
3. Configurar parámetros
```

### Parámetros

```
Upgrade Name: "Increased Turret Damage"
  - Nombre visible en panel de selección
  
Description: "+20% damage to all turrets"
  - Descripción del efecto
  
Icon: [Sprite] (opcional)
  - Icono para UI
  
Upgrade Type: [TurretDamage, TurretFireRate, TurretRange, MiningYield, 
               ResourceGainOnKill, GlobalEnemySlow, GlobalEnemyHealthReduction]
  - Tipo de upgrade (determina qué multiplier afecta)
  
Value: 0.2
  - Valor del upgrade
  - Para multipliers: 0.2 = +20%
  - Se suma al multiplier actual
```

### Tipos de Upgrade y Efectos

```csharp
TurretDamage → Projectile.damageMultiplier
  Value: 0.2 = +20% daño de proyectiles
  
TurretFireRate → Turret.fireRateMultiplier
  Value: 0.15 = +15% velocidad de disparo
  
TurretRange → Turret.rangeMultiplier
  Value: 0.1 = +10% rango de torretas
  
MiningYield → MiningManager.miningYieldMultiplier
  Value: 0.25 = +25% recursos por click minero
  
ResourceGainOnKill → ResourceManager.resourceGainOnKillMultiplier
  Value: 0.3 = +30% recursos por matar enemigos
  
GlobalEnemySlow → Enemy.globalSlowFactor
  Value: 0.1 = 10% slow global a todos los enemigos
  
GlobalEnemyHealthReduction → Health.globalHealthReductionFactor
  Value: 0.15 = -15% vida de enemigos spawneados
```

### Ejemplos

```yaml
# Upgrade_TurretDamage_T1.asset
Upgrade Name: "Damage Boost I"
Description: "+20% turret damage"
Upgrade Type: TurretDamage
Value: 0.2
```

```yaml
# Upgrade_FireRate_T2.asset
Upgrade Name: "Rapid Fire II"
Description: "+30% turret fire rate"
Upgrade Type: TurretFireRate
Value: 0.3
```

```yaml
# Upgrade_MiningYield.asset
Upgrade Name: "Efficient Mining"
Description: "+50% mining yield"
Upgrade Type: MiningYield
Value: 0.5
```

### Configurar Pool de Upgrades

```
1. Hierarchy > UpgradeManager (auto-creado por Bootstrap)
2. Inspector > Upgrades > All Upgrades
3. Arrastrar todos los Upgrade Data assets
4. WaveManager llamará GetRandomUpgrades(3) cada 3 oleadas
```

---

## 💎 Permanent Upgrade Data

**Propósito:** Mejoras permanentes entre runs, compradas con moneda permanente.

### Crear Permanent Upgrade Data

```
1. Project > Create > Gravity Defenders > Permanent Upgrade Data
2. Renombrar: PermanentUpgrade_[Nombre]
3. Configurar parámetros
```

### Parámetros

```
Upgrade Name: "Fortified Ship Hull"
  - Nombre visible en meta progression UI
  
Description: "Ship starts each run with +200 health"
  - Descripción del efecto permanente
  
Icon: [Sprite] (opcional)
  
Upgrade Type: [StartingPrimaryResource, StartingShipHealth, 
               GlobalTurretDamageBonus, GlobalTurretFireRateBonus, GlobalTurretRangeBonus,
               GlobalMiningYieldBonus, GlobalResourceGainOnKillBonus,
               GlobalEnemySlowBonus, GlobalEnemyHealthReductionBonus]
  
Value: 200
  - Valor del upgrade
  - Para bonuses de inicio: cantidad absoluta
  - Para multipliers: incremento (0.1 = +10%)
  
Permanent Currency Cost: 500
  - Costo en moneda permanente
  - Se obtiene al game over (10% de recursos invertidos)
```

### Tipos de Upgrade Permanente

```csharp
StartingPrimaryResource
  Value: 100 = Inicia cada run con +100 recursos primarios
  
StartingShipHealth
  Value: 200 = Nave inicia con +200 vida
  
GlobalTurretDamageBonus
  Value: 0.15 = +15% daño permanente a todas las torretas
  
GlobalTurretFireRateBonus
  Value: 0.1 = +10% fire rate permanente
  
GlobalTurretRangeBonus
  Value: 0.2 = +20% rango permanente
  
GlobalMiningYieldBonus
  Value: 0.25 = +25% yield minero permanente
  
GlobalResourceGainOnKillBonus
  Value: 0.2 = +20% recursos por kill permanente
  
GlobalEnemySlowBonus
  Value: 0.05 = 5% slow global permanente a enemigos
  
GlobalEnemyHealthReductionBonus
  Value: 0.1 = -10% vida de enemigos permanente
```

### Ejemplos

```yaml
# PermanentUpgrade_StartingResources.asset
Upgrade Name: "Resource Cache"
Description: "Start each run with 150 bonus primary resources"
Upgrade Type: StartingPrimaryResource
Value: 150
Permanent Currency Cost: 300
```

```yaml
# PermanentUpgrade_ShipArmor.asset
Upgrade Name: "Reinforced Hull"
Description: "Ship gains +250 maximum health"
Upgrade Type: StartingShipHealth
Value: 250
Permanent Currency Cost: 500
```

```yaml
# PermanentUpgrade_GlobalDamage.asset
Upgrade Name: "Advanced Ammunition"
Description: "All turrets deal 10% more damage permanently"
Upgrade Type: GlobalTurretDamageBonus
Value: 0.1
Permanent Currency Cost: 800
```

### Configurar Upgrades Permanentes

```
1. Hierarchy > MetaProgressionManager
2. Inspector > Permanent Upgrades > All Permanent Upgrades
3. Arrastrar todos los PermanentUpgradeData assets
4. Configurar Ship Parts:
   - Ship Parts Required For Victory: 5 (default)
```

---

## 💰 Turret Cost (Struct)

**Propósito:** Define costo de construcción/expansión en tres tipos de recursos.

### Uso en Inspector

```
Component: TurretBlueprint, MapManager

Inspector:
Cost:
  ├─ Primary Resource: 100
  ├─ Mining Resource A: 50
  └─ Mining Resource B: 25
```

### Código

```csharp
[System.Serializable]
public struct TurretCost
{
    [SerializeField] private int primaryResource;
    [SerializeField] private int miningResourceA;
    [SerializeField] private int miningResourceB;

    public int PrimaryResource => Mathf.Max(0, primaryResource);
    public int MiningResourceA => Mathf.Max(0, miningResourceA);
    public int MiningResourceB => Mathf.Max(0, miningResourceB);
}
```

### Valores Típicos

**Torretas Early-Game:**
```
Primary: 100-150
Mining A: 0
Mining B: 0
```

**Torretas Mid-Game:**
```
Primary: 200-300
Mining A: 50-100
Mining B: 0-50
```

**Torretas Late-Game:**
```
Primary: 400-600
Mining A: 100-200
Mining B: 50-150
```

**Expansión de Escudo:**
```
Primary: 150
Mining A: 50
Mining B: 50
```

---

## ⚖️ Balanceo y Valores Recomendados

### Enemigos

#### Escalado de Dificultad por Oleada

```
Wave 1:  Base stats
Wave 5:  Health +40%, Damage +20%, Speed +8%
Wave 10: Health +90%, Damage +45%, Speed +18%
Wave 20: Health +190%, Damage +95%, Speed +38%

Fórmulas (configurables en WaveManager):
  Health: base * (1 + 0.1 * (wave-1))  [healthGrowthPerWave = 0.1]
  Damage: base * (1 + 0.05 * (wave-1))  [damageGrowthPerWave = 0.05]
  Speed: base * (1 + 0.02 * (wave-1))  [speedGrowthPerWave = 0.02]
```

#### Ratios Recomendados

```
Swarmer (Muchos, frágiles):
  Health: 1x
  Damage: 1x
  Speed: 1.2x
  Drop: 1x
  
Tank (Pocos, duros):
  Health: 3x
  Damage: 2.5x
  Speed: 0.6x
  Drop: 2.5x
  
Ranged (Medio, atacan de lejos):
  Health: 0.8x
  Damage: 1.5x
  Speed: 0.8x
  Drop: 1.5x
  Range: 10-15
```

### Torretas

#### DPS vs Costo

```
Budget Turret (100 recursos):
  DPS: 30-40
  Range: 12-15
  
Standard Turret (200 recursos):
  DPS: 60-80
  Range: 15-18
  
Advanced Turret (300+ recursos):
  DPS: 100-150
  Range: 18-22
  
Specialidad (AoE, Slow, etc.):
  DPS: 50-70% de standard
  Utility: High
```

#### Fire Rate vs Damage

```
Rápida (Fire Rate 3.0, Damage 10):
  DPS = 30
  Mejor para swarmers
  
Balanceada (Fire Rate 1.5, Damage 40):
  DPS = 60
  All-rounder
  
Pesada (Fire Rate 0.5, Damage 150):
  DPS = 75
  Mejor para tanks
```

### Recursos

#### Economía de Run

```
Oleada 1-5:
  Recursos ganados: 500-800
  Costo torreta básica: 100
  Ratio: 5-8 torretas posibles
  
Oleada 5-10:
  Recursos acumulados: 2000-3500
  Costo torreta estándar: 200
  Ratio: 10-17 torretas totales
  
Oleada 10+:
  Upgrades críticos
  Mezcla básicas + avanzadas
  Minería para torretas premium
```

#### Minería

```
Vein Total Resources: 1000
Mining Amount Per Click: 50
Clicks to Deplete: 20

Vein A: Común, spawn chance 60%
Vein B: Raro, spawn chance 40%

Torreta Premium Cost:
  Primary: 300
  Mining A: 100
  Mining B: 50
  
Requisito: 2 veins A + 1 vein B (aproximado)
```

### Upgrades

#### Temporal (cada 3 oleadas)

```
Early (Oleada 3, 6):
  Damage +20%
  Fire Rate +15%
  Mining +50%
  
Mid (Oleada 9, 12):
  Damage +30%
  Range +20%
  Resource Gain +30%
  
Late (Oleada 15+):
  Enemy Slow +15%
  Enemy Health -20%
  Combo multipliers
```

#### Permanente

```
Tier 1 (300-500 moneda):
  +100 starting resources
  +150 ship health
  +10% turret damage
  
Tier 2 (600-1000 moneda):
  +200 starting resources
  +250 ship health
  +15% turret damage
  
Tier 3 (1200+ moneda):
  +20% global damage
  +20% fire rate
  -15% enemy health
```

### Dificultad General

```
Game Jam Balance (30-45 min runs):
  Ship Health: 1000
  Victory Requirement: 5 ship parts
  Ship Part Drop Chance: 4% (waves 20+)
  Expected Victory: Wave 25-30
  
Casual:
  Ship Health: 1500
  Parts Required: 3
  Drop Chance: 6%
  
Hardcore:
  Ship Health: 750
  Parts Required: 7
  Drop Chance: 3%
  Faster wave scaling
```

---

## 📊 Plantilla de Balanceo

### Spreadsheet Recomendado

```
Enemigos:
| Type    | HP  | Speed | Damage | Drop | Wave Intro | Count/Wave |
|---------|-----|-------|--------|------|------------|------------|
| Swarmer | 100 | 5.0   | 10     | 20   | 1          | 60%        |
| Tank    | 300 | 3.0   | 25     | 50   | 4          | 30%        |
| Ranged  | 80  | 4.0   | 15     | 30   | 8          | 10%        |

Torretas:
| Type     | Cost(P/A/B) | DPS | Range | Special      |
|----------|-------------|-----|-------|--------------|
| Basic    | 100/0/0     | 37  | 15    | -            |
| Cannon   | 200/50/0    | 32  | 12    | AoE 3.0      |
| Sniper   | 250/0/50    | 50  | 25    | Min Range 5  |
| Frost    | 150/50/50   | 5   | 18    | Slow 70% 3s  |

Recursos por Oleada:
| Wave | Total Enemies | Total Drops | Cumulative |
|------|---------------|-------------|------------|
| 1    | 8             | 160         | 160        |
| 5    | 16            | ~400        | ~1200      |
| 10   | 26            | ~750        | ~3500      |
| 20   | 46            | ~1500       | ~10000     |
```

---

## 🔧 Testing de Balance

### Checklist de Balance

#### Enemigos
- [ ] Pueden alcanzar la nave si no hay torretas
- [ ] Mueren en tiempo razonable con 2-3 torretas
- [ ] Wave 10 es desafiante pero alcanzable
- [ ] Tanks requieren focus fire o torretas avanzadas

#### Torretas
- [ ] Torreta básica cuesta ~2 oleadas de recursos early
- [ ] Rango cubre 50-70% del mapa desde centro
- [ ] Fire rate se siente responsive (no muy lento)
- [ ] Upgrades son visiblemente efectivos (+20% es notable)

#### Economía
- [ ] Puedes comprar 1-2 torretas por oleada early
- [ ] Mining es atractivo (reward > effort)
- [ ] Moneda permanente siente progreso (1-2 upgrades por run fallido)
- [ ] Victory es posible en 20-30 oleadas con skill

#### Difficulty Curve
- [ ] Wave 1-5: Tutorial, fácil
- [ ] Wave 5-10: Challenge, requiere strategy
- [ ] Wave 10-20: Intense, requiere upgrades y mining
- [ ] Wave 20+: Endgame, ship parts spawn

---

## 📝 Notas Finales

- **Iterar rápido:** ScriptableObjects permiten balanceo sin recompilar
- **Version control:** Commitear assets de configuración para compartir balance
- **Naming convention:** `[Type]_[Name]_[Variant].asset` (ej: `Turret_Basic_T2.asset`)
- **Organization:** Crear carpetas en Project: `Data/Enemies/`, `Data/Turrets/`, `Data/Upgrades/`
- **Testing:** Usar escenas de prueba con valores extremos para encontrar límites

---

**Siguiente:** Ver [Workflow de Desarrollo](./DEVELOPMENT_WORKFLOW.md) para branching y colaboración.
