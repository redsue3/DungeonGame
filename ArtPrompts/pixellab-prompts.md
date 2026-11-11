# PixelLab 그림 프롬프트

> 2026-11-10 작성. 같은 내용을 복사 버튼과 함께 보는 페이지는 아티팩트 '황금 곳간 작업표'에 있다.
> 만든 그림은 아래 경로(`Assets/Sprites/` 기준) 이름으로 저장한다. 그림을 화면에 띄우는 코드는 아직 없다.

## 도구별 공통 설정

- **캐릭터 만들기 (Create character):** 크기 48×48 · 시점 low top-down · 4방향 · 검은 외곽선 한 줄(single color black outline) · 음영 basic · 디테일 medium · 배경 없음
- **캐릭터 만들기 (Create character):** 크기 64×64 · 시점 low top-down · 4방향 · 검은 외곽선 한 줄 · 음영 medium · 디테일 high · 배경 없음
- **애니메이션 (Animate character):** 방금 만든 캐릭터 선택 · 4방향 모두 · 걷기·대기는 템플릿, 공격은 직접 입력(유료 1단계 이상)
- **이미지 만들기 (Create image):** 크기 64×64 · 시점 side 또는 자유 · 외곽선 선택 · 음영 medium · 디테일 high · 배경 있음
- **이미지 만들기 (Create image):** 크기 32×32 · 검은 외곽선 한 줄 · 음영 basic · 디테일 medium · 배경 없음(no background)
- **탑다운 타일셋 (Create top-down tileset):** 타일 크기 24 · 아래(lower)=바닥, 위(upper)=벽 · 전환(transition) 칸은 짧게 · standard 모드
- **맵 오브젝트 (Create map object):** 크기 48×48 · 시점 low top-down · 검은 외곽선 한 줄 · 배경 없음

## 플레이어 캐릭터

### 전사 가론 — `Characters/warrior.png`

도구: 캐릭터 만들기 (Create character)

```
battle-worn mercenary warrior, royal blue full plate armor with silver trim, dented and scratched, short brown hair, broadsword in right hand, round wooden shield on left arm, faded red scarf, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 도적 네린 — `Characters/rogue.png`

도구: 캐릭터 만들기 (Create character)

```
back-alley rogue thief, dark purple hooded cloak, black leather armor, lower face covered by a cloth mask, a dagger in each hand, small gold coin pouch on belt, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 마법사 아젤 — `Characters/mage.png`

도구: 캐릭터 만들기 (Create character)

```
young scholar mage, long purple robe with gold trim, pointed hat, round glasses, tall wooden staff topped with a glowing blue crystal orb, notebook hanging from belt, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 성기사 로젠 — `Characters/paladin.png`

도구: 캐릭터 만들기 (Create character)

```
temple paladin, white and gold full plate armor, cross emblem on chest, holy mace in right hand, small kite shield, faint golden light around head, chibi proportions, dark fantasy dungeon crawler, muted colors
```


## 애니메이션

### 전사 걷기 — `Characters/warrior_walk`

도구: 애니메이션 (Animate character)

```
템플릿 walking (텍스트 입력 없음)
```

### 전사 대기 — `Characters/warrior_idle`

도구: 애니메이션 (Animate character)

```
템플릿 breathing-idle (텍스트 입력 없음)
```

### 전사 공격 — `Characters/warrior_attack`

도구: 애니메이션 (Animate character)

```
swings the broadsword overhead and slams it down in front, shield stays raised
```

### 도적 걷기 — `Characters/rogue_walk`

도구: 애니메이션 (Animate character)

```
템플릿 walking (텍스트 입력 없음)
```

### 도적 대기 — `Characters/rogue_idle`

도구: 애니메이션 (Animate character)

```
템플릿 breathing-idle (텍스트 입력 없음)
```

### 도적 공격 — `Characters/rogue_attack`

도구: 애니메이션 (Animate character)

```
quick double stab forward with both daggers, crossing slash
```

### 마법사 걷기 — `Characters/mage_walk`

도구: 애니메이션 (Animate character)

```
템플릿 walking (텍스트 입력 없음)
```

### 마법사 대기 — `Characters/mage_idle`

도구: 애니메이션 (Animate character)

```
템플릿 breathing-idle (텍스트 입력 없음)
```

### 마법사 공격 — `Characters/mage_attack`

도구: 애니메이션 (Animate character)

```
raises the staff, crystal orb flashes, casts a fireball forward
```

### 성기사 걷기 — `Characters/paladin_walk`

도구: 애니메이션 (Animate character)

```
템플릿 walking (텍스트 입력 없음)
```

### 성기사 대기 — `Characters/paladin_idle`

도구: 애니메이션 (Animate character)

```
템플릿 breathing-idle (텍스트 입력 없음)
```

### 성기사 공격 — `Characters/paladin_attack`

도구: 애니메이션 (Animate character)

```
swings the glowing mace down, burst of golden holy light on impact
```


## 몬스터

### 슬라임 (1층 일반) — `Enemies/slime.png`

도구: 캐릭터 만들기 (Create character)

```
translucent blue-green slime blob, small glowing core of leftover life force inside, dripping, simple cute face, dark fantasy dungeon crawler, muted colors
```

### 도둑 (1층 일반) — `Enemies/thief.png`

도구: 캐릭터 만들기 (Create character)

```
undead petty thief, pale grey skin, tattered brown hood, rusty dagger, hollow glowing eyes, hunched sneaking pose, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 박쥐떼 (1층 일반) — `Enemies/bat_swarm.png`

