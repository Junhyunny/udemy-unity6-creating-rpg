# 상태를 만들기 전에 Animator를 먼저 찾아야 하는 이유

> 질문: 강의에서 애니메이터를 먼저 찾지 않으면 상태가 제대로 할당되지 않는다고 하던데, 어떤 내용인가? 컴포넌트를 먼저 메모리에 로딩하지 않으면 어떤 문제가 생기는가?
>
> 프로젝트 기준: `chapter-045`는 Unity 6000.6.3f1을 사용한다.

## 질문이 나온 코드

`chapter-045/Assets/Scripts/Entity.cs`

```csharp
public virtual void Awake()
{
    animator = GetComponentInChildren<Animator>();   // ① Animator를 찾아서 저장
    rb = GetComponent<Rigidbody2D>();
    stateMachine = new StateMachine();
}
```

`chapter-045/Assets/Scripts/Player.cs`

```csharp
public override void Awake()
{
    base.Awake();                                     // ① Entity.Awake 먼저 실행
    ...
    idleState = new PlayerIdleState(this, stateMachine); // ② 상태 객체 생성
    ...
}
```

`chapter-045/Assets/Scripts/PlayerState.cs`, `EntityState.cs`

```csharp
public PlayerState(Player player, ...) : base(player.animator, ...)  // ③ 이 순간의 player.animator를 넘김
public EntityState(Animator animator, ...)
{
    this.animator = animator;                         // ④ 넘겨받은 값을 복사해서 보관
}
```

## 핵심: "메모리 로딩"이 아니라 "참조를 복사하는 시점" 문제

먼저 오해를 하나 풀자. `GetComponentInChildren<Animator>()`는 컴포넌트를 메모리에 **불러오는** 함수가 아니다. 씬이 열리는 순간 Player 오브젝트와 그 자식의 Animator 컴포넌트는 이미 만들어져 있다. 이 함수는 이미 있는 Animator를 **찾아서 그 위치(참조)를 알려 줄 뿐**이다. ([Component.GetComponentInChildren](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.GetComponentInChildren.html))

문제는 `player.animator` 변수에 그 참조를 **언제 넣느냐**다.

C#에서 `this.animator = animator;`는 Animator 객체를 통째로 복사하지 않는다. "Animator가 어디 있는지 적힌 쪽지"(참조)를 복사한다. 그리고 **복사하는 그 순간의 쪽지 내용**이 저장된다. 나중에 원본 변수(`player.animator`)가 바뀌어도, 상태 객체가 이미 복사해 간 쪽지는 바뀌지 않는다. ([C# 참조 형식](https://learn.microsoft.com/ko-kr/dotnet/csharp/language-reference/keywords/reference-types))

## 순서가 맞을 때와 틀릴 때

**올바른 순서 (현재 코드)**

| 단계 | 실행 | `player.animator` | 상태가 복사해 간 값 |
| --- | --- | --- | --- |
| 1 | `animator = GetComponentInChildren<Animator>()` | Animator 참조 | — |
| 2 | `new PlayerIdleState(this, ...)` | Animator 참조 | **Animator 참조** |
| 3 | 상태의 `Enter()`에서 `animator.SetBool(...)` | | 정상 동작 |

**잘못된 순서 (상태를 먼저 만든 경우)**

```csharp
idleState = new PlayerIdleState(this, stateMachine);  // 이 시점 player.animator == null
animator = GetComponentInChildren<Animator>();        // 이제야 찾음
```

| 단계 | 실행 | `player.animator` | 상태가 복사해 간 값 |
| --- | --- | --- | --- |
| 1 | `new PlayerIdleState(this, ...)` | **null** (아직 안 찾음) | **null** |
| 2 | `animator = GetComponentInChildren<Animator>()` | Animator 참조 | 여전히 **null** |
| 3 | 상태의 `Enter()`에서 `animator.SetBool(...)` | | **NullReferenceException** |

2단계에서 `player.animator`는 채워졌지만 상태 객체는 1단계에서 null을 복사해 갔다. 그래서 애니메이션이 바뀌지 않고, Console에 `NullReferenceException`이 찍힌다. 강의에서 말한 "상태가 제대로 할당되지 않는다"는 이 상황이다.

