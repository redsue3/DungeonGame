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

---

## 성소/모닥불 빈도 축소 + 직접 접촉 트리거로 변경 (2026-07-09)

**증상 (사용자 리포트)**: "성소나 모닥불 같은거 너무 많아... 하기 싫을때도 범위가 넓어서 계속 이벤트가 나와." — 성소·상점·휴식 이벤트가 원하지 않을 때도 계속 튀어나옴.

**원인**:
1. 빈도: `FloorGenerator.AssignRoomTypes`의 타입 풀에 성소 1개·휴식 2개가 매 층(1~3층)마다 들어있어서, 런 전체로 보면 성소 3개·모닥불 6개가 나오고 있었음(devlog 2026-07-07/08 항목에서 "예전과 동일한 비율" 유지라고만 기록하고 층당 개수라는 게 문제라는 인식은 없었음).
2. 트리거 판정: `DungeonManager.TryMove`가 `RoomInfo.Contains`(방 전체 사각형, 최대 5×5) 안에 들어가기만 하면 휴식/상점/성소 이벤트를 발동시켰음. 반면 아이콘은 `DungeonMapUI`에서 방 중심 타일(`CenterX`/`CenterY`)에만 그려지고 있어서, 화면엔 성소/모닥불이 방 한쪽 구석에만 표시되는데 실제 발동 범위는 방 전체였던 것 — "직접 접촉" 없이 근처를 걷기만 해도 이벤트가 튀어나오던 원인.

**수정**:
- `FloorGenerator.AssignRoomTypes(rooms, start, boss, layer)` — `layer` 파라미터 추가. 성소는 `layer == 1`일 때만 풀에 넣어서 **런(게임) 전체에 1개만** 나오게 함. 휴식(Rest)은 풀에서 2개 → 1개로 줄여서 **층당 1개**로 축소. 줄어든 풀 크기(9~10개)는 남은 방 개수(11개)보다 적어지는데, 기존에 이미 있던 fallback(`pool.Count > 0 ? Pop(pool) : TileType.NormalEnemy`)이 그대로 나머지를 일반 조우로 채워줌 — 별도 코드 추가 없이 자연스럽게 처리됨.
- `DungeonManager.TryMove` — 휴식/상점/성소 발동 조건에 `nx == room.CenterX && ny == room.CenterY`를 추가해서, 아이콘이 실제로 표시되는 그 타일에 정확히 올라섰을 때만 이벤트가 뜨도록 변경(기존엔 `RoomAt(nx, ny)`로 방 전체 범위 아무 데나 닿으면 발동). 방을 그냥 지나쳐도(중심 타일을 안 밟으면) 이벤트가 안 뜨고, 방 안에서 이리저리 움직여도 중심 타일을 다시 밟기 전엔 재발동하지 않음.

**참고**: 전투 조우(일반/집단/엘리트/보스)는 원래도 `EnemySpawn`의 정확한 좌표(`TryEngageEnemyAt`)로 판정하고 있어서 이번 변경과 무관 — 방 범위 판정 문제는 휴식/상점/성소 3종에만 있었음.

---

## 2단계 — 코스트(마나) 매커닉 전 직업 공용화 (2026-07-09)

**배경**: 2026-07-08 세션 정리에서 남겨둔 "다음에 이어서 할 것" 2단계. 지금까지는 `BattleManager.UsesMana`가 `player.characterClass == CharacterClass.Mage`일 때만 참이라, 전사/도적/성기사는 카드에 코스트가 적혀 있어도 무시하고 무제한으로 사용할 수 있었음. 이걸 전 직업이 동일하게 코스트를 관리하는 매커닉으로 바꿈.

**핵심 변경 (`BattleManager.cs`)**
- `UsesMana` 프로퍼티(마법사 전용 게이트) 삭제 — `UseCard()`의 코스트 체크/차감이 이제 모든 직업에 무조건 적용됨.
- `BattleUI.RefreshHand()`의 카드 버튼 `interactable` 판정에서도 동일한 마법사 전용 게이트(`bool usesMana = ...`)를 제거해서 백엔드/UI가 다시 일치하게 함.
- 직업별 최대 코스트는 기존 `PlayerDatabase` 값(전사3/도적3/마법사4/성기사3)을 그대로 유지 — 이미 전투 밸런스에 맞춰 잡혀 있던 값이라 새로 조정할 필요 없음.

**카드 코스트 재산정 (`Database/CardDatabase.cs`)**
- 문제: 2026-07-05에 추가한 기본(공용) 카드 10종(`quick_slash`~`toughen`)이 "코스트가 마법사한테만 의미 있으니 전부 무료로 쓰게 하자"는 이유로 전부 0코스트였음. 코스트를 전 직업 공용으로 바꾸면 이 카드들만 계속 공짜가 되어 다른 유료 카드들과 형평이 안 맞음(사용자 피드백: "카드를 노코스트로 다 쓰는게 말이 안되는데").
- 수정: 10종 전부 위력에 맞춰 실제 코스트를 부여 — 데미지/방어막/효과 크기가 비슷한 기존 유료 카드(예: `strike` dmg6/cost1, `heavy_strike` dmg14/cost2, `iron_defense` blk12/cost2)를 기준으로 삼아 대부분 cost1, 수치가 큰 `solid_strike`(dmg11)/`steady_guard`(blk11) 2종만 cost2로 배정.
- 3계층 드롭 카드 `chaos_flame`(dmg8+화상3, 자기피해4)은 그대로 0코스트 유지 — 이건 애초에 자기피해라는 페널티가 코스트 대신 들어간 의도된 디자인이라 이번 변경과 무관.

**신규 매커닉 — "리필 틈" 페널티 (`BattleManager.cs`)**
- 요청: 코스트를 턴 안에서 완전히(0까지) 소진하면 턴이 자동으로 끝나고, 그 대가로 다음 턴 코스트 리필에 페널티(틈)가 생기게 해달라는 설계.
- 구현: `UseCard()`에서 카드 사용 직후 `currentMana <= 0`이고 아직 `PlayerTurn` 상태면(=이 카드로 승리한 경우 제외) `manaRefillPenalty = 1`을 세팅하고 `EndPlayerTurn()`을 직접 호출해 턴을 강제 종료. 다음 `PlayerTurn()` 시작 시 `currentMana = Mathf.Max(0, player.maxMana - penalty)`로 리필하고 페널티를 즉시 0으로 리셋(1턴만 적용, 누적 안 됨).
- 손으로 "턴 종료" 버튼을 눌러 코스트를 남긴 채 끝내는 경우는 페널티 없음 — 페널티는 오직 코스트를 정확히 0으로 만들어 자동 종료된 경우에만 발생.
- 유물 "전쟁의 뿔피리"(매 턴 시작 시 코스트+1)는 페널티 적용 *이후*에 발동하므로, 페널티를 부분적으로 상쇄해줌 (의도된 상호작용, 별도 처리 불필요).

**UI 문구**: "마나" 표기가 이제 전 직업 공용 자원이 됐으므로 전사/도적/성기사한테 "마나"라는 단어가 어색해서 `BattleUI`/`CharacterSelectUI`/`SceneSetup.cs`의 화면 표시 문구를 "코스트"로 변경(내부 필드명 `currentMana`/`maxMana`/`manaCost`는 그대로 유지 — 대규모 리네이밍은 이번 범위 밖).

**다음에 이어서 할 것 (기존 3/4단계, 변동 없음)**
- 3단계: 맵 탐색 중에도 비공격 카드 사용 가능 (코스트만 소모, 덱 순환은 전투 중에만).
- 4단계(추후): 전투 자체를 그리드 위로 통합해서 전투 중 이동/도주/키이팅까지 가능하게.

---

## 적 AI 개선 - 시야선(LOS) 감지/시야 공개 + BFS 추적 + 접촉 전투 (2026-07-13)