도구: 캐릭터 만들기 (Create character)

```
swarm of three small black bats flying close together, red glowing eyes, ragged wings, dark fantasy dungeon crawler, muted colors
```

### 고블린 정찰병 (1층 일반) — `Enemies/goblin_scout.png`

도구: 캐릭터 만들기 (Create character)

```
goblin scout, green skin, pointy ears, leather cap, short spear, spyglass on belt, crouching alert pose, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 해골 병사 (1층 일반) — `Enemies/skeleton_soldier.png`

도구: 캐릭터 만들기 (Create character)

```
skeleton soldier, cracked yellowed bones, rusty short sword, broken round shield, faint blue glow in eye sockets, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 고블린 두목 (1층 엘리트) — `Enemies/goblin_elite.png`

도구: 캐릭터 만들기 (Create character)

```
goblin chieftain, larger and muscular goblin, red war paint, bone necklace, spiked wooden club, furious face, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 해골 기사 (1층 엘리트) — `Enemies/skeleton_elite.png`

도구: 캐릭터 만들기 (Create character)

```
skeleton knight in rusted plate armor, long sword, tattered blue cape, ghostly blue eye glow, upright proud stance, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 거대 슬라임 (1층 보스) — `Enemies/giant_slime_boss.png`

도구: 캐릭터 만들기 (Create character)

```
giant slime boss, huge translucent blue-green blob, old swords and bones floating inside, large glowing core, dripping onto the stone floor, menacing face, dark fantasy dungeon crawler, muted colors
```

### 오크 전사 (2층 일반) — `Enemies/orc.png`

도구: 캐릭터 만들기 (Create character)

```
orc warrior, green skin, tusks, leather and iron armor, big iron axe, tribal markings, chibi proportions, dark fantasy dungeon crawler, muted earthy colors
```

### 독 거미 (2층 일반) — `Enemies/poison_spider.png`

도구: 캐릭터 만들기 (Create character)

```
giant poison spider, dark purple body with toxic green markings, dripping venom fangs, eight glowing eyes, dark fantasy dungeon crawler, muted earthy colors
```

### 오크 졸병 (2층 일반) — `Enemies/orc_grunt.png`

도구: 캐릭터 만들기 (Create character)

```
orc grunt, smaller young orc, simple leather vest, wooden club, oversized dented iron helmet, chibi proportions, dark fantasy dungeon crawler, muted earthy colors
```

### 늪지 독사 (2층 일반) — `Enemies/swamp_viper.png`

도구: 캐릭터 만들기 (Create character)

