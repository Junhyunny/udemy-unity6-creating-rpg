# Physics2D.OverlapCircleAll

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Physics2D.OverlapCircleAll() 함수에 대해서 설명해줘. 해당 에어리어에 있는 값들을 Collider 객체들을 모두 반환하는건가?

## 짧은 답: 맞다

**지정한 원 영역에 겹쳐 있는 Collider2D를 전부 배열로 반환한다.** 광선(Raycast)이 아니라 **영역(Overlap)** 질의라서, 그 안에 닿아 있는 모든 콜라이더가 결과에 들어간다.

## 질문이 나온 코드

```csharp
public void DamageEnemies()
{
    Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnermy);
    foreach (var enemy in enemies)
    {
        Debug.Log("enermies loop");
        enemy.GetComponent<Damaged_Example>().TakeDamage();
    }
}
```

`attackPoint` 위치에 반지름 `attackRadius`인 원을 놓고, 그 안에 있는 **`whatIsEnermy` 레이어의 콜라이더를 모두** 가져와 하나씩 피해를 준다. **범위 공격의 전형적인 구현**이다.

## 파라미터

```csharp
Physics2D.OverlapCircleAll(point, radius, layerMask)
```

| 인자 | 이 코드에서 | 의미 |
| --- | --- | --- |
| `point` | `attackPoint.position` | 원의 중심 (월드 좌표) |
| `radius` | `attackRadius` | 원의 반지름 |
| `layerMask` | `whatIsEnermy` | 이 레이어만 검사 → [layer-and-layermask.md](./layer-and-layermask.md) |

`minDepth` / `maxDepth` 인자도 있지만 2D 평면 게임에서는 거의 쓰지 않는다.

## Raycast와 무엇이 다른가

| | `Raycast` | `OverlapCircleAll` |
| --- | --- | --- |
| 모양 | **선** | **원(영역)** |
| 반환 | `RaycastHit2D` 하나 | `Collider2D[]` 전부 |
| 얻는 정보 | 접촉점, 법선, 거리 | **콜라이더만** |
| 용도 | 접지 판정, 시야 | **범위 공격, 감지 범위** |

**중요: Overlap 계열은 접촉점(`point`)이나 법선(`normal`)을 주지 않는다.** "누가 범위 안에 있는가"만 답한다. 넉백 방향이 필요하면 `enemy.transform.position - transform.position`처럼 직접 계산해야 한다.

## 형제 함수들

| 함수 | 반환 | 특징 |
| --- | --- | --- |
| `OverlapCircle(point, radius, mask)` | `Collider2D` **하나** | 첫 번째 하나만. 존재 여부 확인용 |
| `OverlapCircleAll(...)` | `Collider2D[]` | **매번 새 배열 할당** |
| `OverlapCircle(point, radius, filter, List<Collider2D>)` | `int` (개수) | **리스트 재사용, 할당 없음** |
| `OverlapBox` / `OverlapArea` / `OverlapPoint` | 각 모양 | 사각형 / 영역 / 점 |

접지 판정에 `OverlapCircle`(단수형)을 쓰는 것도 흔하다. 발밑에 작은 원을 놓고 뭔가 있는지만 보면 되기 때문이다.

## 성능 — `All`은 매번 배열을 할당한다

`OverlapCircleAll`은 호출할 때마다 **새 `Collider2D[]`를 만든다.** 그 배열은 곧 쓰레기가 되어 GC 대상이 된다.

- 공격처럼 **가끔 호출**되면 문제없다 (지금 코드가 이 경우다)
- **매 프레임 호출**하면 할당이 쌓여 GC 스파이크가 생긴다

매 프레임 써야 한다면 리스트를 재사용하는 오버로드로 바꾼다.

```csharp
private readonly List<Collider2D> hits = new();
private ContactFilter2D filter;

private void Awake()
{
    filter = new ContactFilter2D { useLayerMask = true, layerMask = whatIsEnermy, useTriggers = false };
}

public void DamageEnemies()
{
    int count = Physics2D.OverlapCircle(attackPoint.position, attackRadius, filter, hits);
    for (int i = 0; i < count; i++) { ... }
}
```

→ [array-vs-list.md](./array-vs-list.md)

## 주의할 점

### ① 같은 대상이 여러 번 들어올 수 있다

**한 게임 오브젝트에 콜라이더가 여러 개 붙어 있으면 각각이 따로 반환된다.** 적이 몸통과 머리에 콜라이더를 하나씩 갖고 있으면 **피해가 두 번 들어간다.**

```csharp
var damaged = new HashSet<Damaged_Example>();
foreach (var col in enemies)
{
    if (col.TryGetComponent<Damaged_Example>(out var target) && damaged.Add(target))
        target.TakeDamage();
}
```

### ② 컴포넌트가 없을 수 있다

```csharp
enemy.GetComponent<Damaged_Example>().TakeDamage();   // 없으면 NullReferenceException
```

`whatIsEnermy` 레이어에 있지만 `Damaged_Example`이 없는 오브젝트가 하나라도 걸리면 **그 자리에서 예외가 난다.** `TryGetComponent`를 쓰면 안전하다.

```csharp
if (enemy.TryGetComponent<Damaged_Example>(out var target)) target.TakeDamage();
```

→ [getcomponent-and-null-handling.md](./getcomponent-and-null-handling.md)

### ③ 트리거 콜라이더도 포함된다

기본적으로 `Is Trigger`가 켜진 콜라이더도 결과에 들어온다. 제외하려면 `ContactFilter2D.useTriggers = false`를 쓰거나 Project Settings의 `Queries Hit Triggers`를 끈다. (이 프로젝트는 `m_QueriesHitTriggers: 1`로 **켜져 있다.**)

### ④ 자기 자신도 잡힐 수 있다

플레이어가 `whatIsEnermy` 레이어에 포함돼 있으면 자기를 때린다. 레이어 분리로 막는다.

## 기즈모로 범위 확인하기

이 프로젝트는 공격 범위를 이미 시각화하고 있다.

```csharp
private void OnDrawGizmos()
{
    Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
    Gizmos.DrawWireSphere(attackPoint.position, attackRadius);   // 공격 범위
}
```

`DrawWireSphere`의 중심·반지름이 `OverlapCircleAll`의 인자와 **정확히 같아야** 보이는 것과 실제 판정이 일치한다. 지금 코드는 일치한다. → [gizmos-debug-visualization.md](./gizmos-debug-visualization.md)

## 체크리스트

- [ ] Overlap 계열이 영역 질의이고 Raycast와 다르다는 걸 안다
- [ ] 접촉점·법선을 주지 않는다는 걸 안다
- [ ] 단수형 `OverlapCircle`과 `All`의 차이를 안다
- [ ] `All`이 매번 배열을 할당한다는 걸 안다
- [ ] 리스트 재사용 오버로드를 쓸 수 있다
- [ ] 콜라이더가 여러 개면 중복 피해가 들어갈 수 있음을 안다
- [ ] `GetComponent` 결과를 확인하지 않으면 예외가 날 수 있음을 안다
- [ ] 기즈모 원과 실제 판정 범위가 같은지 확인했다

## 공식 참고 자료

- [Scripting API: Physics2D.OverlapCircleAll](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCircleAll.html)
- [Scripting API: Physics2D.OverlapCircle](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCircle.html)
- [Scripting API: Physics2D.OverlapBox](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapBox.html)
- [Scripting API: ContactFilter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ContactFilter2D.html)
- [Scripting API: Collider2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Collider2D.html)
- [Scripting API: Component.TryGetComponent](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Component.TryGetComponent.html)
- [Scripting API: Physics2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.html)
