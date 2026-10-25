# 프로젝트 컨텍스트 (미제) — 인덱스

PC용 신작. 작업명 **미제**(untitled). 사용자: 진 이.

**설정·톤·인물·시스템·연출의 정본은 `docs/bible/` 이다.**  
이 파일은 인덱스·레퍼런스 원작 메타·오픈 퀘스천만 유지한다.

---

## 바이블 (정본)

| # | 문서 | 내용 |
|---|------|------|
| 01 | [`bible/01_GAME_BIBLE.md`](bible/01_GAME_BIBLE.md) | 한 줄 정의·기둥·확정·금지 |
| 02 | [`bible/02_CHARACTER_BIBLE.md`](bible/02_CHARACTER_BIBLE.md) | 인물·말투·역린·초상 ID |
| 03 | [`bible/03_WORLD_BIBLE.md`](bible/03_WORLD_BIBLE.md) | 조직·장소·D-30·배경 ID |
| 04 | [`bible/04_STORY.md`](bible/04_STORY.md) | 아크 비트·에피소드 목록 |
| 05 | [`bible/05_GAME_SYSTEM.md`](bible/05_GAME_SYSTEM.md) | 입력·플래그·게이지·아웃컴 |
| 06 | [`bible/06_ASSETS.md`](bible/06_ASSETS.md) | 초상·배경·SFX 목록·규칙 |
| 07 | [`bible/07_PRESENTATION.md`](bible/07_PRESENTATION.md) | 분할·메신저·선택지 연출 |
| 08 | [`bible/08_CONTENT_PIPELINE.md`](bible/08_CONTENT_PIPELINE.md) | Loc·작성 순서·동기화 |

에피소드 문서: [`episodes/`](episodes/)  
에이전트 팀(리더→배정→워커): [`team/README.md`](team/README.md) · 도식 [`team/GUIDE.md`](team/GUIDE.md) · 진행 이력 [`team/HISTORY.md`](team/HISTORY.md)  
긴장 레시피: [`reference-tension-from-lunchbox.md`](reference-tension-from-lunchbox.md)  
프로토타입 실행: [`dev-prototype.md`](dev-prototype.md)

---

## 레퍼런스 작품 (메타)

『총성과 다이아몬드』(2009, SCE/PSP) — **콘셉트 영감만**. IP 리메이크 아님.

| 항목 | 내용 |
|------|------|
| 한국어 타이틀 | 총성과 다이아몬드 |
| 일본어 타이틀 | 銃声とダイヤモンド |
| 영문 표기 | Diamond and The Sound of a Gun Shot (별칭: Juusei to Diamond) |
| 플랫폼 | PSP |
| 일본 발매 | 2009-06-18 (SCE) |
| 한국 발매 | 2009-07-17 한글화 (SCEK) |
| 개발 | Zener Works; SCE Japan Studio 관여 |
| 장르 | 교섭(네고시에이션) 어드벤처 노벨 |

플레이 레퍼런스: [보너스 「도시락 데우기」](https://youtu.be/CpusF__84SU) → [`reference-tension-from-lunchbox.md`](reference-tension-from-lunchbox.md)

원작에서 가져올 것 / 가져오지 말 것 → [`bible/01_GAME_BIBLE.md`](bible/01_GAME_BIBLE.md)

---

## 미결정 (오픈 — 임의 확정 금지)

- **톤·수위 디테일**: 독성·괴롭힘·퀴어 캐릭터 표현의 구체 수위, 유머 vs 암울, 대상 연령대
- **에피소드 수·배치**: 5~6시간 안 단편 개수·순서
- **멀티엔딩**: 전환 성공/실패/회색 비중·분기 깊이
- **회사 디테일**: 사명·출시작·규모 수치
- **캐릭터명**: 사수·개발꼰대·동기·크레이지 A/B 등 미채번 롤 ([`bible/02_CHARACTER_BIBLE.md`](bible/02_CHARACTER_BIBLE.md))
- **싱글/온라인**: 싱글 전제; 멀티는 논의된 바 없음

이미 확정(장르·타깃·핵심 재미·서울·D-30·플레이타임·최종 목표)은 바이블에만 두고, 여기 오픈과 모순되면 바이블을 고친다.

---

## 현재 프로토타입

- 테스트 에피소드: **가족관계증명서** (`ep_family_certificate`)  
  문서 → [`episodes/ep_family_certificate.md`](episodes/ep_family_certificate.md)  
  코드 → `Assets/Scripts/Negotiation/` · Playtest · Unity `NegotiationBootstrap`