```
swamp viper, coiled green snake with mossy scales, hood raised, venom dripping from fangs, small puddle of swamp water, dark fantasy dungeon crawler, muted earthy colors
```

### 도적 약탈자 (2층 일반) — `Enemies/bandit_raider.png`

도구: 캐릭터 만들기 (Create character)

```
human bandit raider, red bandana, patched leather armor decorated with orc trophies, curved scimitar, scarred face, chibi proportions, dark fantasy dungeon crawler, muted earthy colors
```

### 흑마법사 (2층 엘리트) — `Enemies/dark_mage_elite.png`

도구: 캐릭터 만들기 (Create character)

```
corrupted dark mage, black and crimson hooded robe, purple veins of dark magic on hands and face, skull-topped staff, floating dark orb, chibi proportions, dark fantasy dungeon crawler, muted colors
```

### 오크 족장 (2층 보스) — `Enemies/orc_warchief_boss.png`

도구: 캐릭터 만들기 (Create character)

```
orc warchief boss, massive orc, horned iron helmet, fur cloak, giant two-handed cleaver, battle scars, eyes glowing with the dungeon's golden power, dark fantasy dungeon crawler, muted earthy colors
```

### 저주받은 전사 (3층 일반) — `Enemies/cursed_warrior.png`

도구: 캐릭터 만들기 (Create character)

```
cursed veteran adventurer, corroded armor with gold coins fused into it, purple cursed glow leaking from cracks, heavy greatsword, chibi proportions, dark fantasy dungeon crawler, purple and gold tones
```

### 그림자 마녀 (3층 일반) — `Enemies/shadow_witch.png`

도구: 캐릭터 만들기 (Create character)

```
shadow witch scholar, tattered dark violet robe, floating open spellbook, shadowy clawed hands, small purple flames around her, chibi proportions, dark fantasy dungeon crawler, purple tones
```

### 뼈 골렘 (3층 일반) — `Enemies/bone_golem.png`

도구: 캐릭터 만들기 (Create character)

```
bone golem, hulking body made of many bones and skulls bound together, purple glow between the bones, heavy fists, dark fantasy dungeon crawler, purple tones
```

### 원혼 (3층 일반) — `Enemies/wraith.png`

도구: 캐릭터 만들기 (Create character)

```
wraith, translucent floating ghost without legs, tattered grey shroud, pale purple glow, long reaching hands, sorrowful hollow face, dark fantasy dungeon crawler, purple tones
```

### 타락한 기사 (3층 일반) — `Enemies/fallen_knight.png`

도구: 캐릭터 만들기 (Create character)

```
fallen paladin knight, once white and gold plate armor now tarnished black, cracked cross emblem on chest, dark mace, torn tabard, chibi proportions, dark fantasy dungeon crawler, purple tones
```

### 죽음의 기사 (3층 엘리트) — `Enemies/death_knight_elite.png`

도구: 캐릭터 만들기 (Create character)

```
death knight, black spiked plate armor, glowing purple eyes inside the helmet, runic greatsword, tattered black cape, chibi proportions, dark fantasy dungeon crawler, purple tones
```

### 마왕 (3층 보스) — `Enemies/demon_lord_boss.png`

도구: 캐릭터 만들기 (Create character)

```
demon lord boss, horned demon king, dark spiked crown, purple and gold robes, glowing eyes, heavy ledger book chained to his belt, piles of gold coins at his feet, dark fantasy dungeon crawler, purple and gold tones
```


## 카드 그림

### 전사의 일격 — `Cards/strike.png`

도구: 이미지 만들기 (Create image)

```
broadsword slash with a bright white arc trail, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 방어 — `Cards/defend.png`

도구: 이미지 만들기 (Create image)

```
round wooden shield raised to block, sparks flying off the rim, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 분노의 일격 — `Cards/heavy_strike.png`

도구: 이미지 만들기 (Create image)

```
huge two-handed overhead sword strike, red rage aura, stone floor cracking, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 단도 — `Cards/dagger.png`

도구: 이미지 만들기 (Create image)

```
single dagger thrown forward with motion lines, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 회피 — `Cards/evade.png`

