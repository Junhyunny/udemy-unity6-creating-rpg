# GetComponent의 동작 방식과 null 방어

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> GetComponent 함수를 통해 컴포넌트를 가져올 때 어떻게 가져오는거야? 동일한 오브젝트에서 해당 컴포넌트가
> 있으면 가져오는거야? nullable 인데 이런 경우에는 어떻게 방어 로직을 작성하지? 항상 nullable 여부를 확인 후 사용하나?

## 질문이 나온 코드

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponentInChildren<Animator>();
    groundCheckDistance = 1.5f;
}
```

## 1. 어떻게 가져오는가 — "같은 게임 오브젝트의 컴포넌트 목록을 훑는다"

**추측한 대로다.** `GetComponent<T>()`는 **자신이 붙어 있는 게임 오브젝트**의 컴포넌트 목록을 순회하면서 `T`에 대입 가능한 첫 번째 컴포넌트를 반환한다. 없으면 `null`.

- 인터페이스나 부모 클래스로도 찾을 수 있다 (`GetComponent<Collider2D>()`가 `BoxCollider2D`를 찾아줌)
- 같은 타입이 여러 개면 **첫 번째 하나만** 반환한다. 전부 필요하면 `GetComponents<T>()`
- 다른 게임 오브젝트는 **보지 않는다**

### 탐색 범위가 다른 형제 함수들

| 함수 | 탐색 범위 |
| --- | --- |
| `GetComponent<T>()` | **자기 자신만** |
| `GetComponentInChildren<T>()` | **자기 자신 + 모든 자손** (깊이 우선) |
| `GetComponentInParent<T>()` | **자기 자신 + 모든 조상** |
| `GetComponentsInChildren<T>()` | 위 범위의 **전부**를 배열로 |

**주의: `InChildren`/`InParent`는 자기 자신을 포함한다.** 이름만 보면 자식만 볼 것 같지만 아니다.

이 프로젝트의 구조가 그 예다.

```
Player                    ← Player.cs, Rigidbody2D, CapsuleCollider2D  (Layer 0)
└── Animator              ← Animator, SpriteRenderer, PlayerAnimationEvents.cs
```

(`Assets/Scenes/SampleScene.unity` 기준. 자식 게임 오브젝트의 이름이 그냥 `Animator`다.)

- `Player.cs`의 `GetComponentInChildren<Animator>()` → 자식에 있는 Animator를 찾는다
- `PlayerAnimationEvents.cs`의 `GetComponentInParent<Player>()` → 부모의 Player를 찾는다

`GetComponentInChildren`은 **비활성(inactive) 게임 오브젝트를 기본적으로 건너뛴다.** 포함하려면 `GetComponentInChildren<T>(true)`.

### 비용

컴포넌트 목록을 실제로 순회하므로 공짜가 아니다. **`Awake`나 `Start`에서 한 번 찾아 필드에 캐싱**하고, `Update` 안에서 매 프레임 호출하지 않는 것이 원칙이다. 이 프로젝트는 `Awake`에서 캐싱하고 있으니 올바른 패턴이다.

## 2. "항상 null 체크를 하나?" — 아니다. 상황에 따라 다르다

핵심은 **"없을 수 있는가"** 다.

### 없으면 안 되는 컴포넌트 → 체크하지 말고 강제하라

`Player`에게 `Rigidbody2D`는 **없으면 애초에 동작이 불가능한 필수 의존성**이다. 이런 건 매번 `if (rb != null)`로 감싸는 게 아니라 **애초에 없는 상태를 못 만들게** 하는 편이 낫다.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
```

`[RequireComponent]`를 붙이면 **이 스크립트를 붙이는 순간 Unity가 Rigidbody2D를 자동으로 추가**하고, 사용자가 그걸 제거하려 하면 막는다.

