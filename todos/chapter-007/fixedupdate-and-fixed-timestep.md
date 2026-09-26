# FixedUpdate와 고정 시간 간격

> 원본 TODO (`chapter-007/Assets/Example.cs`)
>
> 해당 함수의 역할은 뭐야? 특정 고정된 시간에 업데이트를 처리하는건가?

## 짧은 답: 맞다

`FixedUpdate()`는 **프레임레이트와 무관하게 고정된 시간 간격으로 호출되는 물리 전용 업데이트**다.

이 프로젝트의 간격은 **0.02초**다 (`ProjectSettings/TimeManager.asset`의 `Fixed Timestep: 0.02`). 즉 **초당 50회**가 목표 호출 횟수다.

## Update와의 차이

| | `Update()` | `FixedUpdate()` |
| --- | --- | --- |
| 호출 주기 | **매 프레임 (가변)** | **고정 0.02초** |
| 프레임레이트 영향 | 직접적 | 없음 |
| 프레임당 호출 횟수 | 정확히 1회 | **0회 ~ 여러 회** |
| 경과 시간 | `Time.deltaTime` (매번 다름) | `Time.fixedDeltaTime` (항상 0.02) |
| 용도 | 입력 감지, 시각 처리 | **물리 조작** |

## "프레임당 0회 ~ 여러 회"가 핵심이다

Unity는 프레임을 시작할 때 **"지난 프레임 이후 흐른 시간만큼 물리를 따라잡는다."**

| 프레임레이트 | 프레임 간격 | 프레임당 FixedUpdate 호출 |
| --- | --- | --- |
| 200 FPS | 0.005초 | 대부분 **0회**, 가끔 1회 |
| 50 FPS | 0.02초 | 1회 |
| 25 FPS | 0.04초 | **2회** |
| 10 FPS | 0.1초 | **5회** |

**어느 경우든 "초당 50회"라는 총량은 유지된다.** 이것이 FixedUpdate의 존재 이유다.

프레임레이트가 아무리 달라도 물리 계산은 같은 속도로 진행되므로, **60Hz 노트북과 240Hz 게이밍 PC에서 캐릭터가 똑같이 움직인다.**

## 왜 물리는 고정 간격이어야 하는가

물리 시뮬레이션은 **이전 상태에서 조금씩 나아가는 수치 적분**이다. 간격이 들쭉날쭉하면 오차가 누적되어 결과가 불안정해진다. 특히 `AddForce`처럼 누적되는 연산은 호출 횟수가 곧 결과가 된다.

```csharp
// ❌ Update에서 힘 주기
void Update() { rb.AddForce(Vector2.right * 10f); }
// 240 FPS에서는 초당 240번, 60 FPS에서는 초당 60번 → 고사양 PC에서 4배 빠르다

// ✅ FixedUpdate에서 힘 주기
void FixedUpdate() { rb.AddForce(Vector2.right * 10f); }
// 어느 기기에서든 초당 50번 → 동일하게 움직인다
```

## 무엇을 어디에 둘 것인가

| 코드 | 위치 | 이유 |
| --- | --- | --- |
| `Input.GetKeyDown` / `GetAxisRaw` | **Update** | FixedUpdate에 두면 입력을 **놓칠 수 있다** |
| `rb.AddForce` / `AddTorque` | **FixedUpdate** | 누적되므로 반드시 고정 주기 |
| `rb.linearVelocity = ...` | 둘 다 가능 (FixedUpdate 권장) | 대입은 다음 스텝까지 유지됨 |
| `rb.MovePosition` | **FixedUpdate** | 물리 보간과 맞물림 |
| `transform.position = ...` | Update | 물리 밖 이동 |
| 충돌/트리거 콜백 | (자동) | 물리 루프에서 발생 |

**`Input.GetKeyDown`을 FixedUpdate에 두면 안 되는 이유**: 이 함수는 "이번 **프레임**에 눌렸는가"를 반환한다. 어떤 프레임에는 FixedUpdate가 0번 호출되므로, 하필 그 프레임에 누른 키는 영영 감지되지 않는다.

