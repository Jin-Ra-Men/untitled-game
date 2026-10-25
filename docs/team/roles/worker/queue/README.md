# worker queue

리더가 과제마다 하위 폴더를 만든다.

```text
queue/
  <task-id>/
    INTENT.md
    GATE.md
```

`task-id` 예: `20261025-ep-cast-slots`, `fix-messenger-scale`

끝난 과제는 `_done/`으로 옮기거나 폴더명에 `-done`을 붙인다 (선택).
