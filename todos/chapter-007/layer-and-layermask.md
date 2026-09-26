# Layer와 LayerMask

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> LayerMask는 무엇을 의미하는거야? 어떤 용도로 사용하는거지? 공부할 수 있는 자료 정리해줘.
> Layer라는 개념은 뭐야? Unity에서 없던 Layer를 만들고, 아래 Square 컴포넌트에 있는 Layer를 Ground로
> 설정했는데, 이 Layer를 통해서 Ground인지 아닌지 구분하는거야? 그라운드 레이어에 있는 컴포넌트들과
> 충돌을 관리하기 위해서 레이어를 만든건가? 관련된 내용을 정리해줘

## 짧은 답: 두 추측 다 맞다

- **"Layer로 Ground인지 아닌지 구분한다"** → ✅ 맞다. 이 프로젝트의 접지 판정이 정확히 그 방식이다.
- **"충돌을 관리하기 위해 만든다"** → ✅ 맞다. 다만 그건 Layer 용도 중 하나고, 다른 용도도 있다.

## Layer란 — 게임 오브젝트에 붙이는 "분류 번호"

**Layer는 게임 오브젝트를 그룹으로 묶는 번호표**다. 0~31번까지 **딱 32개**만 존재하고, 게임 오브젝트 하나는 **정확히 하나의 Layer에만** 속한다.

Unity 기본 Layer:

| 번호 | 이름 |
| --- | --- |
| 0 | Default |
| 1 | TransparentFX |
| 2 | Ignore Raycast |
| 3 | (비어 있음) |
| 4 | Water |
| 5 | UI |
| 6~31 | 사용자가 정의 |

이 프로젝트는 **6번에 `Ground`를 추가**했다 (`ProjectSettings/TagManager.asset`).

```yaml
layers:
- Default          # 0
- TransparentFX    # 1
- Ignore Raycast   # 2
-                  # 3
- Water            # 4
- UI               # 5
- Ground           # 6   ← 추가한 것
```

씬에서 `Square`와 `Square (1)`이 `m_Layer: 6`이고, `Player`는 `m_Layer: 0`(Default)이다.

### Layer vs Tag

둘 다 오브젝트를 분류하지만 목적이 다르다.

| | Layer | Tag |
| --- | --- | --- |
| 개수 | 32개 제한 | 제한 없음 |
| 주 용도 | **물리/렌더링 시스템이 일괄 필터링** | 코드에서 개별 식별 |
| 비교 방법 | 비트 연산 (빠름) | 문자열 비교 |
| 예 | "이 광선은 Ground만 맞춘다" | `if (other.CompareTag("Enemy"))` |

**Layer는 엔진이 쓰고, Tag는 내 코드가 쓴다**고 생각하면 대체로 맞다.

## LayerMask란 — "어떤 Layer들을 대상으로 할지" 고르는 비트 집합

Layer가 32개인 건 우연이 아니다. **`int`가 32비트**라서, 비트 하나가 Layer 하나를 담당한다.

```
Layer 번호:  ... 6 5 4 3 2 1 0
비트:        ... 1 0 0 0 0 0 0     ← Ground(6번)만 선택한 마스크
                                     = 2^6 = 64 = (1 << 6)
```

`LayerMask`는 이 `int` 비트필드를 감싼 구조체다. Inspector에서는 드롭다운 체크박스로 보인다.

```csharp
[SerializeField] private LayerMask whatIsGround;
```

여기서 `Ground`를 체크하면 내부적으로 `64`가 저장된다.

### 코드에서 마스크 만들기

```csharp
int groundMask = 1 << 6;                             // 6번 레이어
int groundMask = 1 << LayerMask.NameToLayer("Ground"); // 번호를 이름으로 조회
int groundMask = LayerMask.GetMask("Ground");          // 가장 읽기 쉬움
int mask = LayerMask.GetMask("Ground", "Platform");    // 여러 개는 OR로 합쳐짐
int everythingExceptPlayer = ~LayerMask.GetMask("Player"); // ~ 로 반전
```

**흔한 실수**: `LayerMask.NameToLayer("Ground")`는 마스크가 아니라 **번호(6)** 를 준다. 그대로 Raycast에 넘기면 6은 비트로 `000110`이라 **1번과 2번 레이어를 가리키게 된다.** 반드시 `1 <<` 로 시프트하거나 `GetMask`를 써야 한다.

