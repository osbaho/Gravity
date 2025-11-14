# Workflow de Desarrollo y Git

Guía de buenas prácticas para trabajar en equipo en el proyecto Gravity Defenders durante la Game Jam.

---

## 🌿 Branching Strategy

### Estructura de Ramas

```
main (protected)
  - Siempre compilable
  - Siempre funcional
  - Solo merge de features completadas y testeadas
  
feature/nombre-feature
  - Una rama por persona/feature
  - Ejemplos:
    - feature/enemigos
    - feature/torretas
    - feature/ui-hud
    - feature/sound-effects
    - feature/map-system
  
hotfix/nombre-bug (opcional)
  - Para bugs críticos en main
  - Merge directo sin esperar feature completa
```

### Naming Conventions

```bash
# Features
feature/sistema-principal       # feature/combat
feature/componente-especifico   # feature/turret-aoe
feature/ui-pantalla             # feature/ui-gameover

# Hotfixes
hotfix/descripcion-bug          # hotfix/null-reference-gamemanager

# Testing (locales, no pushear)
test/experimento                # test/cinemachine-orbit
```

---

## 🚀 Workflow Completo

### 1. Crear Rama para Feature

```powershell
# Asegurarte de estar en main actualizado
git checkout main
git pull origin main

# Crear y cambiar a nueva rama
git checkout -b feature/mi-feature

# Confirmar rama actual
git branch
```

### 2. Desarrollar Feature

```powershell
# Trabajo diario
# Editar archivos en Unity
# Probar en Play mode

# Ver cambios
git status

# Añadir cambios
git add Assets/Scripts/...
git add Assets/Prefabs/...

# NO añadir:
# - Escenas de prueba personales (Test_MiNombre.unity)
# - Library/, Temp/, Logs/ (ya en .gitignore)
# - .meta de archivos no añadidos

# Commit con mensaje descriptivo
git commit -m "feat: añade sistema de torretas AoE"
```

### 3. Commits Frecuentes

```powershell
# Commit al finalizar cada sesión de trabajo
git commit -m "feat: implementa detección de enemigos en rango"
git commit -m "fix: corrige NullReference en TurretBuilder"
git commit -m "refactor: extrae lógica de targeting a método separado"

# Push a tu rama para backup
git push origin feature/mi-feature

# Si es tu primer push de la rama:
git push -u origin feature/mi-feature
```

### 4. Actualizar desde Main

```powershell
# Cada día antes de trabajar
git checkout feature/mi-feature
git fetch origin
git merge origin/main

# Si hay conflictos:
# 1. Unity te mostrará archivos en conflicto
# 2. Resolver manualmente en editor de texto
# 3. git add <archivos-resueltos>
# 4. git commit -m "merge: resuelve conflictos con main"

# Probar en Unity que todo sigue funcionando
# Play mode sin errores
```

### 5. Merge a Main

```powershell
# Cuando feature está completa y testeada
git checkout main
git pull origin main

# Merge tu feature
git merge feature/mi-feature

# Si hay conflictos, resolver y commitear
git add .
git commit -m "merge: integra feature/mi-feature"

# Probar en Unity (IMPORTANTE)
# Abrir escena principal
# Play mode completo
# No debe haber errores

# Push a main
git push origin main

# Opcional: Borrar rama local
git branch -d feature/mi-feature

# Opcional: Borrar rama remota
git push origin --delete feature/mi-feature
```

---

## 📝 Commit Message Guidelines

### Formato

```
<tipo>: <descripción breve>

[Cuerpo opcional: detalles, razones, side effects]

[Footer opcional: referencias a issues, breaking changes]
```

### Tipos

```
feat:     Nueva funcionalidad
fix:      Corrección de bug
docs:     Cambios en documentación
refactor: Refactoring sin cambio funcional
test:     Añadir/modificar escenas de prueba
perf:     Mejora de performance
style:    Formato, indentación (no afecta código)
chore:    Mantenimiento (actualizar .gitignore, etc.)
merge:    Merge de ramas
```

### Ejemplos

```bash
# Bueno
git commit -m "feat: añade torreta de slow con AoE"
git commit -m "fix: corrige NullReference en WaveManager.Start()"
git commit -m "refactor: extrae lógica de targeting a TurretTargeting.cs"
git commit -m "docs: actualiza SCENE_SETUP.md con setup de minería"

# Evitar
git commit -m "cambios"
git commit -m "fix"
git commit -m "wip"
git commit -m "asdfasdf"
```

### Commits Atómicos

