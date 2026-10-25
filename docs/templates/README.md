# 에피소드 문서

동료와 초안을 맞춰 쓰고 공유할 때 쓰는 양식이다.  
정본·파이프라인: [`../bible/08_CONTENT_PIPELINE.md`](../bible/08_CONTENT_PIPELINE.md)

## 파일

| 파일 | 용도 |
|------|------|
| [`episode-one-pager.md`](episode-one-pager.md) | 짧은 요약 · 회의 안건 |
| [`episode-template.md`](episode-template.md) | 본편 초안 (상황·인물·흐름·엔딩) |

작성본은 [`../episodes/ep_{id}.md`](../episodes/)에 둔다.  
참고 예시: [`../episodes/ep_family_certificate.md`](../episodes/ep_family_certificate.md)

## 상태

`초안` → `리뷰` → `확정` → `구현` → `플레이 가능`

## 공통으로 맞출 것

- 사안은 작아도 되고, 실패 시 플레이어 비용(전환·팀장·체면)은 분명해야 한다
- **침묵·끼어들기**가 결과에 영향을 주는 구간이 있어야 한다
- 상대의 “건드리면 파탄” 선택지는 함정으로 둔다 (보상처럼 보이지 않게)
- 성공해도 팀장 칭찬은 기본값으로 두지 않는다
- 대사 전문은 Loc JSON; md에는 비트·플래그·키만
- **비트마다** `BackgroundId`와 매체(대면/메신저/독백)를 명시한다
