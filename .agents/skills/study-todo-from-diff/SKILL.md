---
name: study-todo-from-diff
description: Unity 프로젝트의 미커밋 C# 스크립트에서 TODO 주석으로 명시한 학습 질문만 찾아 Unity 공식 문서 링크가 포함된 마크다운으로 정리한다. 현재 챕터 변경의 공부 TODO 정리나 TODO 주석의 학습 문서화를 요청할 때 사용한다. 일반 코드 리뷰나 주석에 없는 학습 주제 발굴에는 사용하지 않는다.
---

# 미커밋 Unity 스크립트의 학습 TODO 정리

미커밋 C# 스크립트에 작성된 `TODO` 학습 질문만 저장소 루트의 `todos/` 문서로 옮긴다.

## 저장소 구조

- 강의 챕터마다 `chapter-<번호>/`에 Unity 프로젝트가 하나씩 들어 있다.
- 학습 문서는 `todos/chapter-<번호>/`에 만든다. `Assets/` 안에 마크다운을 두면 Unity가 `.meta` 파일을 만들기 때문에 프로젝트 밖에 둔다.
- 여러 챕터의 TODO를 한 번에 처리할 때도 문서는 챕터별 디렉터리로 나눈다.

## 변경분 확인

- 저장소 지침(`AGENTS.md`)을 먼저 확인한다.
- `git status --short`, staged diff, unstaged diff를 모두 확인한다.
- `git diff`에 나오지 않는 untracked script도 직접 읽는다.
- TODO 수집 대상은 `chapter-*/Assets/` 아래의 `.cs` 스크립트뿐이다.
- `Library/`, `Temp/`, `obj/`, `Logs/`, `UserSettings/`, `*.csproj`, `*.sln`, `*.slnx`는 Unity와 IDE가 생성하는 산출물이므로 읽지 않는다. `.gitignore`로 이미 제외되어 있다.
- `.meta`, `.asset`, `.unity`, `.prefab`, `.inputactions` 같은 에셋 직렬화 파일은 손대지 않고 TODO 대상에도 넣지 않는다.
- Unity 템플릿이 기본 제공한 스크립트(`Assets/Welcome/` 등)는 사용자가 직접 지목하지 않는 한 제외한다.
- 미커밋 변경에 포함된 `TODO` 주석 중 공부나 개념 확인을 명시한 항목만 수집한다.
- `FIXME`, `XXX`, `HACK` 또는 주석에 없는 잠재 문제는 사용자가 별도로 요청하지 않는 한 대상에 넣지 않는다.

## 조사와 문서화

- 각 TODO 질문에 답하는 범위만 웹에서 조사한다.
- Unity API나 에디터 동작에 관한 질문은 Unity 공식 문서를 1차 자료로 쓴다.
  - Scripting API: `https://docs.unity3d.com/<버전>/Documentation/ScriptReference/`
  - Manual: `https://docs.unity3d.com/<버전>/Documentation/Manual/`
  - 패키지 문서(URP, Input System 등): `https://docs.unity3d.com/Packages/`
- 문서 버전은 `chapter-<번호>/ProjectSettings/ProjectVersion.txt`의 에디터 버전에 맞춘다. 해당 버전 문서가 없으면 가장 가까운 Unity 6 문서를 쓰고 그 사실을 문서에 적는다.
- C# 언어 자체에 관한 질문은 Microsoft Learn의 C# 문서를 쓴다.
- 강의 영상이나 블로그 요약이 아니라 내용을 직접 뒷받침하는 공식 문서 URL을 사용한다.
- API가 버전에 따라 바뀐 경우(`velocity` → `linearVelocity` 등) 현재 프로젝트 버전 기준의 이름과 변경 사실을 함께 적는다.
- `todos/`의 기존 문서를 먼저 확인하고 같은 질문은 중복 생성하지 않고 갱신한다.
- 질문 하나당 설명이 충분히 독립적이면 의미가 드러나는 이름의 마크다운 파일 하나를 만든다.
- 문서에는 질문이 나온 코드, 공부할 내용, 체크리스트, 공식 참고 자료를 담는다.
- `todos/README.md`에는 챕터별로 문서 링크와 완료 체크박스만 간결하게 정리한다.

## 원본 TODO 교체

- 문서화를 마친 원본 TODO 주석은 해당 마크다운을 가리키는 상대 링크 한 줄로 교체한다.
- 형식은 `// TODO: [todos/chapter-<번호>/<파일명>.md](<원본 파일에서 문서까지의 상대 경로>)`로 한다.
  - 예: `chapter-007/Assets/Player.cs` → `// TODO: [todos/chapter-007/rigidbody2d-linear-velocity.md](../../todos/chapter-007/rigidbody2d-linear-velocity.md)`
- 링크 외에 기존 질문 문구를 주석에 남기지 않는다.

## 제한 사항

- TODO 주석으로 명시되지 않은 학습 주제를 임의로 추가하지 않는다.
- 원본 스크립트에서는 TODO 주석을 링크로 바꾸는 작업만 한다. 다른 코드는 수정하지 않는다.
- `.meta` 파일을 새로 만들거나 지우거나 GUID를 수정하지 않는다. `.meta`는 Unity 에디터만 생성한다.
- 씬, 프리팹, 에셋 등 Unity가 직렬화한 파일을 텍스트로 편집하지 않는다.
- 기존 변경을 보존하고 stage, commit 또는 TODO 완료 처리를 하지 않는다.
- 문서와 스킬 본문은 사용자가 다른 언어를 요청하지 않는 한 한글로 작성한다.

## 검증

- 원본의 학습 TODO 수와 생성·갱신한 주제 수가 일치하는지 확인한다.
- 모든 TODO 상대 링크와 `todos/README.md` 링크의 대상 파일이 존재하는지 확인한다.
- `git diff --check`를 실행하고 의도한 문서와 TODO 링크 외에 스크립트를 바꾸지 않았는지 확인한다.
- `git status --short`에 `.meta`나 Unity 생성 파일의 신규 변경이 없는지 확인한다.