```
# Hacer: Un commit por cambio lógico
git commit -m "feat: añade TurretArchetype ScriptableObject"
git commit -m "feat: implementa lógica de targeting en Turret.cs"
git commit -m "feat: crea UI de construcción de torretas"

# Evitar: Un commit gigante con todo
git commit -m "añade todo el sistema de torretas"
```

---

## 🧪 Testing Antes de Merge

### Checklist Pre-Merge

#### Compilación
- [ ] No hay errores de compilación
- [ ] No hay warnings críticos
- [ ] Scripts referencian namespaces correctos

#### Funcionalidad
- [ ] Feature funciona en escena de prueba
- [ ] Feature funciona en escena principal
- [ ] No rompe features existentes
- [ ] Play mode sin NullReferenceExceptions

#### Escena Principal
- [ ] `SampleScene.unity` (o la principal) se abre sin errores
- [ ] SceneBootstrap valida sin errores
- [ ] Managers se crean correctamente
- [ ] Flujo básico de gameplay funciona

#### Assets
- [ ] ScriptableObjects no son null
- [ ] Prefabs no tienen referencias rotas
- [ ] Materials/Sprites se ven correctamente

#### Documentación
- [ ] README.md actualizado si cambiaste setup
- [ ] Comentarios en código complejo
- [ ] ScriptableObjects documentados si añadiste nuevos tipos

### Testing Rápido

```powershell
# En Unity:
1. File > Open Scene > SampleScene.unity
2. Play
3. Esperar 5 segundos
4. Verificar consola (solo warnings aceptables, no errores)
5. Stop

# En terminal:
git status
git diff
# Revisar que solo se commitean archivos relevantes
```

---

## 🚨 Resolución de Conflictos

### Conflictos Comunes

#### 1. Escena (.unity)

**Problema:** Dos personas modificaron la misma escena

**Solución:**
```
1. git checkout --theirs Assets/Scenes/SampleScene.unity
   (Acepta versión de main)
   
2. Abrir escena en Unity
3. Re-aplicar tus cambios manualmente
4. Save escena
5. git add Assets/Scenes/SampleScene.unity
6. git commit -m "merge: reaplica cambios de feature en escena principal"
```

**Prevención:** Usar escenas de prueba individuales, no modificar escena principal

#### 2. Prefab (.prefab)

**Problema:** Dos personas modificaron el mismo prefab

**Solución:**
```
Similar a escena:
1. git checkout --theirs Assets/Prefabs/Enemy.prefab
2. Abrir prefab en Unity
3. Re-aplicar cambios
4. Save
5. git add y commit
```

**Prevención:** Coordinar en equipo quién modifica qué prefab

#### 3. Script (.cs)

**Problema:** Dos personas editaron el mismo archivo C#

**Solución:**
```
1. Abrir archivo en editor de texto
2. Buscar marcadores de conflicto:
   <<<<<<< HEAD
   Tu código
   =======
   Código de main
   >>>>>>> origin/main

3. Decidir qué código mantener o combinar ambos
4. Eliminar marcadores
5. Probar que compila
6. git add y commit
```

**Prevención:** Modular código, evitar que dos personas editen el mismo archivo

#### 4. Meta Files (.meta)

**Problema:** Conflictos en archivos .meta

**Solución:**
```
# Si el archivo asociado existe:
git checkout --theirs archivo.meta
git add archivo.meta

# Si el archivo asociado no existe (borrado):
git rm archivo.meta
```

**Prevención:** Siempre commitear .meta junto con archivos asociados

### Comandos Útiles para Conflictos

```powershell
# Ver archivos en conflicto
git status

# Aceptar versión de main
git checkout --theirs <archivo>

# Aceptar tu versión
git checkout --ours <archivo>

# Abortar merge si algo sale mal
git merge --abort

# Ver diferencias
git diff <archivo>
```

---

## 📦 Qué Commitear y Qué No

### ✅ SÍ Commitear

```
Assets/
  ├── Scenes/
  │   └── SampleScene.unity (escena principal acordada)
  ├── Scripts/
  │   └── *.cs (todos los scripts)
  ├── Prefabs/
  │   └── *.prefab (prefabs compartidos)
  ├── ScriptableObjects/
  │   └── *.asset (configuración de gameplay)
  ├── Materials/
  │   └── *.mat
  ├── Sprites/
  │   └── *.png, *.jpg
  └── Settings/
      └── *.asset (InputActions, etc.)

ProjectSettings/
  └── * (configuración de proyecto)

Packages/
  └── manifest.json (dependencias)

.gitignore
.gitattributes
README.md
Documentation/
```

