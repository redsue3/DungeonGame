# 정리 체크리스트 (게임 완성 시)

> 이 파일 자체도 체크리스트다 — 아래 항목을 전부 실행하고 나면 **이 파일도 같이 지운다.**
> 트리거: 사용자가 "완성됐다", "이제 정리해라" 등으로 명시적으로 말했을 때만 실행한다. 자동으로 감지해서 지우는 장치는 없다 (게임 "완성"은 코드로 판별할 수 있는 상태가 아니라서).

## 1. 저장소 안의 문서
- [ ] `devlog.md` 삭제 — 파일 자체에 적힌 안내 (`> **게임이 완성됐으면 이 파일 삭제해라.**`, 맨 끝 줄).
- [ ] 이 파일(`CLEANUP.md`) 삭제.

## 2. 로컬 자동화 (이 머신, `~/.claude/dungeongame-tools/`)
2026-09-03에 추가한 자동 컴파일 체크 — repo 밖에 있어서 git으로는 안 지워지니 별도로 처리 필요.
- [ ] cron job 삭제: `CronDelete`로 job id 확인 후 제거 (`CronList`로 현재 id 조회 — 세션이 이미 끝나 있으면 7일 내 자동 만료되거나 이미 사라져 있을 수 있음, 그럼 이 항목은 스킵).
- [ ] `~/.claude/dungeongame-tools/` 폴더 전체 삭제 (`check_compile.sh`, `last_signature.txt`, `last_compile.log`).

## 3. Git 정리 (선택, 병합 완료 후)
- [ ] PR #1~#4 브랜치 삭제 (`fix/diagonal-move-gameover-label`, `docs/worldbook-and-bestiary`, `feature/monster-roster-and-map-colors`, `integration-test`) — 전부 master에 병합된 뒤.
- [ ] `checkpoint/before-pr-merge-20260903` 백업 브랜치 — 병합 결과가 안정적으로 확인되면 삭제해도 됨 (보험용이라 서두를 필요는 없음).

## 참고
- 위 1번은 게임 자체를 마무리할 때, 2번은 이 저장소에서 더 이상 Claude와 같이 개발 안 할 때, 3번은 병합이 실제로 끝난 다음이라 셋이 꼭 같은 시점일 필요는 없다. "완성됐다"고 말할 때 전부 한 번에 점검하면 됨.
