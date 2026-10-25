# GATE

task_id: `20261025-ep-opening-d30`

| 칸 | 결과 (pass / fail / n/a) | 증거·메모 |
|----|--------------------------|-----------|
| 테스트 | n/a | 문서만 · Playtest 없음 |
| 린트/키 | n/a | Loc 미작성 (`op.*` 자리만) |
| 화면 | n/a | Unity 미변경 |
| 문서 | pass | `ep_opening` ↔ `04_STORY` ↔ WORLD 배경 ID ↔ CHARACTER `park`/`intern`/`minjae`(사안 예고만). 템플릿에 비트별 BackgroundId 규칙 반영. `ep_family_certificate`는 연결 한 줄만. |
| 보고 | pass | 변경 요약: 아래 |

## 보고 요약

- 신규: `docs/episodes/ep_opening.md` (D-30, T1=`park` 튜토리얼, S3=온보딩 목록 확인 회신 예고)
- 갱신: `04_STORY.md` 발단·목록 · `episodes/README.md` · `ep_family_certificate.md` 연결 메모 · `templates/episode-template.md` · one-pager · templates README
- 사람 확정: 상대 T1 / 사안 S3 / ID `ep_opening`
- 남은 리스크: Loc·C# 미구현. 튜토리얼 조건식·시한 시각은 오픈. T1+S3라 민지는 사안 대상·직접 교섭 아님(문서에 명시).

## 반려 (리뷰·리더 작성)

- (없음) 2026-10-25 review: 검수 통과. inbox `20261025-1448-review-to-leader-ep-opening-d30-pass.md`
