# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

DEffect는 Unity 6 (버전 6000.0.56f1)로 개발된 2D 플랫포머 게임입니다. C#로 작성되었으며, Unity의 New Input System을 사용합니다. 플레이어는 시간 조작, 전투, 플랫포밍 등 다양한 능력을 가지고 있습니다.

## 개발 명령어

### 프로젝트 열기
- Visual Studio 또는 호환 IDE에서 `DEffect.sln` 열기
- Unity Editor (버전 6000.0.56f1)에서 프로젝트 폴더 열기

### Unity 빌드
- 빌드 씬 설정은 `ProjectSettings/EditorBuildSettings.asset`에 구성됨
- Unity Editor의 File > Build Settings에서 빌드 설정 관리

### Git 워크플로우
- 메인 개발 브랜치: `Develop`
- 현재 기능 브랜치: `feature/player`
- 기능 브랜치는 항상 `Develop`에 병합 (`main`/`master`에 직접 병합 금지)

## 아키텍처 개요

### 이중 코드 구조
이 프로젝트는 **서로 다른 목적을 가진 두 개의 병렬 코드베이스**를 가지고 있습니다:

1. **Assets/Scripts/** - 레거시/원본 구현
   - `PlayerStateMachine<T>` 제네릭 클래스를 사용하는 커스텀 FSM 패턴
   - 완전한 게임 시스템 포함 (매니저, 적, 상호작용)
   - 플레이어 컨트롤러는 Unity의 구 Input System과 커스텀 입력 처리 사용

2. **Assets/Unity6/Scripts/** - Unity 6으로 리팩토링된 구현
   - `PlayerInputActions.inputactions`를 사용하는 Unity의 New Input System
   - `PlayerBaseState` 추상 클래스를 사용하는 모던 스테이트 머신 패턴
   - 현재 활발하게 개발 중 (git status 참조)
   - 더 깔끔한 아키텍처와 관심사의 분리

**플레이어 시스템을 수정할 때는 어느 코드베이스에서 작업할지 명확히 하세요.**

### 핵심 매니저 시스템 (싱글톤 패턴)

모든 매니저는 싱글톤 패턴을 사용하며 `DontDestroyOnLoad`를 통해 씬 전환 시에도 유지됩니다:

- **GameManager** - 게임 전체 조정, 플레이어 사망/리스폰, 씬 재시작
- **PlayerPersistenceManager** - 씬 전환 시 플레이어 오브젝트 유지, 스폰 위치 처리
- **SceneTransitionManager** - 페이드 효과를 포함한 씬 로딩, PlayerPersistenceManager와 협력
- **CheckpointManager** - 리스폰 포인트(사망 리스폰)와 낙사 체크포인트(낙하 데미지) 관리
- **PlayerStateManager** - 씬 전환 시 플레이어 스탯(HP, 코인, 스킬) 저장/복원
- **TimeManager** - 반경 기반 시간 정지 능력, `ITimeAffected` 엔티티 관리
- **SkillManager** - 플레이어 스킬 언락/관리 시스템
- **AudioManager** - 사운드 이펙트 및 음악 관리

**중요한 씬 전환 흐름:**
1. SceneTransitionManager가 페이드 효과 시작 및 플레이어 입력 비활성화
2. PlayerPersistenceManager가 다음 씬의 스폰 위치 저장
3. 씬이 비동기로 로드됨
4. PlayerPersistenceManager가 플레이어를 스폰 위치로 이동
5. PlayerStateManager가 플레이어 HP/스탯 복원
6. TimeManager가 플레이어 레퍼런스 갱신

### 스테이트 머신 패턴

**레거시 패턴** (`Assets/Scripts/Player/`):
```csharp
// Enter/Execute/Exit를 명시적으로 가진 제네릭 스테이트 머신
PlayerStateMachine<PlayerController> stateMachine;
State<T> { Enter(), Execute(), Exit() }
```

**Unity 6 패턴** (`Assets/Unity6/Scripts/Player/State/`):
```csharp
// StateMachine이 호출하는 라이프사이클 메서드를 가진 베이스 스테이트
PlayerStateMachine : MonoBehaviour
PlayerBaseState { OnEnter(), OnUpdate(), OnFixedUpdate(), OnExit() }
// 스테이트들: PlayerIdleState, PlayerRunState, PlayerJumpState, PlayerFallState, PlayerCrouchState
```

### 적 AI 시스템

`Assets/Scripts/Enemy/BT/`의 커스텀 Behavior Tree 구현:
- **BehaviorTree** - pause/resume 기능을 가진 루트 트리, Blackboard를 사용한 데이터 공유
- **Node** 타입: ActionNode, ConditionNode, Selector, Sequence, Inverter
- **Blackboard** - 노드 간 데이터 공유를 위한 Dictionary 기반 시스템
- 적 타입: MeleeEnemy (FSM 기반), FlyingEnemy, ManaMan 변형들 (BT 기반), Boss

### 오브젝트 풀링

두 개의 풀링 시스템이 존재합니다:

1. **MemoryPool** (`Assets/Scripts/MemoryPool.cs`) - 커스텀 구현, 레거시 시스템에서 사용
2. **Pool<T>** (`Assets/Unity6/Scripts/ObjectPool/Pool.cs`) - Unity의 ObjectPool 래퍼, Unity6 코드에서 사용

### 입력 시스템

**레거시**: 수동 콜백 바인딩을 사용하는 `InputSystem_Actions.inputactions`
**Unity 6**: PlayerInput 컴포넌트를 사용하는 `Assets/Unity6/InputAction/PlayerInputActions.inputactions`
- C# 클래스 `PlayerInputActions.cs`를 자동 생성
- 액션: Move, Jump, Crouch, Attack, Interact, Time Skill
- 메모리 누수 방지를 위해 OnEnable/OnDisable에서 활성화/비활성화

### 플레이어 능력

- **Time Stop** - `ITimeAffected`를 구현한 엔티티에 영향을 주는 반경 기반 시간 정지
- **Combat** - PlayerAttack 컴포넌트를 사용한 공격 시스템
- **Platforming** - 점프, 웅크리기, 사다리 타기, 움직이는 플랫폼 탑승
- **Interaction** - `IInteractable` 인터페이스를 통한 상호작용 (버튼, 문, 엘리베이터, 자판기)

### 공통 인터페이스

- `IDamageable` - 플레이어와 적의 체력 시스템
- `ITimeAffected` - 시간 정지 능력의 영향을 받는 엔티티
- `IInteractable` - 플레이어가 상호작용할 수 있는 오브젝트

### 씬 관리

- Title 씬과 EndScene은 모든 지속성 매니저를 파괴함 (`PlayerPersistenceManager.destroyOnLoadScenes` 참조)
- 게임플레이 씬은 지속성 플레이어와 매니저를 유지함
- 적절한 상태 처리를 위해 모든 씬 변경에 `SceneTransitionManager` 사용

## 주요 파일 위치

- 플레이어 스테이트: `Assets/Unity6/Scripts/Player/State/`
- 매니저 시스템: `Assets/Scripts/Manager/`
- 적 AI: `Assets/Scripts/Enemy/BT/` 및 `Assets/Scripts/Enemy/`
- 상호작용 오브젝트: `Assets/Scripts/InteractionObjects/`
- 입력 액션: `Assets/Unity6/InputAction/PlayerInputActions.inputactions`

## 개발 참고사항

- 프로젝트는 Unity 6 개선 작업 중 (PlayerStateMachine, PlayerMovement 수정 파일 참조)
- 새로운 플레이어 스테이트 추가 시 Unity6 구현에서 `PlayerBaseState`를 확장
- 새로운 적은 FSM이 아닌 BehaviorTree 시스템을 사용해야 함
- 모든 새로운 씬 지속성 오브젝트는 기존 싱글톤 인스턴스를 확인해야 함
- 입력 액션은 저장 시 C# 코드를 자동 생성 - .inputactions와 .cs 파일 모두 커밋
- 시간 영향을 받는 엔티티는 `ITimeAffected`를 구현하고 TimeManager에 등록해야 함
