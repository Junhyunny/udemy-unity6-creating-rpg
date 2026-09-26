# transform은 어디서 오는가

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> transform 객체는 어디서 등장하는거야? 그냥 부모 클래스에 있는건가?

## 짧은 답: 맞다. 부모 클래스 `Component`의 프로퍼티다

```csharp
private void Flip()
{
    transform.Rotate(0.0f, 180.0f, 0.0f);   // 선언한 적 없는데 그냥 쓸 수 있다
}
```

`transform`은 **`UnityEngine.Component` 클래스가 가진 public 프로퍼티**다. 상속 계층은 이렇다.

```
UnityEngine.Object
└── UnityEngine.Component        ← transform, gameObject, tag 가 여기 있다
    └── UnityEngine.Behaviour    ← enabled 가 여기 있다
        └── UnityEngine.MonoBehaviour   ← Start, Update 같은 이벤트 함수
            └── Player           ← 내 스크립트
```

`Player`가 `MonoBehaviour`를 상속하므로 `Component`의 멤버를 그대로 물려받는다. 그래서 별도 선언 없이 `transform`을 쓸 수 있다.

## 같이 상속받는 것들

| 멤버 | 정의된 곳 | 의미 |
| --- | --- | --- |
| `transform` | `Component` | 이 컴포넌트가 붙은 게임 오브젝트의 Transform |
| `gameObject` | `Component` | 이 컴포넌트가 붙은 게임 오브젝트 |
| `tag` | `Component` | 게임 오브젝트의 태그 (`gameObject.tag`와 같음) |
| `name` | `Object` | 오브젝트 이름 |
| `enabled` | `Behaviour` | 이 컴포넌트 활성 여부 (`Update` 호출 여부에 영향) |

즉 `transform`, `gameObject.transform`, `GetComponent<Transform>()` **세 가지가 전부 같은 것**을 가리킨다. `transform`은 가장 짧은 표기일 뿐이다.

## 왜 Transform만 특별 취급인가

모든 게임 오브젝트는 **반드시 Transform을 하나 가지며 제거할 수 없다.** 위치·회전·크기와 부모-자식 계층이 없으면 씬에 존재할 수가 없기 때문이다.

항상 존재하는 게 보장되니 Unity가 아예 편의 프로퍼티로 뚫어 놓은 것이다. (2D여도 마찬가지다. `RectTransform`은 UI용 Transform 파생 클래스다.)

## 이 프로젝트에서 transform을 쓰는 곳

```csharp
transform.Rotate(0.0f, 180.0f, 0.0f);                      // 좌우 반전
Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
```

세 곳 모두 **"이 캐릭터가 지금 어디에 있고 어느 방향을 보는가"** 를 묻거나 바꾸는 코드다.

## 주의 ① — Rigidbody2D가 있으면 position 직접 대입을 피한다

`transform.position`에 직접 대입하면 물리 엔진을 무시하고 순간이동시키는 것이라 충돌을 뚫고 지나갈 수 있다. Rigidbody2D가 있는 오브젝트는 `linearVelocity`나 `MovePosition`으로 움직이는 게 맞다.

`transform.Rotate`로 뒤집는 현재 코드는, 회전이 물리 계산에 영향을 주긴 하지만 **좌우 반전 용도로는 흔히 쓰이는 방식**이다. (대안으로 `SpriteRenderer.flipX`나 `localScale.x *= -1`도 쓴다.)

## 주의 ② — 성능 미신

예전에는 "`transform`을 필드에 캐싱해야 빠르다"는 조언이 널리 퍼졌다. 과거 버전에서 `transform` 접근이 `GetComponent` 호출과 같았기 때문이다. **현재 Unity에서는 내부적으로 캐시되어 있어 대부분의 경우 캐싱할 필요가 없다.** 수만 번 반복하는 루프가 아니라면 그냥 쓰면 된다.

## 주의 ③ — 회전 중심은 피봇이다

`transform.Rotate`가 도는 축의 위치는 **스프라이트의 피봇**이다. 피봇이 캐릭터 중앙에서 어긋나 있으면 방향을 바꿀 때마다 캐릭터가 옆으로 튄다. → [sprite-pivot-and-rotation-center.md](./sprite-pivot-and-rotation-center.md)

## 체크리스트

- [ ] `transform`이 `Component`의 프로퍼티라는 걸 안다
- [ ] `MonoBehaviour`의 상속 계층을 설명할 수 있다
- [ ] `transform`, `gameObject.transform`, `GetComponent<Transform>()`이 같다는 걸 안다
- [ ] 모든 게임 오브젝트가 Transform을 갖는 이유를 안다
- [ ] Rigidbody2D가 있을 때 `transform.position` 직접 대입을 피하는 이유를 안다
- [ ] `gameObject`와 `transform`을 구분해서 쓸 수 있다

## 공식 참고 자료

- [Scripting API: Component.transform](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component-transform.html)
- [Scripting API: Component](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.html)
- [Scripting API: MonoBehaviour](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.html)
- [Scripting API: Transform](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Transform.html)
- [Manual: Transform component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-Transform.html)
- [Scripting API: Rigidbody2D.MovePosition](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D.MovePosition.html)
