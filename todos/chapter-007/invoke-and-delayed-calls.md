# Invoke와 지연 호출

> 원본 TODO (`chapter-007/Assets/Damaged_Example.cs`)
>
> Invoke 함수를 사용한 이유가 뭘까? 특정 시간동안 대기한 후에 실행되는 함수를 이런 식의 콜박 함수로 지정하는거가?
> 이름이 같으면 동작하나? 해당 객체의 함수를 최우선으로 하는가? 다른 객체의 함수도 가능한가? 장점이 뭐야?
> 컴파일러의 장점을 전혀 이용하지 못하잖아.

## 질문이 나온 코드

```csharp
public void TakeDamage()
{
    sr.color = Color.red;
    lastTimeWasDamaged = Time.time;
    // Invoke("TurnWhite", redColorDuration);
    // Invoke(nameof(TurnWhite), redColorDuration);
}

private void TurnWhite() => sr.color = Color.white;
```

"피격 시 빨개졌다가 0.5초 뒤 원래 색으로" — **지연 실행**이 필요한 전형적인 상황이다.

## 질문별 답

### "특정 시간 대기 후 실행되는 함수를 콜백으로 지정하는 것인가" → 맞다

`Invoke(메서드이름, 초)`는 **지정한 시간이 지난 뒤 그 메서드를 한 번 호출**하도록 Unity에 예약한다. 호출 즉시 반환하므로 게임은 멈추지 않는다.

| 함수 | 동작 |
| --- | --- |
| `Invoke("Method", 2f)` | 2초 후 1회 호출 |
| `InvokeRepeating("Method", 1f, 0.5f)` | 1초 후 시작해 0.5초마다 반복 |
| `CancelInvoke()` | 이 스크립트의 예약을 **전부** 취소 |
| `CancelInvoke("Method")` | 그 이름의 예약만 취소 |
| `IsInvoking("Method")` | 예약이 남아 있는지 확인 |

### "이름이 같으면 동작하나" → 문자열이 정확히 일치해야 한다

**메서드 이름 문자열로 리플렉션 조회**를 한다. 대소문자까지 정확히 맞아야 하고, 틀리면 **컴파일 에러가 아니라 런타임에 조용히 아무 일도 안 일어난다.** (버전에 따라 경고만 남는다.)

**오버로드된 메서드나 인자를 받는 메서드는 호출할 수 없다.** `Invoke`가 부를 수 있는 것은 **매개변수가 없는 메서드**뿐이다.

### "해당 객체의 함수를 최우선으로 하는가 / 다른 객체의 함수도 가능한가"

**다른 객체의 함수는 호출할 수 없다.** `Invoke`는 `MonoBehaviour`의 인스턴스 메서드라서 **자기 자신(`this`)의 메서드만** 찾는다. 우선순위 문제가 아니라 **애초에 대상이 자기 자신으로 고정**되어 있다.

```csharp
Invoke("TurnWhite", 0.5f);        // 이 스크립트의 TurnWhite만 찾는다
otherObject.Invoke("Method", 1f); // 문법상 가능하지만 otherObject 자신의 메서드를 찾는 것
```

`private` 메서드도 호출된다. 리플렉션이라 접근 제한자를 무시한다.

### "컴파일러의 장점을 전혀 이용하지 못하잖아" → **정확한 지적이다**

이게 `Invoke`의 가장 큰 단점이다.

| 문제 | 설명 |
| --- | --- |
| 오타를 못 잡는다 | `Invoke("TrunWhite", ...)` — 컴파일 통과, 런타임에 무반응 |
| 이름 변경에 취약 | IDE의 Rename 리팩터링이 문자열을 놓친다 |
| "사용처 찾기"가 안 된다 | `TurnWhite`를 검색해도 `Invoke` 호출이 안 잡혀 **미사용 메서드처럼 보인다** |
| 인자 전달 불가 | 매개변수 있는 메서드는 못 쓴다 |
| 리플렉션 비용 | 직접 호출보다 느리다 (대부분 무시할 수준) |

**`nameof`가 절반의 해결책이다.**

```csharp
Invoke("TurnWhite", redColorDuration);          // ❌ 오타 시 런타임 무반응
Invoke(nameof(TurnWhite), redColorDuration);    // ✅ 컴파일 시점 검증 + Rename 추적
```

`nameof(TurnWhite)`는 컴파일 시점에 `"TurnWhite"` 문자열로 치환되는데, **해당 이름이 존재하지 않으면 컴파일 에러**가 난다. 이름을 바꾸면 IDE가 함께 바꿔 준다. 주석의 두 줄 중 **아래쪽이 더 나은 코드**인 이유다.

다만 `nameof`를 써도 **인자 전달 불가**와 **리플렉션 조회**는 그대로 남는다.

## 그럼 장점은 뭔가

**짧다.** 한 줄이면 끝이고 상태 변수도, 코루틴 관리도 필요 없다.

```csharp
Invoke(nameof(TurnWhite), 0.5f);   // 이게 전부
```

