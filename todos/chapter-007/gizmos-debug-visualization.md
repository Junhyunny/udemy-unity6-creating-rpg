# Gizmos — 에디터 시각화 도구

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Gizmos는 무엇을 의미하는거야? 디버깅용으로만 사용되는거야? 공부할 수 있는 자료 정리해줘.

> 호출 시점에 대한 내용은 [ondrawgizmos-call-timing.md](./ondrawgizmos-call-timing.md)에 따로 정리했다.

## Gizmos란

**Gizmos는 Scene 뷰에만 그려지는 보조 그래픽**이다. 게임 화면에는 나오지 않고 빌드에도 포함되지 않는, **개발자만 보는 시각적 표시**다.

사실 이미 매일 보고 있다. Unity가 기본으로 그려주는 것들이 전부 기즈모다.

- 카메라의 절두체(視錐) 선
- 라이트의 범위 원
- **Collider2D의 초록색 외곽선**
- 이동/회전/스케일 조작 핸들

`Gizmos` 클래스는 **여기에 내 것을 추가하는 API**다.

## 무엇을 그릴 수 있나

```csharp
private void OnDrawGizmos()
{
    Gizmos.color = Color.red;                          // 이후 그리기의 색

    Gizmos.DrawLine(a, b);                             // 선
    Gizmos.DrawRay(origin, direction);                 // 방향 광선
    Gizmos.DrawWireSphere(center, radius);             // 와이어 구
    Gizmos.DrawSphere(center, radius);                 // 채워진 구
    Gizmos.DrawWireCube(center, size);                 // 와이어 정육면체
    Gizmos.DrawCube(center, size);                     // 채워진 정육면체
    Gizmos.DrawIcon(position, "icon.png");             // 아이콘
    Gizmos.matrix = transform.localToWorldMatrix;      // 로컬 좌표계로 전환
}
```

2D에서는 `DrawLine`, `DrawRay`, `DrawWireSphere`(원처럼 보인다), `DrawWireCube`를 주로 쓴다.

## "디버깅용으로만 사용되는가" — 대체로 그렇지만, 더 정확히는 두 가지 용도다

### ① 런타임 상태 디버깅

값으로는 파악하기 어려운 것을 **눈으로** 본다.

```csharp
private void OnDrawGizmos()
{
    Gizmos.color = isGrounded ? Color.green : Color.red;   // 접지 상태를 색으로
    Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
}
```

`Debug.Log`로 `isGrounded`를 찍으면 콘솔이 수천 줄로 도배되지만, **색으로 보면 한눈에 들어온다.**

### ② 레벨 디자인 보조 (디버깅이 아님)

빈 게임 오브젝트는 Scene 뷰에서 보이지 않는다. 적 스폰 지점, 순찰 경로, 트리거 영역 같은 **"보이지 않는 설정값"에 형태를 부여**하면 디자이너가 마우스로 배치할 수 있게 된다.

```csharp
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 2f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.DrawIcon(transform.position, "spawn.png");
    }
}
```

이건 버그를 찾는 용도가 아니라 **에디터 워크플로를 개선하는 용도**다. 그래서 "디버깅 전용"이라고만 하기에는 좁다.

## Gizmos vs Handles vs Debug

| | 어디서 쓰나 | 특징 |
| --- | --- | --- |
| `Gizmos` | `OnDrawGizmos(Selected)` 안에서만 | 가장 간단. 런타임 스크립트에 그대로 작성 |
| `Handles` | 주로 커스텀 에디터 | **드래그로 조작 가능**, 텍스트 표시 가능. `UnityEditor` 네임스페이스 |
| `Debug.DrawLine/DrawRay` | 아무 데서나 (`Update` 등) | 지속 시간 지정 가능. **Scene 뷰에만 표시** |

`Debug.DrawRay(origin, dir, Color.red, 1f)`는 `Update` 안에서 바로 쓸 수 있어서 **한 프레임만 확인하고 싶을 때** 편하다. `Gizmos`는 지속적으로 상태를 보고 싶을 때 쓴다.

## 표시 켜고 끄기

- **Scene 뷰 툴바의 Gizmos 버튼** — 전체 on/off
- **그 옆 드롭다운** — 스크립트별, 컴포넌트별로 개별 on/off
- **Game 뷰에도 Gizmos 버튼이 있다** — 켜면 게임 화면에도 표시된다 (플레이 중 확인용)

## 이 프로젝트의 코드

```csharp
private void OnDrawGizmos()
{
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
}
```

`Gizmos.color`를 지정하지 않아서 **기본 흰색**으로 그려진다. 접지 상태에 따라 색을 바꾸면 훨씬 유용해진다.

```csharp
private void OnDrawGizmos()
{
    Gizmos.color = isGrounded ? Color.green : Color.red;
    Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
}
```

## 주의

- `Gizmos.color`는 **상태가 유지된다.** 한 번 바꾸면 이후 그리기에 계속 적용되므로, 함수 끝에서 되돌리거나 매번 명시하는 게 안전하다.
- `Gizmos` 호출은 **`OnDrawGizmos` / `OnDrawGizmosSelected` 밖에서는 무시된다.**
- `Handles`를 쓰려면 `using UnityEditor;`가 필요한데, 그대로 두면 **빌드가 깨진다.** `#if UNITY_EDITOR`로 감싸야 한다.

## 체크리스트

- [ ] Gizmos가 Scene 뷰 전용 보조 그래픽이라는 걸 안다
- [ ] 콜라이더 초록선, 카메라 절두체도 기즈모라는 걸 안다
- [ ] 디버깅 외에 레벨 디자인 보조 용도가 있다는 걸 안다
- [ ] `Gizmos.color`로 상태를 색으로 표현할 수 있다
- [ ] `Gizmos`와 `Debug.DrawRay`를 상황에 맞게 고를 수 있다
- [ ] Scene/Game 뷰의 Gizmos 토글 위치를 안다
- [ ] 이 프로젝트의 기즈모에 색을 추가해 봤다

## 공식 참고 자료

- [Scripting API: Gizmos](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Gizmos.html)
- [Manual: Gizmos menu](https://docs.unity3d.com/6000.6/Documentation/Manual/GizmosMenu.html)
- [Scripting API: MonoBehaviour.OnDrawGizmos](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmos.html)
- [Scripting API: MonoBehaviour.OnDrawGizmosSelected](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/MonoBehaviour.OnDrawGizmosSelected.html)
- [Scripting API: Debug.DrawRay](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Debug.DrawRay.html)
