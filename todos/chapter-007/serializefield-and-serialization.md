# [SerializeField]와 직렬화(Serialization)

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> [SerializeField] attribute를 붙이면 private 변수도 Inspector에 노출되네. public으로 바꾸지 않고
> Inspector에 노출시키고 싶을 때 사용하면 되겠네? 근데 굳이 private으로 유지할 필요가 있나?
> 그냥 public으로 변경하면 어떤 문제가 발생하는지 알려줘. SerializeField 키워드의 용도는 무엇인지도 알려줘.
> 직렬화라는 이름을 사용한 이유가 뭐인지도 알려줘.

## 질문이 나온 코드

```csharp
[SerializeField] private Rigidbody2D rb;
[SerializeField] private float moveSpeed = 3.5f;
[SerializeField] private float jumpForce = 8.0f;
[SerializeField] private bool facingRight = true;
```

## 1. "Inspector에 노출시키고 싶을 때 쓰면 된다" — 맞다

정확히 그 용도다. Unity의 규칙은 이렇다.

| 선언 | Inspector 노출 | 직렬화 |
| --- | --- | --- |
| `public float speed;` | ⭕ | ⭕ (기본 동작) |
| `private float speed;` | ❌ | ❌ |
| `[SerializeField] private float speed;` | ⭕ | ⭕ |
| `[HideInInspector] public float speed;` | ❌ | ⭕ |
| `[NonSerialized] public float speed;` | ❌ | ❌ |

## 2. "굳이 private으로 유지할 필요가 있나?" — 있다

`public`으로 바꿔도 Inspector 노출이라는 목적은 똑같이 달성된다. 차이는 **Inspector가 아니라 다른 C# 코드에 대해 생긴다.**

### public으로 열었을 때 생기는 문제

**① 캡슐화가 깨진다 — 아무 스크립트나 값을 바꿀 수 있다**

```csharp
// rb가 public이면 외부 어디서든 이게 가능해진다
player.rb = null;           // 다음 프레임에 NullReferenceException
player.moveSpeed = -9999f;  // 캐릭터가 역주행
player.facingRight = false; // 실제 회전 상태와 불일치 → 스프라이트가 뒤집힌 채 고정
```

특히 `facingRight`는 위험하다. 이 값은 `transform`의 실제 회전 상태와 **짝이 맞아야 하는 내부 상태**다. 외부에서 값만 바꾸면 "변수는 왼쪽인데 그림은 오른쪽"인 상태가 되고, `HandleFlip()`이 영영 이상하게 동작한다.

**② 버그 추적 범위가 커진다**

`moveSpeed`가 이상한 값이 됐을 때, private이면 **`Player.cs` 안과 Inspector 두 군데만** 확인하면 된다. public이면 프로젝트의 모든 스크립트가 용의자가 된다.

**③ 의도가 드러나지 않는다**

`public`은 "이건 다른 클래스가 쓰라고 만든 API다"라는 신호다. `[SerializeField] private`은 "이건 내부 구현인데, 값만 에디터에서 조절하겠다"는 신호다. 같은 결과를 내더라도 **읽는 사람에게 주는 정보가 다르다.**

### 외부에서 읽기는 필요하다면

```csharp
[SerializeField] private float moveSpeed = 3.5f;
public float MoveSpeed => moveSpeed;   // 읽기만 허용
```

이 프로젝트에도 같은 패턴이 있다. `canMove`/`canJump`는 private이고, 외부(`PlayerAnimationEvents`)에는 메서드만 열어 뒀다.

```csharp
public void EnableMovementAndJump(bool enable)
{
    canMove = enable;
    canJump = enable;
}
```

**값을 직접 열지 않고 "무엇을 할 수 있는지"만 열었다.** 이게 캡슐화의 실제 이득이다.

## 3. SerializeField의 진짜 용도 — Inspector는 결과일 뿐이다

`[SerializeField]`의 의미는 "Inspector에 보여줘"가 아니라 **"이 필드를 Unity가 저장/복원 대상으로 삼아라"** 다. Inspector 노출은 그 부수 효과다.

직렬화된 필드는 다음 상황에서 **값이 유지된다.**

- 씬(`.unity`)이나 프리팹 파일에 값이 저장됨
- 에디터를 껐다 켜도 Inspector에서 조정한 값이 남아 있음
- **스크립트를 수정해서 도메인 리로드가 일어나도 값이 살아남음**
- Play 모드 진입/종료 시 상태 복원

