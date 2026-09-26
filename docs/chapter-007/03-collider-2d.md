# 3. Collider 2D — 충돌 형태와 감지

Unity 6 (6000.6.3f1) / chapter-007 기준

## 핵심 개념

`Collider2D`는 **물리 엔진이 보는 오브젝트의 모양**이다. 눈에 보이는 스프라이트와는 완전히 별개다.

- **SpriteRenderer**: 화면에 보이는 그림 → 물리와 무관
- **Collider2D**: 물리 엔진이 쓰는 충돌 형태 → 화면에 안 보임
- 둘의 크기가 달라도 Unity는 경고하지 않는다. **눈에 보이는 것과 부딪히는 것이 다르면 버그처럼 보인다.**

Scene 뷰에서 초록색 외곽선으로 콜라이더 모양을 확인할 수 있다.

## 종류와 선택 기준

| 콜라이더 | 형태 | 비용 | 용도 |
| --- | --- | --- | --- |
| `BoxCollider2D` | 사각형 | 낮음 | 상자, 바닥, 벽, 플랫폼 |
| `CircleCollider2D` | 원 | **가장 낮음** | 공, 캐릭터 발밑 |
| `CapsuleCollider2D` | 캡슐 | 낮음 | 사람형 캐릭터. 모서리에 안 걸림 |
| `PolygonCollider2D` | 임의 다각형 | 높음 | 복잡한 지형 |
| `EdgeCollider2D` | 선분 | 낮음 | 단면 지형, 경계선 |
| `CompositeCollider2D` | 여러 콜라이더 병합 | - | 타일맵 지형 최적화 |

**원칙: 가장 단순한 모양으로 근사한다.** 캐릭터에 PolygonCollider2D를 쓰면 성능도 나쁘고 모서리에 걸려서 이동이 부자연스러워진다.

## 공통 프로퍼티

| 프로퍼티 | 의미 |
| --- | --- |
| `Is Trigger` | 켜면 물리적으로 막지 않고 통과. 콜백만 발생 |
| `Offset` | 게임 오브젝트 원점 기준 콜라이더 중심 이동 |
| `Material` | 마찰/탄성을 정의하는 Physics Material 2D |
| `Density` | Rigidbody2D의 Use Auto Mass가 켜져 있을 때만 질량 계산에 사용 |
| `Layer Overrides` | 특정 레이어와의 충돌 포함/제외 |

모양별 전용 값: `BoxCollider2D`는 `Size`, `CircleCollider2D`는 `Radius`.

## 충돌(Collision) vs 트리거(Trigger)

| | Collision | Trigger |
| --- | --- | --- |
| 물리적으로 막는가 | 예 | **아니오, 통과** |
| 콜백 | `OnCollisionEnter2D/Stay2D/Exit2D` | `OnTriggerEnter2D/Stay2D/Exit2D` |
| 인자 | `Collision2D` (접촉점, 충격량 포함) | `Collider2D` (상대 콜라이더만) |
| 용도 | 벽, 바닥, 밀어내기 | 아이템 획득, 감지 범위, 구역 진입 |

```csharp
void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log($"부딪힘: {collision.gameObject.name}");
}

void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"들어옴: {other.gameObject.name}");
}
```

### 콜백이 발생하는 조건 — 가장 자주 막히는 부분

**두 오브젝트 중 최소 하나에 Rigidbody2D가 있어야 한다.** Collider2D만 붙은 두 오브젝트는 서로 통과하지도, 콜백을 발생시키지도 않는다(둘 다 static body로 취급되기 때문).

| A | B | 충돌하는가 |
| --- | --- | --- |
| Collider만 | Collider만 | ❌ 아무 일도 없음 |
| Collider만 (static) | Collider + Dynamic RB | ✅ |
| Collider + Dynamic RB | Collider + Dynamic RB | ✅ |
| Collider + Kinematic RB | Collider만 (static) | ❌ 기본값에서는 감지 안 됨 |
| Collider + Kinematic RB | Collider + Dynamic RB | ✅ |

> 충돌이 안 될 때 점검 순서: ① Rigidbody2D가 있는가 → ② Collider가 켜져 있는가 →
> ③ Is Trigger 설정이 의도대로인가 → ④ 레이어 충돌 매트릭스에서 꺼져 있지 않은가 → ⑤ 콜백 함수명 오타

## chapter-007 씬의 실제 값

| | Circle | Square |
| --- | --- | --- |
| 콜라이더 | CircleCollider2D | BoxCollider2D |
| 크기 | Radius 0.5 | Size (1, 1) |
| Offset | (0, 0) | (0, 0) |
| Is Trigger | off | off |
| Material | **없음** | **없음** |
| Density | 1 | 1 |

둘 다 Dynamic Rigidbody2D를 갖고 있으므로 충돌이 정상적으로 발생한다. Material이 비어 있어 `Bouncy_Material`은 아직 적용되지 않은 상태다 → [4번 문서](./04-physics-material-2d.md) 참고.

## 확인 문제

- [ ] SpriteRenderer를 2배로 키우면 콜라이더도 같이 커지는가?
- [ ] Collider2D만 붙은 두 오브젝트는 왜 충돌하지 않는가?
- [ ] 아이템 획득 처리에 Collision과 Trigger 중 무엇을 쓰고 왜인가?
- [ ] `OnCollisionEnter2D`의 인자 `Collision2D`에서 `Collider2D`는 어떻게 얻는가?
- [ ] `Circle`의 Is Trigger를 켜면 어떻게 되는가? (직접 확인해 보기)

## 공식 문서

- [Manual: Collider 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/collider-2d-landing.html)
- [Manual: Box Collider 2D component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/box-collider-2d-reference.html)
- [Manual: Circle Collider 2D component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/circle-collider-2d-reference.html)
- [Manual: Introduction to collision](https://docs.unity3d.com/6000.6/Documentation/Manual/CollidersOverview.html)
- [Scripting API: Collider2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D.html)
- [Scripting API: Collider2D.isTrigger](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D-isTrigger.html)
- [Scripting API: Collision2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collision2D.html)
- [Scripting API: MonoBehaviour.OnCollisionEnter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnCollisionEnter2D.html)
- [Scripting API: MonoBehaviour.OnTriggerEnter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnTriggerEnter2D.html)
