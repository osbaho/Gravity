# Setup de Escenas de Prueba

Guía paso a paso para crear escenas funcionales para desarrollo y testing en ramas individuales.

---

## 🎯 Objetivo

Crear una escena mínima viable donde puedas probar tu feature sin depender de la escena principal. Esta guía asume que estás trabajando en una rama individual (ej: `feature/torretas`).

---

## ⚡ Quick Setup (Mínimo Viable)

### 1. Crear Nueva Escena

```
File > New Scene > Basic (Built-in)
Guardar como: Assets/Scenes/Test_NombreFeature.unity
```

### 2. Añadir SceneBootstrap

```
1. Crear GameObject vacío: Hierarchy > Create Empty
2. Renombrar: "SceneBootstrap"
3. Add Component > SceneBootstrap
4. Inspector:
   - Ensure Managers: ✓
   - Validate On Start: ✓
   - Todos los checks: ✓
```

**Importante:** `SceneBootstrap` creará automáticamente todos los managers faltantes.

### 3. Configurar Cámara

```
Si Main Camera ya existe:
  1. Seleccionar Main Camera
  2. Inspector > Tag: MainCamera
  3. Add Component > Cinemachine Brain (opcional pero recomendado)

Si no existe:
  1. GameObject > Camera
  2. Tag: MainCamera
  3. Position: (0, 10, -10)
  4. Rotation: (45, 0, 0)
```

### 4. Añadir Nave Central

```
1. GameObject > Create Empty
2. Renombrar: "CentralShip"
3. Position: (0, 0, 0)
4. Add Component > Central Ship
5. Add Component > Capsule Collider (para visualización)
6. Health se añade automáticamente (RequireComponent)
7. Inspector (Health):
   - Max Health: 1000
```

### 5. Probar Escena

```
1. Play
2. Verificar consola:
   ✓ "[SceneBootstrap] Created missing manager: GameManager"
   ✓ "[SceneBootstrap] Created missing manager: ResourceManager"
   ✓ ... (otros managers)
   ✓ No debe haber errores rojos

3. Hierarchy debe mostrar:
   SceneBootstrap
   Main Camera
   CentralShip
   GameManager (auto-creado)
   ResourceManager (auto-creado)
   ... (otros managers auto-creados)
```

**¡Listo!** Ya tienes una escena funcional. Ahora añade lo que necesites para tu feature.

---

## 🧩 Setup Completo (Recomendado para Testing)

### A. Sistema de Oleadas (WaveManager)

#### 1. Crear Spawn Points

```
1. GameObject > Create Empty
2. Renombrar: "SpawnPoints"
3. Crear 4 hijos vacíos:
   - SpawnPoint_North (Position: 0, 0, 20)
   - SpawnPoint_South (Position: 0, 0, -20)
   - SpawnPoint_East (Position: 20, 0, 0)
   - SpawnPoint_West (Position: -20, 0, 0)
```

#### 2. Crear Enemy Archetypes (ScriptableObjects)

Ver [SCRIPTABLE_OBJECTS.md](./SCRIPTABLE_OBJECTS.md) para detalles completos.

**Quick:**
```
1. Project > Create > Gravity Defenders > Enemy Archetype
2. Renombrar: "Enemy_Swarmer"
3. Configurar:
   - Display Name: "Swarmer"
   - Base Health: 100
   - Base Move Speed: 5
   - Base Damage: 10
   - Base Resource Drop: 20
   - Attack Mode: Melee
   - Attack Range: 1.5
   - Attack Cooldown: 1.0

4. Duplicar para "Enemy_Tank" y "Enemy_Ranged"
   - Tank: Health 300, Speed 3, Damage 20
   - Ranged: Health 80, Speed 4, Attack Mode = Ranged
```

#### 3. Configurar WaveManager

```
1. Hierarchy > Buscar "WaveManager" (auto-creado por Bootstrap)
2. Inspector:
   - Calm Phase Duration: 8
   - Spawn Interval: 0.4
   - Base Enemies Per Wave: 8
   - Enemies Per Wave Growth: 2
   - Waves Per Upgrade: 3
   - Spawn Points: Arrastrar los 4 spawn points
   - Swarmer Archetype: Enemy_Swarmer
   - Tank Archetype: Enemy_Tank
   - Ranged Archetype: Enemy_Ranged
   - Central Ship Override: (dejar vacío, usa GameManager)
```