### 권장 패턴

```csharp
private float xInput;

void Update()        // 입력은 프레임마다 빠짐없이 읽고
{
    xInput = Input.GetAxisRaw("Horizontal");
    if (Input.GetKeyDown(KeyCode.UpArrow)) jumpRequested = true;
}

void FixedUpdate()   // 물리 적용은 고정 주기로
{
    rb.linearVelocityX = xInput * moveSpeed;
    if (jumpRequested) { rb.linearVelocityY = jumpForce; jumpRequested = false; }
}
```

## 이 프로젝트의 현재 상태

`Player.cs`는 물리 조작을 전부 `Update()`에서 하고 있다.

```csharp
private void Update()
{
    xInput = Input.GetAxisRaw("Horizontal");
    HandleMovement();    // rb.linearVelocity 대입
    ...
}
```

`linearVelocity` **대입**은 다음 물리 스텝까지 값이 유지되므로 눈에 띄는 문제는 잘 드러나지 않는다. 다만 주석 처리된 `AddForce` 코드를 되살려 `Update`에 두면 **프레임레이트에 따라 점프 높이가 달라진다.**

## Time 관련 값들

| 값 | 의미 |
| --- | --- |
| `Time.deltaTime` | 지난 프레임부터 흐른 시간 (Update용, 매번 다름) |
| `Time.fixedDeltaTime` | 물리 스텝 간격 (**항상 0.02**) |
| `Time.timeScale` | 시간 배속. 0이면 일시정지, 0.5면 슬로모션 |
| `Time.time` | 게임 시작 후 누적 시간 |

**`Time.timeScale`을 바꾸면 FixedUpdate 호출 빈도도 함께 변한다.** timeScale이 0이면 FixedUpdate는 아예 호출되지 않는다. (일시정지 구현에 쓰인다.)

`FixedUpdate` 안에서 시간을 곱할 때는 `Time.deltaTime`이 아니라 `Time.fixedDeltaTime`을 쓴다. (Unity는 FixedUpdate 안에서 `Time.deltaTime`이 `fixedDeltaTime`을 반환하도록 해 두긴 했지만, 의도를 분명히 하려면 명시하는 게 좋다.)

## Fixed Timestep을 바꿔도 되나

`Project Settings → Time → Fixed Timestep`에서 조정할 수 있다.

- **줄이면** (예: 0.01 = 100Hz) 물리가 정밀해지지만 **CPU 부담 증가**
- **늘리면** (예: 0.04 = 25Hz) 가벼워지지만 빠른 물체가 벽을 뚫는다

기본값 0.02는 대부분의 게임에 적절하다. 함부로 바꾸기보다 `Collision Detection`을 `Continuous`로 바꾸는 게 먼저다.

## 체크리스트

- [ ] FixedUpdate가 고정 간격(이 프로젝트 0.02초)으로 호출된다는 걸 안다
- [ ] 프레임당 호출 횟수가 0~N회로 변동한다는 걸 안다
- [ ] 물리를 고정 간격으로 돌려야 하는 이유를 설명할 수 있다
- [ ] `AddForce`를 Update에 두면 어떤 버그가 나는지 안다
- [ ] `Input.GetKeyDown`을 FixedUpdate에 두면 안 되는 이유를 안다
- [ ] `Time.deltaTime`과 `Time.fixedDeltaTime`을 구분할 수 있다
- [ ] Example.cs를 실행해서 Update와 FixedUpdate 호출 비율이 1:1이 아님을 확인했다

## 공식 참고 자료

- [Scripting API: MonoBehaviour.FixedUpdate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html)
- [Scripting API: Time.fixedDeltaTime](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-fixedDeltaTime.html)
- [Scripting API: Time.deltaTime](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-deltaTime.html)
- [Scripting API: Time.timeScale](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Time-timeScale.html)
- [Manual: Time settings reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-TimeManager.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
