# Agent Team (Cursor용)

클로드 코드 “폴더=역할 세션” 패턴을 Cursor에 맞게 줄인 것.  
세션 간 **메시지 API는 없고**, 배정·보고는 **파일(inbox)** 로 한다.

**구조·GATE·역할·도식** → [`GUIDE.md`](GUIDE.md)  
**작업 끝난 뒤 다음 명령** → [`GUIDE.md` §6](GUIDE.md#6-작업이-끝나면--다음에-누구에게-무슨-명령) (표 + 복붙)

정본 설정: [`../bible/`](../bible/)  
운영 규칙 요약: [`HANDBOOK.md`](HANDBOOK.md) · 진행 이력: [`HISTORY.md`](HISTORY.md) · 교훈: [`MEMORY.md`](MEMORY.md)

---

## 역할

| 역할 | 폴더 | Cursor에서 |
|------|------|------------|
| **리더** | [`roles/leader/`](roles/leader/) | 새 Agent 채팅 → `@docs/team/roles/leader/ROLE.md` 또는 “리더로 동작” |
| **워커** | [`roles/worker/`](roles/worker/) | 배정받은 과제 폴더의 `INTENT.md`를 연 채팅 |
| **리뷰** | [`roles/review/`](roles/review/) | 검수 전용 채팅 |
| **독스** | [`roles/docs/`](roles/docs/) | 바이블·에피소드 md만 |
| **디자인** | [`roles/design/`](roles/design/) | 초상·배경·PRESENTATION (코드 최소) |

---

## 당신(사람)이 하는 법

1. **리더 채팅**을 하나 연다. 첫 메시지 예:
   ```text
   리더로 동작해. docs/team/roles/leader/ROLE.md 따라.
   과제: (원하는 일)
   ```
2. 리더가 **방향 선택지**를 주면 하나를 고른다. (설계 OK 전 코딩 금지)
3. 리더가 `docs/team/inbox/` 또는 `roles/worker/queue/`에 배정 파일을 쓴다.
4. **워커 채팅**을 새로 열고:
   ```text
   워커로 동작. 이 INTENT 수행:
   @docs/team/roles/worker/queue/<과제폴더>/INTENT.md
   ```
5. 워커가 설계안을 내면 **당신이 OK**한 뒤에만 구현.
6. 끝나면 워커가 **GATE.md(관문 표)를 채운다** = 자기점검 성적표 제출. 그다음 **리뷰** 채팅. (자세한 뜻 → [`GUIDE.md`](GUIDE.md) §3)
7. **리뷰 통과 → 다음 명령은 리더** (워커에게 바로 X). 리뷰 채팅이 복붙문을 안 줬으면 [`GUIDE.md` §6.2 D](GUIDE.md#62-상황별-복붙-명령) 사용.
8. 같은 지적이 두 번이면 리더가 [`MEMORY.md`](MEMORY.md) 상단에 한 줄 → 반복되면 [`HANDBOOK.md`](HANDBOOK.md)로 승격.

### 한 채팅만 쓸 때 (가볍게)

```text
팀 모드: 지금은 리더. 코딩하지 말고 배정안·선택지만.
```

이후 “워커로 전환, INTENT 대로 구현”처럼 **같은 채팅에서 역할만 바꿔도** 된다.  
병렬이 필요하면 Agents Window / 채팅을 여러 개 + worktree(코드 충돌 위험 시).

---

## 이 프로젝트 제약 (리더·워커 공통)

- Unity 플레이·MCP 스크린샷·양쪽 `Assets` 동기화가 필요한 일은 **로컬 한 세션** 권장
- 문서 / Loc 초안 / 에피소드 비트는 병렬·별도 채팅에 적합
- 미결정(회사명·엔딩 수 등) 임의 확정 금지 — [`../project-context.md`](../project-context.md)

---

## 파일 흐름

```text
명령 → 리더
      → roles/worker/queue/<id>/INTENT.md  (+ inbox 알림)
      → 워커 채팅이 INTENT 수행
      → GATE.md 관문 표
      → 리뷰 / 리더 재확인
      → (실패 시) 반려 한 줄 → 워커 수정
```
