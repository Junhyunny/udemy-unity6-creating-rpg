# Collider 2D의 역할 — 경계를 만들고 겹침을 막는가

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Collider2D 처럼 Collider 가 들어간 것은 해당 객체의 경계를 만들어주고, 물리적으로 충돌이 발생해서
> 겹치지 않게 해주는거네? 틀리거나 보완해서 설명이 필요하면 공부할 내용으로 추가해줘

## 판정: 절반만 맞다

| 이해한 내용 | 판정 |
| --- | --- |
| "객체의 경계를 만들어준다" | ✅ **맞다** |
| "물리적으로 충돌이 발생해서 겹치지 않게 해준다" | ⚠️ **조건부**. Collider만으로는 안 된다 |

## 보완 1 — Collider는 "모양"만 제공하고, 밀어내는 건 Rigidbody2D다

Collider2D의 역할은 **물리 엔진에게 이 오브젝트의 형태를 알려주는 것**까지다. 실제로 겹침을 해소하고 밀어내는 계산은 Rigidbody2D(물리 시뮬레이션)가 한다.

**Collider2D만 붙은 두 오브젝트는 서로 그냥 통과한다.** 둘 다 움직이지 않는 static body로 취급되어 물리 엔진이 아예 충돌 검사를 하지 않기 때문이다.

| A | B | 겹침이 막히는가 |
| --- | --- | --- |
| Collider만 | Collider만 | ❌ 통과 |
| Collider만 (static) | Collider + Dynamic Rigidbody2D | ✅ |
| Collider + Dynamic RB | Collider + Dynamic RB | ✅ |
| Collider + Kinematic RB | Collider만 (static) | ❌ (Use Full Kinematic Contacts 필요) |

정확한 문장: **"Collider2D는 경계를 정의하고, 그 경계가 실제로 겹침을 막는 것은 최소 한쪽에 Rigidbody2D가 있을 때다."**

## 보완 2 — Is Trigger를 켜면 경계는 있지만 막지 않는다

같은 Collider2D라도 `Is Trigger`를 켜면 **통과시키되 알림만 준다.**

| | Is Trigger off | Is Trigger on |
| --- | --- | --- |
| 물리적으로 막는가 | 예 | **아니오** |
| 콜백 | `OnCollisionEnter2D/Stay2D/Exit2D` | `OnTriggerEnter2D/Stay2D/Exit2D` |
| 콜백 인자 | `Collision2D` (접촉점, 충격량 포함) | `Collider2D` (상대 콜라이더만) |
| 용도 | 벽, 바닥, 밀어내기 | 아이템 획득, 감지 범위, 구역 진입 |

즉 **"겹치지 않게 하는 것"은 Collider2D의 본질이 아니라 선택 가능한 동작**이다.

## 보완 3 — Collider는 "질의(query)"의 대상이기도 하다

충돌 처리 말고도 Collider2D는 **공간을 물어보는 대상**으로 쓰인다. 이 프로젝트의 접지 판정이 그 예다.

```csharp
isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
```

여기서는 아무것도 충돌하지 않는다. 광선을 쏴서 **"저 아래 Ground 레이어 콜라이더가 있냐"고 물어보기만** 한다. `OverlapCircle`, `BoxCast`, `OverlapPoint` 등도 같은 부류다.

→ 자세한 내용은 [physics2d-raycast.md](./physics2d-raycast.md)

## 보완 4 — 보이는 모양과 충돌 모양은 별개다

`SpriteRenderer`(눈에 보이는 그림)와 `Collider2D`(물리가 보는 모양)는 완전히 독립적이다. 스프라이트를 2배로 키워도 콜라이더는 그대로고, Unity는 경고하지 않는다. **보이는 것과 부딪히는 것이 어긋나면 버그처럼 보인다.**

Scene 뷰의 초록색 외곽선으로 실제 콜라이더 모양을 확인하는 습관을 들이는 게 좋다.

## 정리된 문장

> Collider2D는 **물리 엔진이 보는 오브젝트의 형태**를 정의한다.
> 그 형태는 ① Rigidbody2D가 있을 때 충돌 반응(겹침 방지)에 쓰이고,
> ② Is Trigger면 통과시키되 진입 알림에 쓰이고,
> ③ Raycast 같은 공간 질의의 대상이 된다.

## 체크리스트

- [ ] Collider2D만 붙은 두 오브젝트가 왜 통과하는지 설명할 수 있다
- [ ] 겹침을 막는 주체가 Collider가 아니라 물리 시뮬레이션(Rigidbody2D)임을 안다
- [ ] Collision과 Trigger를 용도에 맞게 구분해서 쓸 수 있다
- [ ] Collider가 Raycast 같은 질의의 대상이 된다는 걸 안다
- [ ] SpriteRenderer 크기와 Collider 크기가 별개임을 안다

## 공식 참고 자료

- [Manual: Collider 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/collider-2d-landing.html)
- [Manual: Introduction to collision](https://docs.unity3d.com/6000.6/Documentation/Manual/CollidersOverview.html)
- [Manual: Introduction to Rigidbody 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/introduction-to-rigidbody-2d.html)
- [Scripting API: Collider2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D.html)
- [Scripting API: Collider2D.isTrigger](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D-isTrigger.html)
- [Scripting API: MonoBehaviour.OnCollisionEnter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnCollisionEnter2D.html)
- [Scripting API: MonoBehaviour.OnTriggerEnter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnTriggerEnter2D.html)
