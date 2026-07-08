# 게임 개발 로그

나랑 Claude랑 대화하면서 결정하고 만든 것들 기록.

> **저장소 안내 (2026-07-07 정리)**: 원래 스크립트는 `makingGame` 저장소, Unity 프로젝트는 `DungeonGame` 저장소로 분리할 계획이었으나(아래 2026-07-01 항목 참고), 실제로는 두 로컬 폴더(`~/makingGame`, `~/DungeonGame`) 모두 `github.com/redsue3/makingGame.git` 하나의 원격 저장소를 보고 있었음. 스크립트 내용은 완전히 동일했고, `~/makingGame`에만 있던 NovelAI 프롬프트 4개(`Animation/*/​*_prompts.md`)와 이 로그(`devlog.md`)만 이쪽(`~/DungeonGame`)엔 없었던 상태 → 전부 이 파일과 `Assets/Scripts/Animation/*/`로 합침. 이제부터는 **`DungeonGame` 폴더 하나만 진짜 작업 디렉터리**로 취급한다 (Unity 프로젝트 전체가 여기 있어야 실제로 플레이 가능하므로).

---

## 현재 구조 (2026-06-29 업데이트)

### 캐릭터 4종
| 직업 | HP | 마나 | 패 | 공격보너스 |
|------|-----|------|-----|-----------|
| 전사 | 80  | 3    | 5장 | +2        |
| 도적 | 65  | 3    | 6장 | +0        |
| 마법사 | 55 | 4   | 5장 | +0        |
| 성기사 | 70 | 3   | 5장 | +1        |

### 코어 시스템
- `Card.cs` — id, 타입, 마나비용, damage/block/draw/heal/strength/poison/burn/selfDamage
- `CardDatabase.cs` — 전체 카드 28종 정의 (스타터 14장 + 드롭 14장)
- `Deck.cs` — 드로우/버리기/패/전체카드, ResetForBattle(), attackBonus 전달
- `Character.cs` — HP/방어막/상태이상(독/화상) 공통 처리
- `PlayerCharacter.cs` — 4직업, deck/relics 소유, PlayerDatabase에서 스탯 로드
- `Enemy.cs` — 행동 패턴, OnTurnStart(상태이상), ExecuteAction(attackPower 적용)
- `BattleManager.cs` — 턴제 전투, UseCard/EndPlayerTurn UI 연결 포인트
- `LayerManager.cs` — 계층/스테이지 관리 (1=일반, 2=일반/엘리트, 3=휴식/상점, 4=일반/엘리트, 5=보스)
- `DungeonManager.cs` — 게임 전체 흐름 (CharacterSelect→DungeonMap→Battle→Reward→...)
- `SaveSystem.cs` — JSON 세이브/로드/복원

### 데이터
- `PlayerDatabase.cs` — 직업별 스탯 + 스타터덱 ID
- `EnemyDatabase.cs` — 계층별 몬스터 9종 (일반2+엘리트2+보스1 × 2계층)
- `LootTable.cs` — 계층별 드롭 카드 풀 (1계층 7종, 2계층 7종)

### Factory
- `PlayerFactory.cs` — 직업 선택 → PlayerCharacter 생성 + 덱 빌드 (CardDatabase 사용)
- `EnemyFactory.cs` — EnemyDatabase → Enemy 인스턴스 생성

### 애니메이션
- Warrior / Rogue / Mage / (Paladin 추가 예정) Animator

---

## 수정/개선 내역

### 2026-06-29 대규모 리팩토링
**버그 수정:**
- `BattleManager.UseCard()` 데미지 이중 적용 제거 (Deck.PlayCard로 일원화)
- `card.damage = hand.Count` 버그 제거
- `enemy.rewardGold` → `enemy.RollGoldReward()` 수정
- `LayerManager.GetBossId()` Dictionary 인덱싱 버그 수정
- `Enemy.ExecuteAction()` Buff 후 attackPower 미적용 수정

**구조 개선:**
- `Character` 기반에 poisonStack/burnStack 추가 (적도 상태이상 받음)
- `PlayerCharacter`가 Deck/RelicInventory 소유 (런간 덱 유지)
- `PlayerCharacter` 스탯을 PlayerDatabase에서 로드 (하드코딩 제거)
- `PlayerFactory`가 CardDatabase로 실제 덱 빌드

**신규:**
- `CardDatabase.cs` — 카드 28종 전체 정의
- `DungeonManager.cs` — 게임 흐름 관리 (GameState enum)
- `SaveSystem.cs` — JSON 세이브/로드
- `RelicData.cs`, `RelicInventory.cs` — 유물 시스템 기반
- 4번째 직업: **성기사** (holy_strike/shield_bash/sacred_heal/judgement)
- LayerManager 스테이지 3 → 휴식/상점 로직 추가

---

## 이미지 작업 전 남은 것

- [x] Paladin 애니메이션 파일 추가 (`Animation/Paladin/PaladinAnimator.cs`)
- [x] UI 스크립트 (`System/UIManager.cs` — GameState별 패널, Inspector에서 연결)
- [x] 상점 시스템 (`System/ShopSystem.cs` + DungeonManager.BuyShopItem/RemoveCardFromDeck)
- [x] 유물 효과 구현 (`RelicData.cs` 재설계 + `Database/RelicDatabase.cs` 유물 10종)
- [x] 3계층 몬스터/보스 추가 (저주받은 전사/그림자 마녀/죽음의 기사/마왕)