`private`이고 `[SerializeField]`도 없는 필드는 저장되지 않으므로 **에디터를 다시 열면 초기값으로 돌아간다.**

실제로 씬 파일에 이렇게 저장돼 있다.

```yaml
MonoBehaviour:
  m_Script: {fileID: 11500000, guid: 0232741ae0db84138896fcc8665c841e, type: 3}
  rb: {fileID: 122208775}
```

### 직렬화되는 타입의 제약

아무 타입이나 되는 게 아니다.

| 직렬화됨 | 직렬화 안 됨 |
| --- | --- |
| `int`, `float`, `bool`, `string`, 열거형 | `Dictionary<,>` |
| `Vector2/3`, `Color`, `Quaternion` 등 Unity 기본 구조체 | 인터페이스 타입 필드 |
| `UnityEngine.Object` 파생 (`Rigidbody2D`, `GameObject`, `Transform`…) | `object`, 제네릭 타입 파라미터 |
| 위 타입의 배열과 `List<T>` | `static`, `readonly`, `const` |
| `[Serializable]`이 붙은 일반 클래스/구조체 | 프로퍼티 (`public float X { get; set; }`) |

`private static readonly int IsMovingHash`가 Inspector에 안 보이는 이유가 이것이다. `static`이고 `readonly`라서 애초에 직렬화 대상이 아니다.

## 4. "직렬화"라는 이름을 쓰는 이유

**Serialization = 메모리 위의 객체 그래프를 순차적인(serial) 바이트/텍스트 흐름으로 펼치는 것.**

메모리 속의 객체는 여기저기 흩어진 주소를 서로 참조하는 그물망 구조다. 이걸 파일에 저장하려면 **"한 줄로 늘어선 순서 있는 데이터"로 바꿔야** 한다. 이 변환이 직렬화(serialize), 반대가 역직렬화(deserialize)다.

`serial`은 "연속된, 차례차례의"라는 뜻이다. 메모리의 입체적 구조를 파일이라는 **1차원 나열로 편다**는 이미지에서 나온 이름이다.

Unity가 직렬화를 쓰는 곳은 Inspector 말고도 많다.

- 씬, 프리팹, ScriptableObject를 `.unity`/`.prefab`/`.asset` 파일로 저장
- Inspector가 값을 읽고 쓰는 통로
- Undo/Redo 스냅샷
- 스크립트 컴파일 후 도메인 리로드 시 상태 복원
- 인스턴스화(Instantiate) 시 값 복사

즉 **Unity 에디터가 "값을 기억하는" 모든 동작의 기반**이 직렬화다.

## 곁들여 알아둘 어트리뷰트

```csharp
[Header("Collision details")]          // Inspector에 구분 제목
[SerializeField] private float groundCheckDistance;

[Range(0f, 20f)]                       // 슬라이더로 표시
[SerializeField] private float moveSpeed = 3.5f;

[Tooltip("접지 판정 광선 길이")]        // 마우스 올리면 설명
[SerializeField] private float distance;
```

이 프로젝트는 이미 `[Header("Collision details")]`를 쓰고 있다.

## 체크리스트

- [ ] `[SerializeField] private`과 `public`이 Inspector 관점에서는 같고 코드 관점에서는 다르다는 걸 안다
- [ ] `facingRight`를 public으로 열면 어떤 버그가 가능해지는지 설명할 수 있다
- [ ] 직렬화의 본래 의미가 "저장/복원"이고 Inspector 노출은 부수 효과임을 안다
- [ ] 스크립트 수정 후에도 Inspector 값이 남아 있는 이유를 설명할 수 있다
- [ ] `Dictionary`나 프로퍼티가 직렬화되지 않는다는 걸 안다
- [ ] `static readonly` 필드가 Inspector에 안 보이는 이유를 안다
- [ ] 값을 외부에 읽기 전용으로 노출하는 방법을 안다

## 공식 참고 자료

- [Scripting API: SerializeField](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SerializeField.html)
- [Manual: Script serialization](https://docs.unity3d.com/6000.6/Documentation/Manual/script-serialization.html) — 직렬화 규칙과 지원 타입
- [Scripting API: HeaderAttribute](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/HeaderAttribute.html)
- [Microsoft Learn: 액세스 한정자 (C#)](https://learn.microsoft.com/ko-kr/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
- [Microsoft Learn: 직렬화 (C#)](https://learn.microsoft.com/ko-kr/dotnet/csharp/programming-guide/concepts/serialization/)
