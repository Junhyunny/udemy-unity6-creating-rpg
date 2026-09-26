# Update와 프레임 속도

> 원본 TODO (`chapter-007/Assets/LifecycleExample.cs`)
>
> Update 함수는 120 FPS에서 몇번 업데이트되나? Update 함수는 프레임 속도에 영향을 받나?

## 짧은 답

- **120 FPS에서는 초당 약 120번 호출된다.**
- **영향을 받는다.** `Update`는 "프레임당 정확히 1회"가 규칙이므로, **호출 횟수 = 프레임레이트**다.

| 프레임레이트 | 초당 Update 호출 | 호출 간격 |
| --- | --- | --- |
| 30 FPS | 30회 | 약 0.033초 |
| 60 FPS | 60회 | 약 0.017초 |
| **120 FPS** | **120회** | 약 0.008초 |
| 240 FPS | 240회 | 약 0.004초 |

"약"이라고 쓴 이유는 프레임 시간이 **매번 조금씩 다르기** 때문이다. 정확히 균등한 간격이 아니라, 그 프레임의 처리 부하에 따라 들쭉날쭉하다.

## 그래서 생기는 문제 — 기기마다 게임 속도가 달라진다

```csharp
void Update()
{
    transform.position += new Vector3(0.1f, 0, 0);   // ❌ 프레임마다 0.1씩 이동
}
```

| 기기 | 1초 동안 이동 거리 |
| --- | --- |
| 30 FPS 노트북 | 3.0 유닛 |
| 120 FPS 데스크탑 | **12.0 유닛** |

**같은 코드인데 좋은 기기에서 4배 빠르다.** 이게 프레임레이트 의존 버그다.

## 해결 — Time.deltaTime을 곱한다

`Time.deltaTime`은 **직전 프레임부터 지금까지 흐른 실제 시간(초)** 이다.

```csharp
void Update()
{
    transform.position += new Vector3(5f * Time.deltaTime, 0, 0);   // ✅ 초당 5유닛
}
```

| 기기 | deltaTime | 프레임당 이동 | 1초 이동 |
| --- | --- | --- | --- |
| 30 FPS | 0.033 | 0.167 | **5.0 유닛** |
| 120 FPS | 0.008 | 0.042 | **5.0 유닛** |

프레임이 몇 번 돌든 **1초에 5유닛**으로 통일된다.

> 기억할 문장: **"프레임당 얼마"가 아니라 "초당 얼마"로 생각하고, `Time.deltaTime`을 곱한다.**

## 이 프로젝트의 코드는 왜 deltaTime이 없나

`LifecycleExample.cs`와 `Player.cs` 모두 `Time.deltaTime`을 곱하지 않는데, **버그가 아니다.**

```csharp
void Update()
{
    rb.linearVelocity = new Vector2(Input.GetAxis("Horizontal"), rb.linearVelocityY);
}
```

`linearVelocity`는 **위치가 아니라 속도**다. 단위가 이미 "초당 유닛"이다.

- `transform.position += x` → **위치를 직접 더함** → deltaTime 필요
- `rb.linearVelocity = x` → **속도를 설정** → 물리 엔진이 알아서 시간 적분 → **deltaTime 불필요**

속도를 설정하면 실제 이동은 물리 엔진이 고정 간격(0.02초)으로 처리하므로 프레임레이트와 무관해진다.

### deltaTime이 필요한 경우 / 불필요한 경우

| 코드 | deltaTime |
| --- | --- |
| `transform.position += dir * speed` | **필요** |
| `transform.Rotate(0, angle, 0)` | **필요** |
| `timer += 1` (초 단위 타이머) | **필요** |
| `rb.linearVelocity = ...` | 불필요 |
| `rb.AddForce(...)` (FixedUpdate에서) | 불필요 |
| `Input.GetKeyDown(...)` | 불필요 |

## 프레임레이트는 무엇이 정하나

Unity는 기본적으로 **가능한 한 빨리** 프레임을 그린다. 여기에 두 가지 제한이 걸린다.

| 설정 | 의미 |
| --- | --- |
| `QualitySettings.vSyncCount` | 모니터 주사율에 맞춤. 1이면 120Hz 모니터에서 120 FPS로 제한 |
| `Application.targetFrameRate` | 직접 상한 지정. `-1`이면 무제한 (vSync가 꺼져 있을 때만 유효) |

```csharp
void Awake()
{
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 60;   // 60으로 고정
}
```

모바일에서는 배터리 절약을 위해 30 또는 60으로 제한하는 경우가 많다. **에디터의 Game 뷰 FPS는 실제 빌드와 다를 수 있다**는 점도 기억해 둘 것.

## 직접 확인하는 법

```csharp
void Update()
{
    Debug.Log($"frame={Time.frameCount}  deltaTime={Time.deltaTime:F4}  FPS≈{1f / Time.deltaTime:F0}");
}
```

Game 뷰 우측 상단의 **Stats** 패널에서도 실시간 FPS를 볼 수 있다.

## Update / FixedUpdate / LateUpdate

| | 호출 주기 | 프레임당 횟수 |
| --- | --- | --- |
| `FixedUpdate` | 고정 0.02초 | **0 ~ N회** |
| `Update` | 매 프레임 | **정확히 1회** |
| `LateUpdate` | 매 프레임, 모든 Update 이후 | **정확히 1회** |

→ [fixedupdate-and-fixed-timestep.md](./fixedupdate-and-fixed-timestep.md), [event-function-execution-order.md](./event-function-execution-order.md)

## 체크리스트

- [ ] 120 FPS에서 Update가 초당 약 120번 호출된다는 걸 안다
- [ ] Update가 프레임당 정확히 1회임을 안다
- [ ] 프레임레이트 의존 버그의 예를 들 수 있다
- [ ] `Time.deltaTime`의 의미를 설명할 수 있다
- [ ] `linearVelocity` 대입에 deltaTime이 필요 없는 이유를 안다
- [ ] 위치 직접 변경과 속도 설정의 차이를 구분할 수 있다
- [ ] vSync와 targetFrameRate로 프레임레이트를 제한할 수 있다
- [ ] `Time.frameCount`와 deltaTime을 찍어서 실제 값을 확인했다

## 공식 참고 자료

- [Scripting API: MonoBehaviour.Update](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Update.html)
- [Scripting API: Time.deltaTime](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-deltaTime.html)
- [Scripting API: Time.frameCount](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-frameCount.html)
- [Scripting API: Application.targetFrameRate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Application-targetFrameRate.html)
- [Scripting API: QualitySettings.vSyncCount](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/QualitySettings-vSyncCount.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
- [Manual: Time settings reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-TimeManager.html)
