# Physics2D와 Raycast

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Physics2D, Raycast 용도는 뭐야? 어떤 방식으로 사용하는거지? 어떤 개념인지 공부할 수 있게 정리해줘.

## 질문이 나온 코드

```csharp
private void HandleCollision()
{
    isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
}
```

## Physics2D란

**`Physics2D`는 2D 물리 엔진 전체를 조작하는 정적(static) 클래스**다. 컴포넌트가 아니라서 오브젝트에 붙이지 않고, `Physics2D.어쩌구` 형태로 바로 호출한다.

두 가지 역할이 있다.

1. **전역 물리 설정** — `Physics2D.gravity`, `Physics2D.queriesStartInColliders` 등
2. **공간 질의(spatial query)** — "저기에 뭐가 있냐?"를 물어보는 함수들

질문의 `Raycast`는 두 번째에 해당한다.

## Raycast란 — "보이지 않는 광선을 쏴서 뭐가 맞는지 물어본다"

**Raycast는 충돌이 아니다.** 물리적으로 부딪히거나 밀어내지 않는다. 특정 지점에서 특정 방향으로 **가상의 선을 쏘고, 그 선에 닿는 콜라이더가 있는지 조회**할 뿐이다.

```
     Player
       ●  ← transform.position (시작점)
       |
       |  ← Vector2.down 방향으로 groundCheckDistance 만큼
       ↓
  ━━━━━━━━━━━  ← Ground 레이어 콜라이더에 닿았다 → isGrounded = true
```

## 파라미터 읽는 법

```csharp
Physics2D.Raycast(origin, direction, distance, layerMask)
```

| 인자 | 이 코드에서 | 의미 |
| --- | --- | --- |
| `origin` | `transform.position` | 광선 시작점 (캐릭터 피봇 위치) |
| `direction` | `Vector2.down` = `(0,-1)` | 방향. **길이는 무시되고 방향만 쓰인다** |
| `distance` | `groundCheckDistance` | 광선 길이. `Awake`에서 1.5f로 설정 |
| `layerMask` | `whatIsGround` | **이 레이어만 검사**. → [layer-and-layermask.md](./layer-and-layermask.md) |

## 반환값 — RaycastHit2D인데 왜 bool에 대입되나

```csharp
private bool isGrounded;
isGrounded = Physics2D.Raycast(...);   // 반환형은 RaycastHit2D인데?
```

`Physics2D.Raycast`는 `bool`이 아니라 **`RaycastHit2D` 구조체**를 반환한다. 그런데 이 구조체에 **bool로의 암시적 변환 연산자**가 정의되어 있어서 그대로 조건문이나 bool 변수에 쓸 수 있다.

> "Implicit operator used to return a true or false result indicating if the result is valid or not."

즉 `isGrounded = Physics2D.Raycast(...)`는 "뭔가 맞았으면 true"라는 뜻이다. **아무것도 안 맞으면 구조체가 반환되지만 collider가 비어 있어서 false로 평가된다.**

### 맞은 대상의 정보가 필요하다면

```csharp
RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
if (hit)
{
    Debug.Log($"밟은 것: {hit.collider.gameObject.name}");
    Debug.Log($"닿은 지점: {hit.point}");       // 정확한 접촉 좌표
    Debug.Log($"표면 법선: {hit.normal}");      // 경사면 판정에 사용
    Debug.Log($"거리: {hit.distance}");
}
```

| 멤버 | 의미 |
| --- | --- |
| `collider` | 맞은 콜라이더 (아무것도 안 맞으면 null) |
| `point` | 광선과 콜라이더가 만난 좌표 |
| `normal` | 그 지점의 표면 수직 벡터. **경사면 각도 계산에 쓴다** |
| `distance` | 시작점부터 접촉점까지 거리 |
| `rigidbody` | 맞은 콜라이더의 Rigidbody2D |
| `transform` | 맞은 오브젝트의 Transform |

## 왜 접지 판정에 Raycast를 쓰나

`OnCollisionEnter2D`로도 바닥 접촉을 알 수는 있다. 하지만 Raycast 방식이 더 많이 쓰인다.

