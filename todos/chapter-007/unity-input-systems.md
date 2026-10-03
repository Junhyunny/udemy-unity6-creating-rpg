# Unity 6의 두 입력 시스템

> 학습 질문: Unity 6에 입력 시스템이 두 개 있다고 하는데, 각각 무엇이고 언제 쓰는가?
>
> 기준: `chapter-007`의 Unity 6000.6.3f1, Input System 패키지 1.20.0

## 먼저 답

Unity는 입력을 받는 방법을 두 가지 제공한다.

| | 레거시 Input Manager | Input System 패키지 |
| --- | --- | --- |
| 정체 | Unity에 내장된 기존 입력 시스템 | 패키지 `com.unity.inputsystem`이 제공하는 새 입력 시스템 |
| 대표 코드 | `UnityEngine.Input.GetAxisRaw("Horizontal")`, `Input.GetKeyDown(KeyCode.Space)` | `InputAction`, `PlayerInput`, `Keyboard.current` |
| 입력 설정 | `Edit > Project Settings > Input Manager`의 축과 버튼 | Input Actions 에디터의 Action과 Binding, 또는 코드 |
| 접근 방식 | 정해 둔 축 이름이나 키를 코드에서 조회 | 게임의 동작(`Move`, `Jump`)과 실제 장치 입력을 Binding으로 연결 |
| 선택 기준 | 기존 강의·프로젝트의 코드를 이해하고 유지할 때 | 새 입력 기능을 만들거나 여러 장치·키 재설정 등을 다룰 때 |

