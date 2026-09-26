# chapter-007 — 게임 오브젝트와 컴포넌트, 2D 물리

Unity **6000.6.3f1** / URP 2D / `chapter-007` 프로젝트 기준으로 정리한 학습 자료.

## 이 챕터의 한 줄 요약

> 게임 오브젝트는 빈 껍데기이고, **컴포넌트를 조합해서** 동작을 만든다.
> 2D 물리는 **Rigidbody2D(움직임) + Collider2D(모양) + Physics Material 2D(표면 성질)** 세 조각으로 이뤄진다.

## 학습 순서

| | 문서 | 다루는 것 |
| --- | --- | --- |
| 1 | [게임 오브젝트와 컴포넌트](./01-gameobject-component.md) | GameObject / Component 구조, Transform, MonoBehaviour, 컴포넌트 참조, 이벤트 함수 실행 순서 |
| 2 | [Rigidbody 2D](./02-rigidbody-2d.md) | Body Type, Mass, Gravity Scale, Damping, Constraints, Interpolate, Collision Detection |
| 3 | [Collider 2D](./03-collider-2d.md) | Box / Circle Collider, Is Trigger, 충돌 vs 트리거, 콜백이 발생하는 조건 |
| 4 | [Physics Material 2D](./04-physics-material-2d.md) | Friction, Bounciness, Combine 모드, 적용 우선순위 |
| 5 | [Player 스크립트와 이동](./05-player-script-movement.md) | `linearVelocity`(Unity 6 변경), `Input.GetAxis`, Update vs FixedUpdate |

## 챕터 프로젝트의 실제 구성

```
Circle  ─ Transform · SpriteRenderer · Rigidbody2D(Mass 1)          · CircleCollider2D(r 0.5) · Player.cs
Square  ─ Transform · SpriteRenderer · Rigidbody2D(Mass 5, 위치 동결) · BoxCollider2D(1×1)

Assets/Bouncy_Material.physicsMaterial2D  (friction 0.4, bounciness 0.5) ← 아직 어디에도 미적용
프로젝트 중력 (0, -9.81) · Fixed Timestep 0.02 · Active Input Handling: Both
```

## 이해도 체크리스트

기본 개념

- [ ] 게임 오브젝트와 컴포넌트의 관계를 상속이 아닌 조합으로 설명할 수 있다
- [ ] 모든 게임 오브젝트가 Transform을 갖는 이유를 안다
- [ ] 스크립트가 컴포넌트가 되려면 무엇이 필요한지 안다 (`MonoBehaviour` 상속, 파일명 = 클래스명)
- [ ] 인스펙터 연결과 `GetComponent<T>()`의 차이와 각각의 장단점을 안다

2D 물리

- [ ] Dynamic / Kinematic / Static을 상황에 맞게 고를 수 있다
- [ ] Rigidbody2D가 있을 때 `transform.position` 직접 조작이 왜 문제인지 설명할 수 있다
- [ ] SpriteRenderer와 Collider2D가 별개라는 걸 이해하고 있다
- [ ] 충돌 콜백이 발생하려면 최소 하나에 Rigidbody2D가 필요하다는 걸 안다
- [ ] Collision과 Trigger를 용도에 맞게 구분해서 쓸 수 있다
- [ ] Physics Material 2D를 만들고 콜라이더에 적용할 수 있다

스크립팅

- [ ] `velocity` → `linearVelocity` 변경을 알고 있고 `linearVelocityX/Y`를 쓸 수 있다
- [ ] Y축 속도를 보존하는 이유를 설명할 수 있다
- [ ] `Update`와 `FixedUpdate`를 구분해서 쓸 수 있다
- [ ] 속도 대입과 `AddForce`의 차이를 안다

## 직접 해 볼 과제

1. `Bouncy_Material`을 `Circle`의 콜라이더에 적용하고 튀는지 확인한다
2. `Player.cs`에 `public float speed = 5f;`를 추가해 이동 속도를 조절 가능하게 만든다
3. 입력 읽기는 `Update`, 속도 적용은 `FixedUpdate`로 분리한다
4. `Square`에도 `Player.cs`를 붙이고 Rigidbody2D를 연결해 똑같이 움직이는지 확인한다 (컴포넌트 조합의 재사용성)
5. 바닥 역할을 할 Static 오브젝트(Collider2D만 있는 BoxCollider2D)를 만들어 본다
6. `OnCollisionEnter2D`에 `Debug.Log`를 넣고 언제 호출되는지 콘솔로 관찰한다

## 문서 규칙

- Unity 공식 문서 링크는 프로젝트 에디터 버전(6000.6)에 맞춰 작성했다.
- 마크다운은 Unity가 `.meta` 파일을 만들지 않도록 `Assets/` 밖(`docs/`)에 둔다.
