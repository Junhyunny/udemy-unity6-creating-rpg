# Physics Material 2D와 "머티리얼"이라는 이름

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> RigidBody2D에 Physics2D 머티리얼을 붙혔어. 마찰계수를 0으로 주니깐 벽에서 미끄러지네 어떤 것들이 있어?
> 이 외에도 다른 머티리얼이라는 건 해당 오브젝트에 대한 물리적 특성을 정의하는건가? 어떤 것들을 할 수 있는지 정리해줘.

## 짧은 답

**아니다. Unity에서 "Material"은 서로 완전히 다른 두 가지를 가리킨다.**

| | Physics Material 2D | (렌더링) Material |
| --- | --- | --- |
| 정의하는 것 | **물리적 표면 특성** (마찰, 탄성) | **시각적 표면 특성** (색, 텍스처, 셰이더) |
| 붙는 곳 | Collider2D / Rigidbody2D의 Material 슬롯 | SpriteRenderer / MeshRenderer의 Materials |
| 파일 확장자 | `.physicsMaterial2D` | `.mat` |
| 없으면 | 기본 마찰 0.4 / 탄성 0으로 동작 | 렌더러가 기본 머티리얼 사용 |

이름만 같을 뿐 서로 아무 관계가 없다. "오브젝트의 물리적 특성을 정의한다"는 것은 **Physics Material 2D에만** 해당한다.

## Physics Material 2D가 정의하는 것 — 딱 4개뿐이다

| 프로퍼티 | 범위 | 의미 |
| --- | --- | --- |
| `Friction` | 0 ~ 1 | 마찰 계수. 0 = 얼음, 1 = 고무 |
| `Bounciness` | 0 ~ 1 | 반발 계수. 0 = 안 튐, 1 = 손실 없이 반발 |
| `Friction Combine` | 열거형 | 두 표면의 마찰값 합치는 방식 (기본 Mean) |
| `Bounce Combine` | 열거형 | 두 표면의 탄성값 합치는 방식 (기본 Maximum) |

Combine 선택지는 `Average`, `Mean`, `Multiply`, `Minimum`, `Maximum` 다섯 가지다.

**즉 Physics Material 2D로 할 수 있는 건 "얼마나 미끄러운가"와 "얼마나 튀는가" 두 가지가 전부다.** 질량, 중력, 공기저항 같은 건 Rigidbody2D가, 충돌 모양은 Collider2D가 담당한다.

## "마찰 0인데 벽에서 미끄러진다" — 의도한 동작이다

friction 0은 **접촉면에서 마찰력을 아예 만들지 않겠다**는 뜻이다. 그래서 벽에 밀착한 채로 중력을 받으면 아무것도 붙잡아 주지 않아 그대로 흘러내린다.

이 프로젝트에는 두 머티리얼이 있다.

```
Assets/Bouncy_Material.physicsMaterial2D           friction 0.4, bounciness 0.5
Assets/Materials/Slippy_Material.physicsMaterial2D  friction 0,   bounciness 0
```

`Slippy_Material`은 마찰과 탄성이 둘 다 0이라 **완전히 미끄럽고 전혀 튀지 않는 표면**이다.

### 플랫포머에서 friction을 0으로 두는 흔한 이유

캐릭터에 마찰이 있으면 벽에 붙어서 공중에 멈춰 있거나(벽 끼임), 경사면에서 의도치 않게 감속한다. 이동을 `linearVelocity` 대입으로 직접 제어하는 경우 **마찰은 도움이 되기보다 방해가 되는 경우가 많아서** 0으로 두고, 필요한 감속은 코드로 처리하는 패턴을 자주 쓴다.

### 벽 미끄러짐을 제어하고 싶다면

| 원하는 것 | 방법 |
| --- | --- |
| 벽에서 천천히 흘러내리기 (wall slide) | 벽에 닿았을 때 코드로 `rb.linearVelocityY`를 음수 상한으로 제한 |
| 벽에 완전히 붙기 (wall grab) | 벽 감지 시 `rb.gravityScale = 0` 또는 Body Type을 잠시 Kinematic |
| 바닥에서만 마찰 | 캐릭터는 friction 0, **바닥 콜라이더 쪽에** 마찰 있는 머티리얼 |

마지막 방법이 중요하다. 머티리얼은 **두 표면의 조합**으로 계산되므로, 캐릭터가 friction 0이어도 Friction Combine이 `Maximum`이면 바닥 쪽 값이 이긴다. (기본값은 `Mean`이라 평균이 난다.)

## 적용 우선순위

1. Project Settings → Physics 2D → **Default Material** (전역, 현재 비어 있음)
2. **Rigidbody2D**의 Material (그 바디의 모든 콜라이더에 적용)
3. **Collider2D**의 Material (가장 우선, 개별 콜라이더)

TODO에 "RigidBody2D에 붙혔다"고 적혀 있는데, 이 경우 그 바디에 딸린 콜라이더 전부에 적용된다. 콜라이더별로 다르게 하고 싶으면 Collider2D 쪽에 따로 지정하면 그쪽이 이긴다.

## 그럼 렌더링 Material은 뭘 하나

셰이더 + 그 셰이더의 파라미터(텍스처, 색, 값) 묶음이다. URP 2D에서 스프라이트는 기본적으로 `Sprite-Lit-Default`(2D 조명 받음) 또는 `Sprite-Unlit-Default`(조명 무시)를 쓴다. 피격 시 흰색 플래시, 실루엣, 디졸브 같은 효과가 전부 이 렌더링 머티리얼/셰이더 영역이고, **물리와는 무관하다.**

## 체크리스트

- [ ] Physics Material 2D와 렌더링 Material의 차이를 설명할 수 있다
- [ ] Physics Material 2D로 정의할 수 있는 것이 마찰과 탄성 두 가지뿐임을 안다
- [ ] friction 0일 때 벽에서 미끄러지는 이유를 설명할 수 있다
- [ ] Combine 모드 때문에 내 값만으로 결과가 정해지지 않는다는 걸 이해했다
- [ ] Collider2D / Rigidbody2D / Project Settings 중 어디에 붙는지에 따른 우선순위를 안다
- [ ] wall slide를 구현한다면 머티리얼이 아니라 코드로 해야 하는 이유를 안다

## 공식 참고 자료

- [Manual: Physics Material 2D reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/physics-material-2d-reference.html)
- [Scripting API: PhysicsMaterial2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PhysicsMaterial2D.html)
- [Scripting API: PhysicsMaterialCombine2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PhysicsMaterialCombine2D.html)
- [Scripting API: Collider2D.sharedMaterial](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D-sharedMaterial.html)
- [Manual: Introduction to materials](https://docs.unity3d.com/6000.6/Documentation/Manual/materials-introduction.html) — 렌더링 머티리얼
- [Manual: Material component reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-Material.html)
- [Manual: Shaders in URP](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/shaders-in-universalrp.html)
