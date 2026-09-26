# 4. Physics Material 2D — 마찰과 탄성

Unity 6 (6000.6.3f1) / chapter-007 기준

## 핵심 개념

Physics Material 2D는 **씬의 오브젝트가 아니라 프로젝트 에셋 파일**이다. 컴포넌트처럼 오브젝트에 붙이는 게 아니라, 만들어 둔 에셋을 Collider2D나 Rigidbody2D의 `Material` 슬롯에 연결해서 쓴다.

- 하나의 머티리얼 에셋을 여러 콜라이더가 **공유**한다 → 값 하나 바꾸면 전부 반영
- 표면의 "느낌"(얼음처럼 미끄러운지, 고무처럼 튕기는지)을 정의한다

만드는 법: Project 창에서 우클릭 → Create → 2D → Physics Material 2D

## 프로퍼티

| 프로퍼티 | 범위 | 의미 |
| --- | --- | --- |
| `Friction` | 0 ~ 1 | 마찰 계수. 0 = 얼음, 1 = 고무 |
| `Bounciness` | 0 ~ 1 | 반발 계수. 0 = 안 튐, 1 = 에너지 손실 없이 완전 반발 |
| `Friction Combine` | 열거형 | 두 콜라이더의 마찰값을 합치는 방식 (기본 Mean) |
| `Bounce Combine` | 열거형 | 두 콜라이더의 탄성값을 합치는 방식 (기본 Maximum) |

### Combine 모드

충돌은 항상 **두 표면 사이**에서 일어나므로, 서로 다른 값을 가진 두 머티리얼을 하나의 값으로 합쳐야 한다. 선택지는 `Average`, `Mean`, `Multiply`, `Minimum`, `Maximum` 순이다.

Bounce Combine의 기본값이 `Maximum`인 이유: **하나라도 잘 튀는 표면이면 튕긴다**는 직관에 맞기 때문. 그래서 잘 튀는 공 하나만 만들면 어떤 바닥에 떨어뜨려도 튄다.

## 적용 우선순위

머티리얼은 세 군데에서 지정할 수 있고, 아래쪽이 더 우선한다.

1. **Project Settings → Physics 2D → Default Material** (전역 기본값, 현재 프로젝트는 비어 있음)
2. **Rigidbody2D의 Material** (그 바디에 달린 모든 콜라이더에 적용)
3. **Collider2D의 Material** (가장 우선. 개별 콜라이더에만 적용)

아무 데도 지정하지 않으면 friction 0.4, bounciness 0으로 동작한다 → **기본 상태에서는 아무것도 튀지 않는다.**

## chapter-007의 실제 값

`Assets/Bouncy_Material.physicsMaterial2D`

```yaml
m_Name: Bouncy_Material
friction: 0.4
bounciness: 0.5
m_FrictionCombine: 1   # Mean (기본값)
m_BounceCombine: 4     # Maximum (기본값)
```

**주의: 이 머티리얼은 현재 어디에도 연결되어 있지 않다.** `Circle`과 `Square`의 콜라이더·리지드바디 모두 Material 슬롯이 비어 있고, Project Settings의 Default Material도 비어 있다. 만들어만 두고 적용하지 않은 상태다.

### 직접 해 볼 것

1. `Circle`의 CircleCollider2D → Material 슬롯에 `Bouncy_Material`을 드래그
2. 플레이 → 바닥/사각형에 부딪힐 때 튕기는지 확인
3. `bounciness`를 0.9로 올려 보고, 1.0으로 올리면 어떻게 되는지 확인
4. `friction`을 0으로 낮추고 좌우 이동 시 느낌이 어떻게 달라지는지 확인
5. Rigidbody2D 쪽 Material에 연결했을 때와 Collider2D 쪽에 연결했을 때 차이 확인

## 확인 문제

- [ ] Physics Material 2D는 컴포넌트인가 에셋인가? 차이가 왜 중요한가?
- [ ] 튀는 공과 안 튀는 바닥이 부딪히면 튀는가? Bounce Combine 기본값을 근거로 설명해 보기
- [ ] Bounciness를 1.0으로 두면 왜 공이 영원히 튀는가? 그게 실제로 문제가 되는 경우는?
- [ ] Collider2D와 Rigidbody2D 양쪽에 다른 머티리얼을 넣으면 어느 쪽이 이기는가?
- [ ] 이 챕터에서 `Bouncy_Material`이 실제로는 동작하지 않았던 이유는?

## 공식 문서

- [Manual: Physics Material 2D reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/physics-material-2d-reference.html)
- [Scripting API: PhysicsMaterial2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PhysicsMaterial2D.html)
- [Scripting API: PhysicsMaterialCombine2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/PhysicsMaterialCombine2D.html)
- [Scripting API: Collider2D.sharedMaterial](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D-sharedMaterial.html)
