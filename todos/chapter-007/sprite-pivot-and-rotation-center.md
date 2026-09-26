# 스프라이트 피봇과 회전 중심

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> 무료 픽셀 이미지를 받아서 애니메이션을 그려보면 캐릭터의 피봇 포인트가 맞지 않아서 캐릭터 움직임에 따라
> 땅의 위치나 발의 위치가 흔들린다. 피봇 포인트를 맞추면 해결된다는데, 관련된 개념 등을 정리해줘.
> 이미지 사이즈가 다른 것도 문제인 것 같아. 오브젝트의 중심과 오브젝트의 회전이 발생하는 위치를 알아야 하는 것이 필요하네.

## 피봇이란

**피봇(Pivot)은 스프라이트 이미지 안에서 "이 지점이 곧 오브젝트의 위치다"라고 정한 기준점**이다.

- `transform.position`이 가리키는 곳 = 스프라이트의 피봇
- `transform.Rotate()`로 회전할 때 **회전축이 되는 지점** = 피봇
- `transform.localScale`로 확대/축소할 때 **고정되는 지점** = 피봇

즉 질문의 "오브젝트의 중심"과 "회전이 발생하는 위치"는 **같은 것이고, 그게 피봇**이다. Unity 기본값은 `Center`(이미지 정중앙)다.

## 왜 발이 흔들리는가

애니메이션 프레임들의 피봇이 제각각이면, 프레임이 바뀔 때마다 **"이미지 안에서 발이 있는 위치"가 달라진다.** `transform.position`은 그대로인데 그림만 상하로 튄다.

```
프레임 A (32×32, 피봇 Center)     프레임 B (32×40, 피봇 Center)
  피봇 = 아래에서 16px             피봇 = 아래에서 20px
  → 발이 피봇에서 16px 아래         → 발이 피봇에서 20px 아래
  
같은 transform.position인데 발 높이가 4px 차이 난다 → 프레임 전환마다 덜컹거림
```

**이미지 사이즈가 다른 게 문제라는 추측이 정확하다.** 피봇이 `Center`로 통일돼 있어도, 이미지 높이가 다르면 "중앙에서 발까지의 거리"가 프레임마다 달라진다. 그래서 크기가 제각각인 시트에서 `Center` 피봇은 거의 항상 흔들린다.

## 해결: 피봇을 발밑에 고정한다

캐릭터는 **발바닥이 땅에 닿는 지점**이 의미 있는 기준점이다. 그래서 피봇을 `Bottom`(또는 발 위치의 Custom 좌표)으로 맞추면, 이미지 높이가 달라져도 발 높이는 그대로 유지된다.

### Sprite Editor에서 설정하는 법

1. 이미지 선택 → Inspector의 **Sprite Mode**를 `Multiple`(시트인 경우)로 설정
2. **Sprite Editor** 열기 → 프레임 슬라이스
3. 각 프레임 선택 → **Pivot**을 `Bottom`으로, 또는 `Custom`으로 두고 직접 지정
4. 픽셀 단위로 정밀하게 맞추려면 **Pivot Unit Mode**를 `Pixels`로 바꾼다
5. Apply

`Pivot Unit Mode`가 `Normalized`면 0~1 비율 좌표라서 이미지 크기가 다르면 또 어긋난다. **픽셀 아트에서는 `Pixels`가 안전하다.**

## 같이 맞춰야 하는 것: Pixels Per Unit

**Pixels Per Unit(PPU)** 은 "몇 픽셀을 1 Unity 유닛으로 볼 것인가"다. 기본값 100.

- 32×32 픽셀 이미지, PPU 100 → 월드에서 0.32 × 0.32 유닛
- 32×32 픽셀 이미지, PPU 32 → 월드에서 1 × 1 유닛

**프로젝트의 모든 스프라이트에 같은 PPU를 써야 한다.** 하나만 다르면 그 캐릭터만 유독 크거나 작게 나온다. 픽셀 아트는 보통 원본 타일 크기(16, 32 등)에 맞춘다.

픽셀 아트라면 함께 볼 것:
- **Filter Mode**: `Point (no filter)` — 안 그러면 픽셀이 뭉개진다
- **Compression**: `None` — 픽셀 아트에 압축 아티팩트가 생긴다
- **Pixel Perfect Camera** (URP 2D): 픽셀 격자에 스냅해서 흔들림을 줄여준다

## 이 프로젝트에서 회전이 피봇을 쓰는 지점

```csharp
[ContextMenu("Flip")]
private void Flip()
{
    transform.Rotate(0.0f, 180.0f, 0.0f);
    facingRight = !facingRight;
}
```

Y축으로 180도 돌려 좌우를 뒤집는데, **이 회전의 중심이 피봇**이다. 피봇이 캐릭터 중앙에서 벗어나 있으면 방향 전환할 때마다 캐릭터가 옆으로 순간이동한 것처럼 보인다.

→ 피봇은 **세로로는 발밑, 가로로는 캐릭터 중앙**에 두는 게 일반적인 해법이다 (`Bottom Center`).

## 체크리스트

- [ ] 피봇이 position, 회전축, 스케일 기준점을 동시에 결정한다는 걸 안다
- [ ] 이미지 크기가 다른 프레임에서 Center 피봇이 왜 흔들리는지 설명할 수 있다
- [ ] Sprite Editor에서 피봇을 Bottom 또는 Custom(Pixels)으로 바꿀 수 있다
- [ ] Pivot Unit Mode의 Normalized와 Pixels 차이를 안다
- [ ] Pixels Per Unit을 프로젝트 전체에서 통일해야 하는 이유를 안다
- [ ] 픽셀 아트의 Filter Mode를 Point로 두는 이유를 안다
- [ ] `transform.Rotate`로 뒤집을 때 피봇이 회전 중심이 된다는 걸 확인했다

## 공식 참고 자료

- [Manual: Sprites](https://docs.unity3d.com/6000.6/Documentation/Manual/sprite/sprite-landing.html)
- [Manual: Sprite (2D and UI) texture Import Settings reference](https://docs.unity3d.com/6000.6/Documentation/Manual/texture-type-sprite.html) — Pivot, Pixels Per Unit, Filter Mode
- [Manual: Use the Sprite Editor](https://docs.unity3d.com/6000.6/Documentation/Manual/sprite/sprite-editor/use-editor.html)
- [Manual: Prepare your sprites for the 2D Pixel Perfect Camera in URP](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/2d-pixelperfect-prep-sprites.html)
- [Manual: 2D Pixel Perfect (URP)](https://docs.unity3d.com/6000.6/Documentation/Manual/urp/2d-pixelperfect.html)
- [Scripting API: Sprite.pivot](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Sprite-pivot.html)
- [Scripting API: SpriteAlignment](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/SpriteAlignment.html)