### B. Sistema de Minería

#### 1. Crear Resource Veins

```
1. GameObject > 3D Object > Cube
2. Renombrar: "ResourceVein_A"
3. Position: (5, 0, 5)
4. Scale: (1, 0.5, 1)
5. Add Component > Resource Vein
6. Inspector:
   - Resource Type: A
   - Total Resources: 1000
7. Layer: Mining (crear si no existe)

8. Duplicar para ResourceVein_B
   - Position: (-5, 0, 5)
   - Resource Type: B
```

#### 2. Configurar MiningManager

```
1. Hierarchy > MiningManager
2. Inspector:
   - Mining Amount Per Click: 50
   - Mining Layer Mask: Mining
```

### C. Sistema de Construcción de Torretas

#### 1. Crear Turret Build Slots

```
1. GameObject > 3D Object > Plane
2. Renombrar: "TurretSlot_01"
3. Position: (3, 0, 0)
4. Scale: (0.3, 0.3, 0.3)
5. Add Component > Turret Build Slot
6. Inspector:
   - Placement Point: (dejar en self)
   - Allow Mouse Interaction: ✓

7. Material visual (opcional):
   - Crear material verde "Slot_Available"
   - Asignar a Mesh Renderer

8. Duplicar para crear más slots alrededor de la nave
   - TurretSlot_02: (0, 0, 3)
   - TurretSlot_03: (-3, 0, 0)
   - TurretSlot_04: (0, 0, -3)
```

#### 2. Crear Turret Archetypes y Blueprints

Ver [SCRIPTABLE_OBJECTS.md](./SCRIPTABLE_OBJECTS.md).

**Quick:**
```
1. Project > Create > Gravity Defenders > Turret Archetype
2. Renombrar: "Turret_Basic"
3. Configurar:
   - Display Name: "Basic Turret"
   - Range: 15
   - Fire Rate: 1.5
   - Damage Per Shot: 25
   - Enemy Tag: "Enemy"
   - Attack Type: Projectile
   - Projectile Speed: 70

4. Create > Gravity Defenders > Turret Blueprint
5. Renombrar: "Blueprint_Basic"
6. Configurar:
   - Turret Name: "Basic Turret"
   - Archetype: Turret_Basic
   - Cost > Primary Resource: 100
```

### D. Sistema de Mapas (Opcional)

#### 1. Crear Map Zones

```
1. GameObject > 3D Object > Plane
2. Renombrar: "MapZone_Central"
3. Position: (0, -0.1, 0)
4. Scale: (2, 1, 2)
5. Add Component > Map Zone
6. Inspector:
   - Designate As Central: ✓
   - Allow Mouse Interaction: ✓
   - Auto Expand On Click: ✓

7. Duplicar para zonas periféricas:
   - MapZone_North: (0, -0.1, 20)
   - MapZone_South: (0, -0.1, -20)
   - MapZone_East: (20, -0.1, 0)
   - MapZone_West: (-20, -0.1, 0)
   - Designate As Central: ✗ (solo en Central)
```

#### 2. Configurar MapManager

```
1. Hierarchy > MapManager
2. Inspector:
   - Time Between Waves: 60
   - Shield Expansion Cost:
       - Primary Resource: 150
       - Mining Resource A: 50
       - Mining Resource B: 50
```

### E. UI Básica (Opcional)

#### 1. Canvas Principal

```
1. GameObject > UI > Canvas
2. Inspector:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler > UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

3. Añadir EventSystem (auto-creado si no existe)
```

#### 2. HUD de Recursos

```
1. Canvas > Create > UI > Text - TextMeshPro
2. Renombrar: "ResourceDisplay"
3. RectTransform:
   - Anchor: Top Left
   - Position: (10, -10, 0)
   - Width: 300, Height: 100
4. TextMeshPro:
   - Font Size: 24
   - Text: "Primary: 0\nMining A: 0\nMining B: 0"

5. Conectar con ResourceManager (scripting o visual scripting):
   - ResourceManager.PrimaryResourceChanged → UpdateText
```

---

## 🎨 Visualización y Debug

### Gizmos y Debug Draws