### 2026-06-29 버그 수정 (Claude)
- `MapGenerator.PickNormal/PickElite/GetBossId` — 3계층에서 2계층 몬스터/보스가 등장하던 버그 수정
- `DungeonManager.OnBattleWon` — Victory 조건 `currentLayer >= 2` → `>= 3` 수정 (2계층 보스 처치 시 조기 엔딩 버그)
- `SaveSystem.Save` — LayerManager 의존 제거, `int layer` 직접 받는 오버로드로 변경
- `DungeonManager` — 카드 보상 선택/스킵, 휴식, 상점 이탈 시 자동 세이브 연결

## 유물 10종 (RelicDatabase)
Common: 철심(HP+15), 전투 목걸이(전투시 힘+1), 수호자의 인장(전투시 방어막+8), 뱀 어금니(패+1)
Uncommon: 전쟁의 뿔피리(턴마다 마나+1), 혈약 반지(처치시 HP+3)
Rare: 고대 서적(턴마다 드로우+1), 황금 우상(골드+25%), 불사조의 재(HP30% 이하시 방어막+10)
Boss: 마왕의 왕관(최대마나+1 + 전투시 힘+2)

## 3계층 (2026-06-29 추가)
카드 7종: 영혼 베기/완벽한 가드/역병 일격/영혼 흡수/혼돈 화염/뇌우/축복
몬스터: 저주받은 전사(HP62)/그림자 마녀(HP48)/죽음의 기사 엘리트(HP100)/마왕 보스(HP200)

---

## UI 스크립트 (2026-06-30 추가)

`System/UI/` 폴더에 패널별 컨트롤러 생성. 모두 MonoBehaviour, Inspector에서 프리팹/컴포넌트 연결 필요.

| 스크립트 | 역할 |
|---|---|
| `CharacterSelectUI` | 직업 4종 선택, 스탯/스타터덱 미리보기 |
| `DungeonMapUI` | 9×7 그리드 맵, 인접 타일 이동 버튼 |
| `BattleUI` | HP/마나/패/적 의도 표시, 카드 사용, 턴 종료 |
| `CardUI` | 카드 프리팹용 (이름/마나/설명/타입색) |
| `EnemyPanelUI` | 적 HP바/의도/상태이상/타겟 강조 |
| `RewardUI` | 카드 보상 2~3장 선택 |
| `RestUI` | 휴식 (HP 30% 회복 미리보기) |
| `ShopUI` + `ShopItemUI` | 카드/유물/제거서비스 구매, 카드 제거 패널 |
| `GameOverUI` / `VictoryUI` | 결과 요약 + 재시작/타이틀 버튼 |

### Unity Inspector 연결 가이드
- `UIManager` — 8개 패널 GameObject 연결
- `BattleUI` — battlePanel 하위에 배치, `BattleManager.NotifyUI()`로 자동 갱신
- `DungeonMapUI` — tilePrefab: `Button + Image + TextMeshProUGUI` 구성
- `ShopUI` — shopItemPrefab: `ShopItemUI + Button` 구성, removeCardEntryPrefab: `CardUI + Button`
- 카드/적 프리팹은 prefab으로 만들어 각 UI의 cardPrefab/enemyPanelPrefab 슬롯에 연결

---

## Unity 프로젝트 세팅 (2026-07-01 추가, `작업로그.txt`에서 통합)

**makingGame (스크립트 저장소)** — `github.com/redsue3/makingGame.git`
- `Animation/Paladin/paladin_prompts.md` 추가 — Idle(4f) / Move(6f) / Attack(4f) / Special(심판 6f), NovelAI Diffusion Anime V3 / 64x64 chibi 스타일
- `System/UI/CharacterSelectUI.cs` 버그 수정 — `data.starterCardIds` → `data.starterDeckCardIds`

**DungeonGame (Unity 프로젝트)**
- Unity 6000.5.1f1 / 2D 모드로 프로젝트 생성
- `Assets/Scripts`: 게임 스크립트 43개 전부 복사
- `Assets/Scenes/GameScene.unity`: 씬 자동 세팅 완료 — Managers(DungeonManager/BattleManager/LayerManager), Canvas + 패널 8개(CharacterSelect/DungeonMap/Battle/Reward/Rest/Shop/GameOver/Victory), EventSystem, 메인 카메라(직교, size=5)
- `Assets/Scripts/Editor/SceneSetup.cs`: 씬 빌드 자동화 스크립트
- 폴더 구조: Sprites / Prefabs / Audio / Fonts

**당시 다음 할 일 (전부 이후 항목에서 완료됨)**
- TMP Essential Resources 임포트 → 완료했으나 배치모드 임포트가 반쪽만 성공해서 2026-07-07에 문제로 드러남 (아래 참고)
- NovelAI로 캐릭터 스프라이트 생성 → Sprites/Characters, Sprites/Enemies, Sprites/Cards, Sprites/UI 전부 채워짐
- 각 UI 패널 Inspector 연결 → 2026-07-04 `SceneSetup.cs` 개선으로 코드 자동 연결로 대체됨
- Play 버튼으로 게임 실행 테스트 → 2026-07-07 항목 참고

---

## 배고픔/인벤토리 시스템 + 씬 자동화 (2026-07-04 추가)