### 특정 오브젝트가 마스크에 속하는지 검사

```csharp
if ((whatIsGround.value & (1 << other.gameObject.layer)) != 0)
{
    // other는 Ground 레이어다
}
```

## 이 프로젝트에서의 실제 사용

```csharp
[SerializeField] private LayerMask whatIsGround;

private void HandleCollision()
{
    isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
}
```

캐릭터 발밑으로 광선을 쏘되 **`whatIsGround`에 체크된 레이어만 검사 대상**으로 삼는다.

마스크가 없다면 이 광선은 적, 아이템, 심지어 **플레이어 자신의 콜라이더**까지 맞고 `isGrounded`가 항상 true가 된다. (Project Settings의 `Queries Start In Colliders`가 켜져 있어서 광선 시작점이 콜라이더 안이면 자기 자신도 잡힌다.)

**Layer는 여기서 "이건 밟을 수 있는 땅이다"라는 의미를 부여하는 장치**다. 질문의 추측대로다.

## Layer의 다른 용도들

접지 판정 말고도 Layer는 여러 시스템이 함께 쓴다.

### ① Layer Collision Matrix — 레이어 쌍별 충돌 on/off

`Project Settings → Physics 2D → Layer Collision Matrix`에서 **어떤 레이어끼리 충돌할지** 체크박스로 끄고 켤 수 있다.

- 적끼리는 서로 통과시키기 (Enemy ↔ Enemy 해제)
- 아군 총알이 아군을 안 맞히기 (PlayerBullet ↔ Player 해제)

코드 한 줄 없이 물리 레벨에서 처리되므로 **성능에도 이득**이다. (이 프로젝트는 현재 전부 켜진 기본 상태다.)

### ② Camera Culling Mask

카메라가 **특정 레이어만 렌더링**하게 한다. 미니맵 카메라, UI 전용 카메라를 만들 때 쓴다.

### ③ Raycast 필터링

위에서 본 용도. 모든 `Physics2D` 질의 함수가 `layerMask` 파라미터를 받는다.

### ④ Collider2D의 Include/Exclude Layers

전역 매트릭스와 별개로, **콜라이더 하나에만** 레이어 규칙을 덮어쓸 수 있다.

## 설계할 때 주의

32개는 생각보다 빨리 떨어진다. **개별 오브젝트 식별에는 Tag를, 시스템 수준 필터링에는 Layer를** 쓰는 기준을 지키는 게 좋다. "적 개체마다 레이어 하나씩" 같은 식으로 쓰면 금방 고갈된다.

## 체크리스트

- [ ] Layer가 32개 제한이고 오브젝트당 하나만 가진다는 걸 안다
- [ ] Layer와 Tag를 언제 구분해서 쓰는지 설명할 수 있다
- [ ] LayerMask가 비트필드이고 `1 << layer` 형태로 만들어진다는 걸 안다
- [ ] `NameToLayer`와 `GetMask`의 차이(번호 vs 마스크)를 안다
- [ ] Raycast에 마스크를 안 넘기면 왜 자기 자신을 맞는지 설명할 수 있다
- [ ] Layer Collision Matrix로 충돌을 끄는 방법을 안다
- [ ] 이 프로젝트에서 Ground가 6번 레이어임을 확인했다

## 공식 참고 자료

- [Manual: Layers](https://docs.unity3d.com/6000.6/Documentation/Manual/Layers.html)
- [Manual: Layers and Layer Masks](https://docs.unity3d.com/6000.6/Documentation/Manual/layers-and-layermasks.html)
- [Manual: Layer-based collision detection](https://docs.unity3d.com/6000.6/Documentation/Manual/LayerBasedCollision.html)
- [Manual: Tags and Layers settings reference](https://docs.unity3d.com/6000.6/Documentation/Manual/class-TagManager.html)
- [Scripting API: LayerMask](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/LayerMask.html)
- [Scripting API: Physics2D.Raycast](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.Raycast.html)
- [Scripting API: Physics2D.queriesStartInColliders](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D-queriesStartInColliders.html)