**배경**: 오전 세션이 LOS/BFS 작업 도중 사용량 한도로 끊겨 미커밋으로 남아 있던 것을 이어서 완성. 참고로 지금 작업본은 GitHub에서 새로 클론한 `DungeonGameReal` 폴더다 — 홈에 있는 기존 `DungeonGame` 폴더는 `.git`이 없는 7/8 이전 스냅샷이라 더 이상 쓰지 않는다 (makingGame은 7/8에 통합 완료된 구 저장소).

**시야선 (`Map/DungeonFloor.cs`)**: `HasLineOfSight(x1,y1,x2,y2)` 신규 — Bresenham 직선을 따라가며 중간에 벽이 있으면 false (시작 칸은 검사 제외, 목표 칸 자체는 도달로 취급해서 벽면도 보임). `RevealAround`가 반경 + LOS를 모두 통과한 타일만 밝히도록 변경 → 벽 뒤 방/복도가 미리 보이던 문제 수정.

**감지/추적 (`System/DungeonManager.cs`)**: Idle→Chasing 전환 조건에 LOS 추가 — 벽 너머의 적이 감지 반경만 겹치면 미리 쫓아오기 시작하던 것 수정. 추적 이동은 그리디 한 칸(부호 방향, 벽 지형에서 자주 낌) → `BfsPath` 4방향 BFS 최단 경로의 첫 칸 이동으로 교체. 다른 적이 점유한 칸은 우회하고(목표=플레이어 칸만 예외), 경로가 아예 없으면 제자리 대기.

**접촉 전투 완성 (이번 세션 신규)**: 7/8 항목에 "쫓아오던 적이 플레이어 타일로 들어오면 전투 시작"이라고 적어놨지만 실제 코드는 `TryStepEnemy`가 플레이어 칸 진입을 막기만 하고(return false) 어디서도 적 주도 전투를 열지 않아서, 적이 플레이어를 따라잡아도 플레이어가 먼저 밟기 전엔 전투가 절대 안 열렸음 — `TryMove`의 두 번째 `TryEngageEnemyAt(PlayerX, PlayerY)` 호출은 도달 불가능한 죽은 코드였다. 수정: `StepTowardPlayer`가 BFS 다음 칸이 플레이어 칸이면 이동 대신 접촉 신호(true)를 반환 → `StepEnemies`가 그 적을 반환 → `TryMove`가 `Engage(attacker)`로 해당 적의 방 그룹과 전투 시작. 기존 조우 로직은 `TryEngageEnemyAt`(좌표 판정)과 `Engage(spawn)`(전투 개시)로 분리해 양쪽에서 재사용.

**검증**: Unity 6000.5.3f1 배치 모드로 전체 임포트 + 스크립트 컴파일 통과 (컴파일 에러 0). 설치된 에디터가 6000.5.3f1뿐이라 프로젝트가 6000.5.1f1 → 6000.5.3f1로 업그레이드됨 (`ProjectVersion.txt`/`ProjectSettings.asset`/csproj 재생성 — 함께 커밋, Unity 6 생성물 `.vscode/`·`*.slnx`는 gitignore에 추가). 실제 플레이 확인(벽 뒤 적이 안 쫓아오는지, 따라잡히면 적이 전투를 거는지, 시야가 벽에 막히는지)은 다음에 Unity 열 때 한 번 볼 것.

---

## 3단계 — 맵 탐색 중 비공격 카드 사용 (2026-07-13)

**설계 (사용자 선택: "이동으로 재생")**: 코스트가 전투 밖에서도 상주하는 자원이 됨.
- `PlayerCharacter.currentMana` 신규 — 전투/맵 공용. `BattleManager`의 전투 전용 `currentMana` 필드를 없애고 이걸 쓴다.
- **전투 첫 턴은 리필하지 않고** 맵에서 들고 온 코스트로 시작 (2턴부터 기존처럼 매 턴 풀 리필, 리필 틈 페널티도 그대로). 첫 턴엔 방어막도 초기화하지 않아서 맵에서 미리 쳐둔 방어막을 유지한 채 전투에 들어감 — "준비하고 들어가기"가 성립.
- 맵에서는 **10칸 이동마다 코스트 1 회복** (`MapCardSystem.TilesPerCostRegen`, 가득 찼을 땐 걸음 적립 안 함). 걸어서 회복하는 만큼 배고픔이 닳으므로 자원끼리 맞물림.
- 맵 카드 판정: 공격 요소(데미지/독/화상)가 없고 회복/방어막/강인함/예약버프 효과가 있는 카드만. **코스트만 소모하고 덱은 안 건드림** (덱 순환은 전투 전용 — 7/8 설계 유지). 드로우 전용 카드는 맵에 손패가 없어 제외. 회복은 기존 규칙대로 배고픔을 소모.

**구현**:
- `System/MapCardSystem.cs` 신규 — 이동 재생 / `IsUsableOnMap` 판정 / `UseCard`(Deck.ApplyCardEffect의 시전자 효과와 동일 규칙, 드로우 제외).
- `BattleManager` — `firstPlayerTurn` 플래그로 첫 턴 리필/블록 초기화 생략. 유물 GainMana도 `player.currentMana`에 적용.
- `PlayerCharacter.OnTurnStart(bool resetBlock = true)` — 첫 턴엔 상태이상만 진행.
- `DungeonManager` — `TryMove`에서 `MapCardSystem.OnPlayerMove` 호출, `UseMapCard(card)` 신규 (자해 카드로 죽으면 GameOver 처리).
- `SaveSystem` — `currentMana` 저장/복원. 필드 기본값 -1로 구버전 세이브를 식별해 maxMana로 복원 (하위호환).
- UI — 맵 상단바에 코스트 표시 + "카드" 버튼, `MapCardUI`(신규) 오버레이가 덱에서 사용 가능한 카드를 전투 손패와 같은 카드 프리팹으로 나열 (코스트 부족 시 버튼 비활성). `SceneSetup`에 `BuildMapCardSubPanel` 추가.

**검증**: Unity 6000.5.3f1 배치 — 컴파일 에러 0, `한글 폰트 정적 베이크` 재실행(새 UI 문구 글리프 반영, `NotoSansKR SDF Static.asset` 갱신), `씬 자동 세팅` 재실행 성공("완전히 연결된 씬 세팅 완료" — GameScene/프리팹 재생성). **플레이 확인 필요**: 맵 카드 패널 열기/사용/이동 재생, 방어막 들고 전투 진입 시 첫 턴 유지되는지, 첫 턴 코스트가 들고 온 값으로 시작하는지.

**다음에 이어서 할 것**
- 4단계(추후): 전투 자체를 그리드 위로 통합해서 전투 중 이동/도주/키이팅까지 가능하게.
- 맵/마커 UI 다듬기 (7/8 "미해결 UI 피드백" 참고).

---

## 전체 코드 리뷰 — 수정 후보 목록 (2026-07-14, 아직 아무것도 안 고침)

"코드를 아름답게" 정리 전 사전 조사. 스크립트 56개 전부 읽고 찾은 것들. **한 번에 몰아서 고칠 것 — 아래는 발견만 해둔 상태.**
> **→ 같은 날 오후에 전부 처리 완료. 항목별 처리 내용은 맨 아래 "전체 코드 리뷰 반영" 항목 참고.**

### A. 버그 (아름다움 이전에 실제로 틀린 것)

1. **[심각] 세이브 로드 시 유물 Passive 스탯이 전부 증발** — `SaveSystem.RestorePlayer`가 `player.maxHp`를 복원하지 않고(`SaveData.maxHp`는 저장만 함), 유물도 `relics.Add(id)`만 하고 `ApplyPassiveEffects`를 다시 안 부름. 철심(+15 HP)·마왕의 왕관(+1 코스트)·뱀 어금니(패+1) 등이 이어하기 후 사라지고, currentHp > maxHp 상태도 가능. 단순히 로드 때 ApplyPassiveEffects를 재호출하면 정화의 부적/혼돈의 프리즘(획득 시 1회성 덱 조작)이 또 발동하는 함정이 있음 — 근본 원인은 `RelicTrigger.Passive`가 "상시 스탯"과 "획득 시 1회"를 한 트리거에 섞어둔 것. 트리거 분리(예: `OnAcquire`) 후 로드 시 상시 스탯만 재적용이 정석.