여기에 초기화 시점 검증을 더하면 충분하다.

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponentInChildren<Animator>();

    if (rb == null) Debug.LogError($"{name}: Rigidbody2D가 없습니다.", this);
    if (animator == null) Debug.LogError($"{name}: 자식에 Animator가 없습니다.", this);
}
```

**한 번만 검사하고 시끄럽게 실패한다.** 매 프레임 조용히 넘어가는 것보다, 실행 즉시 콘솔에 원인이 찍히는 게 훨씬 빨리 고쳐진다. `Debug.LogError`의 두 번째 인자로 `this`를 넘기면 콘솔에서 클릭했을 때 해당 오브젝트가 Hierarchy에서 선택된다.

### 있을 수도 없을 수도 있는 컴포넌트 → 반드시 확인하라

충돌한 상대가 체력을 가졌는지처럼 **정말로 불확실한 경우**가 진짜 null 체크 대상이다.

```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.TryGetComponent<Health>(out var health))
    {
        health.TakeDamage(10);
    }
    // 없으면 그냥 아무 일도 안 일어난다 — 정상 흐름
}
```

`TryGetComponent<T>(out T)`는 이 패턴을 위한 함수다. `GetComponent` + null 체크보다 의도가 명확하고, **컴포넌트를 못 찾았을 때 에디터에서 불필요한 할당이 발생하지 않는다.**

### 정리

| 상황 | 방법 |
| --- | --- |
| 필수 의존성 | `[RequireComponent]` + `Awake`에서 1회 `LogError` 검증 |
| Inspector로 주입 | `[SerializeField]` + `Awake`에서 1회 검증 |
| 있을 수도 없을 수도 | `TryGetComponent` |
| 매 프레임 호출 | ❌ 하지 말고 캐싱 |

## 3. Unity의 null은 보통 C#의 null이 아니다 (중요)

`UnityEngine.Object`는 `==` 연산자를 **오버로딩**해 놓았다. 그래서 다음과 같은 함정이 있다.

```csharp
Destroy(someComponent);
// 이후...
if (someComponent == null)   // true  ← Unity가 "파괴됨"을 null로 보고함
if (someComponent is null)   // false ← C# 참조는 아직 살아 있음!
```

이걸 **"fake null"** 이라고 부른다. 네이티브 쪽 객체는 파괴됐지만 C# 래퍼 객체는 남아 있어서 생기는 현상이다.

실무에서 지킬 규칙:

- `UnityEngine.Object` 파생 타입에는 `== null` / `!= null`을 쓴다
- **`is null`, `?.`, `??`, `??=`는 쓰지 않는다** — 오버로딩된 `==`를 우회해서 파괴된 객체를 살아 있다고 판단한다

```csharp
rb?.AddForce(Vector2.up);      // ❌ 파괴된 객체에도 호출 시도
if (rb != null) rb.AddForce(); // ✅
```

## 4. 이 프로젝트 코드에 대한 메모

```csharp
[SerializeField] private Rigidbody2D rb;   // Inspector로도 주입 가능

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();       // 그런데 Awake에서 덮어쓴다
}
```

`rb`가 `[SerializeField]`인데 `Awake`에서 `GetComponent`로 다시 대입하고 있다. **Inspector에서 무엇을 끌어다 놓든 실행 시점에 같은 오브젝트의 Rigidbody2D로 교체된다.** 둘 중 하나로 정하는 게 혼란이 적다. → [inspector-reference-vs-code-assignment.md](./inspector-reference-vs-code-assignment.md)

## 체크리스트

- [ ] `GetComponent`가 같은 게임 오브젝트만 본다는 걸 안다
- [ ] `InChildren` / `InParent`가 자기 자신을 포함한다는 걸 안다
- [ ] `GetComponentInChildren`이 비활성 오브젝트를 건너뛴다는 걸 안다
- [ ] `Awake`/`Start`에서 캐싱해야 하는 이유를 안다
- [ ] 필수 의존성과 선택적 의존성의 방어 방법을 구분할 수 있다
- [ ] `[RequireComponent]`를 쓸 수 있다
- [ ] `TryGetComponent`를 언제 쓰는지 안다
- [ ] Unity의 fake null과 `?.`을 쓰면 안 되는 이유를 설명할 수 있다

## 공식 참고 자료

- [Scripting API: Component.GetComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.GetComponent.html)
- [Scripting API: Component.TryGetComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.TryGetComponent.html)
- [Scripting API: RequireComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RequireComponent.html)
- [Manual: Using components](https://docs.unity3d.com/6000.6/Documentation/Manual/UsingComponents.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
