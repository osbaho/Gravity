# Gravity Defenders

**Género:** Tower Defense con mecánicas de minería y gravedad  
**Plataforma:** PC (Standalone)  
**Engine:** Unity 6000.0.59f1  
**Estado:** En desarrollo para Game Jam

---

## 📋 Tabla de Contenidos

1. [Resumen del Proyecto](#resumen-del-proyecto)
2. [Requisitos Técnicos](#requisitos-técnicos)
3. [Estructura del Proyecto](#estructura-del-proyecto)
4. [Quick Start](#quick-start)
5. [Documentación Detallada](#documentación-detallada)
6. [Workflow de Desarrollo](#workflow-de-desarrollo)
7. [Troubleshooting](#troubleshooting)

---

## 🎮 Resumen del Proyecto

**Gravity Defenders** es un tower defense donde el mapa se reorganiza periódicamente por ondas gravitacionales. Los jugadores defienden una nave central, construyen torretas, minan recursos, y coleccionan partes de nave para reparar y ganar.

### Mecánicas Principales

- **Sistema de Oleadas:** Enemigos spawn en oleadas crecientes con dificultad escalable
- **Construcción de Torretas:** Múltiples arquetipos con costos en recursos primarios y de minería
- **Minería Activa:** Click en vetas de recursos para obtener materiales
- **Ondas Gravitacionales:** El mapa se reorganiza periódicamente, moviendo zonas no protegidas
- **Progresión Meta:** Mejoras permanentes entre runs usando moneda persistente
- **Mejoras de Run:** Cada 3 oleadas, elige entre mejoras temporales
- **Recolección de Partes:** Drop aleatorio de partes de nave al completar oleadas tardías

---

## 🔧 Requisitos Técnicos

### Versión de Unity
- **Unity 6000.0.59f1** (Unity 6)
- Editor configurado para PC Standalone

### Paquetes Requeridos
- **Input System 1.15.0** (obligatorio, no soporta legacy Input)
- **Cinemachine 3.1.5** (opcional, recomendado para cámaras)
- **TextMeshPro** (incluido por defecto, usado en UI)

### Configuración de Proyecto

#### Player Settings
```
Project Settings > Player
├── Active Input Handling: "Input System Package (New)"
├── Api Compatibility Level: .NET Standard 2.1
└── Scripting Backend: IL2CPP (recomendado para PC)
```

#### Tags Requeridos
- `Enemy` - Para todos los enemigos
- `MainCamera` - Para la cámara principal

#### Capas Recomendadas
- `Mining` - Para ResourceVeins (opcional, configurable en MiningManager)
- `Enemies` - Para separar colisiones si es necesario

---

## 📁 Estructura del Proyecto

```
Assets/
├── Scenes/
│   └── SampleScene.unity          # Escena principal (renombrar según necesidad)
│
├── Scripts/
│   ├── Core/                      # Managers principales del juego
│   │   ├── GameManager.cs         # Singleton: flow principal, game over, victoria
│   │   ├── ResourceManager.cs     # Singleton: manejo de recursos (primarios, minería, permanentes)
│   │   ├── WaveManager.cs         # Singleton: spawn de enemigos, oleadas, dificultad
│   │   ├── MapManager.cs          # Singleton: ondas gravitacionales, expansión de escudo
│   │   ├── MiningManager.cs       # Singleton: input de minería, raycast a vetas
│   │   ├── TurretBuilder.cs       # Singleton: construcción de torretas
│   │   └── SceneBootstrap.cs      # Validador y auto-creación de managers
│   │
│   ├── Progression/
│   │   ├── MetaProgressionManager.cs    # Singleton: mejoras permanentes, partes de nave
│   │   ├── UpgradeManager.cs            # Singleton: mejoras temporales por oleada
│   │   ├── PermanentUpgradeData.cs      # ScriptableObject para upgrades permanentes
│   │   └── UpgradeData.cs               # ScriptableObject para upgrades de run
│   │
│   ├── Gameplay/
│   │   ├── CentralShip.cs         # Nave a defender (requiere Health)
│   │   ├── Health.cs              # Componente de vida (IDamageable)
│   │   ├── IDamageable.cs         # Interfaz para recibir daño
│   │   ├── TurretBuildSlot.cs     # Punto de construcción (requiere Collider)
│   │   └── ShipPartPickup.cs      # Pickup de partes de nave
│   │
│   ├── Turrets/
│   │   ├── Turret.cs              # Lógica de torreta (targeting, disparo)
│   │   ├── Projectile.cs          # Proyectil con daño y AoE
│   │   ├── TurretArchetype.cs     # ScriptableObject: configuración de torreta
│   │   ├── TurretBlueprint.cs     # ScriptableObject: blueprint para construcción
│   │   └── TurretCost.cs          # Struct: costo en recursos
│   │
│   ├── Enemies/
│   │   ├── Enemy.cs               # Comportamiento base de enemigos
│   │   └── EnemyArchetype.cs      # ScriptableObject: configuración de enemigo
│   │
│   ├── UI/
│   │   ├── UpgradePanelUI.cs          # Panel de selección de upgrades
│   │   ├── UpgradePanelPresenter.cs   # Presenter para upgrade panel
│   │   ├── MetaProgressionUI.cs       # UI de mejoras permanentes
│   │   ├── MetaProgressionPresenter.cs
│   │   ├── TurretBuildMenu.cs         # Menú de construcción de torretas
│   │   ├── TurretBuildPresenter.cs
│   │   └── RunFlowPanel.cs            # Paneles de inicio/victoria/derrota
│   │
│   └── MapZone.cs                 # Zona del mapa (requiere Collider)
│
├── Settings/
│   └── InputSystem_Actions.inputactions   # Input Actions asset
│
└── Prefabs/                       # (Crear según necesidad del equipo)
    ├── Managers/
    ├── Enemies/
    ├── Turrets/
    └── UI/
```

---

## 🚀 Quick Start

### 1. Clonar y Abrir Proyecto
```bash
git clone https://github.com/osbaho/Gravity.git
cd Gravity
```
- Abrir con Unity Hub (6000.0.59f1)
- Esperar importación de paquetes

### 2. Verificar Configuración
1. Abrir `Project Settings > Player`
2. Confirmar `Active Input Handling: Input System Package (New)`
3. Verificar que `ENABLE_INPUT_SYSTEM` está definido en `Scripting Define Symbols`

### 3. Crear Escena de Prueba
Ver [Guía de Setup de Escenas](./Documentation/SCENE_SETUP.md) para paso a paso completo.

**Mínimo viable:**
1. Crear escena nueva
2. Añadir GameObject vacío "SceneBootstrap" con componente `SceneBootstrap`
3. Añadir cámara con tag `MainCamera`
4. Añadir GameObject vacío "CentralShip" con componentes `CentralShip` + `Health`
5. Play - los managers se crearán automáticamente

### 4. Testing Básico
```
1. Verificar en consola: "[SceneBootstrap] Created missing manager: ..."
2. No debe haber errores de NullReferenceException
3. Verificar que Time.timeScale = 1 (no pausado)
```

---

## 📚 Documentación Detallada

- **[Arquitectura y Sistemas](./Documentation/ARCHITECTURE.md)** - Explicación de managers, eventos, y flujo de gameplay
- **[Setup de Escenas](./Documentation/SCENE_SETUP.md)** - Guía paso a paso para escenas funcionales
- **[ScriptableObjects y Configuración](./Documentation/SCRIPTABLE_OBJECTS.md)** - Cómo crear arquetipos, upgrades y blueprints
- **[Workflow de Desarrollo](./Documentation/DEVELOPMENT_WORKFLOW.md)** - Branching, testing y merge

---

## 🔄 Workflow de Desarrollo

### Branching Strategy
```
main                    # Rama protegida, siempre compilable
├── feature/enemigos    # Rama individual para sistema de enemigos
├── feature/ui          # Rama individual para UI
└── feature/torretas    # Rama individual para torretas
```

### Crear Rama para Feature
```bash
git checkout main
git pull origin main
git checkout -b feature/nombre-feature
```

### Testing Local
1. Crear escena de prueba en `Assets/Scenes/Test_NombreFeature.unity`
2. No commitear escenas de prueba (añadir a `.gitignore` local si es necesario)
3. Probar en Play mode sin errores
4. Validar con SceneBootstrap si añades nuevos managers

### Merge a Main
```bash
# Actualizar desde main
git checkout feature/nombre-feature
git pull origin main
git merge main

# Resolver conflictos si los hay
# Probar que todo funciona

# Merge a main
git checkout main
git merge feature/nombre-feature
git push origin main
```

---

## 🐛 Troubleshooting

> **💡 Guía completa de errores de SceneBootstrap:** Ver [Setup de Escenas - Troubleshooting](./Documentation/SCENE_SETUP.md#-troubleshooting-errores-de-scenebootstrap) para soluciones detalladas de todos los errores de validación.

### Error: "ENABLE_INPUT_SYSTEM not defined"
**Causa:** Active Input Handling no está configurado correctamente  
**Solución:**
1. `Project Settings > Player > Active Input Handling`
2. Cambiar a "Input System Package (New)"
3. Permitir que Unity reinicie

### Error: "No MainCamera found"
**Causa:** Falta cámara con tag MainCamera  
**Solución:**
1. Seleccionar cámara en escena
2. Inspector > Tag > MainCamera
3. Si usas Cinemachine, añade `CinemachineBrain` al mismo GameObject

### Error: "[SceneBootstrap] ..."
**Causa:** SceneBootstrap detectó un problema de configuración  
**Solución:** Ver [guía completa de troubleshooting](./Documentation/SCENE_SETUP.md#-troubleshooting-errores-de-scenebootstrap) con instrucciones paso a paso para cada error específico.

### Error: "OnMouseDown not firing on MapZone/TurretBuildSlot"
**Causa:** Falta Collider en el GameObject  
**Solución:**
1. Seleccionar GameObject
2. `Add Component > Box Collider` (o el que corresponda)
3. Los scripts ahora tienen `[RequireComponent(typeof(Collider))]`

### Advertencia: "ResourceVein not in MiningManager layer mask"
**Causa:** La capa del ResourceVein no está incluida en `miningLayerMask`  
**Solución:**
1. Seleccionar `MiningManager` en escena
2. Inspector > Mining Layer Mask
3. Marcar la capa donde están los ResourceVeins

### Enemigos no toman daño
**Checklist:**
- Enemigos tienen componente `Health`
- Enemigos tienen tag `Enemy`
- TurretArchetype tiene `enemyTag = "Enemy"`
- Proyectiles llegan al target (verificar en Scene view)

### Mejoras no se aplican
**Checklist:**
- `UpgradeManager` existe en escena
- `WaveManager` invoca `TriggerUpgradeSelection()` cada 3 oleadas
- `UpgradePanelPresenter` está conectado a `UpgradePanelUI`
- Panel de upgrades tiene botones de `UpgradeButton` configurados

---

## 🤝 Contribución

### Estándares de Código
- **Namespace:** Todo en `namespace GravityDefenders { ... }`
- **Naming:** PascalCase para públicos, camelCase para privados
- **Serialización:** Preferir `[SerializeField]` sobre campos públicos
- **Eventos:** Exponer como `UnityEvent<T>` con property pública read-only

### Commits
```bash
# Prefijos recomendados
feat: Nueva funcionalidad
fix: Corrección de bug
docs: Cambios en documentación
refactor: Refactoring sin cambio funcional
test: Escenas o pruebas
```

### Code Review Checklist
- [ ] Compila sin errores ni warnings
- [ ] No rompe escenas existentes
- [ ] SceneBootstrap valida sin errores
- [ ] Documentación actualizada si hay cambios de API
- [ ] Commits descriptivos

---

## 📞 Contacto y Soporte

**Líder de Proyecto:** [Oscar Baho](https://github.com/osbaho)  
**Repositorio:** [https://github.com/osbaho/Gravity](https://github.com/osbaho/Gravity)

Para dudas o issues:
1. Revisar esta documentación primero
2. Verificar [TROUBLESHOOTING](#troubleshooting)
3. Crear issue en GitHub con:
   - Descripción del problema
   - Pasos para reproducir
   - Logs de consola (si aplica)
   - Versión de Unity y paquetes

---

## 📄 Licencia

Ver archivo [LICENSE](./LICENSE) para detalles.

---

**¡Buena suerte en la Game Jam! 🚀**
