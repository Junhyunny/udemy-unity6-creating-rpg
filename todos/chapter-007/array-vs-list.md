# 배열(Array)과 List\<T\>의 차이

> 원본 TODO (`chapter-007/Assets/Player.cs`)
>
> Array, List 차이점? 언제 어떤 것을 사용하는게 좋은지. 관련된 내용을 정리해줘.
> 속도, 퍼포먼스, 메모리 효율성, 안정성, 등등

## 질문이 나온 코드

```csharp
// public Collider2D[] enemies;
// public List<Collider2D> enemyList;

public void DamageEnemies()
{
    Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnermy);
    foreach (var enemy in enemies) { ... }
}
```

## 한 문장 요약

**배열은 크기가 고정된 연속 메모리 블록이고, `List<T>`는 그 배열을 감싸서 크기가 변하는 것처럼 보이게 만든 클래스다.**

`List<T>`는 내부에 배열(`T[] _items`)을 들고 있다가, 꽉 차면 **더 큰 배열을 새로 만들어 통째로 복사**한다. 그래서 "List가 배열보다 유연하지만 조금 느리다"는 말이 나온다.

## 비교표

| | `T[]` (배열) | `List<T>` |
| --- | --- | --- |
| 크기 | **생성 시 고정** | 동적 증가 |
| 추가/삭제 | 불가 (새 배열을 만들어야 함) | `Add`, `Remove`, `Insert` |
| 인덱스 접근 | `arr[i]` — 가장 빠름 | `list[i]` — 거의 같음 (얇은 래퍼) |
| 메모리 | 요소 수만큼만 | **여유 공간(Capacity) 포함** |
| Unity 직렬화 | ⭕ | ⭕ |
| 다차원 | `int[,]` 가능 | 불가 (`List<List<T>>`로 흉내) |
| 선언 | `new Collider2D[10]` | `new List<Collider2D>()` |

## 속도

**인덱스 접근 속도는 사실상 같다.** `List<T>`의 인덱서는 내부 배열 접근 한 줄이라 JIT가 인라인해 버린다. "List가 느리다"는 말은 인덱스 접근이 아니라 **크기 변경 비용**을 가리키는 것이다.

### 크기 변경 비용

`List<T>`는 꽉 차면 **Capacity를 2배로 늘린 새 배열을 할당하고 전체를 복사**한다.

```
Count 4, Capacity 4 에서 Add 호출
  → Capacity 8짜리 새 배열 할당
  → 기존 4개 복사
  → 옛 배열은 GC 대상
```

`Add`를 N번 호출할 때 개별 호출은 대부분 O(1)이고, 재할당이 일어나는 순간만 O(N)이다. 전체를 평균 내면 O(1)이라 실전에서는 대체로 문제없다.

**미리 크기를 알면 Capacity를 지정해서 재할당을 없앨 수 있다.**

```csharp
var list = new List<Collider2D>(16);   // 16개까지는 재할당 없음
```

### 중간 삽입/삭제는 둘 다 느리다

`List<T>.Insert(0, x)`나 `Remove(x)`는 **뒤 요소를 전부 밀어야** 하므로 O(N)이다. 배열과 다를 게 없다. 앞뒤 삽입이 잦으면 `Queue<T>`, `LinkedList<T>`를 고려한다.

## 메모리

`List<T>`는 **Count보다 Capacity가 큰 경우가 많다.** 10개를 담고 있어도 내부 배열은 16칸일 수 있다. 배열은 딱 요소 수만큼만 쓴다.

수천 개를 다루는 게 아니라면 차이는 미미하다. 다만 **게임에서 진짜 문제는 총 사용량이 아니라 "매 프레임 새 할당이 발생하는가"** 다. 할당이 쌓이면 GC가 돌고, GC가 돌면 프레임이 튄다.

## 안정성 관점

| | 배열 | `List<T>` |
| --- | --- | --- |
| 범위 밖 접근 | `IndexOutOfRangeException` | `ArgumentOutOfRangeException` |
| 크기 실수 | 컴파일 시점에 못 잡음 | 동적이라 애초에 덜 발생 |
| 의도 표현 | "크기가 안 변한다"를 타입으로 보장 | 누구나 바꿀 수 있음 |
| 외부 노출 | `public T[]`는 내용 변경 가능 | `public List<T>`도 마찬가지 |

**외부에 컬렉션을 노출할 때는 둘 다 위험하다.** 읽기 전용으로 주려면 `IReadOnlyList<T>`를 쓴다.

