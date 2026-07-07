# 도적 (Rogue) — NovelAI 프롬프트

## 공통 설정
- Model: NAI Diffusion Anime V3
- Resolution: 128x128
- Sampler: k_euler / Steps: 28
- CFG Scale: 6

## 공통 Negative Prompt
```
3d, realistic, blurry, text, watermark, logo, signature,
multiple characters, background, shadow on ground,
low quality, bad anatomy, extra limbs, deformed
```

---

## Idle (기본 대기) — 4프레임

**목표:** 후드 쓰고 몸 살짝 숙인 경계 자세, 단검 2개 손에 쥐고 있음

### 프레임 1 (기본)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
dual daggers held in both hands, crossed at chest,
crouched slightly alert stance, hood up covering face shadow,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 2 (살짝 내려감)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
dual daggers held in both hands, crossed at chest,
crouched idle, body 2px lower, weight shifting slightly,
hood up, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (눈 깜빡 or 망토 살짝 흔들)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak slightly fluttering, black leather armor,
dual daggers at rest, cloak edge moving,
idle breathing pose, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4
→ 프레임 1 재사용

---

## Move (이동) — 6프레임

**목표:** 몸 낮게 숙이고 빠르게 대시하는 느낌

### 프레임 1 (대시 준비)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
dash start pose, body leaning far forward,
both daggers held back, right foot pushing off,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (공중 대시)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
mid-dash airborne, body fully horizontal,
both legs stretched behind, daggers forward,
speed lines behind body, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (착지 직전)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
landing pose, one foot touching down,
body still angled forward, daggers ready,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4~6
→ 프레임 1~3 반복

---

## Attack (공격) — 4프레임

**목표:** 앞으로 튀어나가며 단검 2개로 X자 베기

### 프레임 1 (도약)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
attack leap forward, body airborne, legs tucked,
both daggers raised, ready to cross-slash,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (X자 베기 시작)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
dual dagger cross slash mid-motion,
right dagger slashing down-left, left dagger up-right,
X pattern slash marks, body twisting,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (임팩트)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
both daggers fully extended in X cross position,
impact frame, slash effect lines,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (복귀)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
landing after attack, crouched recovery,
daggers returning to chest, alert pose,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

---

## Special (특수기: 잔상 난무) — 6프레임

**목표:** 그림자 분신 3개가 나타나 적을 동시에 난자함

### 프레임 1 (집중)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
crouching focus pose, dark energy gathering around body,
purple shadow aura, eyes glowing white under hood,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (분신 출현)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
shadow clone appearing beside main character,
semi-transparent dark duplicate, purple tint,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (돌진)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
full speed dash attack, afterimage trail behind,
three shadow afterimages, motion blur,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (연속 베기)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
rapid multi-slash, daggers in blur motion,
slash marks radiating outward, intense action pose,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 5 (마지막 일격)
```
pixel art, 16bit game sprite, chibi rogue assassin,
dark purple hooded cloak, black leather armor,
final blow, one dagger thrust forward,
dark energy burst at tip of blade,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 6 (복귀)
→ Idle 프레임 1 재사용

---

## 파일 저장 이름 규칙
```
rogue_idle_01.png ~ rogue_idle_04.png
rogue_move_01.png ~ rogue_move_06.png
rogue_attack_01.png ~ rogue_attack_04.png
rogue_special_01.png ~ rogue_special_06.png
```
