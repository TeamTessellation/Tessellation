# ? FEAT : 우선순위 기반 비동기 실행 이벤트 시스템 (Dynamic/Static/Merged) 구현

## ? 개요
우선순위에 따라 순차적으로 비동기 실행되는 이벤트 시스템을 구현했습니다.
- **Dynamic EventBus**: 호출 시점에 유연하게 우선순위 결정
- **Static EventBus**: 등록 시점에 우선순위 고정 (성능 최적화)
- **Merged Invocation**: 두 방식을 우선순위에 따라 병합 실행
- `UniTask`를 활용한 비동기 이벤트 처리
- Extra Priorities를 통한 세밀한 우선순위 제어
- 이벤트 체인 중단 기능 (`BreakChain`)
- ObjectPool을 활용한 메모리 효율성
- 자동 버스 초기화 및 런타임 관리
- Unity Editor 플레이모드 전환 시 자동 정리 기능

---

## ?? 변경(추가) 사항

### 1) ExecEventArgs (이벤트 인자)
- `Events.Core`의 `EventArgs<T>`를 상속받는 실행 이벤트 인자 베이스 클래스
- `BreakChain` 속성을 통한 이벤트 체인 중단 기능 제공
- 이벤트 풀링 시스템 지원 (`Get()`, `Release()`, `Dispose()`)
- 새로운 이벤트 타입을 정의하면 자동으로 해당 EventBus가 생성됨

**파일**: `ExecEventArgs.cs`
```csharp
public class ExecEventArgs<T> : EventArgs<T> where T : ExecEventArgs<T>, new()
{
    public bool BreakChain { get; set; } = false;
    
    public override void Clear()
    {
        BreakChain = false;
    }
}
```

### 2) ExecPriority (우선순위 Enum)
- 우선순위를 명확하게 표현하기 위한 enum 타입
- `First` (Int32.MinValue): 최우선 실행
- `Normal` (0): 기본 우선순위
- `Last` (Int32.MaxValue): 가장 나중에 실행 (UI 등)

**파일**: `ExecPriority.cs`

### 3) ExecQueue (우선순위 실행 큐)
- 우선순위 값을 가진 액션들을 저장하고 정렬된 순서로 실행하는 큐 클래스
- `ActionWrapper` 내부 클래스를 통한 우선순위 비교 및 정렬
  - `PrimaryPriority`: 기본 우선순위
  - `ExtraPriorities`: 추가 우선순위 (Primary가 같을 때 세밀한 제어)
  - `EnqueuedOrder`: 등록 순서 (FIFO 보장)
- **ObjectPool 사용**: ActionWrapper 재사용으로 GC 압박 감소
- `Enqueue(int priority, ExecAction<TEventArgs> action, params int[] extraPriorities)`: 우선순위와 함께 액션 등록
- `SortByPriority()`: 우선순위에 따라 정렬 (Dirty flag로 불필요한 정렬 방지)
- `ExecuteAll(TEventArgs eventArgs)`: 정렬된 순서대로 모든 액션을 비동기 실행
- 실행 시 스냅샷을 사용하여 실행 중 큐 변경에 안전
- `IReadOnlyList<ActionWrapper>` 구현으로 외부 접근 제공

**파일**: `ExecQueue.cs`

**우선순위 비교 규칙**:
1. Primary Priority 비교 (낮을수록 먼저)
2. Extra Priorities를 순서대로 비교
3. Extra Priorities 개수 비교 (적을수록 먼저)
4. Enqueued Order 비교 (FIFO)

### 4) ExecDynamicEventBus (동적 이벤트 버스)
- 제네릭 정적 클래스로 각 이벤트 타입별 버스 인스턴스 제공
- **호출 시점에 핸들러들이 큐에 우선순위와 함께 액션 등록**
- `Register(ExecEventHandler<TEvent> handler)`: 핸들러 등록
- `Unregister(ExecEventHandler<TEvent> handler)`: 핸들러 해제
- `ClearHandlers()`: 모든 핸들러 제거
- `Invoke(TEvent eventArgs)`: 
  - 등록된 모든 핸들러에 큐와 이벤트 인자 전달
  - 핸들러들이 큐에 우선순위와 함께 액션 등록
  - 우선순위에 따라 정렬 후 순차 실행