2. **[심각] 집단전 타겟 인덱스 불일치로 엉뚱한 적 공격** — `BattleUI.RefreshEnemies`는 전체 `enemies` 리스트 인덱스(죽은 적 포함)를 `selectedTargetIndex`로 넘기는데, `BattleManager.BuildTargets`는 살아있는 적만 필터한 리스트(`alive`)를 그 인덱스로 참조함. 예: [생존, 사망, 생존]에서 세 번째 적을 선택하면 UI는 2를 넘기고 alive는 2칸뿐이라 fallback으로 0번을 때림. 인덱스 대신 Enemy 참조로 넘기거나 양쪽 기준을 통일해야 함. (+ 선택한 적이 죽었을 때 selectedTargetIndex 리셋도 없음)

3. **0코스트 카드를 코스트 0 상태에서 쓰면 "리필 틈" 페널티 발동** — `BattleManager.UseCard`의 자동 턴종료 판정이 `currentMana <= 0` 현재값만 봐서, 이번 카드로 소진된 게 아니어도 걸림. 특히 맵에서 코스트 0으로 전투에 들어온 첫 턴엔 0코스트 카드 한 장에 턴이 강제 종료됨. "이 카드 사용으로 0이 됐을 때"(`card.manaCost > 0 && currentMana == 0`)로 좁혀야 함.

4. **독/화상/자해로 죽인 적은 OnKill 유물(혈약 반지) 미발동** — 처치 판정이 `UseCard` 안에만 있어서 `EnemyTurn`의 상태이상 사망은 안 잡힘. 같은 맥락으로 `CheckHpBelowTriggers`(불사조의 재)도 적 공격 직후에만 호출돼서 독/화상/자해로 30% 이하가 되면 미발동.

5. **플레이어가 자기 턴 중 사망(자해/독)해도 즉시 패배 처리가 안 됨** — `BattleLoop`의 사망 판정이 턴 경계에서만 돌아서, 어둠의 계약 자해 등으로 죽으면 "턴 종료"를 눌러야 GameOver로 넘어감. `UseCard` 뒤에 플레이어 생존 체크 추가 필요.

6. **id 기반 카드 제거의 오제거 가능성** — `Deck.RemoveCard(string id)`는 같은 id 중 첫 장을 지움. 성소 성장형 카드처럼 같은 id에 스탯이 다른 카드가 공존하면 정화의 부적(`RemoveRandomCardFromDeck`)이 고른 카드와 실제 지워지는 카드가 달라질 수 있음. `RemoveCard(Card instance)` 오버로드가 정확함. (+ RemoveCard가 hand는 검색 안 함 — 지금은 상점에서만 써서 무해하지만 잠재 함정)

7. **집단 조우 적 2마리가 같은 타일에 겹칠 수 있음** — `FloorGenerator.SpawnOne`이 방 안 무작위 좌표를 중복 체크 없이 뽑음. `EnemyAt`은 첫 놈만 반환하고 맵 마커도 겹쳐 보임.

### B. 죽은 코드 (삭제 or 살리기 결정 필요)

- `Enemy.RollGoldReward()` + `EnemyData.rewardGoldMin/Max` — 골드는 `LootTable.RollGold(layer)`로만 굴림. EnemyDatabase 13종에 정성껏 적어둔 적별 골드 데이터가 전부 안 쓰임. **살릴지(적별 골드가 더 맛있음) 지울지 결정.**
- `DamageCalculator.CalculateWithCrit` — 어디서도 호출 안 함 (크리티컬 시스템 미구현).
- `PlayerCharacter.dexterityStack` — 올려주는 코드가 없어 `GetFinalBlockBonus()`는 항상 0. 세이브에도 실려 다님.
- `PlayerCharacter.currentFloor` — 미사용.
- `Card.rarity` / `Card.classRestriction` / `CardRarity` enum — 7/5 카드 등급 시스템 1단계에서 만들고 어디서도 안 읽음 (등급 기반 드롭은 basicCardPool 가중치 방식으로 대체된 상태).
- `EnemyData.baseBlock` — 13종 전부 0이고 EnemyFactory도 안 읽음.
- `Deck.GetDrawPile()` — 미사용.
- `SaveData.currentStage` — 저장만 하고 로드에서 안 읽음 (LayerManager 시절 잔재).
- `Animation/` 폴더 전체 (CharacterAnimator + AnimationStateController + 직업별 4종) — 게임 코드 어디서도 호출 안 함. 스프라이트/연출 작업 때 쓸 예정이면 보류, 아니면 삭제.

### C. 구조 개선 (아름답게 만들기 본론)

- **카드 효과 적용 로직 중복** — `Deck.ApplyCardEffect`(전투)와 `MapCardSystem.ApplyEffect`(맵)가 시전자 효과 규칙을 복붙으로 공유. 한쪽을 고치면 다른 쪽을 까먹기 좋은 구조. 시전자 효과를 공용 메서드로 추출.
- `Deck.ApplyCardEffect`의 `caster is PlayerCharacter attacker/pc/defender/pcs/buffer` — 같은 캐스팅을 5번 반복. 메서드 초입에서 한 번만.
- **DungeonManager 책임 과다** (~400줄) — 이동/적 AI(BFS)/조우/보상/상점/성소/식사/세이브가 한 클래스에. 최소한 적 AI(`StepEnemies`/`StepTowardPlayer`/`BfsPath`/`TryStepEnemy`)를 별도 클래스(예: `EnemyAiSystem`)로 분리.
- `DungeonManager.TryMove`의 휴식/상점/성소 분기 3개가 거의 동일 패턴 — `EnterRoom(room)` 하나로 합치기.
- **`UIManager.ShowPanel/HideAll`** — 패널 9개를 switch + 수동 나열 2벌. 새 상태 추가마다 3곳 수정. `Dictionary<GameState, GameObject>` 또는 직렬화 배열로. + `RefreshBattle()`이 매 갱신마다 `GetComponentInChildren<BattleUI>()` — 캐싱.
- **`usedOnceRelics`에 유물 id와 `"killed_0"` 처치 키가 섞여 있음** — 용도가 다른 두 상태를 한 HashSet에. 처치 추적은 별도 필드로 분리 (A-4 고칠 때 같이).
- **이름 정리: mana → cost** — 7/9에 미뤄둔 것. 화면은 전부 "코스트"인데 코드는 `currentMana/maxMana/manaCost`. 전면 리네이밍은 한 번에 (세이브 필드명 `currentMana`는 JsonUtility 하위호환 주의 — 필드명 바꾸면 구세이브 깨짐).
- `LootTable.RollCardRewards` — "최대 100회 재추첨으로 중복 회피" 방식. 뽑힌 항목을 풀 사본에서 제거하며 뽑으면 tries 꼼수 없이 명료해짐.
- 로직 클래스(Character/Deck/Enemy/ShopSystem 등)가 `Debug.Log`로 게임 이벤트를 직접 출력 — 전투 로그 UI를 만들 때쯤 이벤트/콜백으로 분리해야 함. 지금은 유지해도 무방하나 인지.
- MonoBehaviour에 `?.` 연산자 다용 (`UIManager.Instance?`, `panelRoot?.SetActive` 등) — Unity의 destroyed-fake-null을 `?.`가 못 거르는 유명한 함정. 현재 씬 구조(단일 씬, 파괴 없음)에선 무해해서 우선순위 낮음.

### D. 사소 (여유 있으면)

