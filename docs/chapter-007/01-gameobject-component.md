# 1. 게임 오브젝트와 컴포넌트

Unity 6 (6000.6.3f1) / chapter-007 기준

## 핵심 개념

Unity의 씬에 존재하는 모든 것은 **게임 오브젝트(GameObject)** 다. 게임 오브젝트 자체는 기능이 없는 **빈 컨테이너**이고, 실제 동작은 거기에 붙는 **컴포넌트(Component)** 가 담당한다.

- 게임 오브젝트 = 이름 + 태그 + 레이어 + 컴포넌트 목록
- 컴포넌트 = 렌더링, 물리, 입력, 사용자 스크립트 같은 개별 기능 단위
- 기능을 추가한다 = 상속이 아니라 **컴포넌트를 조합(composition)** 한다

> 클래스 상속으로 캐릭터를 만드는 방식과 다르다. "구르는 공"은 `RollingBall` 클래스가 아니라
> `Transform` + `SpriteRenderer` + `Rigidbody2D` + `CircleCollider2D` 컴포넌트의 조합으로 만든다.

## Transform — 유일한 필수 컴포넌트

모든 게임 오브젝트는 `Transform`을 하나 가지며 제거할 수 없다. 위치(Position), 회전(Rotation), 크기(Scale)와 함께 **부모-자식 계층(hierarchy)** 을 표현한다. 자식의 Transform 값은 부모 기준의 상대 좌표다.

## chapter-007 씬의 실제 구성

`Assets/Scenes/SampleScene.unity`

| 게임 오브젝트 | 붙어 있는 컴포넌트 |
| --- | --- |
| `Circle` | Transform, SpriteRenderer, Rigidbody2D, CircleCollider2D, **Player (스크립트)** |
| `Square` | Transform, SpriteRenderer, BoxCollider2D, Rigidbody2D |
| `Main Camera` | Transform, Camera, AudioListener |
| `Global Light 2D` | Transform, Light2D (URP 2D) |

`Circle`이 움직이는 이유는 오브젝트가 특별해서가 아니라 `Rigidbody2D`와 `Player` 컴포넌트를 갖고 있기 때문이다. 같은 컴포넌트를 `Square`에 붙이면 `Square`도 똑같이 움직인다. **이게 컴포넌트 지향 설계의 요점이다.**

## MonoBehaviour — 스크립트도 컴포넌트다

`Assets/Player.cs`

```csharp
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;

    void Update()
    {
        rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal"), rb.linearVelocityY);
    }
}
```

- `MonoBehaviour`를 상속해야 게임 오브젝트에 붙일 수 있는 컴포넌트가 된다.
- **파일명과 클래스명이 반드시 같아야** Unity가 스크립트를 컴포넌트로 인식한다.
- `public` 필드(또는 `[SerializeField] private` 필드)는 인스펙터에 노출되어 직렬화된다.

### 다른 컴포넌트를 참조하는 두 가지 방법

1. **인스펙터 드래그 앤 드롭** — 지금 이 챕터 방식. `public Rigidbody2D rb;`에 씬에서 직접 끌어다 연결한다. 씬 파일에 참조가 저장된다(`rb: {fileID: 122208775}`).
2. **코드에서 찾기** — `rb = GetComponent<Rigidbody2D>();`를 `Awake()`나 `Start()`에서 호출한다. 같은 게임 오브젝트에 붙은 컴포넌트를 런타임에 찾는다.

> 인스펙터 연결은 명시적이지만 실수로 비워두면 `NullReferenceException`이 난다.
> `GetComponent`는 자동이지만 매 프레임 호출하면 비용이 든다 → **캐싱해서 쓴다.**

### 자주 쓰는 이벤트 함수 실행 순서

| 함수 | 호출 시점 |
| --- | --- |
| `Awake()` | 오브젝트 생성 직후 1회. 자기 자신 초기화용 |
| `OnEnable()` | 활성화될 때마다 |
| `Start()` | 첫 `Update` 직전 1회. 다른 오브젝트 참조 초기화용 |
| `FixedUpdate()` | 물리 스텝마다 (기본 0.02초 고정) |
| `Update()` | 매 프레임 (가변) |
| `LateUpdate()` | 모든 `Update` 이후. 카메라 추적용 |

## 확인 문제

- [ ] 게임 오브젝트에서 제거할 수 없는 컴포넌트는 무엇이고 왜 그런가?
- [ ] `Circle`을 그대로 두고 `Square`를 좌우로 움직이게 하려면 무엇을 해야 하나?
- [ ] `public Rigidbody2D rb;`를 `[SerializeField] private Rigidbody2D rb;`로 바꾸면 무엇이 달라지고 무엇이 같은가?
- [ ] `Awake()`와 `Start()`를 언제 구분해서 쓰는가?

## 공식 문서

- [Manual: GameObjects](https://docs.unity3d.com/6000.6/Documentation/Manual/GameObjects.html)
- [Manual: Introduction to components](https://docs.unity3d.com/6000.6/Documentation/Manual/Components.html)
- [Manual: Using components](https://docs.unity3d.com/6000.6/Documentation/Manual/UsingComponents.html)
- [Manual: Transform component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-Transform.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
- [Scripting API: MonoBehaviour](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.html)
- [Scripting API: Component](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.html)
- [Scripting API: GameObject.GetComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/GameObject.GetComponent.html)
- [Manual: Sprite Renderer component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/sprite/renderer/sprite-renderer-reference.html)