- `InvocationQueue(TEvent eventArgs)`: 핸들러 호출 후 정렬된 큐 반환 (Merged 호출용)

**파일**: `ExecDynamicEventBus.cs`

**특징**:
- 유연한 우선순위 결정 (런타임에 조건부로 우선순위 변경 가능)
- 매 호출 시 정렬 필요 (O(n log n))

### 5) ExecStaticEventBus (정적 이벤트 버스)
- **등록 시점에 우선순위를 고정**하여 성능 최적화
- `Register(int priority, ExecAction<TEvent> handler, params int[] extraPriorities)`: 우선순위와 함께 핸들러 등록
- `Unregister(ExecAction<TEvent> handler)`: 핸들러 해제
- `ClearHandlers()`: 모든 핸들러 제거
- `Invoke(TEvent eventArgs)`: 정렬된 순서대로 모든 핸들러 실행
- `GetExecQueue()`: 내부 ExecQueue 접근 (Merged 호출용)

**파일**: `ExecStaticEventBus.cs`

**특징**:
- 성능 최적화 (등록 시 한 번만 정렬)
- 우선순위 고정 (런타임 변경 불가)

### 6) ExecEventBus (통합 이벤트 버스)
- Dynamic과 Static EventBus를 통합 관리하는 Facade 클래스
- `RegisterDynamic()` / `UnregisterDynamic()`: 동적 핸들러 관리
- `RegisterStatic()` / `UnregisterStatic()`: 정적 핸들러 관리
- `ClearDynamicHandlers()` / `ClearStaticHandlers()` / `ClearAllHandlers()`: 핸들러 정리
- **`InvokeSequentially(TEvent args)`**: Dynamic 전체 실행 → Static 전체 실행 (순차)
- **`InvokeMerged(TEvent args)`**: Dynamic과 Static을 우선순위에 따라 병합 정렬하여 실행
  - 병합 정렬(merge sort) 알고리즘 사용
  - 두 큐의 우선순위를 비교하여 낮은(먼저 실행할) 것부터 실행
  - BreakChain 지원
- `IsExecuting`: 현재 실행 중인지 상태 확인

**파일**: `ExecEventBus.cs`

**InvokeMerged 동작**:
```
Dynamic: [5, 25], Static: [10, 20]
실행 순서: 5 (Dynamic) → 10 (Static) → 20 (Static) → 25 (Dynamic)
```

### 7) ExecEventUtil (유틸리티 클래스)
- `RuntimeInitializeOnLoadMethod`를 통한 자동 초기화
- 리플렉션을 사용하여 모든 `ExecEventArgs` 타입 자동 감지
- 각 이벤트 타입에 대한 `ExecDynamicEventBus`, `ExecStaticEventBus` 자동 생성
- `ClearBus()`: 모든 버스의 핸들러 초기화
- Unity Editor에서 플레이모드 종료 시 자동 정리 기능

**파일**: `ExecEventUtil.cs`

### 8) ExecEvents (예제 이벤트)
- `TestExecEventArgs`: 테스트용 예제 이벤트 클래스
- 새로운 이벤트 타입 생성 방법을 보여주는 예시

**파일**: `ExecEvents.cs`

### 9) TestScripts (테스트 코드)
- Region으로 구분된 체계적인 테스트 코드
- 7가지 테스트 케이스 제공:
  1. `ExecEvent/InvokeSequentially`: 순차 실행 테스트
  2. `ExecEvent/InvokeMerged`: 병합 실행 테스트
  3. `ExecEvent/InvokeDynamicOnly`: Dynamic만 실행
  4. `ExecEvent/InvokeStaticOnly`: Static만 실행
  5. `ExecEvent/TestBreakChain`: 이벤트 체인 중단 테스트
  6. `ExecEvent/TestExtraPriorities`: Extra Priorities 테스트
  7. `ExecEvent/TestExecutionState`: 실행 상태 모니터링 테스트

**파일**: `TestScripts.cs`

---

## ? 사용 방법

### 기본 이벤트 사용법 (Dynamic EventBus)