- `ShrineSystem.GenerateUtilityCard(player)` — player 파라미터 미사용.
- `BattleUI.BuildManaPips` — 유물 GainMana로 currentMana가 maxMana를 넘으면 초과분이 표시 안 됨 (◆가 max개까지만).
- `DungeonMapUI.RepaintGrid` — 타일마다 `floor.RoomAt(x,y)` 선형 탐색. 방 중심 좌표만 미리 Dictionary로 만들어두면 충분.
- `ShopSystem.RollRandomFoods` — 같은 식료품 2개가 나올 수 있음 (의도면 유지).
- `OnBattleWon`의 `foreach (… AliveEnemiesInRoom(…)) s.isDead = true` — 순회 중인 lazy enumerable의 필터 조건(isDead)을 순회 중에 바꿈. 현재는 동작하지만 깨지기 쉬운 패턴, `.ToList()` 한 번이면 안전.
- 보스전 승리 → Victory 직행일 때도 `pendingReward`를 만들어둠 (안 쓰고 버려짐).

---

## 전체 코드 리뷰 반영 — "코드 아름답게" 일괄 수정 (2026-07-14)

위 리뷰 목록(A/B/C/D)을 전부 처리. 오전에 이전 세션이 A-1(유물 트리거 분리)/A-6(인스턴스 기준 제거)을 시작해두고 끊겨서 `ShopUI→DungeonManager→ShopSystem` 시그니처가 안 맞아 **컴파일이 깨진 채로** 남아 있었음 — 그것부터 이어서 완성.

### A. 버그 7건 전부 수정
1. **세이브 유물 증발**: `RestorePlayer`가 유물을 먼저 복원하면서 `ApplyPassiveStats`(상시 스탯만)를 다시 적용 → 최대 HP/코스트/패 수가 저장 당시와 같아짐. OnAcquire(1회성 덱 조작)는 재실행 안 함. currentHp/currentCost는 그 뒤 복원(최대치 클램프). PlayerFactory 대신 `new PlayerCharacter()`로 생성 (스타터 덱/시작 식료품을 만들었다 버리는 낭비 제거).
2. **집단전 타겟**: 인덱스 전달을 버리고 `BattleUI.selectedTarget`(Enemy 참조) + `UseCard(handIndex, Enemy target)`로 변경. 선택한 적이 죽으면 첫 생존 적으로 자동 리셋, 전투 시작 시 참조 초기화.
3. **리필 틈 오발동**: 자동 턴종료 조건을 `card.cost > 0 && currentCost == 0`("이 카드로 소진된 경우")로 좁힘 — 0코스트 카드/맵에서 0으로 진입한 첫 턴은 이제 안 걸림.
4. **OnKill/HP30 미발동**: 처치 감지를 `CheckNewKills()`로 분리, `rewardedKills`(HashSet\<Enemy\>)로 추적 — 카드 사용 직후 + 적 턴 상태이상 처리 직후 양쪽에서 호출 (독/화상 처치도 혈약 반지 발동). `CheckHpBelowTriggers`는 플레이어 HP가 깎이는 모든 경로(적 공격/자해/턴 시작 독화상) 뒤에 호출. `usedOnceRelics`에 섞여 있던 `killed_i` 키도 이걸로 분리 (C 항목).
5. **자기 턴 중 사망**: 카드 사용 후 플레이어 사망이면 즉시 `Lose` (전멸 확인이 우선 — 동귀어진은 승리). 턴 시작 독/화상 사망도 처리. PlayerTurn 대기 조건을 `playerTurnEnded || state != PlayerTurn`으로 일반화.
6. **카드 오제거**: `Deck.RemoveCard(Card)` 인스턴스 기준으로 통일하고 hand까지 검색. ShopUI→`DungeonManager.RemoveCardFromDeck(Card)`→ShopSystem 전 구간 참조 전달.
7. **적 스폰 겹침**: `SpawnOne`이 방 안에서 다른 적이 없는 칸 목록 중에서 뽑음.

### B. 죽은 코드 — 결정
- **적별 골드 → 살림**: `OnBattleWon(List<Enemy>)`가 처치한 적별 `RollGoldReward()`를 합산 (정성껏 적어둔 EnemyDatabase 골드 데이터가 드디어 쓰임, 조우 난이도에 보상이 비례). 대신 `LootTable.RollGold`/`goldRange`를 삭제.
- **Animation/ 폴더 → 보류**: 스프라이트/연출 작업 때 쓸 예정이라 유지.
- **삭제**: `CalculateWithCrit`, `dexterityStack`(+`GetFinalBlockBonus`), `currentFloor`, `Card.rarity`/`classRestriction`/`CardRarity`, `EnemyData.baseBlock`(13종 초기화 포함), `Deck.GetDrawPile`/`HandCount`, `SaveData.currentStage`/`maxHp`/`dexterityStack`(+`Save`의 stage 파라미터), `LootTable.GetCardRewardCount`/`RewardType`/`RewardEntry.goldMin·goldMax`, `FoodDatabase.Exists`.

### C. 구조 개선
- **카드 효과 중복 제거**: 시전자 효과(방어막/회복/강인함/자해/예약버프)를 `Card.ApplyCasterEffects(caster)` 하나로 — `Deck.ApplyCardEffect`(전투)와 `MapCardSystem.UseCard`(맵)가 공유. 같은 캐스팅 5회 반복도 사라짐. 드로우만 덱 순환이 필요해 Deck에 남김.
- **적 AI 분리**: `System/EnemyAiSystem.cs` 신규 — LOS 감지/BFS 추적/접촉 판정 이동, DungeonManager는 `EnemyAiSystem.StepAll(floor)` 한 줄만.
- **방 이벤트 분기 통합**: `TryMove`의 휴식/상점/성소 3분기 → `TryEnterRoomEvent` switch 하나 + `EnterRoom`.
- **UIManager**: switch+수동 나열 2벌 → `Dictionary<GameState, GameObject>` 1개, `RefreshBattle`의 매번 `GetComponentInChildren` → Awake 캐싱.
- **mana → cost 전면 리네이밍** (7/9에 미뤄둔 것): `Card.cost`, `PlayerCharacter.currentCost/maxCost`, `PlayerData.maxCost`, `BattleManager.CurrentCost/costRefillPenalty`, `RelicEffectType.GainCost/BonusMaxCost`, UI 필드 `playerCostText`/`costText`. **세이브 JSON 키(`currentMana`, `CardSnapshot.manaCost`)만 하위호환 때문에 옛 이름 유지** (주석 명시). 카드 id/표시명(mana_shield 마력 방벽 등)은 세이브 호환+플레이버라 유지. `SceneSetup` 바인딩 문자열 갱신 + 씬 재생성으로 SerializeField 재연결 완료.
- **LootTable 추첨**: "최대 100회 재추첨" → 뽑힌 항목을 풀 사본에서 제거하며 뽑기.
- Debug.Log 이벤트 분리 / MonoBehaviour `?.` 정리는 리뷰 결론대로 보류 (현 구조에서 무해).

### D. 사소
- `GenerateUtilityCard` 미사용 파라미터 제거, `BuildCostPips`가 최대치 초과 코스트(전쟁의 뿔피리)도 표시, `DungeonMapUI` 방 중심 좌표 Dictionary 캐싱(타일마다 `RoomAt` 선형탐색 제거), 상점 식료품 중복 진열 방지, `OnBattleWon`의 lazy enumerable 순회 중 수정 → `.ToList()`, 최종보스 승리 시 `pendingReward` 생성 생략 + 매직넘버 3 → `FinalLayer` 상수.
- **추가 발견 2건**: (1) 성장형 카드가 강해져도 설명 텍스트가 예전 수치로 남던 문제 → `Card.RebuildDescription()` 추가, 성장 시 갱신. (2) `KoreanFontBaker` 재실행 시 TMP Settings 폴백 교체 실패(에셋을 지웠다 다시 만드는 사이 참조가 null이라 못 찾음, GUID 재사용 운으로만 살아 있었음) → null 슬롯도 교체 대상으로 수정.

