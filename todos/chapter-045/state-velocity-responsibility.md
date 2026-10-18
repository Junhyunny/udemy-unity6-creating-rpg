# 상태 클래스가 속도를 직접 계산할까, Player에게 맡길까

> 질문: 강의는 이동 상태에서 `SetVelocity`의 인자를 player 속성으로 꺼내 계산해서 넘긴다. 인자가 모두 player 내부 값인데 굳이 상태에서 꺼내 설정하는 게 맞나? 게임 업계 코드에서는 이게 일반적인가?
>
> 프로젝트 기준: `chapter-045`는 Unity 6000.6.3f1을 사용한다. Unity 6에서는 `Rigidbody2D.velocity` 대신 `linearVelocity`를 쓴다.

## 질문이 나온 코드

`chapter-045/Assets/Scripts/PlayerStates/PlayerMoveState.cs`

```csharp
// 강의 방식 (주석 처리됨): 상태가 값을 꺼내 계산해서 넘긴다
// player.SetVelocity(player.moveInput.x * player.moveSpeed, player.rb.linearVelocity.y);

// 현재 방식: Player에게 "움직여"라고만 말한다
player.Move();
```

`chapter-045/Assets/Scripts/Player.cs`

```csharp
public void Move()
{
    float xVelocity = this.moveInput.x * this.moveSpeed;
    float yVelocity = this.rb.linearVelocity.y;
    this.SetVelocity(xVelocity, yVelocity);
    this.HandleFlip(xVelocity);
}
```

참고: 이번 리팩터링으로 `SetVelocity`는 `Entity`의 `protected` 메서드가 되었다. 그래서 주석으로 남은 강의 방식 코드는 상태 클래스에서 그대로는 컴파일되지 않는다. 지금 구조는 이미 B 방식을 선택한 셈이다.

## 두 방식의 차이

| | A. 상태가 계산 (강의) | B. Player에게 맡김 (현재) |
| --- | --- | --- |
| 상태 코드 | `player.SetVelocity(player.moveInput.x * player.moveSpeed, ...)` | `player.Move()` |
| "어떻게 움직이나" 규칙이 있는 곳 | 각 상태 클래스 | `Player` 한 곳 |
| 상태가 알아야 하는 것 | `moveInput`, `moveSpeed`, `rb` 등 Player 내부 값 | `Move()`라는 이름 하나 |
| `Player`가 공개해야 하는 것 | `SetVelocity`, `rb`, `moveSpeed` 등 | `Move()` |
| 상태마다 다른 움직임을 넣기 | 쉽다 (상태 파일만 고치면 됨) | `Player`에 메서드가 늘어난다 (`Move`, `Aired`, `Dash`, `WallSlide` …) |

질문에서 느낀 불편함은 객체지향에서 말하는 **캡슐화**와 관련이 있다. 데이터(`moveSpeed`, `rb`)와 그 데이터를 다루는 규칙(속도 계산)을 같은 클래스에 두면, 바깥은 내부 값을 몰라도 된다. "값을 꺼내서 대신 계산하지 말고, 그 값을 가진 객체에게 시켜라"는 원칙을 흔히 **Tell, Don't Ask**라고 부른다. ([C# 객체 지향 프로그래밍 — 캡슐화](https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/object-oriented/))

## 그럼 강의 방식은 틀렸나

틀린 것은 아니다. 상태 패턴에서는 **"이 상태에서 캐릭터가 어떻게 행동하는가"를 상태 클래스에 모으는 것**이 목적이기도 하다. 그렇게 보면 이동 상태가 이동 속도를 계산하는 것도 자연스럽다.

- A 방식의 장점: 대시 상태의 속도, 벽 슬라이드의 감속처럼 상태마다 다른 움직임 규칙이 그 상태 파일 안에 모인다. `Player`는 데이터와 공통 도구(`SetVelocity`)만 가진 얇은 클래스가 된다.
- B 방식의 장점: `Player`의 내부 값이 덜 노출되고, 같은 계산이 여러 상태에서 반복되지 않는다. 지금 프로젝트에서 `Aired()`, `Dash()`, `WallJump()`처럼 이미 B 방식으로 모은 메서드가 많다.

결국 **"움직임 규칙을 어디에 모을 것인가"의 선택**이다. 둘 중 무엇을 고르든 한 프로젝트 안에서는 **일관되게** 쓰는 것이 더 중요하다.

## 게임 업계에서는

Unity에서 상태 패턴을 쓰는 코드는 두 방식이 모두 흔하다. 경향을 정리하면 다음과 같다. (이 부분은 특정 회사의 규칙이 아니라 흔히 볼 수 있는 경향이다)

- **작은 프로젝트·튜토리얼**: A 방식이 많다. 상태 파일만 보면 동작이 다 보여서 배우기 쉽다.
- **팀 단위의 큰 프로젝트**: 이동·물리 처리를 별도 컴포넌트(예: `MovementController`)로 분리하고, 상태는 "어느 방향으로 얼마나 빠르게" 같은 **의도만 전달**하는 구성이 많다. B 방식을 한 단계 더 나눈 형태다. 이렇게 하면 플레이어와 적이 같은 이동 컴포넌트를 재사용할 수 있다.

Unity가 공식으로 배포하는 디자인 패턴 전자책에서도 상태 패턴과 단일 책임 원칙(클래스 하나는 한 가지 이유로만 바뀌어야 한다)을 함께 다룬다. ([Unity — Level up your code with design patterns and SOLID](https://unity.com/resources/design-patterns-solid-ebook))

## 이 프로젝트에 적용해 보기

이번 리팩터링으로 `Entity`가 생기면서 B 방식이 더 자연스러워졌다. 적 캐릭터도 `Entity`를 상속하면 `SetVelocity`, `StopMovement`, `WallSlide` 같은 공통 동작을 그대로 쓴다. 지금 코드에서 판단 기준을 세워 보면:

- 여러 엔티티가 같이 쓰는 움직임 → `Entity`의 메서드 (`StopMovement`, `Stop`)
- 플레이어만의 움직임 → `Player`의 메서드 (`Move`, `Dash`, `WallJump`)
- 상태에서만 의미 있는 일회성 계산 → 상태 클래스 안에서 직접 (`PlayerBasicAttackState`의 공격 방향 계산처럼)

## 체크리스트

- [ ] A(상태가 계산)와 B(Player에게 맡김) 방식의 차이를 표 없이 설명할 수 있다.
- [ ] 캡슐화와 Tell, Don't Ask가 무엇을 줄이려는 원칙인지 설명할 수 있다.
- [ ] 상태 패턴에서 A 방식이 자연스러운 이유도 설명할 수 있다.
- [ ] 이 프로젝트의 움직임 메서드를 `Entity` / `Player` / 상태 중 어디에 둘지 기준을 세웠다.

## 공식 참고 자료

- [C# 객체 지향 프로그래밍](https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/object-oriented/) — 캡슐화, 상속, 다형성
- [C# 개체 — 클래스 인스턴스 만들기](https://learn.microsoft.com/ko-kr/dotnet/csharp/fundamentals/object-oriented/objects) — 객체가 데이터와 동작을 함께 가진다는 것
- [Unity — Level up your code with design patterns and SOLID](https://unity.com/resources/design-patterns-solid-ebook) — Unity 공식 디자인 패턴 전자책 (상태 패턴, 단일 책임 원칙)
- [Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html) — Unity 6의 속도 속성 (이전 이름 `velocity`)
- 관련 문서: [상태 머신의 순환 참조와 설계 선택](../chapter-025/state-machine-circular-references.md)
