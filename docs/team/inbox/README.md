# inbox

리더가 꺼져 있거나, 채팅이 갈라져 있을 때 **배정·반려·완료**를 남기는 곳.

## 파일 이름

```text
YYYYMMDD-HHMM-<from>-to-<role>-<slug>.md
```

예: `20261025-1410-leader-to-worker-ep-flags.md`

## 최소 본문

```markdown
# 배정 / 반려 / 완료

- from: leader | review | worker | human
- to: worker | review | leader | docs | design
- task_dir: docs/team/roles/worker/queue/<id>/   (있으면)
- summary: 한 줄
- action: 읽어라 / 수정해라 / 검수해라

(상세는 INTENT·GATE 경로로)
```
