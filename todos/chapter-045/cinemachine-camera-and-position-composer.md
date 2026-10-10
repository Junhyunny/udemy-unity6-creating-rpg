# Cinemachine 카메라 설치와 2D 타겟 추적·화면 구도

> 질문: Cinemachine 카메라는 일반 카메라와 어떻게 연결되는가? 패키지를 설치하고 플레이어를 추적하게 하려면? `Screen Position`, `Dead Zone`, `Hard Limits`, `Damping`은 각각 무엇을 조절하는가?
>
> 프로젝트 기준: `chapter-045`는 Unity 6000.6.3f1을 사용하며, `Packages/manifest.json`에 `com.unity.cinemachine: 6.6.0`이 이미 들어 있다. 아래 용어와 메뉴는 이 버전의 공식 문서를 기준으로 한다.

## 카메라 역할 구분

| 요소 | 역할 |
| --- | --- |
| Unity `Camera` | 실제로 화면을 렌더링하는 컴포넌트. 보통 `Main Camera`에 있다. |
| `Cinemachine Brain` | Unity Camera에 붙어 활성 Cinemachine Camera를 선택하고 전환·블렌딩 결과를 적용한다. |
| `Cinemachine Camera` | 원하는 위치, 렌즈, 추적 동작을 계산한다. 자체적으로 화면을 렌더링하는 `Camera` 컴포넌트는 없다. |
| `Position Composer` | `Tracking Target`을 화면의 원하는 위치에 두도록 **카메라 위치**를 조절한다. 카메라 회전은 조절하지 않는다. 2D 직교 카메라에 적합하다. |

한 장면에 여러 Cinemachine Camera를 둘 수 있지만 화면은 Unity Camera가 렌더링한다. 먼저 [핵심 요소 설명](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/concept-essential-elements.html)과 [2D 카메라 설명](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/Cinemachine2D.html)을 읽는다.

## 패키지 설치와 확인

이 chapter-045 프로젝트에는 이미 설치되어 있으므로 **다시 설치할 필요가 없다.** 새 프로젝트에서 사용할 때는 다음 순서로 확인한다.

1. Unity 메뉴 **Window > Package Management > Package Manager**를 연다.
2. **In Project**에서 `Cinemachine`을 검색해 설치 여부와 버전을 확인한다.
3. 없다면 **Unity Registry**에서 `Cinemachine`을 찾아 **Install**을 누른다. 검색에 나타나지 않으면 `+` 메뉴의 **Install package by name**에 `com.unity.cinemachine`을 입력할 수 있다. Unity 버전에 맞는 버전을 선택한다.
4. 설치 후 **GameObject > Cinemachine** 메뉴가 나타나는지 확인한다.

