# 미제 (untitled)

PC용 오리지널 게임. 작업명은 당분간 **미제**.

『총성과 다이아몬드』(2009, SCE/PSP)의 **콘셉트**(실시간 교섭·프로파일링·에피소드 서사·긴장 톤)에서 영감을 받되, IP 리메이크·클론이 아니다.  
설정은 **꼰대·독성 조직문화의 영세 게임사**에서 **인사팀(HR) 인턴**이 **정규직 전환 D-30**을 버티는 직장 스릴러(고통의 코미디)다.

## 기술

| 항목 | 결정 |
|------|------|
| 플랫폼 | PC |
| 엔진 | Unity (2D, C#) |
| 표현 | 2D |
| 플레이 | 싱글 플레이 전제 |

## 시그니처 루프 (초안)

- **실시간 교섭** — 면접·면담·팀장 달래기·평가 자리에서 타이밍·침묵·끼어들기가 결과 좌우
- **프로파일링** — 팀장·개발·기획·경영진의 성향·파벌·꼰대 트리거를 파악해 대응에 반영
- **에피소드형 서사** — HR 위기 단편이 정규직 전환 D-30으로 수렴
- **스테이크 미스매치** — 사안은 서류·도장처럼 작아도, 시스템·플레이어 비용은 인질극급으로 진지 (고통의 코미디)

## 프로토타입 실행

가족관계증명서 교섭 루프가 코드로 들어가 있다. 상세 → [`docs/dev-prototype.md`](docs/dev-prototype.md)

```bash
dotnet run --project tools/Playtest
```

조작: `1~4` 선택 · `S` 침묵 · `I` 끼어들기 · 대기 시 침묵 자동 · `Q` 종료.

Unity 2022.3 LTS로 이 저장소 루트를 열면 `NegotiationBootstrap`이 플레이 모드 UI를 띄운다.

## 문서

- [`docs/project-context.md`](docs/project-context.md) — 확정 결정·오픈 퀘스천·레퍼런스
- [`docs/reference-tension-from-lunchbox.md`](docs/reference-tension-from-lunchbox.md) — 「도시락 데우기」 플레이 레퍼런스 → 긴장 레시피
- [`docs/templates/`](docs/templates/) — **에피소드 기획 템플릿** (본편 초안 + 회의용 원페이지)
- [`docs/episodes/`](docs/episodes/) — 채워 넣은 에피소드 초안 모음
- [`docs/test-episode-family-certificate.md`](docs/test-episode-family-certificate.md) — 테스트 에피소드(가족관계증명서) 초안
- [`docs/dev-prototype.md`](docs/dev-prototype.md) — 프로토타입 실행·폴더 구조

## 현재 상태

테스트 에피소드(가족관계증명서) 교섭 Core + 콘솔 플레이테스트 가동. Unity 에디터 연결은 Hub에서 프로젝트 오픈 후 진행.