Unity는 새 프로젝트에 [Input System 패키지를 권장](https://docs.unity3d.com/6000.6/Documentation/Manual/Input.html)한다. [레거시 Input Manager](https://docs.unity3d.com/6000.6/Documentation/Manual/InputLegacy.html)는 현재 사용할 수 있지만 향후 Unity 버전에서 제거될 예정이라고 안내한다.

**`Both`는 세 번째 시스템이 아니다.** 두 시스템을 동시에 활성화하는 프로젝트 설정이다. 기존 `Input.GetKeyDown` 코드가 자동으로 `InputAction` 코드로 바뀌지는 않는다. [설치 및 활성화 문서](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Installation.html)

## 이 저장소에서 확인한 것

- `chapter-007/ProjectSettings/ProjectSettings.asset`의 `activeInputHandler: 2`는 이 프로젝트가 사용하는 **Both** 설정이다. 에디터에서는 `Edit > Project Settings > Player > Other Settings > Active Input Handling`에서 확인한다.
- `chapter-007/Packages/manifest.json`에는 `com.unity.inputsystem: 1.20.0`이 설치되어 있다.
- `chapter-007/Assets/Player.cs`는 `Input.GetAxisRaw("Horizontal")`와 `Input.GetKeyDown(...)`을 사용한다. **현재 플레이어 입력 코드는 레거시 방식**이다.
- `chapter-007/Assets/Settings/InputSystem_Actions.inputactions`라는 새 시스템의 액션 에셋도 있다. 에셋의 존재만으로 `Player.cs`가 새 방식의 입력을 읽는 것은 아니다.

따라서 이 프로젝트에서는 **두 시스템이 켜져 있지만, 현재 살펴보는 `Player.cs`는 레거시 API를 호출**한다.

## 레거시 Input Manager: 축과 키를 읽기

```csharp
private void Update()
{
    float horizontal = Input.GetAxisRaw("Horizontal");

    if (Input.GetKeyDown(KeyCode.Space))
    {
        Debug.Log("공격 버튼을 눌렀다");
    }
}
```

`Horizontal`은 Input Manager에 정의된 **가상 축 이름**이다. `GetAxisRaw`는 평활화하지 않은 값을 읽는다. `GetKeyDown`은 누른 프레임에만 참이므로 매 프레임 실행되는 `Update`에서 확인한다. [Input API](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Input.html), [Input Manager 설정](https://docs.unity3d.com/6000.6/Documentation/Manual/class-InputManager.html)

## Input System 패키지: 장치와 게임 동작을 연결하기

새 시스템에서는 `Move`, `Jump` 같은 **Action**에 키보드·게임패드 등의 **Binding**을 연결할 수 있다. 게임 코드는 Action의 값을 읽거나 `performed` 콜백을 받는다. 예를 들어 `Jump`를 Space와 게임패드 버튼에 함께 연결할 수 있다. [Actions 문서](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Actions.html)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class JumpInputExample : MonoBehaviour
{
    private InputAction jump;

    private void Awake()
    {
        jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        jump.AddBinding("<Gamepad>/buttonSouth");
    }

    private void OnEnable()
    {
        jump.performed += OnJump;
        jump.Enable();
    }

    private void OnDisable()
    {
        jump.performed -= OnJump;
        jump.Disable();
    }

    private void OnDestroy()
    {
        jump.Dispose();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("점프 버튼을 눌렀다");
    }
}
```

위 코드는 두 장치를 한 동작에 묶는 **독립 실행 예제**다. `InputAction`은 `Enable()`을 호출해야 입력을 감시하기 시작하며, 이 예제처럼 코드에서 만든 액션은 사용을 마친 뒤 `Dispose()`한다. [InputAction API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputAction.html)

실제 프로젝트에서는 Input Actions 에디터에서 액션을 정의해 재사용하는 흐름이 권장된다. `PlayerInput` 컴포넌트를 이용하는 방법과 `Keyboard.current`처럼 장치를 직접 읽는 방법도 같은 **Input System 패키지 안의 서로 다른 사용 방식**이다. [워크플로 비교](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Workflows.html)

## 프로젝트 설정을 바꾸면?

| Active Input Handling | 사용할 수 있는 API | 이 프로젝트의 현재 `Player.cs` |
| --- | --- | --- |
| Input Manager (Old) | 레거시 `UnityEngine.Input` | 입력을 읽을 수 있음 |
| Input System Package (New) | 새 `UnityEngine.InputSystem` | 레거시 호출이 동작하지 않으므로 코드 이식 필요 |
| Both | 두 API 모두 | 기존 입력 코드를 유지하며 새 방식도 실험 가능 |

이 설정을 변경하면 **에디터 재시작이 필요**하다. `Both`에서 같은 키를 두 API로 읽으면 같은 사용자 동작을 코드에서 중복 처리할 수 있으므로, 실제 기능 하나에는 입력 경로를 명확히 정한다. [설치 및 활성화 문서](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Installation.html)

## 직접 해 볼 순서

1. `Project Settings > Player > Active Input Handling`에서 `Both`를 확인한다.
2. `Project Settings > Input Manager`에서 `Horizontal` 축의 키 설정을 찾고 `Player.cs`의 `GetAxisRaw("Horizontal")`와 연결해 본다.
3. `Assets/Settings/InputSystem_Actions.inputactions`를 열어 Action, Binding, Control Scheme의 계층을 살펴본다.
4. 별도 테스트 오브젝트에서 위 `JumpInputExample`을 실행한다. Space와 게임패드의 남쪽 버튼을 눌러 같은 로그가 나오는지 확인한다.
5. `Input Manager (Old)`와 `Input System Package (New)` 각각으로 설정을 바꾸면 어떤 코드가 계속 동작하는지 **별도 실험 프로젝트에서** 비교한다.

## 이해도 체크

- [ ] `UnityEngine.Input`이 어느 시스템의 API인지 설명할 수 있다.
- [ ] `InputAction`과 Binding이 각각 무엇을 뜻하는지 설명할 수 있다.
- [ ] `Both`가 두 시스템의 동시 활성화이며 자동 코드 변환은 아니라는 것을 안다.
- [ ] 현재 `Player.cs`가 어느 시스템을 쓰는지 코드로 확인할 수 있다.
- [ ] 새 시스템에서도 Action 사용, `PlayerInput`, 장치 직접 읽기 중 여러 사용 방식이 있음을 안다.

## 공식 참고 자료

- [Unity 6 Manual: Input](https://docs.unity3d.com/6000.6/Documentation/Manual/Input.html)
- [Unity 6 Manual: Legacy Input](https://docs.unity3d.com/6000.6/Documentation/Manual/InputLegacy.html)
- [Unity 6 Scripting API: Input](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Input.html)
- [Input System 1.20: Installation guide](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Installation.html)
- [Input System 1.20: Actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Actions.html)
- [Input System 1.20 API: InputAction](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/api/UnityEngine.InputSystem.InputAction.html)
- [Input System 1.20: Workflows](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/Workflows.html)
