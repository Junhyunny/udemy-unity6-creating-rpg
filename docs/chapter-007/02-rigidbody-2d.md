# 2. Rigidbody 2D — 물리 시뮬레이션의 주체

Unity 6 (6000.6.3f1) / chapter-007 기준

## 핵심 개념

`Rigidbody2D`를 붙이는 순간 그 오브젝트의 **위치와 회전은 Transform이 아니라 물리 엔진이 소유**한다. 중력, 질량, 힘, 관성이 적용되고, 매 물리 스텝(기본 0.02초)마다 계산 결과가 Transform에 반영된다.

> 중요: Rigidbody2D가 있는 오브젝트를 `transform.position`으로 직접 옮기면 물리 엔진과 싸우게 된다.
> 속도(`linearVelocity`)나 힘(`AddForce`)으로 움직여야 한다.

## Body Type — 가장 먼저 정해야 하는 값

| Body Type | 힘/중력 영향 | 직접 이동 | 용도 |
| --- | --- | --- | --- |
| **Dynamic** | 받음 | 속도/힘으로 | 플레이어, 떨어지는 물체. 가장 비쌈 |
| **Kinematic** | 안 받음 | `MovePosition`으로 | 움직이는 발판, 스크립트로 제어하는 장애물 |
| **Static** | 안 받음 | 움직이지 않음 | 바닥, 벽. 가장 쌈 |

Static끼리는 충돌을 감지하지 않는다. **움직이지 않는 바닥에는 Rigidbody2D를 아예 안 붙이는 것도 방법**이다(Collider2D만 있으면 Unity가 내부적으로 static body로 취급).

## 주요 프로퍼티

| 프로퍼티 | 의미 | 기본값 |
| --- | --- | --- |
| `Mass` | 질량(kg). 충돌 시 운동량 계산에 쓰임 | 1 |
| `Gravity Scale` | 프로젝트 중력에 곱하는 배율. 0이면 무중력 | 1 |
| `Linear Damping` | 선형 속도 감쇠(공기저항 느낌) | 0 |
| `Angular Damping` | 회전 속도 감쇠 | 0.05 |
| `Simulated` | 끄면 물리 시뮬레이션에서 완전히 제외 | on |
| `Interpolate` | 물리 스텝 사이 시각적 보간. 플레이어에 Interpolate 권장 | None |
| `Collision Detection` | Discrete / Continuous. 빠른 물체의 관통 방지 | Discrete |
| `Constraints` | 특정 축의 이동/회전 동결 | None |

**Mass에 대한 흔한 오해**: 질량은 낙하 속도에 영향을 주지 않는다(중력 가속도는 질량과 무관). 질량은 **충돌했을 때 누가 누구를 밀어내는가**에 영향을 준다.

프로젝트 중력은 `ProjectSettings/Physics2DSettings.asset`에 `(0, -9.81)`로 설정되어 있고, 물리 스텝은 `TimeManager.asset`의 Fixed Timestep `0.02`(초당 50회)다.

## chapter-007 씬의 실제 값

| | Circle | Square |
| --- | --- | --- |
| Body Type | Dynamic | Dynamic |
| Mass | 1 | **5** |
| Gravity Scale | 1 | 1 |
| Linear Damping | 0 | 0 |
| Angular Damping | 0.05 | **2** |
| Interpolate | None | None |
| Collision Detection | Discrete | Discrete |
| Constraints | None | **Freeze Position X, Y** |

`Square`는 위치가 동결되어 있어 떨어지지도 밀려나지도 않지만 **회전은 가능**하다. 그래서 `Circle`이 부딪히면 `Square`가 제자리에서 돌아간다. Angular Damping 2가 그 회전을 빠르게 감쇠시킨다. 즉 `Square`는 "고정된 회전 장애물" 역할이다.

Constraints는 비트 플래그로 저장된다: `FreezePositionX(1) | FreezePositionY(2) = 3`, `FreezeRotationZ = 4`, 전부 = 7.

## 실험해 보기

1. `Square`의 Constraints를 None으로 바꾸면? → 같이 떨어진다
2. `Circle`의 Gravity Scale을 0으로 바꾸면? → 좌우로만 움직이는 우주 비행
3. `Circle`의 Mass를 100으로 올리고 `Square`의 Constraints를 풀면? → 무거운 공이 사각형을 밀어낸다
4. `Circle`의 Body Type을 Kinematic으로 바꾸면? → 중력도 충돌 반발도 사라진다. 하지만 `linearVelocity` 대입은 여전히 동작한다

## 확인 문제

- [ ] Rigidbody2D가 있는 오브젝트를 `transform.position`으로 옮기면 왜 문제가 되는가?
- [ ] Dynamic / Kinematic / Static을 각각 어떤 오브젝트에 쓰겠는가?
- [ ] Mass를 10배로 늘리면 낙하 속도가 달라지는가?
- [ ] Constraints 값 `3`은 인스펙터에서 어떤 체크박스에 해당하는가?
- [ ] Interpolate는 언제 켜야 하고 왜 모든 오브젝트에 켜지 않는가?

## 공식 문서

- [Manual: Introduction to Rigidbody 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/introduction-to-rigidbody-2d.html)
- [Manual: Rigidbody 2D body types](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/body-types/rigidbody-2d-body-types-landing.html)
- [Manual: Dynamic Body Type reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/body-types/dynamic/dynamic-body-type-reference.html)
- [Manual: Kinematic Body Type reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/body-types/kinematic/kinematic-body-type-reference.html)
- [Manual: Static Body Type reference](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/body-types/static/static-body-type-reference.html)
- [Scripting API: Rigidbody2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D.html)
- [Scripting API: Rigidbody2D.bodyType](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-bodyType.html)
- [Scripting API: Rigidbody2D.gravityScale](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-gravityScale.html)
- [Scripting API: Rigidbody2D.constraints](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-constraints.html)
- [Scripting API: RigidbodyConstraints2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RigidbodyConstraints2D.html)
- [Scripting API: RigidbodyType2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RigidbodyType2D.html)
- [Scripting API: CollisionDetectionMode2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CollisionDetectionMode2D.html)
- [Scripting API: RigidbodyInterpolation2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/RigidbodyInterpolation2D.html)