**배고픔 시스템 (`System/HungerSystem.cs`)**
- 맵에서 3칸 이동할 때마다 배고픔 -6 (`PlayerCharacter.stepsSinceMeal` 로 카운트)
- 배고픔이 0인 상태에서 계속 이동하면 이동틱마다 HP -5 (기아 페널티) — `DungeonManager.MovePlayer` 에서 사망 체크 후 GameOver 전환
- 전투 시작 시 배고픔 -10 (`BattleManager.StartBattle`)
- HP가 가득 차지 않은 상태에서 회복(휴식/카드/유물 전부 포함)하면 회복량에 비례해 배고픔도 소모 — `Character.Heal` 을 virtual 로 바꾸고 `PlayerCharacter.Heal` 에서 override, 만땅 상태에서 회복하면 배고픔 안 깎임

**인벤토리/식료품 (`Inventory.cs`, `Data/FoodData.cs`, `Database/FoodDatabase.cs`)**
- 식료품 6종: 사과/빵/치즈/육포/비상식량/진수성찬 (배고픔 회복량·가격 다름)
- 전투 승리 시 골드와 별개로 50% 확률로 식료품 드롭 (`LootTable.RollFoodDrop`) — 카드 보상만 계속 나오는 게 지루하다는 피드백 반영
- 상점에서도 식료품 2종 구매 가능 (`ShopSystem`, `ShopItemType.Food`)
- 신규 캐릭터는 빵+사과 1개씩 들고 시작 (`PlayerFactory`)
- `DungeonManager.UseFoodItem(id)` 로 언제든 섭취 → 배고픔 회복
- 세이브/로드에 배고픔·인벤토리 반영 (`SaveSystem`)

**UI (`System/UI/InventoryUI.cs`, `FoodItemUI.cs`)**
- 던전맵 화면에 배고픔 게이지 + "인벤토리" 버튼 추가 → 오버레이로 식료품 목록/섭취
- 보상 화면에 식료품 획득 표시, 휴식 화면에 회복 시 배고픔 소모량 미리보기 표시

**씬 자동 세팅 대폭 개선 (`Editor/SceneSetup.cs`)**
- 기존엔 패널 껍데기(스크립트 컴포넌트만 달린 빈 GameObject)만 만들어서 Inspector에서 버튼/텍스트/슬라이더를 전부 수동으로 만들어 연결해야 했음 → 그게 안 되어 있어서 지금까지 유니티에서 실제로 플레이가 안 됐던 원인
- 이제 버튼/텍스트/슬라이더/그리드까지 전부 코드로 생성하고, Card/EnemyPanel/ShopItem/MapTile/FoodItem 프리팹을 `Assets/Prefabs/UI/` 에 실제로 저장해서 각 UI 스크립트의 SerializeField 를 전부 자동 연결함
- `DungeonGame/` 프로젝트에서 유니티 메뉴 `DungeonGame > 씬 자동 세팅` 한 번만 실행하면 바로 Play 가능한 상태가 됨 (Unity 6000.5.1f1 배치모드로 컴파일 오류 없음 + 씬 생성 검증 완료)
- TMP Essential Resources 미임포트 시 자동 임포트 시도 (실패하면 글자가 안 보일 수 있으니 Window > TextMeshPro > Import TMP Essential Resources 수동 실행 안내)

## 유물 드롭 제한 + 덱 조작형 유물 3종 (2026-07-05 추가)

**유물 획득 경로 제한**
- 기존엔 유물이 상점에서만 나왔음 (일반 전투 보상엔 유물이 아예 없었음)
- 이제 엘리트/보스 전투 승리 시에도 유물을 얻을 수 있음 — `LootTable.RollRelicDrop(isBoss, player)`
  - 엘리트: 70% 확률, Boss 등급 유물은 제외
  - 보스: 100% 확정, Boss 등급 유물 포함
  - 이미 보유한 유물은 후보에서 제외 (상점의 `RollRandomRelic`과 동일한 방식)
- 일반 전투는 여전히 유물을 드롭하지 않음 (카드/골드/식료품만)
- `BattleReward.relicId` 추가, `RewardUI`에 유물 획득 텍스트 표시 (`relicRewardText`)

**덱 조작형 유물 3종 (`RelicEffectType`에 신규 타입 추가)**
- `RemoveRandomCard` — **정화의 부적** (Uncommon): 획득 시 덱에서 무작위 카드 1장 제거 (덱 압축)
- `TransformRandomCard` — **혼돈의 프리즘** (Rare): 획득 시 덱의 무작위 카드 1장을 현재 계층 카드 풀의 다른 카드로 교체
- 드로우 계열은 기존 `DrawCard` 타입 재사용, 새 트리거로 차별화 — **매의 눈** (Uncommon): 전투 시작 시 카드 2장 추가 드로우
- 제거/변환은 `RelicDatabase.ApplyPassiveEffects`에서 획득 시점에 1회 처리 (`RemoveRandomCardFromDeck`/`TransformRandomCardInDeck`)

**다음 할 일 (사용자 요청, 아직 미착수)**
- 카드 보상 풀을 일반/희귀/직업 전용 3종 등급으로 재설계 (`LootTable`/`CardDatabase` 구조 변경 필요)

## 카드 등급 시스템 1단계 — 기본(일반) 카드 10종 (2026-07-05 추가)

