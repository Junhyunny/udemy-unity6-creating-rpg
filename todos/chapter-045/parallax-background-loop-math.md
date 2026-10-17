# 패럴랙스 배경의 이동과 무한 반복 계산식

> 질문: 캐릭터 움직임에 따라 배경을 움직이는 코드인데, 배경 이미지(박스)가 어떻게 움직이는지 계산식을 정확히 이해하고 싶다.
>
> 프로젝트 기준: `chapter-045`는 Unity 6000.6.3f1, Cinemachine 6.6.0을 사용한다. 아래 숫자는 `SampleScene.unity`에 저장된 값으로 계산했고, Game 뷰 비율은 16:9(`aspect ≈ 1.78`)로 가정했다.

## 질문이 나온 코드

`chapter-045/Assets/Scripts/ParallaxLayer.cs`

```csharp
public void Move(float distanceToMove)
{
     background.position += Vector3.right * (distanceToMove * parallaxMultiplier);
}

public void LoopBackground(float cameraLeftEdge, float cameraRightEdge)
{
     float imageRightEdge = (background.position.x + imageHalfWidth) - imageWidthOffset;
     float imageLeftEdge = (background.position.x - imageHalfWidth) + imageWidthOffset;

     if (imageRightEdge < cameraLeftEdge)
     {
          background.position += Vector3.right * imageFullWidth;
     }
     else if (imageLeftEdge > cameraRightEdge)
     {
          background.position += Vector3.right * imageFullWidth * -1;
     }
}
```

`ParallaxBackground`는 카메라가 움직인 뒤 한 번씩, 레이어마다 `Move`와 `LoopBackground`를 차례로 호출한다. 두 함수는 하는 일이 다르다.

| 함수 | 하는 일 | 화면에서 보이는 효과 |
| --- | --- | --- |
| `Move` | 카메라가 움직인 거리의 일부만큼 배경을 따라 옮긴다 | 먼 배경일수록 느리게 흘러 입체감이 생김 |
| `LoopBackground` | 배경이 화면 밖으로 충분히 벗어나면 이미지 한 장 너비만큼 순간이동시킨다 | 배경이 끝나지 않고 계속 이어지는 것처럼 보임 |

## 1단계: 배경 오브젝트의 구조 — 같은 그림 3장

씬에서 배경 레이어(`BackgroundSky`, `BackgroundTown`)는 각각 가운데 이미지 1장과 자식 이미지 2장(Left, Right)으로 되어 있다.

```
 [ Left 복사본 ][ 가운데(background) ][ Right 복사본 ]
                        ↑
            코드가 움직이는 Transform은 이것 하나
```

- 코드가 직접 움직이는 건 가운데 이미지의 `Transform`(`background`) 하나다. 자식 2장은 부모를 따라 함께 움직인다.
- `imageFullWidth`는 가운데 이미지의 `SpriteRenderer.bounds.size.x`다. `bounds`는 **월드 좌표 기준** 크기이므로 Scale이 곱해진 값이다.
- `BackgroundTown`: 스프라이트 너비 24 × Scale 1.158 ≈ **27.8**. 자식은 가운데에서 ±27.8 떨어져 있어 세 장이 빈틈없이 이어진다. 세 장이 덮는 범위는 가운데 기준 **±41.7**이다.

## 2단계: `Move` — 카메라를 얼마나 따라갈지

```
배경 이동량 = 카메라 이동량 × parallaxMultiplier
```

예를 들어 카메라가 오른쪽으로 1만큼 움직이면:

| 레이어 | multiplier | 배경 이동량 | 화면 안에서 보이는 움직임 (배경 − 카메라) |
| --- | --- | --- | --- |
| `BackgroundSky` | 1 | +1 | 0 → 화면에 고정 (아주 먼 하늘) |
| `BackgroundTown` | 0.85 | +0.85 | −0.15 → 왼쪽으로 천천히 흐름 (먼 마을) |
| 일반 오브젝트 | (없음 = 0) | 0 | −1 → 빠르게 지나감 (바로 앞 지형) |