### 검증 (Unity 6000.5.3f1 배치모드, 임시 테스트는 확인 후 삭제)
- 컴파일 에러 0, `씬 자동 세팅` 재실행 성공, `한글 폰트 정적 베이크` 재실행 성공(499자).
- 자동 테스트 통과: (1) 세이브/로드 라운드트립 — 철심/마왕의 왕관/뱀 어금니/정화의 부적 획득 후 저장→로드에서 스탯 복원 + 부적 재발동 없음 확인, (2) 맵 생성 600회 — 적 겹침 0/보스방 도달 가능, (3) 보상 추첨 600회 — 중복 0.
- **플레이모드 전체 플레이스루**: 전사로 1계층 시작 → 마왕 처치(Victory)까지 자동 주행 완주, 예외 0건 (최종 골드 734/덱 34장/유물 6개 — 도중에 정화의 부적·혼돈의 프리즘 OnAcquire, 전쟁의 뿔피리 GainCost 경로 실제로 탐).

### 주의 (세이브 호환)
- 구버전 세이브 로드 가능. 단 maxHp는 이제 저장값 대신 "기본 스탯 + 유물 Passive"로 재계산된다 (그게 A-1 수정의 핵심).

**다음에 이어서 할 것 (변동 없음)**
- 4단계: 전투를 그리드 위로 통합해서 전투 중 이동/도주/키이팅.
- 맵/마커 UI 다듬기 (7/8 "미해결 UI 피드백" 참고).
- 실제 플레이 손맛 확인: 적별 골드 보상 체감, 집단전 타겟 클릭, 코스트 소진 자동 턴종료.

---

## 2026-09-02 PR 3종 검증 + 몬스터 도감 로더 추가 (2026-09-03 세션)

**배경**: 7/14 이후 이 파일 갱신 없이 한 달 반 붕 떴다가, 9/2에 로컬 작업이 devlog 대신 **GitHub PR 3개**로 올라와 있는 걸 발견 (`redsue3/DungeonGame`, 전부 미병합 상태):
- PR #1 `fix/diagonal-move-gameover-label` — 던전맵 클릭 이동의 대각선 인접 판정 제거(4방향만 허용, 적 추적 BFS/키보드 이동과 통일) + 게임오버 화면에 직업 무관 "전사"가 하드코딩돼 있던 오표기 수정.
- PR #2 `docs/worldbook-and-bestiary` — `worldbook.md`(세계관 "황금 곳간" 설정, 캐릭터 4명 배경) + `bestiary.md`(몬스터 22종 개별 설정, 헤더 id가 `EnemyDatabase.cs` 키와 1:1 대응하도록 설계) 신규. 순수 문서, 코드 변경 없음.
- PR #3 `feature/monster-roster-and-map-colors` — `EnemyData`에 `phases[]`(페이즈별 hpThreshold+패턴) 추가해서 보스 3종을 100%/66%/33% HP 3페이즈로 재구성, 층당 일반 몬스터 2종→5종(9종 신규), `DungeonMapUI` 바닥색을 계층별로 분리(1층 청회색/2층 흙빛/3층 자주빛). **PR 본문에 스스로 "미검증 상태로 올림"이라고 표시**하고 컴파일/플레이 확인 체크박스가 전부 비어 있었음.

**오늘 한 일 (검증)**:
- PR3 코드 리뷰 — `Enemy.CheckPhaseTransition()`(while 루프로 여러 단계 동시 스킵 가능, `OnTurnStart`에서 `ProcessStatusEffects` 다음에 실행) 타이밍/경계값(0.66/0.33 임계값 부등호 방향) 확인, `EnemyFactory`/`FloorGenerator`/`DungeonMapUI` 변경분에 예전 `AddAction`/`data.pattern` API 잔재가 없는지 전수 검색 — 문제 없음.
- PR1 + PR2 + PR3를 로컬에서 순서대로 병합 테스트 — 셋 다 `DungeonMapUI.cs`를 건드리지만 겹치는 줄이 없어 **충돌 없이 자동 병합됨** (PR1의 4방향 판정 + PR3의 계층별 색상이 한 파일에 공존 확인).
- PR2 자신의 체크리스트("PR3 머지 후 `bestiary.md`의 id 22개가 `EnemyDatabase.cs` 키와 정확히 일치하는지 확인")를 병합된 상태에서 실행 — **22/22 정확히 일치, 누락/잉여 0건**.
- **한계**: 이 환경엔 Unity 배치모드 라이선스가 없어서(`No valid Unity Editor license found`) 7/13~7/14 세션처럼 실제 배치모드 컴파일·플레이스루 검증은 못 했음. 위 검증은 전부 코드 정독 + 로직 재현(아래 참고) 기반의 정적 확인. **다음에 Unity를 직접 열 때 컴파일 에러 0 확인 + PR3 테스트플랜 3항목(잡몹 5종 스폰/페이즈 전환 로그/계층별 바닥색) 실제 플레이 확인 필요.**

**오늘 한 일 (추가) — `MonsterLoreDatabase`**: worldbook.md/bestiary.md 둘 다 자체적으로 "나중에 인게임 도감 UI 만들 때 bestiary.md 파싱해서 MonsterLoreDatabase 같은 걸로 로드" 라고 명시해둔 다음 단계라 이번에 착수.
- `Assets/Scripts/Database/MonsterLoreDatabase.cs` 신규 — `bestiary.md`를 라인 파싱(`### 표시이름 (id)` 헤더 + `- 계층/분류/등급/설정:` 필드)해서 `Dictionary<string, MonsterLore>`로 적재, `Get(id)`/`TryGet(id)` 제공. 다른 Database 클래스(`EnemyDatabase` 등)와 동일한 정적 클래스 + `Get()` 스타일로 맞춤.
- **`bestiary.md`를 repo 루트에서 `Assets/StreamingAssets/bestiary.md`로 이동**: Unity는 `Assets/` 밖 파일을 빌드에 포함하지 않아서(반면 `StreamingAssets`는 원본 그대로 포함) PR2가 만든 위치 그대로는 런타임에 절대 못 읽는 상태였음 — 도감 로더를 실제로 동작시키려면 필수적인 이동. `worldbook.md`/`devlog.md`는 코드가 읽지 않는 순수 설계 문서라 루트에 그대로 둠.
- **검증**: 이 환경엔 C# 컴파일러가 없어서(csc.exe 둘 다 의존성 문제로 실행 안 됨) 파싱 로직을 Python으로 1:1 재현해 실제 `bestiary.md`로 실행 — 22종 전부 필드 정상 채워짐, `EnemyDatabase` id와 누락/잉여 0건. C# 코드 자체의 문법 컴파일은 Unity 열 때 확인 필요(위 "한계" 항목과 동일 사유).
- 아직 이 로더를 실제로 소비하는 인게임 UI(몬스터 조우 시 언락, 목록/상세 패널)는 없음 — worldbook.md에 적힌 대로 그건 별도 후속 작업.

**Git 상태 (당시)**: 로컬 `integration-test` 브랜치에 PR1+PR2+PR3 병합 + 위 신규 커밋을 올려뒀고, 그때는 origin에 push도 PR 병합도 안 한 상태로 남겨둠. → **2026-09-10에 master로 병합 완료** (아래 "PR 통합본(integration-test) master 병합" 항목 참고). 그 사이(9/5~9/9) master는 이 브랜치를 전혀 모른 채 4단계(전투 그리드 통합)로 먼저 진행돼 있었어서, 병합 시 `Enemy.cs`(이 세션의 `phases[]` vs 4단계의 `x,y,spawnId` — 둘 다 살림)와 `devlog.md`(이 섹션의 시간순 재배치)에서 충돌 해소가 필요했음.