도구: 이미지 만들기 (Create image)

```
hooded figure dodging sideways leaving a purple afterimage, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 연격 — `Cards/combo.png`

도구: 이미지 만들기 (Create image)

```
two daggers slashing in an X pattern with crossing trails, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 화염구 — `Cards/fireball.png`

도구: 이미지 만들기 (Create image)

```
blazing fireball flying forward with a trail of embers, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 마력 방벽 — `Cards/mana_shield.png`

도구: 이미지 만들기 (Create image)

```
translucent blue magic barrier with a hexagon pattern, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 번개 — `Cards/lightning.png`

도구: 이미지 만들기 (Create image)

```
lightning bolt striking down from dark clouds, bright flash, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 드로우 마법 — `Cards/draw_spell.png`

도구: 이미지 만들기 (Create image)

```
glowing cards floating out of an open spellbook, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 성스러운 일격 — `Cards/holy_strike.png`

도구: 이미지 만들기 (Create image)

```
mace striking with a burst of golden holy light, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 방패 강타 — `Cards/shield_bash.png`

도구: 이미지 만들기 (Create image)

```
kite shield bashing forward with an impact shockwave, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 신성한 치유 — `Cards/sacred_heal.png`

도구: 이미지 만들기 (Create image)

```
warm golden healing light descending around a glowing cross, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 심판 — `Cards/judgement.png`

도구: 이미지 만들기 (Create image)

```
giant golden sword of light falling from the sky, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 연속 강타 — `Cards/double_strike.png`

도구: 이미지 만들기 (Create image)

```
two consecutive sword slashes forming a double arc, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 철벽 방어 — `Cards/iron_defense.png`

도구: 이미지 만들기 (Create image)

```
iron tower shield wall with rivets and metal plates, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 힘 강화 — `Cards/power_up.png`

도구: 이미지 만들기 (Create image)

```
clenched gauntlet fist surrounded by a red power aura, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 신속 드로우 — `Cards/quick_draw.png`

도구: 이미지 만들기 (Create image)

```
hand quickly drawing three cards with speed lines, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 산성 슬라임 — `Cards/acid_slime.png`

도구: 이미지 만들기 (Create image)

```
green acid slime splash with sizzling bubbles, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 성검 — `Cards/holy_blade.png`

도구: 이미지 만들기 (Create image)

```
upright sword glowing with holy light, small healing sparkles around it, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 회오리 베기 — `Cards/whirlwind.png`

도구: 이미지 만들기 (Create image)

```
spinning sword slash creating a whirlwind of blades, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 독 단검 — `Cards/poison_blade.png`

도구: 이미지 만들기 (Create image)

```
dagger dripping toxic green poison, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 화염 폭발 — `Cards/flame_burst.png`

도구: 이미지 만들기 (Create image)

```
large fire explosion spreading outward, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 마나 폭주 — `Cards/mana_surge.png`

도구: 이미지 만들기 (Create image)

```
blue mana energy erupting from a cracked crystal, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 요새화 — `Cards/fortify.png`

도구: 이미지 만들기 (Create image)

```
stone fortress wall rising from the ground, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 어둠의 계약 — `Cards/dark_pact.png`

도구: 이미지 만들기 (Create image)

```
contract scroll signed in blood, wrapped in dark purple flame, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 신성한 방패 — `Cards/divine_shield.png`

도구: 이미지 만들기 (Create image)

```
golden shield with wings of light spreading behind it, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 영혼 베기 — `Cards/soul_slash.png`

도구: 이미지 만들기 (Create image)

```
ghostly blue sword slash cutting through a wailing spirit, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 완벽한 가드 — `Cards/perfect_guard.png`

도구: 이미지 만들기 (Create image)

```
flawless silver parry, blade meeting blade with a bright glint, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 역병 일격 — `Cards/plague_strike.png`

도구: 이미지 만들기 (Create image)

```
strike trailing a rotten green plague cloud, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 영혼 흡수 — `Cards/soul_drain.png`

