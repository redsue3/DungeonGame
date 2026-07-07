# 마법사 (Mage) — NovelAI 프롬프트

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

**목표:** 지팡이 들고 책 읽거나 마나 구슬 손에 띄운 채 서있음

### 프레임 1 (기본)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
right hand holding tall wooden magic staff, crystal orb on top,
left hand floating small glowing mana orb,
standing idle relaxed pose,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 2 (구슬 위로)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
right hand holding tall magic staff,
left hand raised slightly, mana orb floating higher, pulsing glow,
idle breathing, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (구슬 아래로)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
right hand holding tall magic staff,
left hand lowered, mana orb at hip level, dim glow,
idle pose, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4
→ 프레임 1 재사용

---

## Move (이동) — 6프레임

**목표:** 지팡이로 바닥 짚으며 걷거나 마나로 살짝 떠서 이동

### 프레임 1 (부유 이동 시작)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
hovering glide move, body slightly above ground,
robe trailing behind, staff horizontal,
magic aura under feet, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (부유 중)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
mid-glide float, body tilted forward,
robe flowing behind, small sparkle trail,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (살짝 내려옴)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
glide dipping slightly lower, robe settling,
staff pointing forward slightly,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4~6
→ 프레임 1~3 반복

---

## Attack (공격) — 4프레임

**목표:** 지팡이 앞으로 뻗어 화염구 발사

### 프레임 1 (차지)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
spell charge pose, both hands on staff raised forward,
orange red fireball forming at staff tip, small glowing,
body leaning back slightly,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (발사 직전)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
staff fully extended forward, large fireball at tip,
bright orange yellow glow, face lit by fire,
body weight forward,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (발사 임팩트)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
fireball launched, recoil from casting, body pushed back slightly,
fire trail leaving staff, bright flash,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (복귀)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
post-cast recovery, staff lowering, hands relaxing,
small smoke wisps at staff tip,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

---

## Special (특수기: 번개 폭풍) — 6프레임

**목표:** 하늘에서 번개 여러 발을 연속으로 내리꽂음

### 프레임 1 (의식 시작)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
both arms raised to sky, staff pointed upward,
storm clouds gathering, dark purple aura,
dramatic spell ritual pose,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (번개 모으기)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
staff crackling with electricity, lightning sparks,
yellow blue electric aura surrounding body,
arms wide, hat floating slightly,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (발동)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
staff slamming down forward, massive lightning bolt released,
blinding white yellow flash, electricity arcing,
body recoiling from power,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (연속 번개 1)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
continuous lightning barrage, pointing staff rapidly,
electric bolts streaming forward, intense glow,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 5 (연속 번개 2)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
final lightning surge, maximum power frame,
entire body surrounded by electricity, hair standing,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 6 (기력 소진 복귀)
```
pixel art, 16bit game sprite, chibi mage wizard,
long purple robe with gold trim, pointed hat,
exhausted post-spell pose, staff leaning on for support,
small electric sparks dissipating, steam wisps,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

---

## 파일 저장 이름 규칙
```
mage_idle_01.png ~ mage_idle_04.png
mage_move_01.png ~ mage_move_06.png
mage_attack_01.png ~ mage_attack_04.png
mage_special_01.png ~ mage_special_06.png
```