| | 충돌 콜백 | Raycast |
| --- | --- | --- |
| 시점 | 부딪히는 **순간**에 알림 | **원하는 때 언제든** 물어봄 |
| 현재 상태 | 직접 플래그로 관리해야 함 | **매 프레임 즉시 확인 가능** |
| 방향 구분 | 어디에 부딪혔는지 별도 계산 | 아래로만 쏘면 **바닥만** 판정 |
| 벽/천장 오판 | 벽에 닿아도 Enter가 발생 | 아래 방향만 보므로 오판 없음 |

**벽에 붙어 있을 때 점프가 되면 안 되는데, 충돌 콜백만 쓰면 벽 접촉도 "뭔가에 닿음"으로 잡힌다.** Raycast는 아래쪽만 검사하므로 이 문제가 없다.

## 주의할 점

### ① 자기 자신을 맞을 수 있다

이 프로젝트의 `Queries Start In Colliders` 설정이 **켜져 있다**(`m_QueriesStartInColliders: 1`). 광선 시작점이 콜라이더 내부면 그 콜라이더도 결과에 포함된다는 뜻이다.

`transform.position`은 캐릭터의 CapsuleCollider2D **안쪽**이므로, **`whatIsGround` 마스크가 없으면 자기 자신을 맞아서 `isGrounded`가 항상 true**가 된다. 마스크가 이걸 막고 있다.

### ② 시작점 위치를 확인하라

피봇이 캐릭터 중앙에 있으면 광선이 몸통 한가운데서 출발한다. `groundCheckDistance`가 발바닥까지 닿을 만큼 충분히 길어야 한다. 현재 값 1.5f가 그 여유분이다.

기즈모로 이 구간을 그려서 확인하고 있다. → [ondrawgizmos-call-timing.md](./ondrawgizmos-call-timing.md)

### ③ 광선 하나로는 모서리에서 실패한다

발 한쪽만 플랫폼에 걸쳐 있으면 중앙에서 쏜 광선이 허공을 지나 `isGrounded`가 false가 된다. 실무에서는 **왼발/오른발 두 군데에서 쏘거나** `Physics2D.BoxCast`, `OverlapCircle`을 쓴다.

```csharp
// 원형 영역으로 검사하는 대안
isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, 0.2f, whatIsGround);
```

## Physics2D의 다른 질의 함수들

| 함수 | 모양 | 쓰임 |
| --- | --- | --- |
| `Raycast` | 선 | 접지 판정, 시야 확인, 총알 궤적 |
| `RaycastAll` | 선 | 관통하는 모든 대상 |
| `CircleCast` / `BoxCast` | 원/사각형을 쓸어감 | 두께가 있는 이동 경로 검사 |
| `OverlapCircle` / `OverlapBox` | 영역 | 폭발 범위, 공격 판정, 접지 판정 |
| `OverlapPoint` | 점 | 마우스 클릭 위치에 뭐가 있는지 |
| `Linecast` | 두 점 사이 | 두 오브젝트 사이에 장애물이 있는지 |

접지 판정에는 `Raycast`와 `OverlapCircle`이 가장 많이 쓰인다.

## 체크리스트

- [ ] `Physics2D`가 정적 클래스이고 컴포넌트가 아니라는 걸 안다
- [ ] Raycast가 충돌이 아니라 "질의"라는 걸 이해했다
- [ ] 네 개의 인자가 각각 무엇인지 설명할 수 있다
- [ ] `RaycastHit2D`가 bool로 암시 변환된다는 걸 안다
- [ ] `hit.point`, `hit.normal`이 무엇인지 안다
- [ ] 접지 판정에 충돌 콜백 대신 Raycast를 쓰는 이유를 설명할 수 있다
- [ ] LayerMask가 없으면 자기 자신을 맞는다는 걸 이해했다
- [ ] 광선 하나의 한계(모서리)와 대안을 안다

## 공식 참고 자료

- [Scripting API: Physics2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.html)
- [Scripting API: Physics2D.Raycast](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.Raycast.html)
- [Scripting API: RaycastHit2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RaycastHit2D.html)
- [Scripting API: Physics2D.queriesStartInColliders](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D-queriesStartInColliders.html)
- [Scripting API: Physics2D.OverlapCircle](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCircle.html)
- [Scripting API: Physics2D.BoxCast](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.BoxCast.html)
- [Manual: Collider 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/collider-2d-landing.html)
