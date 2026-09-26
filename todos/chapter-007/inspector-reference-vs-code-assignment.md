# 인스펙터 드래그 앤 드롭 대신 코드로 참조 설정하기

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Unity 도구에서 내가 드래그-앤-드랍으로 Rb 객체를 셋업했는데, 코드에서는 셋업할 수 있는 방법이 없나?

## 짧은 답: 있다. 이 파일에 이미 쓰고 있다

```csharp
[SerializeField] private Rigidbody2D rb;   // ← 인스펙터 드래그 앤 드롭용

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();       // ← 코드로 설정. 이게 이미 있다
}
```

`Awake()`의 `GetComponent<Rigidbody2D>()`가 정확히 "코드에서 셋업하는 방법"이다.

**다만 지금 코드는 두 방법을 동시에 쓰고 있어서, 인스펙터에서 무엇을 끌어다 놓든 실행 시점에 `Awake`가 덮어쓴다.** 둘 중 하나로 정하는 게 좋다.

## 코드로 참조를 얻는 방법들

### ① 같은/자식/부모 오브젝트에서 찾기 — 가장 흔함

```csharp
rb       = GetComponent<Rigidbody2D>();          // 자기 자신
animator = GetComponentInChildren<Animator>();   // 자신 + 자손
player   = GetComponentInParent<Player>();       // 자신 + 조상
```

이 프로젝트가 세 가지를 모두 쓰고 있다. → [getcomponent-and-null-handling.md](./getcomponent-and-null-handling.md)

### ② 씬 전체에서 찾기 — 느리므로 초기화 때 한 번만

```csharp
var manager = FindFirstObjectByType<GameManager>();
var all     = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
```

씬의 모든 오브젝트를 훑기 때문에 **`Update`에서 호출하면 안 된다.** 구버전의 `FindObjectOfType`은 Unity 6에서 `FindFirstObjectByType` / `FindAnyObjectByType`으로 대체됐다.

### ③ 이름/태그로 찾기 — 문자열 의존이라 취약

```csharp
var go = GameObject.Find("Player");             // 이름 바꾸면 조용히 깨짐
var go = GameObject.FindWithTag("Player");      // 태그 기반. Find보다는 나음
```

리팩터링에 약해서 되도록 피한다.

### ④ 없으면 직접 추가하기

```csharp
rb = GetComponent<Rigidbody2D>();
if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
```

### ⑤ 에디터에서 자동으로 채워 넣기 — 두 방식의 절충

`Reset()`은 **컴포넌트를 처음 붙였을 때와 Inspector의 Reset을 눌렀을 때 에디터에서만** 호출된다.

```csharp
private void Reset()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponentInChildren<Animator>();
}
```

이러면 스크립트를 붙이는 순간 **인스펙터 슬롯이 자동으로 채워지고**, 그 값이 씬에 저장된다. 런타임에는 아무 탐색도 하지 않는다. 실수로 빈 슬롯을 남길 위험을 줄이면서 인스펙터 주입의 장점을 유지하는 방법이다.

### ⑥ 필수 컴포넌트를 강제하기

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour { }
```

스크립트를 붙이면 Rigidbody2D가 자동 추가되고, 제거하려 하면 에디터가 막는다.

## 어느 쪽을 써야 하나

| | 인스펙터 주입 (`[SerializeField]`) | 코드 탐색 (`GetComponent`) |
| --- | --- | --- |
| 참조 대상 | **다른 게임 오브젝트**도 가능 | 자신/자식/부모 계층 안 |
| 런타임 비용 | 없음 (씬에 저장된 참조) | 탐색 비용 발생 |
| 연결 확인 | 인스펙터에서 눈으로 보임 | 코드를 읽어야 앎 |
| 실수 가능성 | **빈 슬롯을 남기면 NullReference** | 컴포넌트가 없으면 null |
| 리팩터링 | 이름 바꿔도 연결 유지 (GUID 기반) | 타입만 맞으면 유지 |
| 프리팹 재사용 | 프리팹 내부 참조는 안전 | 항상 안전 |

**실무 기준**

- **같은 오브젝트에 반드시 함께 있는 컴포넌트** (Player ↔ Rigidbody2D) → `GetComponent` + `[RequireComponent]`
- **다른 오브젝트/에셋 참조** (사운드 클립, 이펙트 프리팹, UI 패널) → `[SerializeField]` 인스펙터 주입
- 둘 다 쓰고 싶다면 → `[SerializeField]` + `Reset()` 자동 채움

## 이 프로젝트에 적용한다면

`rb`는 Player 오브젝트에 반드시 함께 있어야 하는 컴포넌트다. 인스펙터 슬롯을 없애고 코드로 통일하는 쪽이 깔끔하다.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    private Rigidbody2D rb;          // [SerializeField] 제거
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
}
```

반대로 인스펙터 주입으로 통일하려면 `Awake`의 `rb = GetComponent...` 줄을 지우면 된다. **지금처럼 둘 다 두면 인스펙터에 값이 보이는데 실제로는 무시되므로 디버깅할 때 헷갈린다.**

## 체크리스트

- [ ] `GetComponent`가 "코드로 참조를 설정하는 방법"임을 안다
- [ ] 현재 코드에서 `[SerializeField] rb`가 `Awake`에 덮어써진다는 걸 확인했다
- [ ] 인스펙터 주입과 코드 탐색의 장단점을 비교할 수 있다
- [ ] `Reset()`이 에디터 전용이고 언제 호출되는지 안다
- [ ] `FindObjectOfType`이 Unity 6에서 무엇으로 바뀌었는지 안다
- [ ] 다른 게임 오브젝트 참조는 왜 인스펙터 주입이 나은지 설명할 수 있다

## 공식 참고 자료

- [Scripting API: Component.GetComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.GetComponent.html)
- [Scripting API: MonoBehaviour.Reset](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Reset.html)
- [Scripting API: RequireComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RequireComponent.html)
- [Scripting API: Object.FindFirstObjectByType](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Object.FindFirstObjectByType.html)
- [Scripting API: SerializeField](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SerializeField.html)
- [Manual: Using components](https://docs.unity3d.com/6000.6/Documentation/Manual/UsingComponents.html)
