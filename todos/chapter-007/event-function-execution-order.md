# 이벤트 함수 실행 순서 (Order of Execution for Event Functions)

> 원본 TODO (`chapter-007/Assets/Example.cs`)
>
> order of execution for every functions. 해당 키워드, 문장에 관련되서 공부할 내용들을 정리해줘.
> 유니티 컴포넌트, 앱, 프레임워크에 관련된 모든 함수들에 대한 라이프사이클이 어떻게 연결되고, 동작하는지 알고 싶어.

## 개념 — Unity는 "이벤트 함수"를 이름으로 찾아 호출한다

`MonoBehaviour`를 상속한 스크립트에 **약속된 이름의 메서드**를 정의해 두면, Unity가 정해진 시점에 알아서 호출한다. 인터페이스 구현도, 등록도 필요 없다. 이름이 곧 계약이다.

그래서 `private void Awake()`처럼 **private이어도 호출된다.** Unity가 리플렉션 기반으로 찾기 때문이다. (오타가 나면 조용히 호출되지 않으므로 주의.)

이 함수들이 호출되는 순서가 **Script Lifecycle**이고, 공식 문서에서는 **"Order of execution for event functions"** 라는 제목으로 전체 순서도를 제공한다.

## 전체 생명주기

### 1단계 — 초기화 (오브젝트당 한 번)

| 함수 | 시점 | 용도 |
| --- | --- | --- |
| `Awake()` | 오브젝트 생성 직후. **비활성 상태여도 호출** | **자기 자신** 초기화, `GetComponent` 캐싱 |
| `OnEnable()` | 활성화될 때마다 (재활성화 시 반복) | 이벤트 구독 |
| `Start()` | **첫 `Update` 직전**, 활성 상태일 때만 | **다른 오브젝트** 참조, 초기화 순서 의존 로직 |

**`Awake`와 `Start`를 나눈 이유가 핵심이다.**

씬의 모든 오브젝트의 `Awake`가 **전부 끝난 뒤에** `Start`가 시작된다. 그래서:

- `Awake`에서는 다른 오브젝트가 아직 초기화 안 됐을 수 있다 → **자기 것만 준비**
- `Start`에서는 모두 `Awake`를 마친 상태가 보장된다 → **남을 참조해도 안전**

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();          // 내 컴포넌트 → Awake
}

private void Start()
{
    gameManager.RegisterPlayer(this);          // 남을 참조 → Start
}
```

`Awake`/`Start` 사이의 오브젝트 간 순서는 보장되지 않는다. 꼭 필요하면 `Project Settings → Script Execution Order`로 조정한다.

### 2단계 — 물리 루프 (고정 주기, 프레임당 0~N회)

```
FixedUpdate()
  → 내부 물리 시뮬레이션 진행
  → OnTriggerXXX2D() / OnCollisionXXX2D()
  → yield WaitForFixedUpdate