화면 안에서 배경이 흐르는 속도는 `(multiplier − 1) × 카메라 이동량`이다. multiplier가 1에 가까울수록 멀리 있는 것처럼 보인다.

## 3단계: `LoopBackground` — 언제, 얼마나 순간이동하나

multiplier가 1보다 작으면 배경은 카메라보다 조금씩 뒤처진다. 계속 오른쪽으로 가면 언젠가 가운데 이미지가 화면 왼쪽 밖으로 빠져나간다. 그때 세 장 전체를 오른쪽으로 **이미지 한 장 너비(`imageFullWidth`)만큼** 옮긴다.

세 장이 같은 그림이므로, 옮긴 뒤 "Right 복사본이 있던 자리"에 "가운데"가 오고 "가운데가 있던 자리"에 "Left 복사본"이 온다. 화면에 보이는 그림은 순간이동 전과 똑같아서 플레이어는 눈치채지 못한다.

```
순간이동 전:   [L][C][R]        카메라 →→→ 오른쪽으로 감
                    |화면|
순간이동 후:      [L][C][R]     세 장이 한 칸(imageFullWidth) 오른쪽으로
                    |화면|      화면에 보이는 그림은 동일
```

### 각 변수의 의미

| 변수 | 계산식 | 뜻 |
| --- | --- | --- |
| `cameraLeftEdge` | `카메라 x − cameraHalfWidth` | 화면 왼쪽 끝의 월드 x |
| `cameraRightEdge` | `카메라 x + cameraHalfWidth` | 화면 오른쪽 끝의 월드 x |
| `imageRightEdge` | `가운데 x + imageHalfWidth − imageWidthOffset` | 가운데 이미지의 오른쪽 끝을 **offset만큼 안쪽으로 당긴** 값 |
| `imageLeftEdge` | `가운데 x − imageHalfWidth + imageWidthOffset` | 가운데 이미지의 왼쪽 끝을 **offset만큼 안쪽으로 당긴** 값 |

### 조건문 해석

- `imageRightEdge < cameraLeftEdge`: 가운데 이미지의 (당겨진) 오른쪽 끝이 화면 왼쪽 끝보다 더 왼쪽이다. 가운데 이미지가 화면 왼쪽으로 거의 다 빠져나갔으므로 세 장을 **오른쪽으로 +imageFullWidth** 옮긴다.
- `imageLeftEdge > cameraRightEdge`: 반대로 가운데 이미지가 화면 오른쪽으로 빠져나갔으므로 **왼쪽으로 −imageFullWidth** 옮긴다.

### `imageWidthOffset`은 왜 있나

offset이 0이면 가운데 이미지가 화면에서 **완전히** 사라진 뒤에 순간이동한다. 그 직전 화면의 오른쪽은 Right 복사본 한 장만으로 채워야 한다. 화면이 이미지보다 넓으면 Right 복사본 오른쪽 끝 너머가 비어 보인다.

offset만큼 이미지 끝을 안쪽으로 당기면 **조금 더 일찍** 순간이동한다. 빈 공간이 보이기 전에 다음 이미지가 준비된다. 정리하면 offset은 이 두 조건을 만족해야 한다.

```
(1) 빈 공간이 안 보이려면:   화면 너비 ≤ imageFullWidth + imageWidthOffset
(2) 왔다 갔다 하지 않으려면: imageWidthOffset < 화면 너비 / 2
```

(2)를 어기면, 오른쪽으로 순간이동한 바로 다음 스텝에 왼쪽 조건도 참이 되어 다시 왼쪽으로 돌아온다. 이 동작이 매 스텝 반복되면 배경이 깜빡이거나 떨린다.

## 4단계: 이 프로젝트 값으로 직접 계산하기

`ParallaxBackground.Awake`의 화면 반폭 계산식은 현재 이렇게 되어 있다.

```csharp
cameraHalfWidth = mainCamera.orthographicSize - mainCamera.aspect;
```

직교 카메라의 `orthographicSize`는 **화면 세로의 절반**이다. 화면 가로의 절반은 `orthographicSize × aspect`다. 그래서 위 식은 `-`가 아니라 `*`가 맞다. ([Camera.orthographicSize](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Camera-orthographicSize.html), [Camera.aspect](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Camera-aspect.html))