```csharp
// 1. 이벤트 인자 클래스 정의
public class PlayerDamageEventArgs : ExecEventArgs<PlayerDamageEventArgs>
{
    public Player Target;
    public int Damage;
    
    public override void Clear()
    {
        base.Clear();
        Target = null;
        Damage = 0;
    }
}

// 2. OnEnable에서 핸들러 등록 (여러 핸들러 등록 가능)
private void OnEnable()
{
    ExecDynamicEventBus<PlayerDamageEventArgs>.Register(OnPlayerDamage);
    ExecDynamicEventBus<PlayerDamageEventArgs>.Register(OnPlayerDamageEffect);
}

// 3. 핸들러 메서드 구현
private void OnPlayerDamage(ExecQueue<PlayerDamageEventArgs> queue, PlayerDamageEventArgs args)
{
    // 우선순위 0: 데미지 계산 (가장 먼저)
    queue.Enqueue(0, async (eventArgs) =>
    {
        int finalDamage = CalculateDamage(eventArgs.Damage, eventArgs.Target.Defense);
        eventArgs.Damage = finalDamage;
        LogEx.Log($"Calculated damage: {finalDamage}");
    });
    
    // 우선순위 1: 데미지 적용
    queue.Enqueue(1, async (eventArgs) =>
    {
        eventArgs.Target.HP -= eventArgs.Damage;
        LogEx.Log($"Applied damage. Current HP: {eventArgs.Target.HP}");
        
        // 죽었으면 체인 중단
        if (eventArgs.Target.HP <= 0)
        {
            eventArgs.BreakChain = true;
        }
    });
    
    // 우선순위 2: 사운드 재생 (BreakChain이 true면 실행 안 됨)
    queue.Enqueue(2, async (eventArgs) =>
    {
        await PlayDamageSound();
    });
}

// 다른 핸들러에서 이펙트 추가
private void OnPlayerDamageEffect(ExecQueue<PlayerDamageEventArgs> queue, PlayerDamageEventArgs args)
{
    // 우선순위 1.5로 데미지 적용과 사운드 사이에 실행
    queue.Enqueue(1, async (eventArgs) =>
    {
        await ShowDamageEffect(eventArgs.Target.Position);
    }, 5); // Extra Priority로 세밀한 순서 조정
}

// 4. OnDisable에서 핸들러 해제
private void OnDisable()
{
    ExecDynamicEventBus<PlayerDamageEventArgs>.Unregister(OnPlayerDamage);
    ExecDynamicEventBus<PlayerDamageEventArgs>.Unregister(OnPlayerDamageEffect);
}

// 5. 이벤트 발생
public async void DealDamage(Player target, int damage)
{
    using var args = PlayerDamageEventArgs.Get();
    args.Target = target;
    args.Damage = damage;
    
    await ExecDynamicEventBus<PlayerDamageEventArgs>.Invoke(args);
}
```

### Static EventBus 사용법

```csharp
// 등록 시점에 우선순위 고정 (성능 최적화)
private void OnEnable()
{
    ExecStaticEventBus<GameStateChangeEventArgs>.Register(0, async args =>
    {
        // 가장 먼저 실행
        await SaveGameState();
    });
    
    ExecStaticEventBus<GameStateChangeEventArgs>.Register(1, async args =>
    {
        // 두 번째로 실행
        await UpdateUI();
    });
    
    // Extra Priorities로 세밀한 제어
    ExecStaticEventBus<GameStateChangeEventArgs>.Register(1, async args =>
    {
        // Priority 1 중에서 더 먼저 실행
        await PlayTransitionEffect();
    }, 0); // Extra Priority 0
}

// 호출
private async void ChangeGameState(GameState newState)
{
    using var args = GameStateChangeEventArgs.Get();
    args.NewState = newState;
    
    await ExecStaticEventBus<GameStateChangeEventArgs>.Invoke(args);
}
```

### Merged Invocation 사용법