도구: 이미지 만들기 (Create image)

```
wisps of souls being pulled into a dark open hand, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 혼돈 화염 — `Cards/chaos_flame.png`

도구: 이미지 만들기 (Create image)

```
chaotic swirling purple and red flames, unstable and dangerous, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 뇌우 — `Cards/thunder_storm.png`

도구: 이미지 만들기 (Create image)

```
storm clouds with several lightning bolts striking at once, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 축복 — `Cards/blessing.png`

도구: 이미지 만들기 (Create image)

```
warm golden light rays with drifting white feathers, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 빠른 베기 — `Cards/quick_slash.png`

도구: 이미지 만들기 (Create image)

```
quick light sword slash leaving a thin silver arc, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 묵직한 일격 — `Cards/solid_strike.png`

도구: 이미지 만들기 (Create image)

```
heavy war hammer blow with a cloud of dust on impact, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 관통 — `Cards/puncture.png`

도구: 이미지 만들기 (Create image)

```
spear thrust piercing forward, a small green poison drop on the tip, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 수비 태세 — `Cards/guard_up.png`

도구: 이미지 만들기 (Create image)

```
fighter raising a shield into a guard stance, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 굳건한 수비 — `Cards/steady_guard.png`

도구: 이미지 만들기 (Create image)

```
shield planted firmly on stone ground, braced and unmovable, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 반격 태세 — `Cards/counter_stance.png`

도구: 이미지 만들기 (Create image)

```
sword held low in a counter stance, a glint on the blade, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 집중 — `Cards/focus.png`

도구: 이미지 만들기 (Create image)

```
closed eye with a glowing blue sigil above it, calm concentration, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 아드레날린 — `Cards/adrenaline.png`

도구: 이미지 만들기 (Create image)

```
pounding red heart with sharp energy lines, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 재정비 — `Cards/second_wind.png`

도구: 이미지 만들기 (Create image)

```
bandaged hand with a swirl of green healing breath, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```

### 담금질 — `Cards/toughen.png`

도구: 이미지 만들기 (Create image)

```
glowing hot blade being tempered on an anvil, sparks flying, pixel art card illustration, dark fantasy, dramatic lighting, centered, dark vignette background
```


## 유물

### 철심 — `Relics/iron_heart.png`

도구: 이미지 만들기 (Create image)

```
heart-shaped iron charm with rivets, pixel art item icon, dark fantasy, centered
```

### 전투 목걸이 — `Relics/battle_torc.png`

도구: 이미지 만들기 (Create image)

```
bronze torc necklace with a red gem, pixel art item icon, dark fantasy, centered
```

### 수호자의 인장 — `Relics/defenders_sigil.png`

도구: 이미지 만들기 (Create image)

```
silver seal stamp with a shield emblem, pixel art item icon, dark fantasy, centered
```

### 뱀 어금니 — `Relics/serpent_fang.png`

도구: 이미지 만들기 (Create image)

```
curved green snake fang hanging on a leather cord, pixel art item icon, dark fantasy, centered
```

### 전쟁의 뿔피리 — `Relics/war_horn.png`

도구: 이미지 만들기 (Create image)

```
carved war horn with brass bands, pixel art item icon, dark fantasy, centered
```

### 혈약 반지 — `Relics/blood_ring.png`

도구: 이미지 만들기 (Create image)

```
silver ring with a blood red gem, a drop of blood beneath, pixel art item icon, dark fantasy, centered
```

### 고대 서적 — `Relics/ancient_tome.png`

도구: 이미지 만들기 (Create image)

```
old leather-bound tome with glowing blue runes, pixel art item icon, dark fantasy, centered
```

### 황금 우상 — `Relics/golden_idol.png`

도구: 이미지 만들기 (Create image)

```
small golden idol statue with a smug face, pixel art item icon, dark fantasy, centered
```

### 불사조의 재 — `Relics/phoenix_ash.png`

도구: 이미지 만들기 (Create image)