[Unity Package Manager 설치 안내](https://docs.unity3d.com/6000.6/Documentation/Manual/upm-ui-install.html) · [이름으로 패키지 설치](https://docs.unity3d.com/6000.6/Documentation/Manual/upm-ui-quick.html) · [Cinemachine 설치 안내](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/InstallationAndUpgrade.html)

## 플레이어 추적 설정

1. **GameObject > Cinemachine > Cinemachine Camera**를 만든다. Unity Camera(`Main Camera`)에 `Cinemachine Brain`이 붙었는지 확인한다. 첫 Cinemachine Camera를 만들 때 자동으로 추가될 수 있다.
2. Cinemachine Camera 컴포넌트의 **Tracking Target**에 플레이어의 Transform을 지정한다. `Look At Target`은 별도의 회전 제어에 쓰는 대상이므로, 위치를 따라가는 2D 예제에는 필수가 아니다.
3. **Position Control**을 **Position Composer**로 선택한다. 2D에서는 Unity Camera의 투영을 **Orthographic**으로 두고, 필요에 따라 Cinemachine Camera의 **Lens > Orthographic Size**로 보이는 높이를 조절한다.
4. 플레이어를 움직여 Game 뷰와 Scene 뷰에서 실제 Unity Camera가 어떻게 움직이는지 비교한다. 프레이밍 가이드를 켜면 구도 영역을 확인하기 쉽다.

현재 `chapter-045/Assets/Scenes/SampleScene.unity`에도 `Main Camera`의 Brain, 플레이어를 가리키는 `Tracking Target`, `Position Composer`가 설정되어 있다. [기본 환경 설정](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/setup-cinemachine-environment.html) · [Tracking Target 설정](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/setup-procedural-behavior.html) · [Cinemachine Camera 속성](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachineCamera.html)

## Position Composer의 네 가지 설정

| 설정 | 의미 | 움직임에서 확인할 점 |
| --- | --- | --- |
| `Screen Position` | 타겟을 두고 싶은 화면상 기준점. `(0, 0)`은 화면 중앙, 각 축의 `-0.5`와 `0.5`는 양쪽 화면 끝이다. 월드 좌표가 아니다. | Y를 음수로 옮기면 플레이어가 화면 중앙보다 아래에 놓이는지 본다. |
| `Dead Zone` | `Screen Position`을 중심으로 카메라가 **반응하지 않는** 영역. `Size`는 화면 너비·높이에 대한 비율이다. | 플레이어가 영역 안에서 조금 움직일 때 카메라가 가만히 있는지 본다. |
| `Hard Limits` | 타겟이 화면에서 벗어나면 안 되는 **허용 영역**. `Size`와 `Offset`으로 영역을 정한다. | 플레이어가 빠르게 움직여도 화면상의 허용 영역 밖으로 나가지 않는지 본다. |
| `Damping` | 카메라가 원하는 위치로 이동할 때의 반응 속도. X·Y·Z 축별로 조절하며 작은 값은 빠르고 큰 값은 느리다. | Dead Zone 밖에서 카메라가 즉시 따라오는지, 부드럽게 지연되는지 비교한다. |

`Hard Limits`는 **화면 속 타겟의 위치 제한**이다. 카메라가 월드의 맵 경계를 넘어가지 못하게 하는 설정과 혼동하지 않는다. 맵 경계 제한은 별도의 [Cinemachine Confiner 2D](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachineConfiner2D.html)를 공부한다. 네 설정의 정확한 정의는 [Position Composer 공식 문서](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachinePositionComposer.html)에 있다.

`Screen Position`의 화면 끝 값은 공식 문서의 **속성 표와 API에서 `±0.5`**로 설명한다. 같은 문서 하단의 Shot composition 요약에는 `±1`이라고 적혀 있어 표기가 서로 다르다. 이 문서는 [ScreenComposerSettings API](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/api/Unity.Cinemachine.ScreenComposerSettings.html)와 설치된 패키지의 툴팁을 기준으로 `±0.5`를 사용한다.

현재 씬 값은 `Screen Position (0, -0.1)`, `Dead Zone (0.1, 0.2)`, `Hard Limits (0.12, 0.22)`, `Damping (1, 1, 1)`이다. 이 값을 출발점으로 **한 설정씩만** 바꾸면 효과를 구분하기 쉽다.

## 직접 해볼 실습

1. `Screen Position`을 `(0, 0)`과 `(0, -0.2)`로 바꾸고, 플레이어가 화면에서 차지하는 위치를 비교한다.
2. `Dead Zone`을 껐다 켠 뒤 천천히 좌우로 움직인다. `Size`를 키워 카메라가 움직이기 시작하는 시점을 비교한다.
3. `Hard Limits`를 끄고 켠 뒤 빠르게 움직인다. 화면 속 플레이어의 허용 위치가 달라지는지 본다. 맵 경계를 제한하는 기능으로 해석하지 않는다.
4. `Damping`의 X와 Y를 각각 `0`과 큰 값으로 바꿔 수평·수직 추적 반응을 따로 관찰한다.

## 이해도 체크

- [ ] Unity Camera, Cinemachine Brain, Cinemachine Camera의 역할을 각각 설명할 수 있다.
- [ ] 설치 여부를 Package Manager에서 확인하고, 새 프로젝트에 패키지를 설치할 수 있다.
- [ ] Tracking Target을 지정하고 Position Composer로 2D 플레이어를 추적할 수 있다.
- [ ] Screen Position과 Dead Zone의 위치·크기 단위를 구분할 수 있다.
- [ ] Hard Limits와 월드 경계 제한의 차이를 설명할 수 있다.
- [ ] Damping을 높이거나 낮추면 카메라 반응이 어떻게 달라지는지 설명할 수 있다.

## 공식 참고 자료

- [Cinemachine 핵심 요소](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/concept-essential-elements.html) — Camera, Brain, Cinemachine Camera 관계
- [Cinemachine 설치](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/InstallationAndUpgrade.html) · [Unity Package Manager](https://docs.unity3d.com/6000.6/Documentation/Manual/upm-ui-install.html) — 패키지 설치
- [Cinemachine Camera 컴포넌트](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachineCamera.html) · [Tracking Target 설정](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/setup-procedural-behavior.html) — 추적 대상과 위치 제어
- [Position Composer](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachinePositionComposer.html) — Screen Position, Dead Zone, Hard Limits, Damping
- [ScreenComposerSettings API](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/api/Unity.Cinemachine.ScreenComposerSettings.html) — Screen Position 값의 기준
- [Cinemachine 2D](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/Cinemachine2D.html) · [Confiner 2D](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/manual/CinemachineConfiner2D.html) — 2D 카메라와 월드 경계