```csharp
// Dynamic과 Static을 함께 사용하고 우선순위에 따라 병합 실행
private void Setup()
{
    // Dynamic 핸들러 등록 (조건부 우선순위)
    ExecDynamicEventBus<TurnStartEventArgs>.Register((queue, args) =>
    {
        if (args.Player.IsFirstTurn)
        {
            queue.Enqueue(0, async a => await ShowTutorial());
        }
        else
        {
            queue.Enqueue(10, async a => await RefreshUI());
        }
    });
    
    // Static 핸들러 등록 (고정 우선순위)
    ExecStaticEventBus<TurnStartEventArgs>.Register(5, async args =>
    {
        await DrawCard(args.Player);
    });
    
    ExecStaticEventBus<TurnStartEventArgs>.Register(7, async args =>
    {
        await ResetActionPoints(args.Player);
    });
}

// Merged 호출 - Dynamic과 Static이 우선순위 순서대로 섞여서 실행됨
private async void StartTurn(Player player)
{
    using var args = TurnStartEventArgs.Get();
    args.Player = player;
    
    // 첫 턴: ShowTutorial(0) -> DrawCard(5) -> ResetActionPoints(7)
    // 이후: DrawCard(5) -> ResetActionPoints(7) -> RefreshUI(10)
    await ExecEventBus<TurnStartEventArgs>.InvokeMerged(args);
}
```

### Extra Priorities 사용법

```csharp
// 같은 Primary Priority 내에서 세밀한 순서 제어
ExecStaticEventBus<TestExecEventArgs>.Register(10, Handler1);           // (10)
ExecStaticEventBus<TestExecEventArgs>.Register(10, Handler2, 5);        // (10, 5)
ExecStaticEventBus<TestExecEventArgs>.Register(10, Handler3, 5, 3);     // (10, 5, 3)
ExecStaticEventBus<TestExecEventArgs>.Register(10, Handler4, 8);        // (10, 8)

// 실행 순서:
// 1. Handler1 (10) - Extra가 없으므로 가장 먼저
// 2. Handler2 (10, 5) - Extra[0]이 5
// 3. Handler3 (10, 5, 3) - Extra[0]이 5로 같고 개수가 더 많아서 뒤로
// 4. Handler4 (10, 8) - Extra[0]이 8로 가장 큼
```

### BreakChain 사용법

```csharp
private void OnGameEvent(ExecQueue<GameEventArgs> queue, GameEventArgs args)
{
    queue.Enqueue(0, async (eventArgs) =>
    {
        if (!ValidateGameState())
        {
            LogEx.LogWarning("Invalid game state. Breaking event chain.");
            eventArgs.BreakChain = true;
            return;
        }
        
        await ProcessEvent();
    });
    
    queue.Enqueue(1, async (eventArgs) =>
    {
        // BreakChain이 true면 실행되지 않음
        await UpdateUI();
    });
}
```

---

## ? 특징 및 장점

### 설계 철학
1. **Dynamic vs Static 분리**: 유연성과 성능을 상황에 맞게 선택
2. **Merged Invocation**: 두 방식의 장점을 결합
3. **우선순위 세밀 제어**: Primary + Extra Priorities로 복잡한 순서 관리
4. **메모리 효율**: ActionWrapper와 EventArgs 모두 풀링 사용

### 주요 장점
- ? **유연성**: Dynamic은 런타임 조건에 따라 우선순위 변경 가능
- ? **성능**: Static은 등록 시 한 번만 정렬하여 호출 오버헤드 최소화
- ? **병합 실행**: InvokeMerged로 두 방식을 우선순위 기준으로 통합 실행
- ? **세밀한 제어**: Extra Priorities로 복잡한 실행 순서 관리
- ? **비동기 처리**: UniTask 기반 효율적인 비동기 실행
- ? **메모리 효율**: ObjectPool로 GC 압박 최소화
- ? **안전성**: 
  - try-finally로 예외 시에도 상태 복구
  - 스냅샷 방식으로 실행 중 큐 변경에 안전
- ? **체인 중단**: BreakChain으로 조건부 실행 중단
- ? **자동 관리**: 리플렉션 기반 자동 초기화 및 정리

### 성능 비교

| 방식 | 등록 비용 | 호출 비용 | 적합한 상황 |
|------|-----------|-----------|-------------|
| **Dynamic** | O(1) | O(n log n) | 호출이 적고 우선순위가 동적으로 변하는 경우 |
| **Static** | O(n log n) | O(n) | 호출이 빈번하고 우선순위가 고정된 경우 |
| **Merged** | - | O(n + m) | 두 방식을 함께 사용할 때 |