Los siguientes componentes tienen gizmos en Scene view cuando seleccionados:

- **Turret:** Círculo rojo = rango de ataque
- **Projectile:** Círculo amarillo = explosión radius (si AoE)
- **MapZone:** Depende de implementación visual

### Añadir Indicators Visuales

```csharp
// Ejemplo: Visualizar zona protegida
void OnDrawGizmos()
{
    if (IsShielded)
    {
        Gizmos.color = Color.green;
    }
    else
    {
        Gizmos.color = Color.red;
    }
    Gizmos.DrawWireCube(transform.position, transform.localScale * 10f);
}
```

### Console Logging

Los managers ya tienen logs útiles:
```
[GameManager] GAME OVER! Your ship was destroyed.
[WaveManager] Wave 5 commencing.
[ResourceManager] Added 20 Primary Resource. Total: 340
[MiningManager] Mined 50 of type A
```

Configurar filtros en Console:
```
Filter: [GameManager]  → Ver solo eventos de GameManager
```

---

## 🧪 Testing Checklist

### Setup Inicial
- [ ] SceneBootstrap en escena
- [ ] Main Camera con tag "MainCamera"
- [ ] CentralShip con Health component
- [ ] Play sin errores en consola

### Sistema de Oleadas
- [ ] WaveManager configurado con spawn points
- [ ] Enemy archetypes asignados
- [ ] Enemigos spawn cada X segundos
- [ ] Enemigos se mueven hacia CentralShip
- [ ] Enemigos atacan y causan daño
- [ ] Enemigos dropean recursos al morir
- [ ] Oleadas incrementan dificultad

### Sistema de Torretas
- [ ] TurretBuildSlots tienen collider
- [ ] Click en slot abre menú (si implementado)
- [ ] Construcción consume recursos
- [ ] Torreta apunta a enemigos
- [ ] Torreta dispara proyectiles
- [ ] Proyectiles causan daño
- [ ] Upgrades afectan daño/fire rate/range

### Sistema de Minería
- [ ] ResourceVeins tienen collider en capa correcta
- [ ] MiningManager.miningLayerMask incluye capa
- [ ] Click en veta mina recursos
- [ ] Recursos se añaden a ResourceManager
- [ ] Veta se destruye cuando depleted

### Sistema de Mapas
- [ ] MapZones tienen collider
- [ ] Zona central está shielded al inicio
- [ ] Timer de onda gravitacional cuenta
- [ ] Zonas se reorganizan al triggear onda
- [ ] Expansión de escudo consume recursos
- [ ] Vetas spawn en zonas protegidas

### Game Flow
- [ ] Nave muere → Game Over → Reinicia escena
- [ ] Cada 3 oleadas → Panel de upgrades → Pausa
- [ ] Colectar parte de nave → Progreso se registra
- [ ] 5 partes → Victoria

---

## 🚫 Errores Comunes y Soluciones

### "NullReferenceException: Object reference not set..."

**GameManager.cs line 68:**
```
Causa: CentralShip no asignado y no encontrado
Solución: Crear GameObject con CentralShip component
```

**WaveManager.cs line 142:**
```
Causa: centralShip es null
Solución: 
  1. Asignar Central Ship Override en WaveManager
  2. O asegurar que GameManager.CentralShipTransform esté disponible
```

**MiningManager.cs line 56:**
```
Causa: mainCamera es null
Solución: Crear cámara con tag MainCamera
```

### "Tag 'Enemy' is not defined"

```
Solución:
1. Edit > Project Settings > Tags and Layers
2. Tags > + 
3. Nombre: Enemy
4. Asignar tag a prefabs/GameObjects de enemigos
```

### "OnMouseDown not firing"

```
Causa: Falta Collider en GameObject
Solución:
  - TurretBuildSlot: Add Component > Box Collider
  - MapZone: Add Component > Box Collider
  - ResourceVein: Add Component > Box Collider
```

### "Enemies not spawning"

```
Checklist:
  1. WaveManager tiene spawn points asignados
  2. Enemy archetypes no son null
  3. Central ship existe (target para enemigos)
  4. Play en escena (no en prefab edit mode)
```

### "Input no responde (minería)"