**다음에 이어서 할 것 (당시 기준)**
- Unity 열어서 컴파일 확인 + PR3 테스트플랜 3항목 실제 플레이 검증.
- PR 3개 병합 순서/방식 결정 (구두로 이미 검증됐지만 최종 승인은 필요).
- 몬스터 도감 인게임 UI (MonsterLoreDatabase 소비하는 쪽) — 아직 미착수.
- 기존에 밀려있던 4단계(전투 그리드 통합)/맵 UI 다듬기/손맛 확인은 여전히 미착수.

---

## 4단계 — 전투를 그리드 위로 통합 (2026-09-09, ⚠️ Unity 미검증)

**배경**: 이 세션은 완전히 새 PC에서 시작 — Unity 에디터가 설치 안 된 상태로 시작해서, 아래 구현은 전부
코드만 작성하고 Unity 배치모드 컴파일/씬 자동 세팅/플레이 검증을 **하나도 못 거쳤다**. 지금까지 세션들이
전부 "배치모드로 컴파일 에러 0 확인 + 자동 테스트 통과 + 실제 플레이스루"를 거친 것과 다르게, 이번 건
**다음에 Unity를 열면 제일 먼저 전부 검증해야 한다** (아래 "검증 체크리스트" 참고).

**설계 (7/8~7/13에 미뤄뒀던 4단계 그대로 착수)**:
- 전투는 더 이상 추상 화면이 아니라, 접촉이 일어난 그 자리(플레이어/적의 실제 그리드 좌표)에서 그대로 이어진다.
- **이동**: 플레이어 턴마다 인접 1칸만 이동 가능(`BattleManager.hasMovedThisTurn`). 카드 코스트와는 별개 자원 —
  이동했다고 카드를 못 내는 건 아니고, 이동 후에도 카드를 낼 수 있다.
- **사거리**: `Card.attackRange`(기본 1=근접) 신규 필드. 단일 대상 공격/독/화상 카드만 체비쇼프 거리로 체크,
  AoE는 무관. 마법사 화염구/번개(3), 성기사 심판(2), 3계층 혼돈 화염(3)에 부여해서 클래스별로 "키이팅"이
  실제로 의미 있게 만듦(다른 카드는 전부 그대로 근접 1).
- **적 AI**: 다음 의도가 플레이어를 직접 노리는 행동(공격/독/화상)인데 인접하지 않았으면, 이번 턴은 공격
  대신 방 범위 안에서 한 칸 접근만 하고 의도는 소모하지 않는다(다음 턴에 그대로 실행). 방어/버프처럼
  자기 자신에게 거는 행동은 거리 무관하게 그대로 실행.
- **도주**: 방 경계 밖으로 이동하면 즉시 전투 종료(보상 없음). 그 순간 인접해 있던 살아있는 적 중 의도가
  공격인 애들은 "이탈 공격" 1회를 무료로 때린다 — 근접 적에 붙어있다 도주하면 위험하고, 원거리 카드로
  거리를 벌려둔 다음 도주하면 안전해지는 구조라 사거리 카드의 존재 의미가 생김.
- **방 경계 버그를 하나 발견해서 같이 고침**: 적이 `EnemyAiSystem`으로 맵 전체를 BFS로 쫓아오다 복도 등
  원래 방(RoomInfo) 밖에서 접촉하는 경우가 원래도 흔한 설계였는데(7/13 "접촉 전투 완성" 항목 참고), 그
  홈룸 사각형을 그대로 전투 경계로 쓰면 접촉 위치 자체가 이미 "방 밖" 취급되어 첫 이동에 오작동 도주가
  나거나, 집단전 무리원이 경계 밖에 있어 영원히 못 쫓아오는 상태가 될 뻔했다. `DungeonManager.BuildBattleBounds`가
  홈룸 사각형 + 플레이어/적들의 실제 접촉 좌표를 전부 포함하도록 넓힌 뒤 패딩 2칸을 더한 경계를 만들어서
  전달한다(보상/처치 판정에 쓰는 `currentRoom`은 실제 RoomInfo 그대로 유지 — 이 경계와는 별개).

**구현**:
- `System/BattleGridSystem.cs` (신규) — 전투 중 이동/도주/방-범위 BFS 접근. 던전맵 전체를 쫓는
  `EnemyAiSystem`과는 경계 조건이 달라서(전투는 방 하나로 좁고 플레이어가 도주로 나갈 수 있음) 재사용하지 않고 분리.
- `Enemy.cs` — `x, y, spawnId` 추가 (전투 중 그리드 좌표 + 도주 시 원래 EnemySpawn으로 되돌리기 위한 참조).
- `Card.cs` — `attackRange`(기본 1) 추가, 사거리>1이면 설명에 "사거리 N" 자동 표시.
- `Database/CardDatabase.cs` — 화염구/번개/심판/혼돈 화염에 사거리 부여.
- `BattleManager.cs` — `BattleState.Fled` 추가, `StartBattle`이 `DungeonFloor`/`RoomInfo`(경계)를 받도록 변경,
  `TryMovePlayer()` 신규, `UseCard()`에 사거리 체크 추가, `EnemyTurn()`이 인접 여부로 접근/공격 분기.
  **주의**: `BattleLoop`의 while 조건에 `state != Fled`를 넣었다 — 안 넣으면 Fled 상태에서 PlayerTurn도
  EnemyTurn도 아닌 채로 while이 매 프레임 그대로 다시 돌아서(코루틴이 한 프레임도 안 쉬고) 에디터가
  멈추는 무한루프에 빠진다. 이 부분이 이번 구현에서 제일 위험한 지점이라 특히 눈여겨봐서 검증할 것.
- `System/DungeonManager.cs` — `Engage()`가 적 좌표(x,y,spawnId)를 넘기고 `BuildBattleBounds()`로 전투 경계를
  만든다. `OnBattleFled()` 신규 — 도주 시 적 최종 위치/생사를 EnemySpawn에 되돌리고(죽은 적은 사망 처리,
  산 적은 위치 갱신 + Chasing 유지) 보상 없이 맵으로 복귀.
- `System/SaveSystem.cs` — `CardSnapshot.attackRange` 추가(기본 1, 없는 구버전 세이브와 하위호환).
  실제로는 CardDatabase 등록 카드는 로드 시 `CardDatabase.Create()`로 최신 정의를 다시 읽어오기 때문에
  영향이 없고(사거리 카드가 전부 등록 카드라 안전), 성소 카드처럼 스냅샷 그대로 복원되는 카드에 대비한
  것 — "모든 필드를 스냅샷한다"는 이 파일의 기존 원칙을 지키려고 추가.
- `System/UI/BattleUI.cs` — 전투 그리드 렌더링(방 범위 +1칸, 던전맵과 같은 `MapTile` 프리팹 재사용, 고정
  배치라 카메라 추종 불필요) + WASD/화살표 이동 입력 + 인접 칸 클릭 이동/적 칸 클릭 타겟 선택. 카드 버튼
  interactable에 사거리 체크(`InRangeOfTarget`) 추가 — `Refresh()` 호출 순서를 `RefreshEnemies → RefreshHand`로
  바꿔야 selectedTarget이 먼저 확정돼서 사거리 판정이 그 프레임에 바로 정확해진다(원래는 반대 순서였음).
- `Editor/SceneSetup.cs` — `BuildBattlePanel`에 그리드 영역(왼쪽) 추가, 적 상태 패널 영역을 오른쪽으로
  줄여서 배치(레이아웃 좌표는 눈으로 확인 안 한 값이라 실제로 보면 손볼 여지 있음).

**검증 체크리스트 (다음 Unity 세션에서 제일 먼저 할 것)**:
1. Unity로 프로젝트 열기 → 컴파일 에러 0 확인 (제일 걱정되는 건 `SceneSetup.cs`의 `Bind()` 리플렉션 필드명 오타).
2. `DungeonGame > 씬 자동 세팅` 실행 → 성공 로그 확인.
3. `DungeonGame > 한글 폰트 정적 베이크` 재실행 (이번에 추가된 새 문구 - "이동 완료", "도주" 등 - 글리프 반영).
4. Play 모드에서 전투 진입 → 그리드가 보이는지, 인접 칸 클릭/WASD로 실제로 이동되는지, 방 밖으로 나가면
   도주가 되고 인접했던 적이 이탈 공격을 하는지, 마법사로 화염구를 사거리 밖에서 눌러보고 버튼이
   비활성화되는지, 적이 멀리 있을 때 공격 대신 접근만 하는지 확인.
