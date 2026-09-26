# OnDrawGizmos는 언제 호출되는가

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> OnDrawGizmos 함수는 언제 호출되는거야? Update 함수처럼 매 프레임마다 호출되는거야?
> 아니면 특정 이벤트가 발생했을 때만 호출되는거야? Gizmos는 디버깅용으로만 사용되는거야?
> 공부할 수 있는 자료 정리해줘. 화면 라이프사이클에 의해 실행되는건가?

> Gizmos 자체의 개념과 사용법은 [gizmos-debug-visualization.md](./gizmos-debug-visualization.md)에 정리했다.

## 짧은 답

**"화면 라이프사이클에 의해 실행되는가" → 맞다.** `OnDrawGizmos`는 **에디터가 Scene 뷰(또는 Gizmos가 켜진 Game 뷰)를 다시 그릴 때마다** 호출된다. Update처럼 프레임마다 호출되지만, **Update와 성격이 다르다.**

| | `Update()` | `OnDrawGizmos()` |
| --- | --- | --- |
| 호출 주체 | 게임 루프 | **에디터의 뷰 렌더링** |
| Play 모드 필요 | 필요 | **불필요. 에디트 모드에서도 호출됨** |
| 빌드에서 | 호출됨 | **호출 안 됨 (코드가 제거됨)** |
| 호출 조건 | 컴포넌트 enabled | Gizmos 표시가 켜져 있을 때 |
| 오브젝트 선택 | 무관 | 무관 (`OnDrawGizmos` 기준) |

## 호출 시점 정리

1. **에디트 모드(Play 안 눌러도)에서도 호출된다.** Scene 뷰를 움직이거나 오브젝트를 드래그하면 다시 그려지면서 호출된다.
2. **Play 모드에서는 매 프레임 호출된다.** Scene 뷰가 계속 갱신되기 때문이다.
3. **Scene 뷰 툴바 또는 Game 뷰의 Gizmos 토글이 꺼져 있으면 호출되지 않는다.**
4. **빌드된 게임에는 아예 포함되지 않는다.** 에디터 전용 함수라 성능 걱정 없이 써도 된다.
5. 이벤트 함수 실행 순서상 **`Update`/`LateUpdate` 이후, 렌더링 단계**에 위치한다.

### 중요: 스크립트가 disabled여도 호출된다

`OnDrawGizmos`는 컴포넌트의 `enabled`를 끄더라도 호출된다. (게임 오브젝트 자체를 비활성화하면 호출되지 않는다.) `Update`와 다른 점이라 헷갈리기 쉽다.

## 형제 함수: OnDrawGizmosSelected

```csharp
private void OnDrawGizmos()          // 항상 그린다
private void OnDrawGizmosSelected()  // 이 오브젝트가 선택됐을 때만 그린다
```

**질문에 있던 "특정 이벤트가 발생했을 때만 호출되는가"에 해당하는 게 `OnDrawGizmosSelected`다.** Hierarchy에서 해당 오브젝트를 선택한 동안에만 호출된다.

씬에 오브젝트가 많을 때 `OnDrawGizmos`로 전부 그리면 화면이 선으로 뒤덮인다. **선택한 것만 보고 싶으면 `OnDrawGizmosSelected`로 바꾸면 된다.**

## 이 프로젝트의 코드

```csharp
private void OnDrawGizmos()
{
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
}
```

접지 판정용 광선(`Physics2D.Raycast`)과 **똑같은 구간에 선을 그어서 눈으로 확인**하는 코드다.

```csharp
isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
```

에디트 모드에서도 호출되므로, **Play를 누르지 않고도 `groundCheckDistance` 값을 인스펙터에서 조절하면서 선 길이가 캐릭터 발밑에 알맞은지 바로 볼 수 있다.** 이게 Gizmos의 핵심 이점이다.

> 참고: `groundCheckDistance`는 `Awake()`에서 `1.5f`로 덮어써진다. 에디트 모드에서는 `Awake`가 호출되지 않으므로, **Play 전후로 기즈모 선 길이가 달라질 수 있다.**

## 주의할 점

- **`OnDrawGizmos` 안에서 게임 상태를 바꾸지 않는다.** 그리기 전용 함수이고 에디트 모드에서도 호출되므로, 여기서 값을 바꾸면 Play도 안 눌렀는데 씬이 변경된다.
- 무거운 계산을 넣으면 에디터가 느려진다. Scene 뷰를 그릴 때마다 실행되기 때문이다.
- `Gizmos.xxx` 호출은 **`OnDrawGizmos` / `OnDrawGizmosSelected` 안에서만** 유효하다. `Update`에서 호출하면 아무것도 그려지지 않는다.

## 체크리스트

- [ ] `OnDrawGizmos`가 에디터의 뷰 렌더링에 의해 호출된다는 걸 안다
- [ ] Play 모드가 아니어도 호출된다는 걸 확인했다
- [ ] 빌드에는 포함되지 않는다는 걸 안다
- [ ] `OnDrawGizmos`와 `OnDrawGizmosSelected`를 구분할 수 있다
- [ ] 컴포넌트가 disabled여도 호출된다는 걸 안다
- [ ] Gizmos 토글을 꺼서 표시를 끌 수 있다
- [ ] 이 코드가 Raycast 구간을 시각화하는 것임을 이해했다

## 공식 참고 자료

- [Scripting API: MonoBehaviour.OnDrawGizmos](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmos.html)
- [Scripting API: MonoBehaviour.OnDrawGizmosSelected](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmosSelected.html)
- [Manual: Order of execution for event functions](https://docs.unity3d.com/6000.6/Documentation/Manual/execution-order.html)
- [Manual: Gizmos menu](https://docs.unity3d.com/6000.6/Documentation/Manual/GizmosMenu.html)
- [Scripting API: Gizmos](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Gizmos.html)