## 왜 `Awake`에서 찾고 `Start`에서 시작하나

Unity는 씬의 모든 오브젝트에 대해 `Awake()`를 먼저 다 호출한 뒤, `Start()`를 호출한다. ([이벤트 함수 실행 순서](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html), [MonoBehaviour.Awake](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Awake.html), [MonoBehaviour.Start](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Start.html))

- `Awake`: 자기 자신의 준비. 컴포넌트를 찾고(`GetComponent`) 상태 객체를 만든다. **이 함수 안에서도 순서가 중요하다** — 위에서 본 것처럼 찾기가 만들기보다 먼저다.
- `Start`: 모두 준비된 뒤의 시작. `stateMachine.Initialize(idleState)`로 첫 상태의 `Enter()`를 호출해 애니메이터를 실제로 사용한다.

현재 구조에서는 `Player.Awake()`의 첫 줄 `base.Awake()`가 이 순서를 보장한다. `base.Awake()`를 상태 생성 코드보다 아래로 옮기거나 지우면 위의 "잘못된 순서"가 된다.

## 다른 방법: 복사하지 않고 매번 읽기

상태가 Animator를 따로 보관하지 않고, 필요할 때마다 `player.animator`를 읽으면 순서 문제는 사라진다.

```csharp
// 예시: 보관하지 않고 매번 player를 통해 읽기
player.animator.SetBool(animationStateName, true);
```

대신 매번 `player`를 거쳐야 하고, `EntityState`가 `Player`에 다시 의존하게 된다. 이번 리팩터링에서 `EntityState`가 `Player` 대신 `Animator`만 받도록 바꾼 것은 이 의존을 끊기 위해서이므로, 지금 구조에서는 "먼저 찾고 나중에 만든다"는 순서를 지키는 쪽이 맞다.

## 직접 확인해 보기

1. `Player.Awake()`에서 `base.Awake();`를 상태 생성 코드 아래로 잠시 옮긴다.
2. Play를 누르고 Console에서 `NullReferenceException`이 어느 줄에서 나는지 확인한다. (`EntityState.Enter`의 `animator.SetBool`일 것이다)
3. 원래대로 되돌린다.

## 체크리스트

- [ ] `GetComponent`는 컴포넌트를 "만드는" 것이 아니라 이미 있는 것을 "찾는" 것임을 설명할 수 있다.
- [ ] 참조 형식 필드에 대입하면 그 순간의 참조가 복사된다는 것을 설명할 수 있다.
- [ ] 상태를 먼저 만들면 왜 상태 안의 `animator`가 null로 남는지 설명할 수 있다.
- [ ] `Awake`와 `Start`의 실행 순서와 각각 어떤 일을 맡기는지 설명할 수 있다.
- [ ] `base.Awake()`가 `Player.Awake()`의 첫 줄에 있어야 하는 이유를 설명할 수 있다.

## 공식 참고 자료

- [Component.GetComponentInChildren](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.GetComponentInChildren.html) — 자신과 자식에서 컴포넌트 찾기
- [MonoBehaviour.Awake](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Awake.html) · [MonoBehaviour.Start](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Start.html) — 초기화 함수의 호출 시점
- [이벤트 함수 실행 순서](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html) — 모든 Awake 뒤에 Start
- [NullReferenceException](https://learn.microsoft.com/ko-kr/dotnet/api/system.nullreferenceexception) — null 참조를 사용할 때 나는 예외
- [C# 참조 형식](https://learn.microsoft.com/ko-kr/dotnet/csharp/language-reference/keywords/reference-types) — 참조를 대입할 때 일어나는 일
- [C# base 키워드](https://learn.microsoft.com/ko-kr/dotnet/csharp/language-reference/keywords/base) — 부모 클래스의 메서드 호출
- 관련 문서: [GetComponent와 null 처리](../chapter-007/getcomponent-and-null-handling.md), [이벤트 함수 실행 순서](../chapter-007/event-function-execution-order.md)
