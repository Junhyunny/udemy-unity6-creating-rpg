# Composite Collider 2D의 Geometry Type: Polygons와 Outlines

> 질문: Composite Collider 2D의 Geometry Type을 `Polygons`와 `Outlines`로 설정하면 무엇이 달라지는가?
>
> 학습 맥락: chapter-045의 타일맵 작업. 프로젝트 에디터 버전은 Unity 6000.6.3f1이다.

## 먼저 이해할 것

Composite Collider 2D는 참여하는 여러 Collider 2D의 모양을 합쳐 새 충돌 형상을 만든다. `Geometry Type`은 **합친 형상을 내부가 채워진 영역으로 만들지, 경계선만으로 만들지** 결정한다. 화면에 그려지는 스프라이트나 타일 그림을 바꾸는 설정은 아니다. [Unity 매뉴얼: Composite Collider 2D 컴포넌트](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/composite-collider/composite-collider-2d-reference.html)

| | `Outlines` | `Polygons` |
| --- | --- | --- |
| 생성되는 형상 | 연결된 **경계선(에지)**. Edge Collider 2D와 유사 | 내부가 채워진 **다각형 영역**. Polygon Collider 2D와 유사 |
| 경계 안쪽만 차지하는 다른 콜라이더 | 경계선과 닿지 않으면 충돌·트리거 접촉으로 잡히지 않음 | 내부도 충돌·트리거 영역에 포함됨. 단, 구멍은 제외 |
| 일반적인 선택 | 캐릭터가 이동하는 바닥·플랫폼의 표면 | 내부에 들어왔는지 판정해야 하는 영역, 특히 트리거 |
| 생성 형상과 비용 | 보통 에지 수가 적고 효율적 | 여러 볼록 다각형으로 분해될 수 있어 보통 더 비쌈 |

**`Outlines`의 선은 닫힌 고리다.** 열린 선이라는 뜻이 아니다. 고리 안쪽이 비어 있으므로, 다른 콜라이더가 경계에 닿거나 경계를 통과할 때 접촉하고 경계 안에만 있으면 접촉하지 않는다. [Unity API: Outlines](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CompositeCollider2D.GeometryType.Outlines.html)

**`Polygons`의 내부는 충돌 영역이다.** 합쳐진 외곽을 여러 볼록 다각형으로 분해할 수 있다. 외곽 안에 만들어진 구멍은 충돌·트리거 영역이 아니다. Unity는 내부 판정이 필요한 경우에 이 모드를 쓰도록 설명한다. [Unity API: Polygons](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CompositeCollider2D.GeometryType.Polygons.html)

## chapter-045 타일맵에서 직접 비교하기

1. 타일맵 게임 오브젝트의 `Tilemap Collider 2D`와 `Composite Collider 2D`가 같은 Rigidbody 2D에 속하는지 확인한다. 타일맵 콜라이더의 `Composite Operation`을 `Merge`로 설정해야 복합 형상에 참여한다. [Unity 매뉴얼: Tilemap Collider 2D](https://docs.unity3d.com/6000.6/Documentation/Manual/tilemaps/work-with-tilemaps/tilemap-collider-2d-reference.html)
2. 몇 개의 타일로 바닥과 **가운데가 빈 고리**를 만든다. Scene 뷰에서 `Geometry Type`을 번갈아 바꾸고, 경계선과 내부가 어떻게 생성되는지 비교한다.
3. 작은 Dynamic Rigidbody 2D와 Collider 2D가 달린 물체를 바닥 위에서 움직여 두 모드의 표면 접촉을 비교한다. 타일 사이 이음새에서 접촉이 튀는지도 본다.
4. 고리의 빈 공간과 다각형으로 채워진 내부에 작은 콜라이더를 각각 놓는다. Composite Collider 2D의 `Is Trigger`를 켜서 경계에 닿지 않는 내부 물체가 어느 모드에서 감지되는지 비교한다. **빈 구멍**과 **채워진 내부**를 구별한다.
5. 타일 사이에 작은 틈이 남으면 `Extrusion Factor`와 Composite Collider 2D의 `Vertex Distance` 설명을 읽고 형상 병합 결과를 확인한다. 이 값은 모드 선택과 별개의 설정이다.

## 이해도 체크

- [ ] `Outlines`가 닫힌 외곽선이면서도 내부 판정이 없는 이유를 설명할 수 있다.
- [ ] `Polygons`가 내부 판정에 필요한 경우와 구멍이 제외되는 경우를 구분할 수 있다.
- [ ] 이동용 타일 바닥과 내부 감지용 트리거에 어느 모드를 먼저 시도할지 설명할 수 있다.
- [ ] 두 모드에서 생성되는 에지·다각형 수와 접촉 결과를 Scene 뷰에서 비교했다.

## 공식 참고 자료

- [Unity 매뉴얼: Composite Collider 2D 컴포넌트](https://docs.unity3d.com/6000.6/Documentation/Manual/2d-physics/collider/composite-collider/composite-collider-2d-reference.html) — Geometry Type의 기본 정의와 설정
- [Unity API: GeometryType.Outlines](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CompositeCollider2D.GeometryType.Outlines.html) — 경계선 접촉, 플랫폼 용도와 효율
- [Unity API: GeometryType.Polygons](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/CompositeCollider2D.GeometryType.Polygons.html) — 내부 판정, 구멍, 형상 분해
- [Unity 매뉴얼: Tilemap Collider 2D 컴포넌트](https://docs.unity3d.com/6000.6/Documentation/Manual/tilemaps/work-with-tilemaps/tilemap-collider-2d-reference.html) — 타일맵 결합과 Extrusion Factor
