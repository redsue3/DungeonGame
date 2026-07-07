# 성기사 (Paladin) — NovelAI 프롬프트

## 공통 설정
- Model: NAI Diffusion Anime V3
- Resolution: 128x128
- Sampler: k_euler / Steps: 28
- CFG Scale: 6

## 공통 Negative Prompt (모든 모션 동일하게 붙이기)
```
3d, realistic, blurry, text, watermark, logo, signature,
multiple characters, background, shadow on ground,
low quality, bad anatomy, extra limbs, deformed
```

---

## Idle (기본 대기) — 4프레임

**목표:** 방패 앞에 들고 성스러운 기운이 은은하게 감도는 대기 자세

### 프레임 1 (기본 자세)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
right hand holding glowing holy mace, golden light on head,
left hand holding large shield with golden cross symbol,
standing idle upright pose, divine aura glow,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 2 (살짝 내려감)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
right hand holding glowing holy mace,
left hand holding large shield with golden cross,
standing idle pose, body slightly lower 2px, gentle holy glow,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 3 (기본으로 복귀)
→ 프레임 1과 동일하게 재사용 가능

### 프레임 4 (살짝 올라감 + 오라 밝아짐)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
right hand holding glowing holy mace, golden halo shimmer,
left hand holding large shield with cross,
idle inhale pose, body 1px higher, bright golden aura pulse,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

---

## Move (이동) — 6프레임

**목표:** 방패 들고 무게감 있게 전진하는 모습, 발걸음에서 신성한 빛 잔상

### 프레임 1 (오른발 앞)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
march forward pose, right leg forward, left leg back,
shield raised forward, mace at side,
golden light footstep glow,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (공중)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
mid-march airborne, both feet slightly off ground,
body upright, shield raised, mace ready,
holy sparkle under feet,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (왼발 앞)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
march forward, left leg forward, right leg back,
shield forward, mace at side, golden footprint glow,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (공중 2)
→ 프레임 2와 유사하게 재사용

### 프레임 5 (오른발 앞 2)
→ 프레임 1 재사용

### 프레임 6 (왼발 앞 2)
→ 프레임 3 재사용

---

## Attack (공격: 성스러운 강타) — 4프레임

**목표:** 철퇴를 성스러운 빛으로 감싸 강하게 내리침

### 프레임 1 (준비 — 철퇴 위로)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
attack windup, mace raised above head, golden holy light charging,
shield lowered, body leaning back, divine energy gathering on weapon,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (스윙 시작)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
mace mid-swing downward, blazing golden holy light on head,
body twisting forward, arm extended, holy aura burst,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (임팩트 — 빛 폭발)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
mace fully extended downward strike, massive golden light explosion,
holy cross symbol radiating from impact, body crouched forward,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (복귀)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
post-attack recovery, mace returning to side,
golden light fading, shield raising, upright stance,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

---

## Special (특수기: 심판) — 6프레임

**목표:** 하늘에서 신성한 빛기둥이 내리꽂히며 적을 심판함

### 프레임 1 (기도 — 심판 시작)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
prayer pose, shield planted on ground, mace raised to sky,
eyes closed, divine energy building, golden halo expanding,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (신성한 빛 집결)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
divine light converging, golden rays from above gathering at mace tip,
holy aura blazing, cross symbol glowing on armor,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (선고 — 팔 앞으로 뻗음)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
arm thrust forward, mace pointing at enemy,
blinding golden light burst from mace, holy judgment beam charging,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (빛기둥 강하)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
massive holy light pillar descending from above, divine beam,
character silhouetted by overwhelming golden radiance,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 5 (잔광)
```
pixel art, 16bit game sprite, chibi paladin holy knight,
white and gold full plate armor, cross emblem on chest,
afterglow of judgment, fading golden rays, cross shaped light dissipating,
character standing proud, mace lowered, divine aura calming,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 6 (복귀)
→ Idle 프레임 1 재사용

---

## 파일 저장 이름 규칙
```
paladin_idle_01.png ~ paladin_idle_04.png
paladin_move_01.png ~ paladin_move_06.png
paladin_attack_01.png ~ paladin_attack_04.png
paladin_special_01.png ~ paladin_special_06.png
```