5. **제일 중요**: 도주 시나리오에서 에디터가 멈추지 않는지(위 BattleLoop 무한루프 위험 지점) 반드시 확인.
6. 복도에서 쫓아오던 적에게 붙잡혀 전투가 시작되는 경우(집단조우 포함)도 별도로 테스트 — `BuildBattleBounds`가
   실제로 방 밖 접촉을 잘 처리하는지 확인 필요.
7. 레이아웃(그리드/적 패널 좌우 배치)이 겹치거나 잘리면 `SceneSetup.BuildBattlePanel`의 `Anchor()` 좌표 조정.

**다음에 이어서 할 것**
- 위 검증 체크리스트 전부 통과시키기.
- 맵/마커 UI 다듬기 (7/8 "미해결 UI 피드백" 참고, 아직 미착수).
- 실제 플레이 손맛 확인: 적별 골드 보상 체감, 집단전 타겟 클릭, 코스트 소진 자동 턴종료 (7/14부터 계속 미룸).

---

## Unity 검증 1~7 시도 — 1~3 배치모드 통과, 4단계 자동화는 실패로 보류 (2026-09-10)

**환경**: 이 PC엔 Unity 6000.6.0f1만 설치돼 있고 프로젝트도 이미 이 버전으로 올라가 있어서, 배치모드로 1~3단계를 직접 실행해볼 수 있었음.

**1~3단계 — 배치모드로 전부 통과**:
- 컴파일: `error CS` 0건, exit 0 (스크립트 866개, 컴파일 1.1초).
- `DungeonGame > 씬 자동 세팅`: "완전히 연결된 씬 세팅 완료!" 로그 확인, exit 0.
- `DungeonGame > 한글 폰트 정적 베이크`: "NotoSansKR SDF Static.asset로 폴백 교체됨" 로그 확인, exit 0.
- 즉 저번에 걱정했던 `SceneSetup.cs`의 `Bind()` 리플렉션 필드명 오타 같은 컴파일/씬 구성 문제는 없음.

**4~7단계 — 자동 플레이테스트 시도, 실패**:
- `Assets/Scripts/Editor/PlaytestRunner.cs`(임시, 확인 후 삭제 예정 — 이 세션 시작 시점에 이미 존재했음, 아마 오전에 끊긴 세션의 산물)로 `DungeonManager`/`BattleManager` API를 직접 호출해 맵 로밍→전투→도주(무한루프 워치독 포함)→복도 추격까지 자동으로 몰아붙이는 스크립트를 실행 시도.
- `-batchmode -nographics -executeMethod PlaytestRunner.Run`으로 실행했는데 `EditorApplication.EnterPlaymode()` 호출 이후 **9분간 로그가 전혀 진행 안 되고**(`[PLAYTEST]` 태그 한 줄도 안 찍힘) CPU만 계속 오르는 채로 멈춰서 강제 종료(`taskkill /F`)함.
- 로그 마지막에 `UnityEditor.Search.SearchInit.IndexationOnStartup()`에서 난 `ArgumentOutOfRangeException`이 있었음(우리 게임 코드와 무관한 Unity 내부 Search 인덱서 문제) — 이게 `EditorApplication.delayCall` 큐를 막아서 `Bootstrap()` 재시도 콜백이 영영 못 돌았을 가능성이 있으나 확정은 못함.
- **결국 4단계(전투 그리드 통합) 실제 동작은 이번에도 검증 못함.** 자동화는 포기하고 다음엔 Unity 에디터를 직접 열어서 수동으로 확인하기로 함.

**집에서(다음 세션) 할 일 — Unity 에디터 직접 열어서 수동 검증**:
1. Unity Hub에서 `DungeonGame` 프로젝트 열기 (6000.6.0f1). 강제종료 직후라 "프로젝트가 이미 열려있음" 류 경고가 뜰 수 있는데 무시하고 진행.
2. 콘솔에 컴파일 에러 없는지 육안 확인.
3. Play 버튼 → 전투 진입 → 그리드가 보이는지, 인접 칸 클릭/WASD로 실제로 이동되는지 확인.
4. 방 밖으로 나가서 **도주** 시도 — **이때 에디터가 멈추는지가 제일 중요** (멈추면 `BattleManager.BattleLoop`의 `state != Fled` 조건 관련 버그, 9/9 항목 참고). 멈추면 강제 종료하기 전에 Unity 콘솔/Stack Trace부터 확인.
5. 도주 시 인접했던 적의 "이탈 공격" 발동 확인, 마법사 화염구를 사거리 밖에서 눌러 버튼이 비활성화되는지, 적이 멀리 있을 때 공격 대신 접근만 하는지 확인.
6. 복도에서 쫓아오던 적에게 붙잡혀 전투가 시작되는 경우(집단조우 포함)도 테스트 — `BuildBattleBounds`가 방 밖 접촉을 잘 처리하는지.
7. 레이아웃(그리드/적 패널 좌우 배치)이 겹치거나 잘리면 `SceneSetup.BuildBattlePanel`의 `Anchor()` 좌표 조정.
8. 위 확인이 다 끝나면 `Assets/Scripts/Editor/PlaytestRunner.cs` 삭제 (문제 재현용으로 더 필요하면 남겨둬도 됨).

**참고**: 이번 세션에서 만든 임시 배치모드 로그 파일(`compile_check.log`/`scene_setup.log`/`font_bake.log`/`playtest.log`, 프로젝트 루트)은 검증용이라 지워도 무방.

---

## 4단계 코드 리뷰 결과 반영 (2026-09-10, Unity 실행 검증은 여전히 안 됨)

배치모드 검증(위 항목)은 컴파일/씬/폰트까지만 통과했고 실제 플레이 확인은 못한 채로, `/code-review a789e70 --level high`로 4단계(전투 그리드 통합) 커밋을 정적 리뷰함. 실제 버그 3건 + 중복/성능 3건 발견 → 실제 버그 3건을 고쳤는데, 그 수정 2건이 서로 상호작용해서 새 버그 2건을 또 만들어서 한 번 더 고침(교차 검증 재리뷰로 발견). 전부 배치모드 컴파일 확인만 거쳤고 **Play 모드로 직접 돌려본 적은 없음** — 아래 전부 다음 Unity 세션에서 실제 확인 필요.

**1차 수정 — 리뷰에서 발견한 실제 버그 3건**:
- `BattleUI.cs` 적 패널로 타겟 재선택 시 `RefreshEnemies()`만 호출하고 `RefreshHand()`는 안 불러서 카드 활성화(사거리 판정)가 즉시 안 맞던 문제 → 타겟 클릭 핸들러에 `RefreshHand()` 추가.
- `DungeonManager.Engage()` — 플레이어가 적 타일로 직접 걸어들어가 조우하면 그 적의 좌표가 플레이어 좌표와 겹친 채로 전투가 시작돼서(`BattleUI`가 `isPlayer` 칸이면 `occupant=null` 처리) 그리드에 안 보이고 클릭 타겟팅도 안 되던 문제 → `ResolvePlayerTileOverlap`/`FindNearestFreeTile`/`IsFreeTile` 신규로 겹친 적을 인접한 빈 칸(4방향→대각선→반경 확장 순)으로 밀어냄.
- `BattleGridSystem.TryMovePlayer` — 대각선 이동 시 목적지 칸만 체크하고 지나가는 두 직교 칸은 안 봐서, 양쪽 다 벽인 코너를 대각선으로 뚫고 지나갈 수 있던 문제(WASD/던전맵은 4방향 전용이라 여기만 가능했던 구멍) → 대각선 이동은 양쪽 직교 칸이 둘 다 걸을 수 있어야 허용하도록 체크 추가.