**데이터 모델**
- `Card`에 `CardRarity rarity` (Common/Rare)와 `CharacterClass? classRestriction` (null = 전 직업 공용) 필드 추가
- 기존 카드는 전부 기본값(Common, 공용)이라 동작 변화 없음

**기본 카드 10종 (`CardDatabase`, 전부 공용/Common)**
| id | 이름 | 비용 | 타입 | 효과 |
|---|---|---|---|---|
| quick_slash | 빠른 베기 | 0 | Attack | 데미지 3 |
| solid_strike | 묵직한 일격 | 2 | Attack | 데미지 11 |
| puncture | 관통 | 1 | Attack | 데미지 6, 독 1 |
| guard_up | 수비 태세 | 1 | Defense | 방어막 6 |
| steady_guard | 굳건한 수비 | 2 | Defense | 방어막 11 |
| counter_stance | 반격 태세 | 1 | Defense | 방어막 5, 드로우 1 |
| focus | 집중 | 1 | Skill | 드로우 2 |
| adrenaline | 아드레날린 | 1 | Skill | 힘 +1, 드로우 1 |
| second_wind | 재정비 | 1 | Skill | 회복 5 |
| toughen | 담금질 | 2 | Skill | 힘 +2 |

**주의: 아직 보상/상점 드롭 풀(`LootTable.cardPool`)에는 연결 안 함.** 희귀 카드·직업 전용 카드까지 다 만든 다음에 `LootTable`을 등급 기반 추첨 방식으로 한번에 다시 짤 예정 (지금 기존 계층별 풀에 그냥 섞어 넣으면 기존 밸런스가 깨짐).

## 마나 코스트 = 마법사 전용 매커닉으로 전환 (2026-07-05)

**방향**: 코스트 시스템 자체를 대부분 안 쓰는 쪽으로 가고, 마나는 일단 마법사만의 고유 매커닉으로 취급.

- `BattleManager`에 `UsesMana => player.characterClass == CharacterClass.Mage` 추가
  - `UseCard()`에서 마나 부족 체크/차감을 마법사일 때만 수행 — 전사/도적/성기사는 카드에 적힌 코스트와 무관하게 항상 사용 가능
  - `BattleUI.RefreshHand()`의 카드 버튼 `interactable` 판정도 동일하게 마법사만 코스트로 막히게 수정 (안 그러면 백엔드는 허용해도 UI에서 버튼이 계속 비활성화되는 모순 발생)
- 기본(일반) 카드 10종은 전부 코스트 0으로 변경 (전 직업 공용이니 마법사가 뽑아도 무료로 사용 가능)
- 기존 전사/도적/성기사 스타터 카드(strike/defend/dagger/holy_strike 등)의 코스트 숫자 자체는 그대로 뒀음 — 위 매커닉 변경으로 어차피 그 직업들한텐 소모되지 않으니 무해하지만, UI에 코스트 뱃지가 그대로 보이는 건 사소한 표시상 불일치로 남아있음 (필요하면 다음에 정리)
- 계층 드롭 풀 카드(공용, 마법사도 얻을 수 있음)는 코스트 그대로 유지 — 마법사의 마나 관리 정체성은 이 공용 풀 + 마법사 전용 카드에서 계속 의미 있게 작동함

## 캐릭터 성소(Shrine) 이벤트 — 카드 제작 (2026-07-05 추가)

**개념**: 맵 타일 중 하나가 성소(⛩)로, 방문하면 화면 전환 후 "{직업}의 성소 접촉 — 카드 제작" 타이틀과 함께 **공격/유틸/방어** 3분기가 뜬다. 하나를 고르면 그 자리에서 생성된 카드를 덱에 얻는다. 스토리/직업별 연출은 나중에, 지금은 매커닉만 구현.

**분기별 카드 생성 규칙** (`System/ShrineSystem.cs`, 방문할 때마다 새로 굴림)
- **공격** — 50/50으로 둘 중 하나:
  - *회심의 한방*: 코스트 2, 데미지 = 레벨 × (힘+1) × 랜덤(3~5)
  - *성장하는 검*: 코스트 0, 기본 데미지 2~3, 사용할 때마다 데미지 영구 +1~2 (`Card.growOnUse`)
- **유틸** — 50/50으로 둘 중 하나, 둘 다 코스트 0:
  - *집중*: 다음에 내는 공격 카드 데미지 +3~6 예약
  - *가호*: 다음에 내는 방어 카드 방어막 +3~6 예약
- **방어** — *대방벽*: 코스트 2, 방어막 = (레벨 + 최대체력) × 랜덤(0.15~0.25)

**"레벨" 관련 가정**: 이 게임엔 아직 캐릭터 레벨 시스템이 없어서, 공식의 "레벨"은 현재 던전 계층(`DungeonManager.CurrentLayer`)으로 대체했다. 나중에 진짜 레벨 시스템이 생기면 여기 교체 필요.

**새 카드 매커닉 (`Card.cs`)**: `growOnUse`(사용마다 영구 성장), `buffNextAttack`/`buffNextDefense`(다음 카드 예약 버프) 3개 필드 추가. 예약 버프는 `PlayerCharacter.pendingAttackBonus`/`pendingDefenseBonus`에 쌓였다가 실제로 공격/방어 카드를 낼 때 `Deck.ApplyCardEffect`에서 1회 소모된다.

