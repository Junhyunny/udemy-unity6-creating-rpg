# 5. Player 스크립트 — 입력과 속도 제어

Unity 6 (6000.6.3f1) / chapter-007 기준

## 대상 코드

`chapter-007/Assets/Player.cs`

```csharp
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal"), rb.linearVelocityY);
    }
}
```

세 줄짜리지만 이 안에 챕터의 핵심이 다 들어 있다.

## 1) `linearVelocity` — Unity 6에서 이름이 바뀐 API

**Unity 6부터 `Rigidbody2D.velocity`는 `Rigidbody2D.linearVelocity`로 바뀌었다.** 각속도(`angularVelocity`)와 구분을 명확히 하기 위한 변경이다. 예전 강의나 블로그의 `rb.velocity` 코드를 그대로 쓰면 obsolete 경고가 난다.

| 예전 (~Unity 2022) | Unity 6 |
| --- | --- |
| `rb.velocity` | `rb.linearVelocity` |
| `rb.velocity.x` | `rb.linearVelocityX` |
| `rb.velocity.y` | `rb.linearVelocityY` |

`linearVelocityX` / `linearVelocityY`가 따로 있는 이유: `Vector2`는 구조체(값 타입)라 `rb.linearVelocity.y = 5;`처럼 **한 축만 대입할 수 없다.** 그래서 위 코드도 Y축은 기존 값을 그대로 읽어 새 `Vector2`를 만들어 통째로 대입한다.

```csharp
// X만 바꾸고 싶다 — 아래 세 줄은 같은 의미
rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
rb.linearVelocity = new Vector2(speed, rb.linearVelocityY);
rb.linearVelocityX = speed;   // Unity 6에서 가장 간결
```

**Y를 보존하는 이유**: Y까지 0으로 덮어쓰면 매 프레임 중력이 만든 낙하 속도가 지워져서 공중에 뜬 것처럼 보인다. "좌우는 내가 제어하고, 상하는 물리에 맡긴다"는 2D 플랫포머의 기본 패턴이다.

## 2) 속도 대입 vs 힘 주기

| 방식 | 코드 | 특징 |
| --- | --- | --- |
| 속도 직접 대입 | `rb.linearVelocity = ...` | 즉각 반응, 정밀한 제어. 관성·미끄러짐 없음 |
| 힘 주기 | `rb.AddForce(...)` | 질량과 관성이 반영되어 자연스러움. 제어가 덜 정밀 |

플랫포머 캐릭터는 대개 속도 대입(반응성 우선), 밀려나는 상자·폭발 효과는 힘 주기를 쓴다.

## 3) `Update()` vs `FixedUpdate()`

| | `Update()` | `FixedUpdate()` |
| --- | --- | --- |
| 호출 주기 | 매 프레임 (**가변**, 프레임레이트에 따라 다름) | 고정 (이 프로젝트 0.02초 = 초당 50회) |
| 용도 | 입력 감지, 시각적 처리 | **물리 처리** |

물리 코드를 `Update()`에 두면 프레임레이트에 따라 결과가 달라진다. 현재 코드는 `Update()`에서 `linearVelocity`를 대입하고 있는데, 속도 대입은 다음 물리 스텝까지 유지되므로 눈에 띄는 문제는 잘 안 보인다. 하지만 `AddForce`를 `Update()`에서 쓰면 **고사양 PC에서 캐릭터가 더 빨라지는** 전형적인 버그가 난다.

권장 패턴 — 입력은 `Update`, 적용은 `FixedUpdate`:

```csharp
public class Player : MonoBehaviour
{
    public Rigidbody2D rb;
    public float speed = 5f;

    private float horizontal;

    void Update()                 // 입력은 프레임마다 놓치지 않고 읽는다
    {
        horizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()            // 물리 적용은 고정 주기로
    {
        rb.linearVelocityX = horizontal * speed;
    }
}
```

## 4) `Input.GetAxis("Horizontal")`

구형 **Input Manager**(레거시 입력)의 API다. `Project Settings → Input Manager`에 정의된 축 이름을 문자열로 참조한다. `Horizontal`은 A/D, ←/→, 게임패드 좌우 스틱에 기본 매핑되어 있다.

| 함수 | 반환값 |
| --- | --- |
| `Input.GetAxis("Horizontal")` | -1 ~ 1, **부드럽게 가속·감속** (미끄러지는 느낌) |
| `Input.GetAxisRaw("Horizontal")` | -1, 0, 1만 (즉각 반응, 플랫포머에서 자주 선호) |

현재 코드에는 `speed` 곱셈이 없어서 최대 속도가 초당 1유닛이다. **매우 느리게 느껴진다면 이게 원인이다** — `* speed`를 붙여 보자.

> **참고**: 이 프로젝트에는 새 Input System 패키지(`com.unity.inputsystem` 1.20.0)와
> `Assets/Settings/InputSystem_Actions.inputactions`가 이미 들어 있다.
> 강의는 레거시 `Input.GetAxis`로 진행한다. 이 프로젝트의 Active Input Handling은
> **Both**(`ProjectSettings.asset`의 `activeInputHandler: 2`)로 설정되어 있어서 두 방식이 모두 동작한다.
> 이 값을 "Input System Package (New)"로만 바꾸면 `Input.GetAxis`는 런타임 예외를 낸다.

## 확인 문제

- [ ] `rb.linearVelocity.y = 0;`은 왜 컴파일되지 않는가?
- [ ] Y축 속도를 그대로 유지하는 이유는?
- [ ] `AddForce`를 `Update()`에서 호출하면 어떤 버그가 생기는가?
- [ ] `GetAxis`와 `GetAxisRaw` 중 즉각적인 조작감을 원하면 어느 쪽인가?
- [ ] 이동 속도를 조절 가능하게 만들려면 코드를 어떻게 바꿔야 하는가?
- [ ] `rb`를 인스펙터에서 연결하지 않고 코드로 얻으려면?

## 공식 문서

- [Scripting API: Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html)
- [Scripting API: Rigidbody2D.linearVelocityX](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocityX.html)
- [Scripting API: Rigidbody2D.linearVelocityY](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocityY.html)
- [Scripting API: Rigidbody2D.AddForce](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D.AddForce.html)
- [Scripting API: MonoBehaviour.Update](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Update.html)
- [Scripting API: MonoBehaviour.FixedUpdate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html)
- [Scripting API: Input.GetAxis](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Input.GetAxis.html)
- [Scripting API: Time.fixedDeltaTime](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-fixedDeltaTime.html)
- [Manual: Input Manager](https://docs.unity3d.com/6000.6/Documentation/Manual/class-InputManager.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
- [Input System 패키지 문서](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html)