### ❌ NO Commitear

```
Library/         # Cache de Unity (auto-generado)
Temp/            # Archivos temporales
Logs/            # Logs de Unity
obj/             # Binarios de compilación
Builds/          # Builds del juego

Assets/Scenes/Test_*.unity    # Escenas de prueba personales
Assets/Scenes/*_Backup.unity  # Backups

*.csproj         # Auto-generados por Unity
*.sln            # Auto-generados por Unity
*.user           # Configuración local de usuario
*.suo            # Visual Studio user options

UserSettings/    # Preferencias locales de Unity
.vscode/         # Configuración local de editor
.idea/           # Configuración local de Rider
```

### .gitignore Recomendado

```gitignore
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# Visual Studio / Rider
.vs/
.vscode/
.idea/
*.csproj
*.sln
*.suo
*.user
*.unityproj
*.pidb
*.booproj

# OS
.DS_Store
Thumbs.db

# Test scenes (personal)
Assets/Scenes/Test_*.unity
Assets/Scenes/Test_*.unity.meta
```

---

## 🤝 Coordinación de Equipo

### Daily Standup (Opcional para Jam)

```
Cada día, compartir en Discord/Slack:
1. Qué hice ayer
2. Qué voy a hacer hoy
3. Algún blocker o necesito ayuda con X

Ejemplo:
"Ayer: Implementé sistema de torretas AoE
 Hoy: Voy a añadir UI de selección de torretas
 Blocker: Necesito sprites para iconos de torretas"
```

### Comunicación de Cambios Grandes

```
Antes de cambiar arquitectura o APIs públicas:
1. Avisar en grupo
2. Esperar feedback
3. Documentar cambios en README.md
4. Merge con cuidado

Ejemplos de cambios grandes:
- Renombrar GameManager → GameController
- Cambiar signature de método público
- Mover archivos entre carpetas
- Cambiar namespace
```

### División de Trabajo

```
Feature Owner (una persona responsable):
- feature/combat → Juan
- feature/ui → María
- feature/sound → Pedro
- feature/polish → Sofia

Shared Resources (coordinar):
- SampleScene.unity → Solo Juan modifica esta semana
- Prefabs/Enemy.prefab → Solo María modifica
- ResourceManager.cs → Evitar editar a la vez
```

---

## 🔧 Herramientas y Comandos Útiles

### Configurar Git para Unity

```powershell
# Configurar nombre y email
git config --global user.name "Tu Nombre"
git config --global user.email "tu@email.com"

# Merge tool para conflictos (opcional)
git config --global merge.tool p4merge

# Alias útiles
git config --global alias.st status
git config --global alias.co checkout
git config --global alias.br branch
git config --global alias.ci commit
git config --global alias.unstage 'reset HEAD --'
git config --global alias.last 'log -1 HEAD'
```

### Comandos de Inspección

```powershell
# Ver historial
git log --oneline --graph --decorate --all

# Ver cambios no commiteados
git diff

# Ver cambios en staging
git diff --staged

# Ver quién modificó cada línea
git blame Assets/Scripts/Core/GameManager.cs

# Ver ramas remotas
git branch -r

# Ver estado limpio
git status --short
```

### Deshacer Cambios

```powershell
# Deshacer cambios en archivo (no staged)
git checkout -- Assets/Scripts/MyScript.cs

# Deshacer staging (pero mantener cambios)
git reset HEAD Assets/Scripts/MyScript.cs

# Deshacer último commit (mantener cambios)
git reset --soft HEAD~1

# Deshacer último commit (borrar cambios) ⚠️ PELIGROSO
git reset --hard HEAD~1

# Crear commit que revierte otro commit
git revert <commit-hash>
```

### Stashing (Guardar trabajo temporal)

```powershell
# Guardar cambios sin commitear
git stash

# Guardar con mensaje
git stash save "WIP: torreta AoE a medio implementar"

# Ver lista de stashes
git stash list

# Recuperar último stash
git stash pop

# Recuperar stash específico
git stash apply stash@{0}

# Borrar stash
git stash drop stash@{0}
```

---

## 🐛 Troubleshooting Git

### "Your branch and 'origin/main' have diverged"

```powershell
# Ver diferencias
git log HEAD..origin/main
git log origin/main..HEAD

# Opción 1: Merge (recomendado)
git pull origin main

# Opción 2: Rebase (avanzado)
git pull --rebase origin main

# Si hay conflictos, resolver y:
git rebase --continue
```