**중요 — 세이브 포맷 변경**: 성소 카드는 `CardDatabase`에 등록되지 않은, 그때그때 스탯이 굴려지는 카드라 기존처럼 id만 저장하면 로드할 때 사라진다. 그래서 `SaveSystem`의 덱 저장 방식을 `string[] deckCardIds` → `CardSnapshot[] deckCards`(카드 전체 스탯 스냅샷)로 바꿨다. **이전에 만들어둔 save.json은 로드해도 덱이 빈 상태로 복원된다** (에러는 안 나지만 카드가 다 사라짐 — 테스트 중인 세이브 파일이 있다면 참고).

**미완성/후속 작업**
- 성소 타일 아이콘은 이모지(⛩)만 있음, 전용 아트 없음
- 직업별 스토리/대사 연출 없음 (요청대로 나중으로 미룸)
- 공격/유틸 분기의 "50/50 랜덤" 선택은 매번 방문 시 재확인 없이 바로 결정됨 — 원했던 게 "분기 안에서 또 고르기"였다면 UI 한 단계 더 추가해야 함

## TMP 기본 스프라이트 에셋 깨진 참조로 Play 크래시 (2026-07-07 수정)

**증상**: Unity 에디터에서 Play를 누르면 버튼/패널 레이아웃이 깨지고 콘솔에 `NullReferenceException`이 반복해서 찍힘 (한글 텍스트가 있는 Label마다 발생).

**원인**: 2026-07-01에 `SceneSetup.EnsureTMPEssentials()`가 `-batchmode`(화면 없는 자동화 모드)로 "TMP Essential Resources"를 임포트했는데, 그 임포트 창 자체가 그래픽 디바이스 없이는 못 뜨는 GUI 창이라 절반만 성공함. 그 결과 `Assets/TextMesh Pro/Resources/TMP Settings.asset`이 프로젝트에 실제로 존재하지 않는 `Default Sprite Asset`(guid `c41005c129ba4d66911b75229fd70b45`)을 계속 참조 → 텍스트 레이아웃을 다시 계산할 때마다(`CanvasUpdateRegistry.PerformUpdate` → `TMP_Text.SetArraySizes` → `MaterialReferenceManager`) 존재하지 않는 스프라이트 에셋을 참조하려다 NullReferenceException.

**수정**: `TMP Settings.asset`에서 `m_defaultSpriteAsset` 참조 제거(null) + `m_enableEmojiSupport: 0` (이 프로젝트는 텍스트에 `<sprite>` 태그를 전혀 안 씀). 실제 게임 캐릭터/카드/적 이미지와는 무관한 문제였음 — 그쪽 스프라이트 61개는 이미 다 정상 존재하고,애초에 UI 코드가 그 이미지들을 참조하지도 않음(색깔 사각형 + TMP 텍스트로만 구성됨).

**검증**: Unity 배치모드로 `EnterPlayModeOptions.DisableDomainReload`를 걸고 Play를 120프레임 실제로 돌려서 에러 0건 확인 (임시 스모크 테스트 스크립트는 확인 후 삭제).

**남은 문제**: `LiberationSans SDF` 폰트가 한글을 지원하지 않아 한글 라벨이 □로 보임 (치명적이진 않음, 텍스트만 안 보임). 한글 지원 폰트를 TMP 폴백으로 추가하면 해결 — 아직 미착수.

## 던전맵 9x7 그리드 폐기 → 분기형(슬레이 더 스파이어 스타일) 맵으로 전면 교체 (2026-07-07)

**증상 (사용자 리포트)**: "클릭을 해도 안 움직이고 캐릭터가 어떻게 움직이는지 확인이 안 됨."

**원인**: 기존 `MapGenerator`는 9×7=63칸짜리 네모 그리드에 조우/휴식/상점 등 실제 콘텐츠가 있는 칸을 12개만 무작위로 흩뿌리고 나머지 51칸은 `TileType.Empty`로 남겨뒀음. `DungeonMapUI`는 플레이어 인접 8칸 중 `Empty`가 아닌 칸만 버튼을 활성화했는데, 인접 8칸이 우연히 대부분/전부 Empty인 경우가 많아서 "클릭해도 반응 없는 칸"이 태반이었고, 어느 칸이 실제로 갈 수 있는 칸인지 시각적으로도 구분이 잘 안 됐음.

**해결**: 그리드 기반을 완전히 버리고, 슬레이 더 스파이어류 로그라이크 카드게임에서 쓰는 **층(floor) + 분기 경로(DAG)** 방식으로 맵 생성기를 새로 짬.

- `MapNode.cs` (신규) — 방 하나 (`floor`, `x`(층 내 가로위치 0~1), `type`, `next`(다음 층 노드 id 리스트)). 예전 `MapTile` 클래스를 대체 (`MapTile.cs`는 `TileType` enum만 남김).
- `DungeonMap.cs` — 2D 배열 대신 `List<MapNode>` + 간선 그래프. `PlayerX/Y` 대신 `CurrentNodeId`(-1 = 입장 전, 0층 선택 대기). `ReachableNodes()`가 지금 위치에서 실제로 갈 수 있는 노드만 반환.
- `MapGenerator.cs` — 보스층 제외 5개 인코운터 층(방 개수 2/3/3/2/1)에 노드를 만들고:
  1. 랜덤 워크 경로 6개를 그어 층마다 서로 연결 (인접 슬롯 -1/0/+1 로만 이동해서 선이 너무 안 꼬이게)
  2. 들어오는/나가는 경로가 없는 고립 노드를 가까운 슬롯끼리 강제 연결해서 보정
  3. 마지막 인코운터 층은 전부 보스로 연결
  4. 방 타입은 예전과 동일한 비율(일반4/집단2/엘리트1/휴식2/상점1/성소1)을 유지하되, 0층엔 전투만, 보스 직전 층엔 휴식을 보장 배치 (스파이어 룰 그대로)
