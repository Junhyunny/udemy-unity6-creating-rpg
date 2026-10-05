# 상태 머신의 순환 참조와 설계 선택

> 원본 TODO: `chapter-025/Assets/Player.cs`, `chapter-025/Assets/EntityState.cs`
>
> 질문: `Player`와 각 상태, `EntityState`와 `StateMachine`이 서로 참조한다. 잘못된 설계인가? 게임 개발에서 상태 머신을 어떻게 구성하는가?
>
> 프로젝트 버전: Unity 6000.6.3f1. 아래 패턴 자료는 에디터 버전과 무관한 설계 설명이다.

## 현재 코드에서 일어나는 일

`Player.Awake()`는 상태 머신과 `PlayerIdleState`, `PlayerMoveState`를 만들고 보관한다. 상태 객체는 생성자에서 `Player`와 `StateMachine`을 받는다. 상태 머신은 현재 `EntityState`를 보관한다.

```text
Player ──보유──> StateMachine ──currentState──> EntityState
   │                                      │          │
   └──보유──> IdleState / MoveState ──────┘          │
   <──────────────── 상태가 Player를 참조 ───────────┘

EntityState ──참조──> StateMachine ──참조──> EntityState
```

`PlayerIdleState.Update()`는 `player.moveInput`을 검사하고 `stateMachine.ChangeState(player.moveState)`를 호출한다. 즉 상태는 **주인의 데이터**, **다음 상태**, **전환 담당 객체**를 모두 알고 있다. 두 TODO는 같은 객체 관계의 서로 다른 부분을 가리키므로 한 문서에서 다룬다.

## 순환 참조가 곧 오류인가?

**그 자체로는 아니다.** Unity의 [State 패턴 공식 학습 자료](https://learn.unity.com/course/design-patterns/tutorial/develop-a-modular-flexible-codebase-with-the-state-programming-pattern)도 플레이어가 상태 머신을 보유하고, 상태가 플레이어를 참조하며, 상태가 전환을 요청하는 예를 사용한다. 플레이어 행동처럼 상태가 주인의 위치·입력·애니메이션을 읽어야 할 때 자연스러운 구성이 될 수 있다.

다만 다음 두 문제를 구분해야 한다.

| 문제 | 이 코드에 해당하는가? | 의미 |
| --- | --- | --- |
| 객체가 서로 참조 | 해당 | `Player` 인스턴스와 상태 인스턴스가 서로를 가리킴. C# GC에서 이 사실만으로 메모리 누수가 생기지는 않음 |
| 클래스가 서로의 구체 타입을 앎 | 해당 | 상태가 `Player`와 `StateMachine`, 다른 구체 상태에 묶여 재사용·독립 테스트가 어려워짐 |

첫 번째는 **생명주기** 문제이고, 두 번째는 **결합도** 문제다. 도달할 수 없는 객체끼리 서로 참조해도 GC가 수집할 수 있다. 반면 외부의 이벤트 구독처럼 객체를 계속 도달 가능하게 만드는 참조는 별도로 관리해야 한다. [Microsoft C#의 자동 메모리 관리](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/basic-concepts#79-automatic-memory-management)

## 이 강의 코드의 장단점

### 장점

- `Idle`과 `Move`의 조건 및 동작을 상태별 클래스로 나눴다.
- `Enter → Update → Exit`의 전환 과정을 추적하기 쉽다.
- 상태가 둘뿐이고 `Player` 하나에만 쓸 목적이라면 구조가 단순하다.

### 커질 때 점검할 점

- `EntityState`가 모든 상태에 `Player`와 `StateMachine`을 요구한다. 플레이어만 쓰는 상태라면 `Player`를 통해 머신에 접근하거나, 머신만 쓰는 상태라면 주인 참조를 줄일 수 있다. **둘 다 반드시 저장할 필요는 없다.** Unity 공식 예제도 플레이어가 머신을 이미 보유하므로 상태 생성자에는 플레이어 하나만 전달할 수 있다고 설명한다.
- `PlayerIdleState`가 `player.moveState`라는 구체적인 다음 상태를 직접 안다. 상태가 늘어나면 전환 규칙이 여러 상태 클래스에 흩어질 수 있다.
- 다른 캐릭터의 상태에도 같은 추상 클래스를 쓰려면 `EntityState`의 `Player` 필드가 걸림돌이 된다. 이때 필요한 데이터와 전환 기능만 제공하는 컨텍스트 인터페이스나 공통 기반 타입을 검토할 수 있다. 추상화는 실제 재사용 요구가 생길 때 도입한다.

## 게임에서 흔히 선택하는 구성

| 상황 | 구성 | 이 프로젝트에 대한 판단 |
| --- | --- | --- |
| 상태가 적고 조건이 단순함 | `enum`과 `switch` | 작은 예제에는 충분하지만 조건이 늘면 한 클래스가 커짐 |
| 상태별 로직이 분명함 | 상태 객체 + `Enter/Update/Exit` + 머신 | **현재 강의 코드**. Unity 공식 State 패턴 예제와 비슷함 |
| 여러 캐릭터에 상태 로직을 재사용함 | 상태가 구체 `Player` 대신 필요한 기능의 인터페이스를 받음 | 재사용이 필요해질 때 고려 |
| 전환 조건을 한곳에서 관리해야 함 | 상태는 신호/결과를 반환하고 머신 또는 컨텍스트가 전환 결정 | 전환 규칙이 흩어져 유지보수가 어려울 때 고려 |

한 가지 정답은 없다. Unity는 State 패턴을 게임에서 널리 쓰는 방법으로 소개하면서도 **상태가 몇 개뿐이라면 구조가 과할 수 있다**고 설명한다. [Unity Learn: State programming pattern](https://learn.unity.com/course/design-patterns/tutorial/develop-a-modular-flexible-codebase-with-the-state-programming-pattern)

## 직접 점검해 볼 것

1. `Player`, `StateMachine`, `EntityState` 사이의 참조 화살표를 직접 그린다.
2. `PlayerIdleState`에서 `player.moveInput`, `player.moveState`, `stateMachine.ChangeState`가 각각 왜 필요한지 표시한다.
3. `JumpState`를 추가한다고 가정하고 어느 파일을 고쳐야 하는지 적어 본다. 수정 범위가 넓어지면 결합도를 낮출 근거가 된다.
4. `Player`가 `StateMachine`을 노출한다고 가정하고 상태 생성자에서 머신 인자를 없앨 수 있는지 설계만 비교해 본다.

## 이해도 체크

- [ ] 객체 간 순환 참조와 클래스 간 결합도를 구분할 수 있다.
- [ ] 순환 참조 자체가 C#의 메모리 누수를 뜻하지 않는 이유를 설명할 수 있다.
- [ ] 현재 상태가 `Player`와 `StateMachine`을 모두 받는 이유와 줄일 수 있는 조건을 설명할 수 있다.
- [ ] 상태 수와 재사용 요구에 따라 `switch`, 상태 객체, 인터페이스 중 무엇을 택할지 설명할 수 있다.

## 공식 참고 자료

- [Unity Learn: State programming pattern](https://learn.unity.com/course/design-patterns/tutorial/develop-a-modular-flexible-codebase-with-the-state-programming-pattern) — 플레이어·상태·머신 구성 예제와 선택 기준
- [Unity: Game programming patterns and SOLID](https://unity.com/blog/games/level-up-your-code-with-game-programming-patterns) — 추상화와 의존성 역전의 판단 기준
- [Microsoft Learn: C# automatic memory management](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/basic-concepts#79-automatic-memory-management) — 도달 가능성과 GC