**2차 수정 — 위 수정 2건이 만든 새 버그 2건 (교차 검증 재리뷰로 발견)**:
- `FindNearestFreeTile`의 폴백 반경이 4칸까지 확장되는데 `BuildBattleBounds`의 패딩은 2칸뿐이라, 좁은 코너에서 밀려난 적이 전투 경계(`battleBounds`) 밖으로 나가면 `BattleGridSystem.BfsPath`가 `room.Contains` 필터에 막혀 그 적이 전투 내내 접근/공격을 영영 못 하고(그리드에 렌더링도 안 됨) 멈춰버리는 문제 → 폴백 반경을 패딩과 맞춰 2로 제한.
- `BattleGridSystem`에 추가한 대각선 코너컷 체크가 `BattleUI.RepaintGridTiles`의 `isAdjacent`(하이라이트/클릭 가능 판정)엔 반영이 안 돼서, 코너 막힌 대각선 타일이 여전히 밝게+클릭 가능하게 보이는데 실제로 누르면 `TryMovePlayer`가 조용히 `Blocked`를 반환해 아무 반응이 없는 "죽은 클릭"이 생기던 문제(7/7에 있었던 "클릭해도 안 움직이는 칸" 증상과 같은 사용자 경험 재발) → `BattleGridSystem.CanStepTo(floor, fx, fy, dx, dy)` 공용 헬퍼로 뽑아서 `TryMovePlayer`와 `BattleUI.RepaintGridTiles` 양쪽이 같은 기준을 쓰게 함.

**검증**: 5건 전부 배치모드 컴파일 에러 0 확인(2회 재확인). **Play 모드 실제 동작은 미검증** — 특히 접촉 조우 시 밀려난 적 위치, 좁은 코너에서의 대각선 이동/타일 하이라이트 일치 여부는 9/9 항목의 기존 체크리스트(4번 도주 무한루프 등)에 반드시 같이 넣어서 확인할 것.

---

## 전체 브랜치 조사 + 로드맵 수립 + PR 통합본(integration-test) master 병합 (2026-09-10)

**배경**: `git branch -a`로 원격에 master 말고도 7개 브랜치가 더 있는 걸 발견 — 9/3 세션(바로 위 항목)이 만든 `integration-test`(PR1+PR2+PR3 통합본)가 끝내 master에 병합되지 않은 채 방치돼 있었고, 그 사이 master는 7/14(`7ad21d1`)에서 곧장 9/9 4단계로 건너뛰어버려서 두 라인이 갈라진 상태였음. 추가로 `노트북-로컬` 브랜치엔 devlog 3단계(맵카드)와는 다른 설계의 경쟁 "3단계"(`ExplorationCostSystem.cs` 기반 탐사 코스트+기습+부서지는 벽)가, `본체-로컬`엔 코드 변경 없는 상태 스냅샷 html 1개가 있었음.

**전체 조사 결론**:
- **`노트북-로컬`의 경쟁 3단계는 병합 보류** — master 3단계(코스트를 맵/전투 공용 자원으로)를 4단계가 그대로 전제하고 있어서(전투 첫 턴은 맵에서 들고 온 코스트로 시작), 자원을 아예 분리하는 노트북 설계를 그대로 얹으면 4단계 설계가 무너짐. "기습"/"부서지는 벽" 아이디어 자체는 나중에 master의 단일 코스트 자원 위에 재설계해서 살릴 수 있음 — 폐기 아님, 보류.
- **`integration-test`(PR1+PR2+PR3)는 병합 진행** — `Enemy.cs`에서만 master(4단계, `x/y/spawnId`)와 겹치는데 서로 다른 필드/메서드 영역이라 충돌이 작고, `FloorGenerator.cs`/`DungeonMapUI.cs`는 master 4단계가 아예 손대지 않은 파일이라 저위험 판단.
- **본체-로컬**은 코드 변경이 없어 병합 대상 아님(참고 문서로만 남김).

**병합 작업**:
- `git merge --no-commit --no-ff origin/integration-test` — 예상대로 `Enemy.cs`, `devlog.md` 2개만 충돌, 나머지(`EnemyData.cs`/`EnemyDatabase.cs`/`EnemyFactory.cs`/`FloorGenerator.cs`/`DungeonMapUI.cs`/`GameOverUI.cs` + 신규 `MonsterLoreDatabase.cs`/`bestiary.md`/`worldbook.md`/`CLEANUP.md`)는 자동 병합됨.
- `Enemy.cs` 충돌 해소: master의 `x, y, spawnId`(4단계, 전투 그리드 좌표)와 `integration-test`의 `phases`/`currentPhaseIndex`(보스 페이즈제)를 **둘 다 유지** — 서로 다른 관심사라 공존 가능.
- `devlog.md` 충돌 해소: 9/3 세션 기록이 시간순으로 9/9(4단계) 이전에 오도록 재배치.

**완료**: 병합 결과 Unity 배치모드 컴파일 확인(에러 0) → 신규 에셋 `.meta` 3개(`MonsterLoreDatabase.cs.meta`/`StreamingAssets.meta`/`bestiary.md.meta`, integration-test에 애초에 없던 것 — Unity 라이선스 없는 환경에서 작성돼서 한 번도 임포트된 적이 없었음) 생성해서 커밋 → `origin/master`에 push 완료(`151f96e..c3bb290`).

**아직 안 한 것 (다음에 할 일)**:
- PR3 자체 테스트플랜(잡몹 5종 스폰/페이즈 전환 로그/계층별 바닥색) + 몬스터 도감 UI는 여전히 미착수 (9/3 세션이 이미 남긴 숙제, 이번 병합으로도 안 풀림).
- **대각선 이동 비대칭 미해결 이슈**: `fix/diagonal-move-gameover-label`(PR1, 9/2)은 "적 추적 BFS가 4방향뿐이라 대각선을 허용하면 적이 못 쫓아오는 도주 경로가 생긴다"는 근거로 오버월드 대각선을 완전히 제거했음. 반면 오늘 세션 앞서 4단계 전투 그리드에는 같은 문제(코너컷)를 막기만 하고 대각선 자체는 유지했는데, `BattleGridSystem.StepEnemyToward`의 접근 BFS도 똑같이 4방향뿐이라 이론상 같은 종류의 구멍이 전투 그리드에도 남아있을 수 있음. 4단계 devlog는 "사거리 카드로 키이팅"을 목적으로 명시했지만, 이게 "의도된 설계"인지 "PR1이 이미 한 번 고친 것과 같은 결함"인지는 아직 판단을 안 함 — 다음 Unity 수동 검증(9/9 체크리스트) 때 전투 그리드에서 적이 대각선 방향으로만 도주 가능한 경로가 실제로 존재하는지 직접 확인하고 필요하면 PR1과 같은 방식(대각선 완전 제거)으로 통일할지 결정할 것.
- **브랜치 정리는 Play 모드 검증 끝난 뒤로 보류(사용자 결정, 2026-09-10)**: `CLEANUP.md` 3번 항목대로 병합에 쓰인 4개 브랜치(`fix/diagonal-move-gameover-label`/`docs/worldbook-and-bestiary`/`feature/monster-roster-and-map-colors`/`integration-test`)와 백업용 `checkpoint/before-pr-merge-20260903`을 삭제할 수 있는 상태지만, 병합 결과가 실제 플레이에서 안정적인지 아직 아무도 눈으로 확인 못 했으므로 급하게 지우지 않기로 함. **9/9 Unity 수동 검증 체크리스트(1~8번)를 전부 통과한 뒤에** 이 브랜치들 삭제 여부를 다시 물어볼 것.

> **게임이 완성됐으면 이 파일 삭제해라.** (전체 정리 체크리스트는 `CLEANUP.md` 참고 — 이 파일 말고도 지워야 할 게 있음)