- `DungeonManager.cs` — `MovePlayer(dx,dy)` → `MoveToNode(nodeId)`로 교체. `currentBattleTile`(MapTile) → `currentBattleNode`(MapNode).
- `DungeonMapUI.cs` — `GridLayoutGroup` 기반 정렬을 버리고 노드의 `floor`/`x`를 화면 좌표로 직접 환산해서 자유 배치. 노드끼리 실선으로 연결선을 그리고(지금 위치에서 나가는 선만 밝은 노란색, 나머지는 옅게), 지금 위치에서 갈 수 있는 노드만 원래 색으로 밝게 표시 + 클릭 가능, 나머지는 어둡게 표시해서 클릭해봐야 반응 없는 칸이 원천적으로 없어짐. 입장 전엔 화면 아래에 "시작" 지점을 표시하고 0층 노드들로 선을 그어서 어디서부터 시작하는지도 보이게 함.
- `SceneSetup.cs` — 던전맵 패널의 `GridArea`에서 `GridLayoutGroup` 제거 (자유 배치라 레이아웃 그룹이 필요 없어짐).

**검증**: 배치모드 자동 테스트 2종을 만들어서 확인 후 삭제 — (1) 1~3계층 각 200회씩 총 600회 맵을 생성해서 고립 노드/도달 불가 노드가 없는지, 0층에서 BFS로 보스까지 실제로 도달되는지 검증 → 전부 통과. (2) 실제 Play 모드에서 `DungeonManager`를 직접 몰아서 캐릭터 선택 이후부터 보스 처치(Victory)까지 전체 플레이스루를 한 번 완주시켜서 새 `DungeonMapUI` 렌더링 코드까지 실제로 타게 만들고 예외 0건 확인.

**참고**: 세이브 데이터에 맵 자체는 저장되지 않는 구조(계층 번호만 저장하고 맵은 매번 새로 생성)라 이번 변경으로 세이브 포맷 변경은 없음.

## 한글 폰트 폴백 추가 (2026-07-07)

**증상**: `LiberationSans SDF`가 한글을 지원하지 않아서 게임 내 모든 한글 라벨이 □로 보임.

**수정**: Noto Sans KR(구글, OFL 무료 라이선스)을 `Assets/Fonts/NotoSansKR.ttf`로 받아서 TMP 폰트 에셋(`Assets/Fonts/NotoSansKR SDF.asset`, 동적 아틀라스 모드)으로 생성하고, `TMP Settings`의 폴백 폰트 목록(`m_fallbackFontAssets`)에 등록. 각 텍스트 컴포넌트를 일일이 바꿀 필요 없이, `LiberationSans SDF`에 없는 문자(한글)만 자동으로 이 폰트에서 찾아 그려줌.

**검증**: Play 모드 실행 후 로그 확인 - 한글 음절 관련 "글리프 없음" 경고가 전부 사라짐. 남은 "글리프 없음" 경고는 ⚔/🛡 같은 이모지 아이콘뿐 (한글과 무관, 원래도 있던 별개의 사소한 문제).

**참고**: Noto Sans KR variable font 원본이 약 10MB라 빌드 용량에 영향을 줌 - 나중에 최적화하려면 정적 단일 굵기(Regular) 버전으로 교체하거나 실제 쓰는 글자만 정적 베이킹하는 방식 고려.

---

## 2026-07-07 오늘 세션 정리 - 다음에 이어서 할 것

오늘 한 일 (위 항목들 커밋 + `github.com/redsue3/DungeonGame.git` push 완료):
- TMP 크래시 수정, 저장소(makingGame/DungeonGame) 통합
- 던전맵 9x7 그리드 → 분기형(슬레이 더 스파이어 스타일) 맵으로 전면 교체
- 한글 폰트 폴백(Noto Sans KR) 추가

**다음에 확인/이어서 할 것**
- ⚔ 🛡 같은 이모지 아이콘이 아직 □로 보임 (한글과는 무관, TMP 폴백에 이모지 지원 폰트가 없어서 그런 것 — 필요하면 이모지 지원 폰트를 폴백에 추가하거나, 아이콘을 이모지 대신 실제 스프라이트 이미지로 교체)
- 카드 등급 시스템(2026-07-05 항목): 기본 카드 10종을 실제 보상/상점 드롭 풀에 아직 연결 안 함
- Noto Sans KR 폰트 용량(10MB) 최적화 — 지금은 안 급함

> **게임이 완성됐으면 이 파일 삭제해라.**

---

## SaveSystem 이어하기 연결 (2026-07-08 추가)

**증상**: 세이브 파일은 만들어지는데 로드하는 버튼/흐름이 UI 어디에도 연결 안 되어 있었음 (7/7 항목에서 발견만 해두고 미착수 상태였음).