### "Cannot lock ref 'refs/heads/feature/...'"

```powershell
# Limpiar referencias obsoletas
git remote prune origin
git fetch --prune
```

### "Modified files in working directory"

```powershell
# Ver qué archivos cambiaron
git status

# Si son cambios de Unity no deseados (ej: .meta)
git checkout -- <archivo>

# Si son muchos archivos:
git reset --hard HEAD
⚠️ Esto BORRA todos los cambios no commiteados
```

### "Unmerged paths" tras conflicto

```powershell
# Ver archivos en conflicto
git status

# Resolver conflictos manualmente
# Luego añadir archivos resueltos:
git add <archivos-resueltos>

# Finalizar merge
git commit
```

---

## 📊 Workflow Visual

```
Developer A                Main                Developer B
    |                       |                        |
    |--- feature/combat --->|                        |
    |  (día 1-2)            |                        |
    |                       |<--- feature/ui --------|
    |                       |     (día 1-2)          |
    |                       |                        |
    |--- pull main -------->|<-------- pull main ----|
    |                       |                        |
    |--- continuar trabajo  |   continuar trabajo ---|
    |                       |                        |
    |--- merge combat ----->|                        |
    |                       |--- pull main --------->|
    |                       |                        |
    |                       |<---- merge ui ---------|
    |                       |                        |
    |<------ pull main      |      pull main ------->|
    |                       |                        |
```

---

## 🎯 Best Practices Resumen

### DO ✅

1. **Commitear frecuentemente** (al menos una vez por sesión)
2. **Pull de main diariamente** antes de trabajar
3. **Probar antes de mergear** (Play mode sin errores)
4. **Mensajes de commit descriptivos**
5. **Crear escenas de prueba propias** (no modificar main scene)
6. **Comunicar cambios grandes** al equipo
7. **Usar branching** (feature branches)
8. **Documentar código complejo**

### DON'T ❌

1. **No commitear Library/, Temp/, Logs/**
2. **No hacer force push** a main (git push -f)
3. **No mergear sin probar**
4. **No commitear "WIP"** a main
5. **No editar archivos de otros** sin coordinar
6. **No ignorar conflictos** (resolverlos correctamente)
7. **No dejar ramas huérfanas** (limpiar tras merge)
8. **No romper main** (siempre debe compilar)

---

## 🏁 Checklist Final Pre-Release

```
Code:
[ ] Todo compila sin errores
[ ] No hay warnings críticos
[ ] No hay TODOs pendientes críticos

Functionality:
[ ] Gameplay loop completo funciona
[ ] Win/lose conditions funcionan
[ ] UI responde correctamente
[ ] No hay NullReferenceExceptions

Assets:
[ ] Todos los prefabs tienen referencias correctas
[ ] Todos los ScriptableObjects configurados
[ ] Sprites/Models cargados
[ ] Sounds/Music implementados (si aplica)

Performance:
[ ] 60 FPS en máquina de desarrollo
[ ] No memory leaks evidentes
[ ] Garbage collection razonable

Build:
[ ] Build para PC compila
[ ] Build ejecuta sin crashes
[ ] Controles funcionan en build

Documentation:
[ ] README.md actualizado
[ ] Créditos en game/README
[ ] Licencias de assets third-party

Git:
[ ] Main limpio (no trabajo WIP)
[ ] Todas las features mergeadas
[ ] Tags de versión (git tag v1.0)
[ ] Backup remoto actualizado
```

---

## 📚 Recursos Adicionales

### Aprender Git

- [Git Book (español)](https://git-scm.com/book/es/v2)
- [GitHub Git Cheat Sheet](https://education.github.com/git-cheat-sheet-education.pdf)
- [Atlassian Git Tutorials](https://www.atlassian.com/git/tutorials)

### Unity + Git

- [Unity Manual: External Version Control](https://docs.unity3d.com/Manual/ExternalVersionControlSystemSupport.html)
- [GitHub Unity .gitignore Template](https://github.com/github/gitignore/blob/main/Unity.gitignore)

### Herramientas GUI

- **GitKraken:** GUI amigable para Git
- **Sourcetree:** Git GUI gratuito
- **GitHub Desktop:** Simplificado para GitHub
- **Unity Version Control (Plastic SCM):** Alternativa a Git (pago)

---

**¡Éxito en la Game Jam! 🎮🚀**

Si tienes dudas sobre Git/workflow, preguntar antes de hacer push a main.