```

**프레임당 몇 번 돌지 정해져 있지 않다.** 프레임이 길어지면 여러 번, 짧으면 0번일 수도 있다. → [fixedupdate-and-fixed-timestep.md](./fixedupdate-and-fixed-timestep.md)

**충돌/트리거 콜백은 물리 루프에 속한다.** `Update`가 아니라 `FixedUpdate` 쪽 타이밍에 발생한다.

### 3단계 — 게임 로직 루프 (프레임당 정확히 1회)

| 순서 | 함수 | 용도 |
| --- | --- | --- |
| 1 | `Update()` | 입력 감지, 일반 로직 |
| 2 | (코루틴 `yield return null` 재개) | |
| 3 | `LateUpdate()` | **모든 Update 이후**. 카메라 추적 |

**`LateUpdate`가 따로 있는 이유**: 카메라가 플레이어를 따라갈 때, 플레이어의 `Update`가 아직 안 끝났는데 카메라가 먼저 움직이면 화면이 한 프레임씩 떨린다. `LateUpdate`는 모든 `Update`가 끝난 뒤 호출되므로 이 문제가 없다.

### 4단계 — 렌더링

```
OnPreCull → OnWillRenderObject → OnPreRender → OnRenderObject → OnPostRender
OnDrawGizmos()     ← 에디터에서만
OnGUI()            ← 구형 IMGUI. 프레임당 여러 번 호출될 수 있다
```

`OnDrawGizmos`가 여기 속한다. → [ondrawgizmos-call-timing.md](./ondrawgizmos-call-timing.md)

### 5단계 — 종료

| 함수 | 시점 |
| --- | --- |
| `OnApplicationPause(bool)` | 앱이 백그라운드로 가거나 돌아올 때 (모바일) |
| `OnApplicationFocus(bool)` | 포커스 변화 |
| `OnDisable()` | 비활성화될 때마다. **이벤트 구독 해제 위치** |
| `OnDestroy()` | 파괴 직전 |
| `OnApplicationQuit()` | 종료 직전 |

**`OnEnable`에서 구독했으면 `OnDisable`에서 해제한다.** 이게 짝이 맞지 않으면 파괴된 오브젝트가 계속 이벤트를 받아 예외가 난다.

### 에디터 전용

| 함수 | 시점 |
| --- | --- |
| `Reset()` | 컴포넌트를 붙였을 때 / Inspector에서 Reset |
| `OnValidate()` | Inspector 값이 바뀔 때마다 |

## 한 프레임의 전체 흐름

```
┌─ 프레임 시작
│
├─ [물리] FixedUpdate  →  물리 시뮬레이션  →  충돌/트리거 콜백
│     (필요한 횟수만큼 반복. 0회일 수도, 3회일 수도 있다)
│
├─ [입력] 입력 이벤트 처리
│
├─ [로직] Update
├─         코루틴 재개
├─         LateUpdate
│
├─ [렌더] 카메라 컬링 → 렌더링 → OnDrawGizmos(에디터) → OnGUI
│
└─ 프레임 끝
```

## 이 프로젝트에서 확인하기

`Assets/Example.cs`가 정확히 이걸 눈으로 보려고 만든 스크립트다.

```csharp
public class Example : MonoBehaviour
{
    private void Awake()       { Debug.Log("Awake was called"); }
    void Start()               { Debug.Log("Start was called"); }
    void Update()              { Debug.Log("Update was called"); }
    private void FixedUpdate() { Debug.Log("FixedUpdate was called"); }
}
```

씬의 `Square`와 `Square (1)` 두 오브젝트에 붙어 있다. Play를 누르고 콘솔을 보면:

1. `Awake`가 **두 개 다** 먼저 찍힌다
2. 그다음 `Start`가 두 개 찍힌다
3. 이후 `FixedUpdate`와 `Update`가 섞여서 반복된다 — **비율이 1:1이 아니다**

콘솔의 **Collapse 버튼을 끄고** 보면 순서가 잘 보인다.

### 직접 해 볼 것

- `OnEnable` / `OnDisable` / `OnDestroy`를 추가하고 Play/Stop 시 순서 확인
- `LateUpdate`를 추가해서 `Update` 뒤에 오는지 확인
- 게임 오브젝트를 비활성 상태로 두고 시작 → `Awake`는 호출되는데 `Start`는 안 되는 것 확인
- `Time.frameCount`를 함께 찍어서 프레임 번호별로 정리

## 체크리스트

- [ ] Unity가 이벤트 함수를 이름으로 찾아 호출한다는 걸 안다
- [ ] private 메서드도 호출되는 이유를 안다
- [ ] `Awake`와 `Start`를 나눈 이유를 설명할 수 있다
- [ ] 모든 `Awake`가 끝난 뒤 `Start`가 시작된다는 걸 안다
- [ ] 충돌 콜백이 물리 루프에 속한다는 걸 안다
- [ ] `LateUpdate`가 필요한 상황(카메라 추적)을 설명할 수 있다
- [ ] `OnEnable`/`OnDisable`이 짝이라는 걸 안다
- [ ] Example.cs를 실행해서 실제 순서를 콘솔로 확인했다

## 공식 참고 자료

- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html) — **전체 순서도. 이 문서의 원본**
- [Scripting API: MonoBehaviour](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.html) — 모든 이벤트 함수 목록
- [Scripting API: MonoBehaviour.Awake](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Awake.html)
- [Scripting API: MonoBehaviour.Start](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Start.html)
- [Scripting API: MonoBehaviour.Update](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.Update.html)
- [Scripting API: MonoBehaviour.FixedUpdate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.FixedUpdate.html)
- [Scripting API: MonoBehaviour.LateUpdate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.LateUpdate.html)
- [Scripting API: MonoBehaviour.OnValidate](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnValidate.html)