```
glass vial of glowing ember ash with a small red feather, pixel art item icon, dark fantasy, centered
```

### 마왕의 왕관 — `Relics/demon_crown.png`

도구: 이미지 만들기 (Create image)

```
dark spiked crown with a purple gem, pixel art item icon, dark fantasy, centered
```

### 정화의 부적 — `Relics/purifying_charm.png`

도구: 이미지 만들기 (Create image)

```
white paper talisman with a soft blue purifying glow, pixel art item icon, dark fantasy, centered
```

### 매의 눈 — `Relics/hawk_eye.png`

도구: 이미지 만들기 (Create image)

```
amber amulet shaped like a hawk eye, pixel art item icon, dark fantasy, centered
```

### 혼돈의 프리즘 — `Relics/chaos_prism.png`

도구: 이미지 만들기 (Create image)

```
floating crystal prism scattering chaotic rainbow light, pixel art item icon, dark fantasy, centered
```


## 음식

### 사과 — `Food/apple.png`

도구: 이미지 만들기 (Create image)

```
shiny red apple with a leaf, pixel art item icon, centered
```

### 빵 — `Food/bread.png`

도구: 이미지 만들기 (Create image)

```
crusty loaf of bread, pixel art item icon, centered
```

### 치즈 — `Food/cheese.png`

도구: 이미지 만들기 (Create image)

```
wedge of yellow cheese with holes, pixel art item icon, centered
```

### 육포 — `Food/dried_meat.png`

도구: 이미지 만들기 (Create image)

```
strips of dried jerky tied with string, pixel art item icon, centered
```

### 비상 식량 — `Food/ration.png`

도구: 이미지 만들기 (Create image)

```
travel ration pack wrapped in cloth and twine, pixel art item icon, centered
```

### 진수성찬 — `Food/feast.png`

도구: 이미지 만들기 (Create image)

```
roast chicken on a plate with bread and grapes, pixel art item icon, centered
```


## 바닥·벽 타일

### 1층 곳간 입구 (청회색 돌) — `Tiles/layer1_tileset.png`

도구: 탑다운 타일셋 (Create top-down tileset)

```
아래(lower): blue-gray cobblestone dungeon floor, worn and slightly mossy
위(upper): dark blue-gray stone brick wall top
전환(transition): crumbled stone edge
```

### 2층 오크 거점 (흙빛) — `Tiles/layer2_tileset.png`

도구: 탑다운 타일셋 (Create top-down tileset)

```
아래(lower): packed brown dirt floor with scattered straw and small bones
위(upper): rough earthen wall reinforced with wooden stakes
전환(transition): dirt mound edge
```

### 3층 곳간 심부 (자주빛 저주) — `Tiles/layer3_tileset.png`

도구: 탑다운 타일셋 (Create top-down tileset)

```
아래(lower): cursed dark purple stone floor with faint glowing cracks and a few scattered gold coins
위(upper): dark violet stone wall with glowing purple veins
전환(transition): cracked stone edge with purple glow
```


## 맵 오브젝트

### 상점 — `Map/shop.png`

도구: 맵 오브젝트 (Create map object)

```
small merchant stall with a hooded shopkeeper, hanging lantern, crates of goods and a coin scale, dark fantasy dungeon
```

### 휴식 (모닥불) — `Map/rest.png`

도구: 맵 오브젝트 (Create map object)

```
campfire with crossed logs inside a ring of stones, warm orange glow, bedroll beside it, dark fantasy dungeon
```

### 성소 — `Map/shrine.png`

도구: 맵 오브젝트 (Create map object)

```
ancient stone altar shrine with a glowing golden rune, offering candles, dark fantasy dungeon
```

### 보물상자 — `Map/chest.png`

도구: 맵 오브젝트 (Create map object)

```
small wooden treasure chest with iron bands and a gold lock, slightly worn, dark fantasy dungeon
```

### 바닥 보상 — `Map/floor_loot.png`

도구: 맵 오브젝트 (Create map object)

```
small pile of gold coins and a tiny leather pouch on stone floor, dark fantasy dungeon
```