```
Causa: Input System no activo
Solución:
  1. Project Settings > Player > Active Input Handling
  2. Cambiar a "Input System Package (New)"
  3. Permitir restart de Unity
  4. Verificar ENABLE_INPUT_SYSTEM definido
```

### "Compilation Error: ENABLE_INPUT_SYSTEM not defined"

```
Causa: Intencional, forza uso de nuevo Input System
Solución: Ver error anterior
```

---

## 🎬 Escenarios de Prueba Específicos

### Probar Solo Torretas

```
1. Setup mínimo (SceneBootstrap, Camera, CentralShip)
2. Añadir TurretBuildSlots
3. Crear Turret Archetypes y Blueprints
4. Añadir recursos manualmente:
   - Play
   - Consola: ResourceManager.Instance.AddResources(1000)
5. Click en slot, seleccionar blueprint
6. Verificar construcción y comportamiento
```

### Probar Solo Enemigos

```
1. Setup mínimo
2. Crear Enemy Archetype
3. Script temporal para spawn manual:

using UnityEngine;
using GravityDefenders;

public class ManualEnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyArchetype archetype;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemy();
        }
    }
    
    void SpawnEnemy()
    {
        GameObject go = new GameObject("TestEnemy");
        go.transform.position = transform.position;
        Enemy enemy = go.AddComponent<Enemy>();
        Health health = go.AddComponent<Health>();
        enemy.Initialize(archetype);
        enemy.SetTarget(GameManager.Instance.CentralShipTransform);
    }
}
```

### Probar Solo Minería

```
1. Setup mínimo
2. Crear ResourceVeins con colliders
3. Configurar MiningManager.miningLayerMask
4. Play
5. Click en vetas
6. Verificar consola y ResourceManager values
```

### Probar Solo Ondas Gravitacionales

```
1. Setup mínimo
2. Crear MapZones con colliders
3. Configurar MapManager:
   - Time Between Waves: 10 (para testing rápido)
4. Play
5. Esperar 10 segundos
6. Verificar que zonas se mueven
```

---

## 📦 Templates de Escenas

### Template: Minimal Testing Scene

```
Hierarchy:
├── SceneBootstrap
├── Main Camera
└── CentralShip
```

**Uso:** Testing de managers, lógica de gameplay sin visuals.

### Template: Combat Testing Scene

```
Hierarchy:
├── SceneBootstrap
├── Main Camera (con Cinemachine)
├── CentralShip
├── SpawnPoints (4 hijos)
├── TurretSlots (4+ hijos)
└── Ground (Plane para visualización)
```

**Uso:** Testing de torretas, enemigos, combat loop.

### Template: Full Feature Scene

```
Hierarchy:
├── SceneBootstrap
├── Main Camera + Virtual Cameras
├── CentralShip
├── SpawnPoints
├── TurretSlots
├── MapZones (5+ zonas)
├── ResourceVeins (varios)
├── Canvas (UI completa)
└── Environment (decoración)
```

**Uso:** Testing integrado, demo, game jam playtest.

---

## 🔄 Workflow: Crear → Probar → Iterar

```
1. Crear rama feature
   git checkout -b feature/mi-feature

2. Crear escena de prueba
   Assets/Scenes/Test_MiFeature.unity

3. Setup mínimo
   SceneBootstrap + Camera + CentralShip

4. Añadir componentes necesarios para tu feature
   Ej: TurretBuildSlots, Enemy spawner, etc.

5. Implementar feature
   Editar scripts relevantes

6. Probar en Play mode
   Verificar funcionalidad sin errores

7. Refinar y pulir
   Ajustar valores, fix bugs

8. Merge a main (sin escena de prueba)
   git add Assets/Scripts/...
   git commit -m "feat: implementa mi-feature"
   (No commitear Test_MiFeature.unity)
```

---

## 📝 Notas Finales

- **Escenas de prueba:** No deben comittearse a menos que sean demos oficiales
- **SceneBootstrap:** Siempre incluir para auto-setup de managers
- **Naming:** `Test_NombreFeature.unity` para evitar conflictos
- **Cleanup:** Borrar escenas de prueba antes de merge a main
- **Documentación:** Si tu feature requiere setup especial, documéntalo aquí

---

**Siguiente paso:** Ver [ScriptableObjects y Configuración](./SCRIPTABLE_OBJECTS.md) para crear arquetipos y configuración de gameplay.
