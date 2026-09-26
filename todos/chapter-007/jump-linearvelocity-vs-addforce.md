# 점프 구현 — linearVelocity 직접 대입 vs AddForce

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> 점프를 구현하는 방법이 위처럼 linearVelocity를 직접 변경하는 방법이 있는데, addForce 함수를 사용하는 방법도 있네.
> 이떄 AddForce 함수를 통해 Vector2.up * 5.0f 힘만큼 특정 모드로 올리는거야? 위 점프와 이 기능의 차이점은 뭐지?

## 질문이 나온 코드

```csharp
// 현재 사용 중인 방식
private void TryToJump()
{
    if (isGrounded && canJump)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}

// 주석 처리된 대안
// rb.AddForce(Vector2.up * 5.0f, ForceMode2D.Impulse);
```

## 1. "Vector2.up * 5.0f 힘만큼 특정 모드로 올리는 것" — 맞다

- `Vector2.up` = `(0, 1)`, 여기에 `5.0f`를 곱하면 `(0, 5)` → **위쪽으로 크기 5**
- 두 번째 인자 `ForceMode2D`가 **그 숫자를 어떻게 해석할지** 정한다

### ForceMode2D 두 가지

| 모드 | 의미 | 단위 | 쓰임 |
| --- | --- | --- | --- |
| `Force` (기본값) | **지속적인 힘**. 물리 스텝의 시간만큼 누적 적용 | N (뉴턴) | 바람, 가속, 로켓 추진 |
| `Impulse` | **순간적인 충격**. 즉시 운동량 변화 | N·s | 점프, 폭발, 타격 |

같은 5라도 결과가 완전히 다르다.

- `Force`: 매 물리 스텝마다 조금씩 속도가 붙는다. **`FixedUpdate`에서 계속 호출해야** 의미가 있다
- `Impulse`: 호출 즉시 속도가 확 바뀐다. **한 번만 호출**하면 된다

점프는 순간적인 동작이므로 `Impulse`가 맞다. 주석의 코드가 올바르게 `Impulse`를 쓰고 있다.

## 2. 핵심 차이 — 질량과 기존 속도를 고려하느냐

### `linearVelocity` 직접 대입

```csharp
rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
```

- **기존 Y속도를 완전히 무시하고 덮어쓴다**
- **질량(Mass)과 무관하다**
- 결과가 항상 똑같다 → **점프 높이가 완벽하게 일정하다**

### `AddForce(..., Impulse)`

```csharp
rb.AddForce(Vector2.up * 5.0f, ForceMode2D.Impulse);
```

- **기존 속도에 더한다** (Δv = 충격량 / 질량)
- **질량에 반비례한다.** Mass가 2배면 같은 힘으로 속도 변화는 절반
- 낙하 중에 쓰면 아래로 향하던 속도를 상쇄하는 데 힘이 먼저 쓰인다

### 구체적인 비교

Mass 1, 낙하 중이라 현재 Y속도가 -8인 상태에서 점프한다면:

| 방식 | 점프 직후 Y속도 |
| --- | --- |
| `linearVelocity.y = 8` | **+8** (항상 동일) |
| `AddForce(up * 8, Impulse)` | -8 + 8 = **0** (거의 안 올라감) |

**이게 가장 중요한 차이다.** 낙하 중 더블 점프를 `AddForce`로 구현하면 "가끔 점프가 안 먹는" 버그가 된다. 그래서 더블 점프도 보통 `linearVelocity`를 덮어쓰는 방식으로 만든다.

### 요약

| | `linearVelocity` 대입 | `AddForce(Impulse)` |
| --- | --- | --- |
| 기존 속도 | **무시하고 덮어씀** | 더해짐 |
| 질량 영향 | 없음 | 있음 (반비례) |
| 점프 높이 | 항상 일정 | 상황/질량에 따라 변함 |
| 제어 | 정밀 | 물리적으로 자연스러움 |
| 적합한 용도 | **플레이어 점프** | 폭발, 넉백, 밀려남 |

**플랫포머 캐릭터의 점프는 "예측 가능함"이 자연스러움보다 중요**하므로 현재 코드처럼 `linearVelocity` 대입이 일반적인 선택이다.

## 3. X속도를 보존하는 이유

```csharp
rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
//                              ^^^^^^^^^^^^^^^^^^^ 기존 X 유지
```

Y만 바꾸고 싶은데 `Vector2`는 값 타입이라 `rb.linearVelocity.y = jumpForce;`가 컴파일되지 않는다. 그래서 X를 읽어서 새 벡터를 통째로 대입한다.

Unity 6에서는 한 줄로 쓸 수 있다.

```csharp
rb.linearVelocityY = jumpForce;   // X는 건드리지 않음
```

X를 보존하지 않으면 **점프하는 순간 좌우 이동이 멈춰서** 제자리 점프만 하게 된다.

## 4. 호출 위치에 대한 주의

현재 `TryToJump()`는 `Update()` → `HandleInput()` 경로로 호출된다.

- **입력 감지(`Input.GetKeyDown`)는 `Update`가 맞다.** `FixedUpdate`에 두면 입력을 놓칠 수 있다
- **`linearVelocity` 대입**은 다음 물리 스텝까지 유지되므로 `Update`에서도 큰 문제가 없다
- 하지만 **`AddForce`를 쓴다면 반드시 `FixedUpdate`로 옮겨야 한다.** `Update`에서 호출하면 프레임레이트가 높을수록 힘이 더 많이 누적되어 **고사양 PC에서 더 높이 점프한다**

## 체크리스트

- [ ] `ForceMode2D.Force`와 `Impulse`의 차이를 설명할 수 있다
- [ ] 점프에 `Impulse`를 쓰는 이유를 안다
- [ ] `AddForce`가 질량에 반비례한다는 걸 안다
- [ ] 낙하 중 `AddForce` 점프가 왜 약해지는지 설명할 수 있다
- [ ] X속도를 보존하지 않으면 어떻게 되는지 안다
- [ ] `AddForce`를 `Update`에서 호출하면 안 되는 이유를 안다
- [ ] 두 방식을 직접 바꿔가며 조작감을 비교해 봤다

## 공식 참고 자료

- [Scripting API: Rigidbody2D.AddForce](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D.AddForce.html)
- [Scripting API: ForceMode2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ForceMode2D.html)
- [Scripting API: Rigidbody2D.linearVelocity](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocity.html)
- [Scripting API: Rigidbody2D.linearVelocityY](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-linearVelocityY.html)
- [Manual: Introduction to Rigidbody 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/rigidbody/introduction-to-rigidbody-2d.html)
- [Scripting API: MonoBehaviour.FixedUpdate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html)