### 우선순위 비교 규칙
```
비교 순서:
1. Primary Priority (낮을수록 먼저)
2. Extra Priorities[0], [1], ... (순서대로 비교)
3. Extra Priorities 개수 (적을수록 먼저)
4. Enqueued Order (등록 순서, FIFO)
```

---

## ?? 주의사항

### 필수 사항
- ? **이벤트 인자 풀링**: `Get()`으로 가져오고 `using` 또는 `Release()` 필수
- ? **핸들러 등록/해제**: `OnEnable`/`OnDisable`에서 관리 권장
- ? **실행 중 큐 수정 금지**: 스냅샷 사용으로 반영 안 됨

### 베스트 프랙티스
- ? **Dynamic**: 조건부 우선순위, 복잡한 로직, 호출 빈도 낮음
- ? **Static**: 고정 우선순위, 단순 로직, 호출 빈도 높음
- ? **Merged**: 두 방식을 함께 사용하고 통합 우선순위 순서가 중요할 때
- ? **Extra Priorities**: 같은 Primary Priority 내에서 세밀한 순서 제어 필요 시

### 주의해야 할 케이스
```csharp
// ? 잘못된 사용: 실행 중 큐에 추가 (반영 안 됨)
queue.Enqueue(0, async args =>
{
    queue.Enqueue(1, async a => { /* 실행되지 않음 */ });
});

// ? 올바른 사용: 핸들러에서 모든 액션 등록
private void OnEvent(ExecQueue<EventArgs> queue, EventArgs args)
{
    queue.Enqueue(0, FirstAction);
    queue.Enqueue(1, SecondAction);
}
```

---

## ? 테스트

### 테스트 환경
- Unity 2022.3 이상
- UniTask 패키지 필요

### 제공되는 테스트
`TestScripts.cs`에 7가지 테스트 케이스 포함:
1. **InvokeSequentially**: Dynamic 전체 → Static 전체 순차 실행
2. **InvokeMerged**: 우선순위 기반 병합 실행
3. **InvokeDynamicOnly**: Dynamic만 실행
4. **InvokeStaticOnly**: Static만 실행
5. **TestBreakChain**: 이벤트 체인 중단 동작 확인
6. **TestExtraPriorities**: Extra Priorities 비교 로직 검증
7. **TestExecutionState**: IsExecuting 상태 확인

### 실행 방법
Unity에서 `TestScripts` 컴포넌트 우클릭 → `ExecEvent/` 메뉴에서 테스트 선택

---

## ?? 알려진 문제 (Known Issues)

현재 알려진 문제는 없습니다.

---

## ? 추가 참고사항

### 아키텍처 다이어그램
```
ExecEventBus<T> (Facade)
├─ ExecDynamicEventBus<T>
│  └─ ExecQueue<T>
│     └─ ActionWrapper (ObjectPool)
└─ ExecStaticEventBus<T>
   └─ ExecQueue<T>
      └─ ActionWrapper (ObjectPool)

ExecEventUtil (Auto Initialize & Cleanup)
```

### 파일 구조
```
Events/ExecEvent/
├─ ExecEventArgs.cs         (이벤트 인자 베이스)
├─ ExecPriority.cs          (우선순위 Enum)
├─ ExecQueue.cs             (우선순위 큐)
├─ ExecDynamicEventBus.cs   (동적 이벤트 버스)
├─ ExecStaticEventBus.cs    (정적 이벤트 버스)
├─ ExecEventBus.cs          (통합 이벤트 버스)
├─ ExecEventUtil.cs         (유틸리티)
└─ ExecEvents.cs            (예제 이벤트)

Test/
└─ TestScripts.cs           (테스트 코드)
```

---

## ? 체크리스트

- [x] Namespace 규칙 확인 (`PriortyExecEvent` - 오타 포함, 기존 코드와 일관성 유지)
- [x] public 함수의 경우 XML 주석 작성
- [x] 코드 리뷰 완료
- [x] 테스트 코드 작성 및 검증
- [x] 메모리 누수 체크 (ObjectPool 사용)
- [x] 예외 처리 (try-finally)
- [x] BreakChain 동작 검증
- [x] Merged 병합 로직 검증

---

## 연관 PR

(해당 시 작성)

---

## 연관 이슈

#19 (해당 시 작성)

