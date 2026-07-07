# 전사 (Warrior) — NovelAI 프롬프트

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

**목표:** 살짝 위아래로 숨쉬는 느낌, 방패를 앞에 들고 서있음

### 프레임 1 (기본 자세)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
right hand holding broadsword pointing down,
left hand holding round shield facing forward,
standing idle pose, feet together,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 2 (살짝 내려감)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
right hand holding broadsword pointing down,
left hand holding round shield facing forward,
standing idle pose, body slightly lower 2px,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

### 프레임 3 (기본으로 복귀)
→ 프레임 1과 동일하게 재사용 가능

### 프레임 4 (살짝 올라감)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
right hand holding broadsword pointing down,
left hand holding round shield facing forward,
standing idle pose, body slightly higher 1px, inhale pose,
pure white background, transparent bg,
side view, facing right, single character,
64x64 sprite, clean pixel lines, no outline blur
```

---

## Move (이동) — 6프레임

**목표:** 방패 앞에 들고 뛰는 모습, 발이 교차됨

### 프레임 1 (오른발 앞)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
running pose, right leg forward, left leg back,
shield raised forward, sword back,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (공중)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
mid-run airborne, both feet off ground slightly,
body leaning forward, shield raised,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (왼발 앞)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
running pose, left leg forward, right leg back,
shield raised forward, sword back,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4~6
→ 프레임 1~3 반전하거나 재사용

---

## Attack (공격) — 4프레임

**목표:** 칼을 뒤로 빼다가 앞으로 강하게 내리침

### 프레임 1 (준비 — 칼 뒤로)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
attack windup pose, sword raised above head behind,
left foot forward, shield lowered, body twisted back,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (스윙 시작)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
sword mid-swing downward, body twisting forward,
left foot planted, right arm extended,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (임팩트 — 칼이 앞으로 뻗음)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
sword fully extended forward horizontal slash,
body leaning forward, left leg bent,
motion blur on sword, impact frame,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (복귀)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
post-attack recovery pose, sword returning to side,
body straightening up, shield raising again,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

---

## Special (특수기: 대지 강타) — 6프레임

**목표:** 온몸에 기운 모았다가 칼로 땅을 내리쳐 충격파 발생

### 프레임 1 (차지 시작)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
power charge pose, both hands gripping sword handle,
aura glowing around body, yellow white glow,
crouching slightly, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 2 (차지 최대)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
full power charge, sword raised high with both hands,
bright golden energy aura, whole body glowing,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 3 (내리찍기)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
slamming sword downward with full force,
body bent forward, sword pointing straight down,
speed lines, pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 4 (임팩트)
```
pixel art, 16bit game sprite, chibi warrior knight,
royal blue full plate armor, silver trim,
sword hit ground impact, shockwave lines on ground,
dust cloud at feet, body crouched low,
pure white background, transparent bg,
side view facing right, single character,
64x64 sprite, clean pixel lines
```

### 프레임 5~6 (복귀)
→ 프레임 4에서 서서히 Idle 자세로 복귀

---

## 파일 저장 이름 규칙
```
warrior_idle_01.png ~ warrior_idle_04.png
warrior_move_01.png ~ warrior_move_06.png
warrior_attack_01.png ~ warrior_attack_04.png
warrior_special_01.png ~ warrior_special_06.png
```