**수정**: `DungeonManager.LoadRun()` 신규 — 세이브 데이터로 플레이어 복원 + 저장된 계층 재생성 후 `DungeonMap` 상태로 진입. `CharacterSelectUI`에 이어하기 버튼 추가 (`SaveSystem.HasSave()`일 때만 노출), `SceneSetup`에도 반영.

---

## 던전맵 분기형 그래프 → 실제 타일 그리드(픽셀던전 스타일)로 전면 교체 (2026-07-08)

**배경**: 사용자가 지금까지의 맵(슬레이 더 스파이어 스타일 분기형 노드 그래프)이 "완전 슬더슬 따라한거"라 픽셀던전처럼 실제로 걸어다니는 던전으로 갈아엎자고 요청. 게임 핵심 루프 전체를 다시 짜는 수준이라 단계별로 나눠서 진행하기로 함 (1단계: 그리드 맵 + 쫓아오는 적, 이번에 완료. 2단계: 코스트를 전 직업 공용으로 전환 + 전투 중 이동/키이팅, 3단계: 맵 탐색 중 카드 사용 — 둘 다 다음 세션).

**신규 (`Map/DungeonFloor.cs`, `Map/FloorGenerator.cs`)** — `MapNode.cs`/`DungeonMap.cs`/`MapGenerator.cs` 대체:
- 44×30 타일 그리드에 겹치지 않는 방 13개(시작방 1 + 콘텐츠 11 + 보스 1)를 무작위 배치 후, 방 중심끼리 최소 스패닝 트리 + 여분 간선 3개로 연결해서 L자 복도로 깎음.
- 방 타입 배정 비율은 기존 노드 시스템과 동일(일반4/집단2/엘리트1/휴식2/상점1/성소1)하게 재사용, 시작방에서 가장 먼 방을 보스방으로 지정(층 개념이 없어져서 "가장 깊은 곳"을 거리로 대신함).
- `EnemySpawn`이 그리드 위에 실체로 존재 (`Idle`/`Chasing` 상태). 플레이어가 감지 반경(4칸) 안에 들어오면 `Chasing`으로 전환되어 매 플레이어 이동마다 그리디하게 한 칸씩 쫓아옴. 집단 조우는 `roomId`가 같은 적들이 한 번에 같이 전투에 들어감.
- 첫 구현 때 방 13개 중 4~6개가 배치 실패로 누락되는 문제 발견(44×30보다 좁은 34×22 그리드 + 재시도 예산 부족) → 그리드를 44×30으로 키우고 재시도 예산을 방당 60→400회로 늘려서 해결. 배치모드 900회 반복 생성 검증(방 도달가능성 BFS, 보스방 존재) 전부 통과.

**`DungeonManager`**: `MoveToNode(nodeId)` → `TryMove(dx,dy)`로 교체 — 벽이면 무시, 바닥이면 이동 + 배고픔 처리 + 적 AI 스텝 + 조우 판정(플레이어가 적 타일로 들어가거나, 쫓아오던 적이 플레이어 타일로 들어오면 둘 다 기존과 동일하게 `BattleManager.StartBattle` 호출). `CurrentMap`(DungeonMap) → `CurrentFloor`(DungeonFloor)로 교체, `currentBattleNode` → `currentRoom`(전투/휴식/상점/성소 공용)으로 정리.

**`DungeonMapUI` 전면 재작성** — 바닥 타일만 실체 오브젝트로 만들고(벽은 빈 공간), 플레이어를 항상 중심(0,0)에 고정한 채 나머지 타일/적 마커를 플레이어 기준 상대좌표로 그려서 카메라 추종 효과를 냄. 타일 오브젝트는 층이 바뀔 때만 새로 만들고 매 이동마다는 색/위치만 갱신(원래 노드 13개 재생성하던 것과 달리 이제 타일이 수백 개라 매번 Destroy+Instantiate하면 느려서). 안개: 미방문=완전히 안 보임, 방문했지만 시야 밖=어둡게, 시야 안=밝게. 이동은 인접 타일 클릭 또는 방향키/WASD.

**정리**: `LayerManager.cs`(7/7 리팩토링 이후 죽은 코드) 삭제, 그 안에 있던 `BattleReward` 클래스는 `System/BattleReward.cs`로 분리해서 이전.

**플레이 테스트 피드백 반영**:
- 상점/모닥불을 안 쓰고 나가면(구매 안 함/그냥 지나가기) 그 자리에 계속 남아서 나중에 다시 이용 가능하게 수정 (`ShopUI`/`RestUI`가 둘 다 `LeaveShop()`을 재사용하면서 방문만 해도 무조건 `isCleared=true`가 되던 버그 — `RestUI`는 `LeaveRestSite()` 신규로 분리, `ShopUI`는 실제로 뭘 산 적이 있을 때만 클리어되게 `shopPurchasedThisVisit` 플래그 추가).
- 배고픔 소모 속도가 그리드 이동(한 칸 단위)에서 너무 빠르게 느껴짐 — 예전 노드 이동(한 번 클릭 = 방 하나 이동) 대비 그리드 한 칸은 훨씬 잘게 쪼개져 있는데 소모 주기가 그대로였던 게 원인. `HungerSystem.TilesPerHungerTick` 3 → 60 (20배)로 조정.
- 맵 타일이 작고 다닥다닥 붙어 보기 불편하다는 피드백 → `DungeonMapUI`에 마우스 휠 줌 추가 (14~48px 범위, `PlayerPrefs`에 저장돼서 다음에 켜도 유지).

