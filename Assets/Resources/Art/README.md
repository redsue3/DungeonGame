# 그림 넣는 곳 (Art)

이 폴더에 **정해진 이름**으로 그림 파일(png/jpg)을 넣으면 게임이 자동으로 불러다 씁니다.
파일이 없으면 지금처럼 색 박스와 글자로 그려지니, 원하는 것부터 하나씩 넣으면 됩니다.

- 파일 이름은 아래 표의 영어 이름 그대로(소문자) 써 주세요. 확장자는 png 추천.
- Unity가 열려 있으면 넣는 즉시 Sprite로 자동 임포트됩니다(설정 손댈 필요 없음).
- 그림 비율: 캐릭터/몬스터/카드는 정사각형에 가까운 그림, 배경은 16:9(1920×1080) 권장.

## Backgrounds/ — 화면 배경

| 파일 이름 | 쓰이는 화면 |
|---|---|
| `title.png` | 직업 선택(첫 화면) |
| `map.png` | 던전 맵 |
| `battle.png` | 전투 |
| `reward.png` | 전투 보상 |
| `rest.png` | 휴식 |
| `shop.png` | 상점 |
| `shrine.png` | 성소 |
| `gameover.png` | 게임 오버 |
| `victory.png` | 승리 |

계층마다 다른 배경을 쓰려면 뒤에 `_계층번호`를 붙이세요. 예: `map_2.png`, `battle_3.png`.
계층별 그림이 없으면 공용 그림(`map.png`)을 씁니다.

## Characters/ — 플레이어 캐릭터 (맵과 전투 칸에 표시)

| 파일 이름 | 직업 |
|---|---|
| `warrior.png` | 전사 |
| `rogue.png` | 도적 |
| `mage.png` | 마법사 |
| `paladin.png` | 성기사 |

## Enemies/ — 몬스터 (맵과 전투 칸에 표시)

| 파일 이름 | 몬스터 |
|---|---|
| `slime.png` | 슬라임 |
| `thief.png` | 도둑 |
| `bat_swarm.png` | 박쥐떼 |
| `goblin_scout.png` | 고블린 정찰병 |
| `skeleton_soldier.png` | 해골 병사 |
| `goblin_elite.png` | 고블린 두목 |
| `skeleton_elite.png` | 해골 기사 |
| `giant_slime_boss.png` | 거대 슬라임 |
| `orc.png` | 오크 전사 |
| `poison_spider.png` | 독 거미 |
| `orc_grunt.png` | 오크 졸병 |
| `swamp_viper.png` | 늪지 독사 |
| `bandit_raider.png` | 도적 약탈자 |
| `dark_mage_elite.png` | 흑마법사 |
| `orc_warchief_boss.png` | 오크 족장 |
| `cursed_warrior.png` | 저주받은 전사 |
| `shadow_witch.png` | 그림자 마녀 |
| `bone_golem.png` | 뼈 골렘 |
| `wraith.png` | 원혼 |
| `fallen_knight.png` | 타락한 기사 |
| `death_knight_elite.png` | 죽음의 기사 |
| `demon_lord_boss.png` | 마왕 |

## Cards/ — 카드 그림 (카드 글자 뒤에 반투명으로 깔림)

| 파일 이름 | 카드 |
|---|---|
| `strike.png` | 전사의 일격 |
| `defend.png` | 방어 |
| `heavy_strike.png` | 분노의 일격 |
| `dagger.png` | 단도 |
| `evade.png` | 회피 |
| `combo.png` | 연격 |
| `fireball.png` | 화염구 |
| `mana_shield.png` | 마력 방벽 |
| `lightning.png` | 번개 |
| `draw_spell.png` | 드로우 마법 |
| `holy_strike.png` | 성스러운 일격 |
| `shield_bash.png` | 방패 강타 |
| `sacred_heal.png` | 신성한 치유 |
| `judgement.png` | 심판 |
| `double_strike.png` | 연속 강타 |
| `iron_defense.png` | 철벽 방어 |
| `power_up.png` | 힘 강화 |
| `quick_draw.png` | 신속 드로우 |
| `acid_slime.png` | 산성 슬라임 |
| `holy_blade.png` | 성검 |
| `whirlwind.png` | 회오리 베기 |
| `poison_blade.png` | 독 단검 |
| `flame_burst.png` | 화염 폭발 |
| `mana_surge.png` | 마나 폭주 |
| `fortify.png` | 요새화 |
| `dark_pact.png` | 어둠의 계약 |
| `divine_shield.png` | 신성한 방패 |
| `soul_slash.png` | 영혼 베기 |
| `perfect_guard.png` | 완벽한 가드 |
| `plague_strike.png` | 역병 일격 |
| `soul_drain.png` | 영혼 흡수 |
| `chaos_flame.png` | 혼돈 화염 |
| `thunder_storm.png` | 뇌우 |
| `blessing.png` | 축복 |
| `quick_slash.png` | 빠른 베기 |
| `solid_strike.png` | 묵직한 일격 |
| `puncture.png` | 관통 |
| `guard_up.png` | 수비 태세 |
| `steady_guard.png` | 굳건한 수비 |
| `counter_stance.png` | 반격 태세 |
| `focus.png` | 집중 |
| `adrenaline.png` | 아드레날린 |
| `second_wind.png` | 재정비 |
| `toughen.png` | 담금질 |
| `war_cry.png` | 기합 |
| `quick_hands.png` | 빠른 손놀림 |
| `ember.png` | 불씨 |
| `short_prayer.png` | 짧은 기도 |