```csharp
private readonly List<Enemy> enemies = new();
public IReadOnlyList<Enemy> Enemies => enemies;   // 외부에서 Add 불가
```

## 선택 기준

| 상황 | 선택 |
| --- | --- |
| API가 반환하는 것을 그대로 받음 | 주는 대로 (`OverlapCircleAll`은 배열) |
| 개수가 고정, 자주 순회 | **배열** |
| 런타임에 추가/삭제 | **`List<T>`** |
| Inspector에 노출 | 둘 다 가능. `List<T>`가 편집하기 편하다 |
| 매 프레임 호출되는 코드 | **재사용 가능한 컬렉션 + NonAlloc API** |
| 키로 조회 | `Dictionary<K,V>` (단, Unity 직렬화 안 됨) |
| 중복 없는 집합 | `HashSet<T>` |

## 이 프로젝트에 적용하면

```csharp
public void DamageEnemies()
{
    Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsEnermy);
    foreach (var enemy in enemies) { ... }
}
```

**`OverlapCircleAll`은 호출할 때마다 새 배열을 할당한다.** 공격이 초당 몇 번 수준이면 신경 쓸 일이 아니지만, 매 프레임 호출되는 코드라면 GC 부담이 된다.

Unity는 이걸 피하려고 **미리 만든 `List<T>`에 결과를 채워 주는 오버로드**를 제공한다.

```csharp
private readonly List<Collider2D> hits = new();      // 필드로 한 번만 생성
private ContactFilter2D filter;

private void Awake()
{
    filter = new ContactFilter2D { useLayerMask = true, layerMask = whatIsEnermy, useTriggers = false };
}

public void DamageEnemies()
{
    Physics2D.OverlapCircle(attackPoint.position, attackRadius, filter, hits);  // hits 재사용
    foreach (var hit in hits) { ... }
}
```

**이게 배열 대신 `List<T>`를 쓰는 대표적인 이유다.** "크기가 변해서"가 아니라 **"같은 컬렉션을 재사용해서 할당을 없애려고"** 쓴다. → [physics2d-overlapcircleall.md](./physics2d-overlapcircleall.md)

## `foreach`에 대한 참고

예전에는 "`List<T>`의 `foreach`는 박싱 때문에 느리다"는 말이 있었다. **현재 `List<T>`의 열거자는 구조체라 박싱이 없다.** 배열 `foreach`는 컴파일러가 `for` 루프로 바꿔 준다. 둘 다 신경 쓸 수준이 아니다.

단, **`IEnumerable<T>`로 받아서 `foreach`를 돌리면 박싱이 발생한다.** 인터페이스로 받을 때만 주의하면 된다.

## 체크리스트

- [ ] `List<T>`가 내부적으로 배열을 쓴다는 걸 안다
- [ ] Count와 Capacity를 구분할 수 있다
- [ ] 인덱스 접근 속도는 사실상 같다는 걸 안다
- [ ] 재할당이 언제 일어나고 어떻게 피하는지 안다
- [ ] 중간 삽입/삭제가 둘 다 O(N)임을 안다
- [ ] `IReadOnlyList<T>`로 읽기 전용 노출을 할 수 있다
- [ ] `OverlapCircleAll`이 매번 배열을 할당한다는 걸 안다
- [ ] 할당을 피하려고 `List<T>`를 재사용하는 패턴을 이해했다

## 공식 참고 자료

- [Microsoft Learn: 배열 (C#)](https://learn.microsoft.com/ko-kr/dotnet/csharp/programming-guide/arrays/)
- [Microsoft Learn: List\<T\> 클래스](https://learn.microsoft.com/ko-kr/dotnet/api/system.collections.generic.list-1)
- [Microsoft Learn: Array 클래스](https://learn.microsoft.com/ko-kr/dotnet/api/system.array)
- [Microsoft Learn: 컬렉션과 데이터 구조](https://learn.microsoft.com/ko-kr/dotnet/standard/collections/)
- [Microsoft Learn: 일반적으로 사용되는 컬렉션 형식](https://learn.microsoft.com/ko-kr/dotnet/standard/collections/commonly-used-collection-types)
- [Scripting API: Physics2D.OverlapCircle](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCircle.html) — List 오버로드
- [Scripting API: ContactFilter2D](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/ContactFilter2D.html)
- [Manual: Script serialization](https://docs.unity3d.com/6000.6/Documentation/Manual/script-serialization.html) — 배열/List는 직렬화되고 Dictionary는 안 되는 이유