**아직 미해결로 남겨둔 UI 피드백** (사용자가 나중으로 미룸):
- 적/플레이어 마커가 텍스트+색깔 사각형뿐이라 구분이 잘 안 됨 — 나중에 움직임(애니메이션) 넣을 때 같이 정리 예정.
- 상단 상태바(HP/골드/배고픔) 배치, 안개(방문/시야) 대비도 손볼 예정.

**다음에 이어서 할 것**
- 2단계: 코스트(마나)를 마법사 전용 → 전 직업 공용으로 전환, 카드 사용/이동이 같은 코스트를 공유, 코스트 0 → 자동 리드로우(턴 종료) + 리필 틈 페널티.
- 3단계: 맵 탐색 중에도 비공격 카드 사용 가능 (코스트만 소모, 덱 순환은 전투 중에만).
- 4단계(추후): 전투 자체를 그리드 위로 통합해서 전투 중 이동/도주/키이팅까지 가능하게.
- 맵/마커 UI 다듬기 (위 "미해결 UI 피드백" 참고).

---

## 밀려있던 항목 3개 처리 (2026-07-08)

**카드 등급 드롭 풀 연결**: 2026-07-05에 만든 기본(공용) 카드 10종이 `LootTable.cardPool`에 안 이어져 있던 문제 — `basicCardPool` 리스트를 새로 두고 static 생성자에서 1~3계층 풀 전부에 합쳐 넣음 (가중치 4, 계층 전용 카드보다 낮게 잡아서 기본 카드가 너무 자주 나오지 않게 함).

**이모지 아이콘(⚔🛡👑🔥🛒⛩) □ 깨짐 수정**: 원인은 TMP 폴백 체인에 컬러 이모지 글꼴이 없어서(SDF 벡터 아웃라인이 필요한데 Windows의 이모지 글꼴은 컬러 비트맵이라 애초에 SDF 방식으론 못 그림) — 이모지 폰트를 추가하는 대신, 이미 잘 작동하는 한글 폴백 체인으로 렌더링되는 일반 텍스트로 전부 교체함 (`DungeonMapUI` 방 아이콘 → "적"/"적×2"/"정예"/"보스"/"휴식"/"상점"/"성소", `EnemyPanelUI`/`BattleUI`의 방어막·의도 아이콘 → "방어 N"/"[공격]"/"[방어]"/"[버프]"/"[독]"/"[화상]", 배고픔 경고 ⚠ → "(위험!)"). 타일 라벨이 줌으로 작아질 수 있어서 `MapTile` 프리팹 라벨에 TMP 오토사이징(6~26pt) 추가.

**Noto Sans KR 폰트 용량 최적화**: 원인은 기존 폰트 에셋이 Dynamic 아틀라스 방식이라 런타임에 글리프를 그때그때 그리기 위해 10.4MB 원본 ttf 파일 전체를 빌드에 그대로 들고 있어야 했던 것. `Editor/KoreanFontBaker.cs`(신규, 메뉴 `DungeonGame > 한글 폰트 정적 베이크`)를 만들어서 `Assets/Scripts` 전체(에디터 툴 코드는 제외)를 긁어 실제 쓰이는 한글 446자만 수집 → `NotoSansKR SDF Static.asset`을 새로 굽고(2048×2048 단일 아틀라스, samplingPointSize 48 — 처음엔 90/1024 조합으로 시도했다가 페이지가 여러 장으로 쪼개져서 오히려 더 커지는 문제가 있었음) TMP Settings 폴백 목록을 새 Static 에셋으로 교체. 이제 원본 10.4MB ttf는 어디서도 참조가 안 돼서 빌드에서 자동으로 빠짐 (10.4MB → 아틀라스 텍스처 약 4MB로 축소). 기존 Dynamic 에셋(`NotoSansKR SDF.asset`)과 원본 ttf 파일은 정리 안 하고 프로젝트에 그대로 남겨둠(더 이상 아무도 참조 안 해서 빌드엔 안 잡히지만, 혹시 몰라 안전하게 보존 — 필요하면 나중에 수동 삭제).
  - **주의**: 코드에 새 한글 텍스트가 추가되면 그 글자가 정적 아틀라스에 없어서 안 보일 수 있음 → 그때는 `DungeonGame > 한글 폰트 정적 베이크` 메뉴를 다시 실행해서 문자셋을 갱신해야 함.
  - 이 작업 중 TMP 저수준 API(`TMP_FontAsset.CreateFontAsset`/`TryAddCharacters`) 관련 예외가 몇 번 났음(소스 폰트 참조를 저장 전에 끊으면 아틀라스 텍스처가 날아가는 문제, `enableMultiAtlasSupport`로 인한 빈 텍스처 슬롯 null 참조) — 전부 스크립트 안에서 수정 완료, 최종적으로 컴파일 에러/예외 없이 완료 확인함. 7/7 TMP 크래시 전례가 있어서 조심스럽게 진행했고, 플레이 화면에서 한글이 잘 보이는지 최종 확인 필요.

> **게임이 완성됐으면 이 파일 삭제해라.**