"0.5초 뒤 한 번만 하면 되는 단순한 일"에는 여전히 실용적이다. 하지만 **팀 코드베이스에서는 대안을 쓰는 경우가 많다.**

## 대안 비교

### ① 코루틴 — 가장 일반적

```csharp
public void TakeDamage()
{
    sr.color = Color.red;
    StopAllCoroutines();                      // 연속 피격 시 이전 예약 취소
    StartCoroutine(TurnWhiteAfter(redColorDuration));
}

private IEnumerator TurnWhiteAfter(float delay)
{
    yield return new WaitForSeconds(delay);
    sr.color = Color.white;
}
```

**장점**: 문자열 없음, **인자 전달 가능**, 여러 단계 연출 가능, `StopCoroutine`으로 개별 취소.
**주의**: 게임 오브젝트가 비활성화되면 코루틴이 중단된다.

### ② 시간 비교 — 지금 이 코드가 쓰는 방식

```csharp
private void Update()
{
    currentTimeInGame = Time.time;
    if (currentTimeInGame > lastTimeWasDamaged + redColorDuration)
    {
        if (sr.color != Color.white) TurnWhite();
    }
}

public void TakeDamage()
{
    sr.color = Color.red;
    lastTimeWasDamaged = Time.time;
}
```

**장점**: 문자열도 코루틴도 없고, **연속 피격 시 타이머가 자동으로 갱신**된다. Inspector에서 `lastTimeWasDamaged` 값을 눈으로 볼 수 있어 디버깅이 쉽다.
**단점**: `Update`가 계속 돌아야 하고, 상태 변수가 늘어난다.

`Invoke` 버전을 주석 처리하고 이 방식으로 바꾼 것은 **연속 피격 처리 때문에 합리적인 선택**이다. `Invoke`는 호출할 때마다 예약이 **누적**되므로, 0.1초 간격으로 세 번 맞으면 예약이 세 개 쌓여 첫 예약이 끝나는 순간 흰색이 되어 버린다. (`CancelInvoke`로 막을 수는 있다.)

### ③ Awaitable / async — Unity 6의 최신 방식

```csharp
private async Awaitable TurnWhiteAfter(float delay)
{
    await Awaitable.WaitForSecondsAsync(delay);
    sr.color = Color.white;
}
```

### 정리

| 방식 | 문자열 | 인자 | 취소 | 적합한 상황 |
| --- | --- | --- | --- | --- |
| `Invoke` | ⚠️ (`nameof`로 완화) | ❌ | `CancelInvoke` | 아주 단순한 1회 지연 |
| 코루틴 | ✅ 없음 | ⭕ | `StopCoroutine` | **대부분의 경우** |
| 시간 비교 | ✅ 없음 | - | 자동 갱신 | **반복 갱신되는 상태** |
| `Awaitable` | ✅ 없음 | ⭕ | `CancellationToken` | 최신 코드베이스 |

## 주의: Time.timeScale의 영향

`Invoke`, `WaitForSeconds`, `Time.time`은 **모두 스케일된 시간**을 따른다. `Time.timeScale = 0`(일시정지)이면 진행되지 않는다.

일시정지 중에도 진행시키려면 `Time.unscaledTime`이나 `WaitForSecondsRealtime`을 쓴다.

## 체크리스트

- [ ] `Invoke`가 문자열 리플렉션으로 동작한다는 걸 안다
- [ ] 오타가 컴파일 에러가 아니라 런타임 무반응이 되는 걸 안다
- [ ] 자기 자신의 메서드만 호출 가능하다는 걸 안다
- [ ] 매개변수 있는 메서드는 호출할 수 없다는 걸 안다
- [ ] `nameof`가 무엇을 해결하고 무엇을 못 하는지 구분할 수 있다
- [ ] `Invoke` 예약이 누적된다는 걸 알고 `CancelInvoke`를 쓸 수 있다
- [ ] 코루틴으로 같은 기능을 작성할 수 있다
- [ ] 현재 코드의 시간 비교 방식이 연속 피격에 강한 이유를 안다
- [ ] `Time.timeScale = 0`일 때 어떻게 되는지 안다

## 공식 참고 자료

- [Scripting API: MonoBehaviour.Invoke](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Invoke.html)
- [Scripting API: MonoBehaviour.InvokeRepeating](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.InvokeRepeating.html)
- [Scripting API: MonoBehaviour.CancelInvoke](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.CancelInvoke.html)
- [Scripting API: MonoBehaviour.IsInvoking](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.IsInvoking.html)
- [Manual: Coroutines](https://docs.unity3d.com/6000.6/Documentation/Manual/Coroutines.html)
- [Scripting API: MonoBehaviour.StartCoroutine](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.StartCoroutine.html)
- [Scripting API: WaitForSeconds](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/WaitForSeconds.html)
- [Scripting API: Time.time](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-time.html)
- [Scripting API: Time.timeScale](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-timeScale.html)
- [Microsoft Learn: nameof 식](https://learn.microsoft.com/ko-kr/dotnet/csharp/language-reference/operators/nameof)
