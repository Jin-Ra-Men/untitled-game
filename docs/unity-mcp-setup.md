# Unity ↔ Cursor MCP 연결

[MCP for Unity](https://github.com/CoplayDev/unity-mcp) (오픈소스)로 에디터와 Cursor를 연결한다.

## 이미 해 둔 것

- Unity 프로젝트(`untitled-game/untitled-game`)에 패키지 `com.coplaydev.unity-mcp` 추가
- Cursor 프로젝트 설정: [`.cursor/mcp.json`](../.cursor/mcp.json)
- Windows용 `uv` / `uvx` 설치

## 사용자가 할 일 (한 번)

1. **Unity**에서 프로젝트 다시 포커스 / 재실행 → 패키지 임포트 끝날 때까지 대기
2. 메뉴 **Window → MCP for Unity**
3. Status가 연결 대기/실행 중인지 확인. 필요하면 Start / Auto-Configure
4. Cursor에서 **Settings → MCP** 로 이동해 `unityMCP`가 켜져 있는지 확인 (필요 시 Toggle ON)
5. Cursor 창을 **한 번 재시작**하면 MCP 도구가 잡히는 경우가 많음

## 확인

채팅에서 예를 들어:

> Unity 콘솔 최근 에러/경고 요약해줘

연결되면 씬/콘솔 도구를 에이전트가 호출할 수 있다.

## 참고

- Hub가 연 실제 프로젝트 경로: `C:\untitled-game\untitled-game`
- 깃 워크스페이스 루트(`C:\untitled-game`)의 Assets와는 별도이니, 스크립트 수정 후 싱크가 필요하면 말해 주세요.