| 값 | 현재 코드 (`12 − 1.78`) | 올바른 식 (`12 × 1.78`) |
| --- | --- | --- |
| `cameraHalfWidth` | 10.22 | 21.33 |
| 코드가 생각하는 화면 너비 | 20.44 | 42.67 |

`BackgroundTown`(너비 27.8, 반폭 13.9, offset 10)을 따라가 보자. 가운데 이미지 x를 `c`, 카메라 x를 `camX`라고 하자.

**현재 코드(반폭 10.22) 기준**

- 오른쪽 순간이동 조건: `c + 13.9 − 10 < camX − 10.22` → `camX − c > 14.12`
- 그 순간 실제 화면 오른쪽 끝: `c + 14.12 + 21.33 = c + 35.45` → 세 장이 덮는 `c + 41.7` 안쪽이므로 빈 공간 없음
- 순간이동 직후 `camX − c' = 14.12 − 27.8 = −13.68` → 왼쪽 조건(`camX − c < −14.12`)에 걸리지 않으므로 되돌아가지 않음

즉 **지금은 잘못된 반폭 덕분에 우연히 일찍 순간이동해서 잘 동작한다.**

**식만 `*`로 고쳤을 때(반폭 21.33)**

- 오른쪽 순간이동 조건: `camX − c > 13.9 − 10 + 21.33 = 25.23`
- 그 직전 실제 화면 오른쪽 끝: `c + 25.23 + 21.33 = c + 46.56` → 세 장이 덮는 `c + 41.7`보다 4.86만큼 넘어감 → **화면 오른쪽에 빈 공간이 보인다**
- 조건 (1)로 확인: 화면 너비 42.67 ≤ 27.8 + 10 = 37.8 → 거짓

그래서 반폭 식을 고칠 때는 `BackgroundTown`의 offset도 함께 조정해야 한다. 조건 (1)에서 offset ≥ 14.87, 조건 (2)에서 offset < 21.33이므로 **15~21 사이**가 안전하다. Game 뷰 비율이 바뀌면 화면 너비도 바뀌므로 범위도 달라진다.

`BackgroundSky`(multiplier 1)는 카메라와 똑같이 움직여서 `camX − c`가 변하지 않는다. 그래서 순간이동 조건에 처음부터 걸리지 않는 한 `LoopBackground`가 동작하지 않는다.

## 체크리스트

- [ ] `Move`에서 multiplier 1, 0.85, 0일 때 화면 안의 배경이 각각 어떻게 보이는지 설명할 수 있다.
- [ ] 같은 그림 3장을 붙여 두고 한 장 너비만큼 옮기면 왜 순간이동이 보이지 않는지 설명할 수 있다.
- [ ] `imageRightEdge < cameraLeftEdge`가 참이 되는 상황을 그림으로 그릴 수 있다.
- [ ] `imageWidthOffset`이 너무 작을 때(빈 공간)와 너무 클 때(왔다 갔다)의 문제를 설명할 수 있다.
- [ ] `orthographicSize`와 `aspect`로 화면 가로 반폭을 구할 수 있다.
- [ ] `SpriteRenderer.bounds`가 Scale이 반영된 월드 크기라는 것을 확인했다.

## 공식 참고 자료

- [Camera.orthographicSize](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Camera-orthographicSize.html) — 직교 카메라 크기는 세로의 절반
- [Camera.aspect](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Camera-aspect.html) — 가로 ÷ 세로 비율
- [Renderer.bounds](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Renderer-bounds.html) — 월드 공간 기준 경계 상자
- [Bounds](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Bounds.html) — `size`, `extents`(반폭)의 의미
- [Transform.position](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Transform-position.html) — 월드 좌표 위치, 자식은 부모를 따라 이동
- [CinemachineCore.CameraUpdatedEvent](https://docs.unity3d.com/Packages/com.unity.cinemachine@6.6/api/Unity.Cinemachine.CinemachineCore.html) — 카메라 갱신 직후 호출되는 이벤트
