#!/usr/bin/env node
// 던전 게임 밸런스 시뮬레이터 (Unity 밖에서 node로 실행)
//
//   node Tools/BalanceSim/balance-sim.js                 # 분석만 (표 출력 + result.json)
//   node Tools/BalanceSim/balance-sim.js --apply         # 목표 수치를 EnemyDatabase.cs에 써넣음
//   node Tools/BalanceSim/balance-sim.js --runs 400      # 직업당 런 수 (기본 600)
//
// 게임 데이터(카드/직업/적/유물/드롭/층 구성)는 C# 소스에서 직접 읽는다. 숫자를 여기 따로 적지 않는다.
//
// 전투력 정의
//   플레이어 전투력 = sqrt( (최대HP + 4 x 턴당 방어·회복) x 턴당 피해 )
//   적 전투력       = sqrt( (HP + 전투 동안 쌓는 방어막) x 턴당 피해 )
//   두 값의 비율이 '강함 %'. HP와 공격을 둘 다 30% 올리면 전투력도 30% 오른다.
//   비율의 제곱은 맞대결에서 잃는 체력 비율과 비슷하다 (강함 70% -> 체력 약 49% 손실).
//
// 잘 뽑음 / 못 뽑음
//   런마다 보상 카드·유물을 무작위로 받고(보상 운), 체크포인트마다 4턴짜리 전투를 여러 판 돌린다(손패 운).
//   모든 판의 전투력 분포에서 상위 10%를 '잘 뽑음', 하위 10%를 '못 뽑음'으로 보고 둘의 평균을 기준값으로 쓴다.
//
// 단순화한 것: 전투 그리드의 이동·접근·사거리(적이 붙기 전 한 턴 허비, 마법사 키이팅), 맵에서 미리 쓰는 카드,
//              음식, 상점 유물 구매. 힘(strength)은 전투가 끝나면 0으로 돌아간다고 본다 (BattleManager 수정과 같음).

'use strict';
const fs = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..', '..');
const read = rel => fs.readFileSync(path.join(ROOT, 'Assets', 'Scripts', rel), 'utf8');
const args = process.argv.slice(2);
const argVal = (name, def) => { const i = args.indexOf(name); return i >= 0 ? args[i + 1] : def; };
const APPLY = args.includes('--apply');
const RUNS = parseInt(argVal('--runs', '600'), 10);
const SEED = parseInt(argVal('--seed', '20261110'), 10);

// ---------- 목표 (사용자 지정) ----------
// growLag: 층마다 적 성장률 = 플레이어 성장률 - growLag (2026-11-11 사용자 요청: 똑같이 키우면 적이 너무 강하다)
// elite: 2026-11-11 사용자 요청으로 90% -> 72% (능력치 20% 낮춤) - 만전 상태면 1층부터 이길 수 있게
const TARGET = { normalMin: 0.40, normalMax: 0.70, elite: 0.72, boss: 1.10, growLag: 0.05 };
const FIGHT_TURNS = 4;        // 방어막을 체력으로 환산할 때 쓰는 기준 전투 길이
const BOSS_TURNS = 6;         // 보스 패턴 평균을 낼 때 보는 턴 수
const BATTLES_PER_CHECK = 20; // 체크포인트마다 런당 측정 전투 수

// ---------- 난수 ----------
function mulberry32(a) { return function () { a |= 0; a = a + 0x6D2B79F5 | 0; let t = Math.imul(a ^ a >>> 15, 1 | a); t = t + Math.imul(t ^ t >>> 7, 61 | t) ^ t; return ((t ^ t >>> 14) >>> 0) / 4294967296; }; }
let rng = mulberry32(SEED);
const rInt = (lo, hiExcl) => lo + Math.floor(rng() * (hiExcl - lo));
const pick = arr => arr[Math.floor(rng() * arr.length)];
// Unity Mathf.RoundToInt = 0.5에서 짝수 쪽으로
const roundToInt = x => { const f = Math.floor(x), d = x - f; if (Math.abs(d - 0.5) < 1e-9) return f % 2 === 0 ? f : f + 1; return Math.round(x); };

// ---------- 파싱 ----------
function parseCards() {
  const src = read('Database/CardDatabase.cs');
  const re = /\["(\w+)"\]\s*=\s*new Card\("\w+",\s*"([^"]+)",\s*(\d+),\s*CardType\.(\w+)((?:\s*,\s*\w+:\s*[\w.\-]+)*)\s*\)/g;
  const map = { dmg: 'dmg', blk: 'blk', draw: 'draw', heal: 'heal', str: 'str', poison: 'poison', burn: 'burn', selfDmg: 'selfDmg', aoe: 'aoe', growOnUse: 'grow', buffNextAttack: 'buffAtk', buffNextDefense: 'buffDef', range: 'range' };
  const cards = {};
  let m;
  while ((m = re.exec(src))) {
    const c = { id: m[1], name: m[2], cost: +m[3], type: m[4], dmg: 0, blk: 0, draw: 0, heal: 0, str: 0, poison: 0, burn: 0, selfDmg: 0, aoe: false, grow: 0, buffAtk: 0, buffDef: 0, range: 1 };
    const are = /(\w+):\s*([\w.\-]+)/g; let a;
    while ((a = are.exec(m[5]))) { const k = map[a[1]]; if (k) c[k] = a[2] === 'true' ? true : a[2] === 'false' ? false : +a[2]; }
    cards[c.id] = c;
  }
  return cards;
}

function parsePlayers() {
  const src = read('Database/PlayerDatabase.cs');
  const re = /\[CharacterClass\.(\w+)\]\s*=\s*new PlayerData\s*\{([\s\S]*?)\}\s*,/g;
  const out = {}; let m;
  while ((m = re.exec(src))) {
    const b = m[2];
    const num = (k, d) => { const mm = new RegExp(k + '\\s*=\\s*(-?\\d+)').exec(b); if (mm) return +mm[1]; if (d === undefined) throw new Error(k + ' 없음'); return d; };
    const fnum = k => { const mm = new RegExp(k + '\\s*=\\s*(-?\\d+(?:\\.\\d+)?)f?').exec(b); return mm ? +mm[1] : 0; };
    out[m[1]] = {
      cls: m[1], name: /displayName\s*=\s*"([^"]+)"/.exec(b)[1],
      maxHp: num('maxHp'), maxCost: num('maxCost'), hand: num('startHandSize'), atk: num('baseAttackBonus'),
      blk: num('baseBlockBonus', 0), manaPerKill: num('manaPerKill', 0),
      hpPerLevel: num('hpPerLevel', 0), atkPerLevel: fnum('attackPerLevel'), blkPerLevel: fnum('blockPerLevel'),
      deck: [.../starterDeckCardIds\s*=\s*new\[\]\s*\{([\s\S]*?)\}/.exec(b)[1].matchAll(/"(\w+)"/g)].map(x => x[1]),
    };
  }
  return out;
}

function parseEnemies() {
  // --enemy-file: 다른 버전의 EnemyDatabase.cs(예: 수정 전 원본)와 비교할 때
  const alt = argVal('--enemy-file', null);
  const src = alt ? fs.readFileSync(alt, 'utf8') : read('Database/EnemyDatabase.cs');
  const starts = [...src.matchAll(/\["(\w+)"\]\s*=\s*new EnemyData/g)];
  const enemies = {};
  starts.forEach((s, i) => {
    const from = s.index, to = i + 1 < starts.length ? starts[i + 1].index : src.length;
    const b = src.slice(from, to);
    const e = {
      id: s[1], name: /displayName = "([^"]+)"/.exec(b)[1], hp: +/hp = (\d+)/.exec(b)[1],
      goldMin: +/rewardGoldMin = (\d+)/.exec(b)[1], goldMax: +/rewardGoldMax = (\d+)/.exec(b)[1],
      isElite: /isElite = true/.test(b), isBoss: /isBoss = true/.test(b), phases: [], span: [from, to],
    };
    const parts = b.split(/hpThreshold = /).slice(1);
    for (const p of parts) {
      const th = parseFloat(p);
      const acts = [...p.matchAll(/intent = EnemyIntent\.(\w+),\s*value = (\d+)/g)].map(x => ({ intent: x[1], value: +x[2] }));
      e.phases.push({ th, acts });
    }
    enemies[e.id] = e;
  });
  return { enemies, src };
}

function parseRelics() {
  const src = read('Database/RelicDatabase.cs');
  const ids = [...src.matchAll(/id = "(\w+)", displayName = "([^"]+)", rarity = RelicRarity\.(\w+)/g)];
  return ids.map((m, i) => {
    const seg = src.slice(m.index, i + 1 < ids.length ? ids[i + 1].index : src.length);
    const effects = [...seg.matchAll(/trigger = RelicTrigger\.(\w+),\s*effectType = RelicEffectType\.(\w+),\s*value = (\d+)/g)]
      .map(x => ({ trigger: x[1], type: x[2], value: +x[3] }));
    return { id: m[1], name: m[2], rarity: m[3], effects };
  });
}

function parseLoot() {
  const src = read('Database/LootTable.cs');
  const entries = s => [...s.matchAll(/id = "(\w+)",\s*weight = (\d+)/g)].map(x => ({ id: x[1], w: +x[2] }));
  const basic = entries(/basicCardPool[\s\S]*?\};/.exec(src)[0]);
  const pools = {};
  for (const L of [1, 2, 3]) {
    const seg = new RegExp('\\[' + L + '\\]\\s*=\\s*new List<RewardEntry>\\s*\\{([\\s\\S]*?)\\}\\s*,').exec(src)[1];
    pools[L] = entries(seg).concat(basic);
  }
  const eliteRelic = parseFloat(/EliteRelicChance\s*=\s*([\d.]+)f/.exec(src)[1]);
  return { pools, eliteRelic };
}

function parseFloorEnemies() {
  const src = read('Map/FloorGenerator.cs');
  const lists = fn => {
    const seg = new RegExp('private static string ' + fn + '[\\s\\S]*?\\n    \\}').exec(src)[0];
    return [...seg.matchAll(/new\[\]\s*\{([^}]*)\}/g)].map(x => [...x[1].matchAll(/"(\w+)"/g)].map(y => y[1]));
  };
  const bossSeg = /private static string GetBossId[\s\S]*?;/.exec(src)[0];
  const bosses = [...bossSeg.matchAll(/"(\w+)"/g)].map(x => x[1]);
  // 집단 조우 규칙: 무리 수와 무리원 능력치 배율 (상수가 없으면 예전 규칙 = 2마리, 원래 능력치)
  const size = /const int\s+GroupSize\s*=\s*(\d+)/.exec(src);
  const group = size ? { count: +size[1], scale: 1 / +size[1] } : { count: 2, scale: 1 };
  // 층별 일반 방 두 마리 확률 (배열이 없으면 예전 규칙 = 항상 한 마리)
  const pairSeg = /NormalPairChances\s*=\s*\{([^}]*)\}/.exec(src);
  const pair = pairSeg ? pairSeg[1].split(',').map(s => parseFloat(s)) : [0, 0, 0];
  return { normal: lists('PickNormal'), elite: lists('PickElite'), boss: bosses, group, pair };
}
const OLD_GROUP = { count: 2, scale: 1 }; // origin/master 1cd93eb까지의 규칙

// 마법사 마나: ManaPerBonusDamage마다 공격 카드 피해 +1 (PlayerCharacter.cs 상수)
// 레벨: 막타 경험치, 다음 레벨 필요량 = A + B x (레벨-1) (LevelSystem.cs)
const LV = (() => { let src = ''; try { src = read('System/LevelSystem.cs'); } catch (e) { return null; }
  const c = k => +new RegExp('const int\\s+' + k + '\\s*=\\s*(\\d+)').exec(src)[1];
  const f = /XpToNext\(int level\)\s*=>\s*(\d+)\s*\+\s*(\d+)\s*\*\s*\(level - 1\)/.exec(src);
  return { max: c('MaxLevel'), normal: c('XpNormal'), elite: c('XpElite'), boss: c('XpBoss'), a: +f[1], b: +f[2] }; })();
const xpToNext = lv => LV.a + LV.b * (lv - 1);
function levelUp(run) {
  run.level++; run.maxHp += run.hpPerLevel;
  run.atk = run.baseAtk + Math.floor(run.atkPerLevel * (run.level - 1) + 0.001);
  run.blk = run.baseBlk + Math.floor(run.blkPerLevel * (run.level - 1) + 0.001);
}
// 적 하나를 막타로 쓰러뜨렸을 때 경험치 -> 오른 레벨 수 (PlayerCharacter.GainXp와 같은 규칙)
function gainXpFor(run, e) {
  if (!LV) return 0;
  const base = e.isBoss ? LV.boss : e.isElite ? LV.elite : LV.normal;
  run.xp += Math.max(1, roundToInt(base * (e.scale || 1)));
  let ups = 0;
  while (run.level < LV.max && run.xp >= xpToNext(run.level)) { run.xp -= xpToNext(run.level); levelUp(run); ups++; }
  if (run.level >= LV.max) run.xp = 0;
  return ups;
}
const MANA_PER_DMG = +(/ManaPerBonusDamage\s*=\s*(\d+)/.exec(read('PlayerCharacter.cs')) || [0, 1])[1];
const CARDS = parseCards();
const PLAYERS = parsePlayers();
const { enemies: ENEMIES, src: ENEMY_SRC } = parseEnemies();
const RELICS = parseRelics();
const LOOT = parseLoot();
const FLOOR = parseFloorEnemies();
console.log(`파싱: 카드 ${Object.keys(CARDS).length}, 직업 ${Object.keys(PLAYERS).length}, 적 ${Object.keys(ENEMIES).length}, 유물 ${RELICS.length}, ` +
  `층별 일반 ${FLOOR.normal.map(x => x.length)}, 엘리트 ${FLOOR.elite.map(x => x.length)}, 보스 ${FLOOR.boss}`);
for (const ids of [...FLOOR.normal, ...FLOOR.elite, FLOOR.boss]) for (const id of ids) if (!ENEMIES[id]) throw new Error('적 데이터 없음: ' + id);

// ---------- 카드 가치 (보상 고르기·상점용 대략치) ----------
function cardScore(c, atk) {
  let v = 0;
  if (c.dmg > 0) v += (c.dmg + atk) * (c.aoe ? 1.4 : 1) + c.grow * 6;
  v += c.poison * 2.5 + c.burn * 3 + c.blk * 0.9 + c.heal * 0.8 + c.draw * 3.5 + c.str * 5 + c.buffAtk * 0.8 + c.buffDef * 0.5 - c.selfDmg * 0.7;
  return v / Math.max(c.cost, 0.6);
}
const deckMeanScore = (deck, atk) => deck.reduce((s, c) => s + cardScore(c, atk), 0) / deck.length;
const clone = c => ({ ...c });

// ---------- 전투 ----------
// 적 상태 객체
function makeEnemy(e) {
  return { id: e.id, name: e.name, isElite: e.isElite, isBoss: e.isBoss, scale: e.scale || 1, maxHp: e.hp, hp: e.hp, block: 0, atkPow: 0, poison: 0, burn: 0, phases: e.phases, phase: 0, idx: 0 };
}
function enemyPeek(en) { const p = en.phases[en.phase].acts; return p[en.idx % p.length]; }
function enemyCheckPhase(en) {
  while (en.phase + 1 < en.phases.length && en.hp <= en.maxHp * en.phases[en.phase + 1].th) { en.phase++; en.idx = 0; }
}
function dmgTo(t, amount) { const ab = Math.min(t.block, amount); t.block -= ab; t.hp = Math.max(0, t.hp - (amount - ab)); }
function statusTick(t) {
  if (t.poison > 0) { t.hp = Math.max(0, t.hp - t.poison); t.poison--; }
  if (t.burn > 0) { t.hp = Math.max(0, t.hp - roundToInt(t.burn * 1.5)); t.burn--; }
}
function intentDamage(en) {
  const a = enemyPeek(en);
  if (a.intent === 'Attack') return a.value + en.atkPow;
  if (a.intent === 'Poison') return a.value * 1.5;
  if (a.intent === 'Burn') return a.value * 2;
  return 0;
}

// 플레이어 한 턴: 손패에서 가치/코스트가 높은 카드부터. incoming = 이번 적 턴에 들어올 피해(없으면 dummy 값)
function playTurn(P, foes, incoming) {
  let dealt = 0, defended = 0;
  const alive = () => foes.filter(f => f.hp > 0);
  for (let guard = 0; guard < 40; guard++) {
    if (alive().length === 0) break;
    const hand = P.hand;
    let best = -1, bestV = 0;
    const atk = P.atk + P.str;
    const needBlock = Math.max(0, incoming - P.block);
    for (let i = 0; i < hand.length; i++) {
      const c = hand[i];
      if (c.cost > P.cost) continue;
      let v = 0;
      const tgts = c.aoe ? alive().length : 1;
      if (c.dmg > 0) v += (c.dmg + atk + P.pendAtk) * tgts;
      v += c.poison * 2.5 * tgts + c.burn * 3 * tgts;
      v += Math.min(c.blk > 0 ? Math.max(0, c.blk + P.pendDef + P.blk) : 0, needBlock) + Math.min(c.heal, P.maxHp - P.hp);
      v += c.draw * 3 + c.str * 4 + c.buffAtk * 0.8 + c.buffDef * 0.4 - c.selfDmg * 0.7;
      if (c.selfDmg >= P.hp) v = -1;
      const score = c.cost === 0 ? v * 100 : v / c.cost;
      if (v > 0.5 && score > bestV) { bestV = score; best = i; }
    }
    if (best < 0) break;
    const c = hand.splice(best, 1)[0];
    P.cost -= c.cost;
    // 효과 적용 (Deck.ApplyCardEffect + Card.ApplyCasterEffects 순서)
    const targets = c.aoe ? alive() : [alive().sort((a, b) => a.hp - b.hp)[0]];
    if (c.dmg > 0) {
      const bonus = P.atk + P.str + P.pendAtk; P.pendAtk = 0;
      for (const t of targets) { const before = t.hp; dmgTo(t, Math.max(0, c.dmg + bonus)); dealt += before - t.hp; enemyCheckPhase(t); if (t.hp <= 0) P.onKill(true, t); }
      if (c.grow > 0) c.dmg += c.grow;
    }
    for (const t of targets) { t.poison += c.poison; t.burn += c.burn; }
    if (c.blk > 0) { const b = Math.max(0, c.blk + P.pendDef + P.blk); P.block += b; defended += Math.min(b, needBlock); P.pendDef = 0; }
    if (c.heal > 0) { const before = P.hp; P.hp = Math.min(P.maxHp, P.hp + c.heal); defended += P.hp - before; }
    if (c.str > 0) P.str += c.str;
    if (c.selfDmg > 0) dmgTo(P, c.selfDmg);
    P.pendAtk += c.buffAtk; P.pendDef += c.buffDef;
    P.discard.push(c);
    if (c.draw > 0) draw(P, c.draw);
    if (P.hp <= 0) break;
    if (c.cost > 0 && P.cost === 0) { P.penalty = 1; break; } // 코스트 소진 → 턴 자동 종료 + 다음 턴 리필 -1
  }
  P.discard.push(...P.hand); P.hand = [];
  return { dealt, defended };
}
function draw(P, n) {
  for (let i = 0; i < n; i++) {
    if (P.drawPile.length === 0) { P.drawPile = P.discard; P.discard = []; }
    if (P.drawPile.length === 0) return;
    const k = Math.floor(rng() * P.drawPile.length);
    P.hand.push(P.drawPile.splice(k, 1)[0]);
  }
}
function hasRelicEffect(run, trigger, type) {
  let v = 0;
  for (const id of run.relics) for (const e of RELICS.find(r => r.id === id).effects) if (e.trigger === trigger && e.type === type) v += e.value;
  return v;
}
function newBattleState(run, hp) {
  const P = {
    maxHp: run.maxHp, hp: hp ?? run.maxHp, block: 0, str: 0, atk: run.atk + Math.floor((run.mana || 0) / MANA_PER_DMG), blk: run.blk || 0, cost: run.maxCost, penalty: 0,
    pendAtk: 0, pendDef: 0, hand: [], discard: [], drawPile: run.deck.slice(), poison: 0, burn: 0, phoenix: false,
    mana: run.mana || 0,
    // byCard: 카드로 직접 쓰러뜨림(막타). 마법사 마나는 이때만 +manaPerKill, 전투 중에도 바로 피해에 반영
    onKill: (byCard, foe) => {
      const h = hasRelicEffect(run, 'OnKill', 'HealHp'); if (h) P.hp = Math.min(P.maxHp, P.hp + h);
      if (byCard && run.manaPerKill) P.mana += run.manaPerKill;
      if (byCard && foe) { const before = run.maxHp; if (gainXpFor(run, foe) > 0) { P.maxHp = run.maxHp; P.hp += run.maxHp - before; P.blk = run.blk; } }
      P.atk = run.atk + Math.floor(P.mana / MANA_PER_DMG);
    },
  };
  P.str += hasRelicEffect(run, 'OnBattleStart', 'GainStrength');
  P.block += hasRelicEffect(run, 'OnBattleStart', 'GainBlock');
  draw(P, hasRelicEffect(run, 'OnBattleStart', 'DrawCard'));
  return P;
}
function playerTurnStart(run, P, first) {
  if (!first) { P.block = 0; statusTick(P); P.cost = Math.max(0, run.maxCost - P.penalty); P.penalty = 0; }
  draw(P, run.hand);
  P.cost += hasRelicEffect(run, 'OnTurnStart', 'GainCost');
  draw(P, hasRelicEffect(run, 'OnTurnStart', 'DrawCard'));
}
function phoenixCheck(run, P) {
  if (!P.phoenix && P.hp > 0 && P.hp <= P.maxHp * 0.3) { const b = hasRelicEffect(run, 'OnHpBelow30', 'GainBlock'); if (b) { P.block += b; P.phoenix = true; } }
}

// 측정용 전투: 체력 무한 허수아비가 매 턴 dummyHit 만큼 때린다고 보고 4턴 동안 낸 피해·막은 양을 잰다
function measureBattle(run) {
  const P = newBattleState(run);
  const dummy = { hp: 1e9, maxHp: 1e9, block: 0, poison: 0, burn: 0, phases: [{ th: 1, acts: [{ intent: 'Attack', value: 0 }] }], phase: 0, idx: 0, atkPow: 0 };
  const hit = Math.round(run.maxHp * 0.12);
  let dealt = 0, defended = 0;
  for (let t = 0; t < FIGHT_TURNS; t++) {
    playerTurnStart(run, P, t === 0);
    const hp0 = dummy.hp;
    const r = playTurn(P, [dummy], hit);
    statusTick(dummy); // 독·화상 틱 (적 턴 시작)
    dealt += hp0 - dummy.hp; defended += r.defended;
    P.hp = run.maxHp; // 측정은 체력 손실 없이
  }
  const O = dealt / FIGHT_TURNS, B = defended / FIGHT_TURNS;
  return { O, B, power: Math.sqrt((run.maxHp + FIGHT_TURNS * B) * O) };
}

// 실제 전투 (검증용)
function fight(run, enemyIds, enemyTable) {
  const P = newBattleState(run);
  const foes = enemyIds.map(id => makeEnemy(enemyTable[id]));
  for (let turn = 0; turn < 30; turn++) {
    playerTurnStart(run, P, turn === 0);
    if (P.hp <= 0) return { win: false, hpLost: 1 };
    const incoming = foes.filter(f => f.hp > 0).reduce((s, f) => s + intentDamage(f), 0);
    playTurn(P, foes, incoming);
    phoenixCheck(run, P);
    if (P.hp <= 0) return { win: false, hpLost: 1 };
    if (foes.every(f => f.hp <= 0)) return { win: true, hpLost: 1 - P.hp / P.maxHp };
    for (const en of foes) {
      if (en.hp <= 0) continue;
      statusTick(en); enemyCheckPhase(en); en.block = 0;
      if (en.hp <= 0) { P.onKill(false); continue; }
      const a = enemyPeek(en); en.idx++;
      if (a.intent === 'Attack') dmgTo(P, a.value + en.atkPow);
      else if (a.intent === 'Defend') en.block += a.value;
      else if (a.intent === 'Buff') en.atkPow += a.value;
      else if (a.intent === 'Poison') P.poison += a.value;
      else if (a.intent === 'Burn') P.burn += a.value;
      phoenixCheck(run, P);
      if (P.hp <= 0) return { win: false, hpLost: 1 };
    }
    if (foes.every(f => f.hp <= 0)) return { win: true, hpLost: 1 - P.hp / P.maxHp };
  }
  return { win: false, hpLost: 1 - P.hp / P.maxHp };
}

// ---------- 적 전투력 ----------
function enemyPower(e) {
  const turns = e.isBoss ? BOSS_TURNS : FIGHT_TURNS;
  let atkPow = 0, dmg = 0, blk = 0;
  const per = turns / e.phases.length;
  for (let t = 0; t < turns; t++) {
    const ph = e.phases[Math.min(e.phases.length - 1, Math.floor(t / per))];
    const startT = Math.floor(Math.floor(t / per) * per);
    const a = ph.acts[(t - startT) % ph.acts.length];
    if (a.intent === 'Attack') dmg += a.value + atkPow;
    else if (a.intent === 'Defend') blk += a.value;
    else if (a.intent === 'Buff') atkPow += a.value;
    else if (a.intent === 'Poison') dmg += a.value * (a.value + 1) / 2;
    else if (a.intent === 'Burn') { for (let k = a.value; k > 0; k--) dmg += roundToInt(k * 1.5); }
  }
  const dpt = dmg / turns;
  return { hp: e.hp, hpEff: e.hp + blk, dpt, power: Math.sqrt((e.hp + blk) * dpt) };
}
// EnemyFactory.Create(id, statScale)와 같은 규칙 (반올림 후 최소 1)
function factoryScale(e, s) {
  if (s === 1) return e;
  const sc = v => v <= 0 ? 0 : Math.max(1, roundToInt(v * s));
  return { ...e, scale: s, id: e.id + '@' + s.toFixed(3), hp: sc(e.hp), goldMin: sc(e.goldMin), goldMax: sc(e.goldMax),
    phases: e.phases.map(p => ({ th: p.th, acts: p.acts.map(a => ({ intent: a.intent, value: sc(a.value) })) })) };
}
function scaleEnemy(e, s) {
  return { ...e, hp: Math.max(1, Math.round(e.hp * s)), phases: e.phases.map(p => ({ th: p.th, acts: p.acts.map(a => ({ intent: a.intent, value: Math.max(1, Math.round(a.value * s)) })) })) };
}
function fitEnemy(e, targetPower) {
  let lo = 0.2, hi = 6;
  for (let i = 0; i < 40; i++) { const mid = (lo + hi) / 2; if (enemyPower(scaleEnemy(e, mid)).power < targetPower) lo = mid; else hi = mid; }
  // 반올림 때문에 생기는 오차 중 목표에 가장 가까운 쪽
  const a = scaleEnemy(e, lo), b = scaleEnemy(e, hi);
  return Math.abs(enemyPower(a).power - targetPower) <= Math.abs(enemyPower(b).power - targetPower) ? a : b;
}

// ---------- 런 진행 ----------
function rollCards(layer, count) {
  const pool = LOOT.pools[layer].slice(); const out = [];
  while (out.length < count && pool.length) {
    const tot = pool.reduce((s, e) => s + e.w, 0); let r = rng() * tot;
    for (let i = 0; i < pool.length; i++) { r -= pool[i].w; if (r < 0) { out.push(pool[i].id); pool.splice(i, 1); break; } }
  }
  return out;
}
function offerCards(run, layer, count) {
  const ids = rollCards(layer, count);
  let best = null, bestS = -1;
  for (const id of ids) { const s = cardScore(CARDS[id], run.atk); if (s > bestS) { bestS = s; best = id; } }
  if (best && bestS >= deckMeanScore(run.deck, run.atk) * 0.95) run.deck.push(clone(CARDS[best]));
}
// 보물상자 (TreasureTable.cs). 런 진행에선 미믹 전투를 이긴다고 보고, 함정·음식은 전투력에 영향이 없어 건너뛴다.
const TREASURE = (() => { let src; try { src = read('Database/TreasureTable.cs'); } catch (e) { return null; }
  const c = k => +new RegExp('const (?:int|float)\\s+' + k + '\\s*=\\s*([\\d.]+)').exec(src)[1];
  return { chests: c('ChestsPerFloor'), loot: c('FloorLootPerFloor'),
    w: { gold: c('WeightGold'), card: c('WeightCard'), food: c('WeightFood'), relic: c('WeightRelic'), mimic: c('WeightMimic'), trap: c('WeightTrap') },
    goldMin: c('ChestGoldMinPerLayer'), goldMax: c('ChestGoldMaxPerLayer'), mimicRelic: c('MimicRelicChance'),
    lootGoldPct: c('LootGoldChancePercent'), lootMin: c('LootGoldMinPerLayer'), lootMax: c('LootGoldMaxPerLayer') }; })();
function openChest(run, L) {
  const w = TREASURE.w, total = w.gold + w.card + w.food + w.relic + w.mimic + w.trap; let r = rng() * total;
  if ((r -= w.gold) < 0) { run.gold += Math.round(L * rInt(TREASURE.goldMin, TREASURE.goldMax + 1) * (1 + run.goldPct / 100)); return; }
  if ((r -= w.card) < 0) { offerCards(run, L, 3); return; }
  if ((r -= w.food) < 0) return;
  if ((r -= w.relic) < 0) { gainRelic(run, false); return; }
  if ((r -= w.mimic) < 0) { battleRewards(run, [pick(FLOOR.normal[L - 1])], 'normal'); if (rng() < TREASURE.mimicRelic) gainRelic(run, false); return; }
}
function gainRelic(run, isBoss) {
  const avail = RELICS.filter(r => !run.relics.includes(r.id) && (isBoss || r.rarity !== 'Boss'));
  if (!avail.length) return;
  const r = pick(avail); run.relics.push(r.id);
  for (const e of r.effects) {
    if (e.trigger === 'Passive' && e.type === 'BonusMaxHp') { run.maxHp += e.value; }
    if (e.trigger === 'Passive' && e.type === 'BonusMaxCost') run.maxCost += e.value;
    if (e.trigger === 'Passive' && e.type === 'BonusHandSize') run.hand += e.value;
    if (e.trigger === 'Passive' && e.type === 'BonusGoldPct') run.goldPct += e.value;
    if (e.trigger === 'OnAcquire' && e.type === 'RemoveRandomCard' && run.deck.length > 5) run.deck.splice(rInt(0, run.deck.length), 1);
    if (e.trigger === 'OnAcquire' && e.type === 'TransformRandomCard') run.deck[rInt(0, run.deck.length)] = clone(CARDS[rollCards(run.layer, 1)[0]]);
  }
}
function battleRewards(run, ids, kind, scale = 1) {
  run.gold += Math.round(ids.reduce((s, id) => { const e = factoryScale(ENEMIES[id], scale); return s + (e.goldMin + e.goldMax) / 2; }, 0) * (1 + run.goldPct / 100));
  run.mana += run.manaPerKill * ids.length; // 마법사: 막타마다 마나 (런 진행에선 쓰러뜨린 적 수만큼 막타를 친다고 봄)
  for (const id of ids) gainXpFor(run, { ...ENEMIES[id], scale });
  for (const c of run.deck) if (c.grow > 0) c.dmg += Math.round(c.grow * 1.5); // 성장형 카드: 전투당 평균 1.5회 사용
  if (kind === 'boss') { gainRelic(run, true); if (run.layer < 3) offerCards(run, run.layer, 3); }
  else if (kind === 'elite') { if (rng() < LOOT.eliteRelic) gainRelic(run, false); offerCards(run, run.layer, 2); }
  else offerCards(run, run.layer, 1);
}
function shrine(run) {
  const L = run.layer, opts = [];
  if (rng() < 0.5) opts.push({ id: 'shrine_burst', name: '성소의 회심일격', cost: 2, type: 'Attack', dmg: L * 1 * rInt(3, 6) });
  else opts.push({ id: 'shrine_growing', name: '성소의 성장하는 검', cost: 0, type: 'Attack', dmg: rInt(2, 4), grow: rInt(1, 3) });
  opts.push(rng() < 0.5 ? { id: 'shrine_focus', cost: 0, type: 'Skill', buffAtk: rInt(3, 7) } : { id: 'shrine_ward', cost: 0, type: 'Skill', buffDef: rInt(3, 7) });
  opts.push({ id: 'shrine_bastion', cost: 2, type: 'Defense', blk: roundToInt((L + run.maxHp) * (0.15 + rng() * 0.10)) });
  const full = opts.map(o => ({ dmg: 0, blk: 0, draw: 0, heal: 0, str: 0, poison: 0, burn: 0, selfDmg: 0, aoe: false, grow: 0, buffAtk: 0, buffDef: 0, range: 1, ...o }));
  full.sort((a, b) => cardScore(b, run.atk) - cardScore(a, run.atk));
  run.deck.push(full[0]);
}
function shop(run) {
  const price = 50 + run.layer * 20 + rInt(-10, 15);
  const ids = rollCards(run.layer, 3);
  const best = ids.map(id => CARDS[id]).sort((a, b) => cardScore(b, run.atk) - cardScore(a, run.atk))[0];
  if (run.gold >= price && best && cardScore(best, run.atk) > deckMeanScore(run.deck, run.atk)) { run.gold -= price; run.deck.push(clone(best)); }
  else if (run.gold >= 75 && run.deck.length > 6) {
    run.gold -= 75;
    let w = 0; run.deck.forEach((c, i) => { if (cardScore(c, run.atk) < cardScore(run.deck[w], run.atk)) w = i; });
    run.deck.splice(w, 1);
  }
}
function newRun(cls) {
  const p = PLAYERS[cls];
  return { cls, maxHp: p.maxHp, maxCost: p.maxCost, hand: p.hand, atk: p.atk, blk: p.blk, manaPerKill: p.manaPerKill, mana: 0,
    level: 1, xp: 0, baseAtk: p.atk, baseBlk: p.blk, hpPerLevel: p.hpPerLevel, atkPerLevel: p.atkPerLevel, blkPerLevel: p.blkPerLevel,
    deck: p.deck.map(id => clone(CARDS[id])), relics: [], gold: 0, goldPct: 0, layer: 1 };
}
const snapshot = run => ({ ...run, deck: run.deck.map(clone), relics: run.relics.slice() });

// 체크포인트: L1시작, L1보스직전, L2시작, L2보스직전, L3시작, L3보스직전
const CHECKS = ['1층 시작', '1층 보스 직전', '2층 시작', '2층 보스 직전', '3층 시작', '3층 보스 직전'];
function simulateRun(cls) {
  const run = newRun(cls); const snaps = [];
  for (let L = 1; L <= 3; L++) {
    run.layer = L;
    snaps.push(snapshot(run));
    const rooms = [];
    const normals = L === 1 ? 5 : 6;
    for (let i = 0; i < normals; i++) rooms.push('normal');
    rooms.push('group', 'group', 'elite', 'shop'); if (L === 1) rooms.push('shrine');
    if (TREASURE) { for (let i = 0; i < TREASURE.chests; i++) rooms.push('chest'); for (let i = 0; i < TREASURE.loot; i++) rooms.push('loot'); }
    for (let i = rooms.length - 1; i > 0; i--) { const j = rInt(0, i + 1); [rooms[i], rooms[j]] = [rooms[j], rooms[i]]; }
    for (const r of rooms) {
      if (r === 'normal') battleRewards(run, rng() < FLOOR.pair[L - 1] ? [pick(FLOOR.normal[L - 1]), pick(FLOOR.normal[L - 1])] : [pick(FLOOR.normal[L - 1])], 'normal');
      else if (r === 'group') battleRewards(run, Array.from({ length: FLOOR.group.count }, () => pick(FLOOR.normal[L - 1])), 'normal', FLOOR.group.scale);
      else if (r === 'elite') battleRewards(run, [pick(FLOOR.elite[L - 1])], 'elite');
      else if (r === 'shop') shop(run);
      else if (r === 'shrine') shrine(run);
      else if (r === 'chest') openChest(run, L);
      else if (r === 'loot') { if (rng() * 100 < TREASURE.lootGoldPct) run.gold += Math.round(L * rInt(TREASURE.lootMin, TREASURE.lootMax + 1) * (1 + run.goldPct / 100)); }
    }
    snaps.push(snapshot(run));
    battleRewards(run, [FLOOR.boss[L - 1]], 'boss');
  }
  return snaps;
}

// ---------- 실행 ----------
const pct = (arr, p) => { const s = arr.slice().sort((a, b) => a - b); return s[Math.min(s.length - 1, Math.floor(p * s.length))]; };
const classes = Object.keys(PLAYERS);
const playerStats = {}; // cls -> check -> {bad, good, avg, median, hp, O, B}
const sampleRuns = {};  // cls -> check -> [run snapshots] (검증용)
for (const cls of classes) {
  rng = mulberry32(SEED + cls.length * 7919);
  const per = CHECKS.map(() => ({ powers: [], O: [], B: [], hp: [] }));
  sampleRuns[cls] = CHECKS.map(() => []);
  for (let r = 0; r < RUNS; r++) {
    const snaps = simulateRun(cls);
    snaps.forEach((s, ci) => {
      if (r < 150) sampleRuns[cls][ci].push(s);
      for (let b = 0; b < BATTLES_PER_CHECK; b++) {
        const m = measureBattle({ ...s, deck: s.deck.map(clone) });
        per[ci].powers.push(m.power); per[ci].O.push(m.O); per[ci].B.push(m.B);
      }
      per[ci].hp.push(s.maxHp);
    });
  }
  playerStats[cls] = per.map(x => {
    const bad = pct(x.powers, 0.10), good = pct(x.powers, 0.90);
    const mean = a => a.reduce((s, v) => s + v, 0) / a.length;
    return { bad, good, avg: (bad + good) / 2, median: pct(x.powers, 0.5), hp: mean(x.hp), O: mean(x.O), B: mean(x.B),
      Obad: pct(x.O, 0.10), Ogood: pct(x.O, 0.90) };
  });
}
// 직업 평균 (적은 모든 직업이 공유하므로)
const classAvg = CHECKS.map((_, ci) => {
  const m = k => classes.reduce((s, c) => s + playerStats[c][ci][k], 0) / classes.length;
  return { bad: m('bad'), good: m('good'), avg: m('avg'), hp: m('hp'), O: m('O'), B: m('B') };
});

// 층별 기준: 1층은 1층 시작 평균 전투력 그대로. 2·3층 적은 이전 층 기준 x (1 + 플레이어 성장률 - growLag)
// -> 한 마리씩은 층이 내려갈수록 플레이어보다 조금씩 약해지고, 대신 깊은 층 일반 방에 두 마리가 자주 나온다(NormalPairChances).
const layerStartIdx = [0, 2, 4], layerBossIdx = [1, 3, 5];
const base = [classAvg[0].avg];
const grow = [0], playerGrow = [0];
for (let L = 1; L < 3; L++) {
  const pg = classAvg[layerStartIdx[L]].avg / classAvg[layerStartIdx[L - 1]].avg - 1;
  const g = Math.max(0, pg - TARGET.growLag);
  playerGrow.push(pg); grow.push(g); base.push(base[L - 1] * (1 + g));
}

const INTENT_KO = { Attack: '공격', Defend: '방어', Buff: '강화', Poison: '독', Burn: '화상' };
const actsText = e => e.phases.map(p => p.acts.map(a => INTENT_KO[a.intent] + a.value).join(' ')).join(' / ');
const newEnemies = {};
const enemyRows = [];
for (let L = 0; L < 3; L++) {
  const ids = FLOOR.normal[L];
  const cur = ids.map(id => enemyPower(ENEMIES[id]).power);
  const lo = Math.min(...cur), hi = Math.max(...cur);
  ids.forEach((id, k) => {
    const ratio = hi > lo ? TARGET.normalMin + (TARGET.normalMax - TARGET.normalMin) * (cur[k] - lo) / (hi - lo) : (TARGET.normalMin + TARGET.normalMax) / 2;
    newEnemies[id] = fitEnemy(ENEMIES[id], base[L] * ratio);
    enemyRows.push({ layer: L + 1, kind: '일반', id, ratio });
  });
  for (const id of FLOOR.elite[L]) { newEnemies[id] = fitEnemy(ENEMIES[id], base[L] * TARGET.elite); enemyRows.push({ layer: L + 1, kind: '엘리트', id, ratio: TARGET.elite }); }
  const b = FLOOR.boss[L]; newEnemies[b] = fitEnemy(ENEMIES[b], base[L] * TARGET.boss); enemyRows.push({ layer: L + 1, kind: '보스', id: b, ratio: TARGET.boss });
}
for (const r of enemyRows) {
  const o = enemyPower(ENEMIES[r.id]), n = enemyPower(newEnemies[r.id]);
  const L = r.layer - 1;
  Object.assign(r, {
    name: ENEMIES[r.id].name,
    oldHp: ENEMIES[r.id].hp, oldDpt: o.dpt, oldPower: o.power, oldRatio: o.power / classAvg[layerStartIdx[L]].avg,
    newHp: newEnemies[r.id].hp, newDpt: n.dpt, newPower: n.power, newRatio: n.power / classAvg[layerStartIdx[L]].avg,
    newRatioAtBoss: n.power / classAvg[layerBossIdx[L]].avg,
    oldActs: actsText(ENEMIES[r.id]),
    newActs: actsText(newEnemies[r.id]),
  });
}

// 검증: 각 층 시작 시점 덱으로 실제 전투 (보스는 보스 직전 덱)
function validate(baseTable, group) {
  // 집단 무리원은 EnemyFactory 배율을 적용한 사본으로 싸운다
  const table = { ...baseTable };
  const memberId = id => { const e = factoryScale(baseTable[id], group.scale); table[e.id] = e; return e.id; };
  const groupLabel = group.scale === 1 ? `집단 ${group.count}마리` : `집단 ${group.count}마리(각 1/${group.count})`;
  const out = [];
  for (let L = 0; L < 3; L++) {
    const kinds = [];
    for (const [deck, ci] of [['층 시작 덱', layerStartIdx[L]], ['보스 직전 덱', layerBossIdx[L]]]) {
      kinds.push(
        { kind: '일반 1마리', deck, ci, roll: () => [pick(FLOOR.normal[L])] },
        { kind: '일반 2마리', deck, ci, roll: () => [pick(FLOOR.normal[L]), pick(FLOOR.normal[L])] },
        { kind: groupLabel, group: true, deck, ci, roll: () => Array.from({ length: group.count }, () => memberId(pick(FLOOR.normal[L]))) },
        { kind: '엘리트', deck, ci, roll: () => [pick(FLOOR.elite[L])] },
        { kind: '보스', deck, ci, roll: () => [FLOOR.boss[L]] });
    }
    for (const k of kinds) {
      const row = { layer: L + 1, kind: k.kind, isGroup: !!k.group, deck: k.deck, byClass: {} };
      let wins = 0, n = 0, lost = [];
      for (const cls of classes) {
        let cw = 0, cn = 0;
        for (const s of sampleRuns[cls][k.ci]) {
          for (let t = 0; t < 4; t++) {
            const res = fight({ ...s, deck: s.deck.map(clone) }, k.roll(), table);
            cn++; n++; if (res.win) { cw++; wins++; lost.push(res.hpLost); }
          }
        }
        row.byClass[cls] = cw / cn;
      }
      row.win = wins / n; row.hpLostMed = lost.length ? pct(lost, 0.5) : 1; row.hpLostP90 = lost.length ? pct(lost, 0.9) : 1;
      out.push(row);
    }
  }
  return out;
}
rng = mulberry32(SEED + 1);
const validationOld = validate(ENEMIES, OLD_GROUP);
rng = mulberry32(SEED + 1);
const validationNew = validate(newEnemies, FLOOR.group);

// ---------- 출력 ----------
const f1 = x => (Math.round(x * 10) / 10).toFixed(1);
const p0 = x => Math.round(x * 100) + '%';
console.log('\n## 플레이어 전투력 (직업 평균, 못 뽑음=하위10% / 잘 뽑음=상위10%)');
console.log('| 시점 | 못 뽑음 | 잘 뽑음 | 평균(기준) | 최대HP | 턴당 피해 | 턴당 방어 | 직전 대비 |');
classAvg.forEach((c, i) => console.log(`| ${CHECKS[i]} | ${f1(c.bad)} | ${f1(c.good)} | ${f1(c.avg)} | ${f1(c.hp)} | ${f1(c.O)} | ${f1(c.B)} | ${i ? p0(c.avg / classAvg[i - 1].avg - 1) : '-'} |`));
console.log('\n## 직업별 기준 전투력(평균)');
console.log('| 직업 | ' + CHECKS.join(' | ') + ' |');
for (const cls of classes) console.log(`| ${PLAYERS[cls].name} | ` + playerStats[cls].map(s => f1(s.avg)).join(' | ') + ' |');
console.log('\n## 층별 적 기준 전투력 (1층 = 1층 시작 평균, 이후 층은 x(1+g))');
base.forEach((b, L) => console.log(`${L + 1}층: 기준 ${f1(b)} (적 +${p0(grow[L])}, 플레이어 +${p0(playerGrow[L])}, 플레이어 층 시작 평균 ${f1(classAvg[layerStartIdx[L]].avg)}, 일반 방 2마리 확률 ${p0(FLOOR.pair[L])})`));
console.log('\n## 적 수치 (기존 -> 새 값)');
console.log('| 층 | 구분 | 적 | 목표 | HP | 턴당 피해 | 강함(층 시작) | 강함(보스 직전) | 패턴 |');
for (const r of enemyRows) console.log(`| ${r.layer} | ${r.kind} | ${r.name} | ${p0(r.ratio)} | ${r.oldHp}->${r.newHp} | ${f1(r.oldDpt)}->${f1(r.newDpt)} | ${p0(r.oldRatio)}->${p0(r.newRatio)} | ${p0(r.newRatioAtBoss)} | ${r.newActs} |`);
console.log('\n## 검증 전투 승률 (기존 -> 새 값)');
console.log('| 층 | 덱 | 상대 | 승률 | 이겼을 때 잃은 체력(중앙값) | 직업별 승률(새 값) |');
validationNew.forEach((v, i) => console.log(`| ${v.layer} | ${v.deck} | ${v.kind} | ${p0(validationOld[i].win)} -> ${p0(v.win)} | ${p0(validationOld[i].hpLostMed)} -> ${p0(v.hpLostMed)} | ` +
  classes.map(c => PLAYERS[c].name + ' ' + p0(v.byClass[c])).join(', ') + ' |'));

const result = {
  generatedAt: new Date().toISOString(), runs: RUNS, seed: SEED, target: TARGET, checks: CHECKS,
  classAvg, playerStats: Object.fromEntries(classes.map(c => [PLAYERS[c].name, playerStats[c]])),
  base, grow, playerGrow, pair: FLOOR.pair, enemyRows, validationOld, validationNew, classNames: classes.map(c => PLAYERS[c].name), groupOld: OLD_GROUP, groupNew: FLOOR.group,
};
fs.writeFileSync(path.join(__dirname, 'result.json'), JSON.stringify(result, null, 1));

// ---------- --apply: EnemyDatabase.cs 숫자만 바꿔 쓰기 ----------
if (APPLY) {
  let src = ENEMY_SRC; const edits = [];
  for (const id of Object.keys(newEnemies)) {
    const e = ENEMIES[id], n = newEnemies[id];
    let seg = src.slice(e.span[0], e.span[1]);
    seg = seg.replace(/hp = \d+/, 'hp = ' + n.hp);
    const vals = n.phases.flatMap(p => p.acts.map(a => a.value)); let k = 0;
    seg = seg.replace(/(intent = EnemyIntent\.\w+,\s*value = )(\d+)/g, (_, pre) => pre + vals[k++]);
    if (k !== vals.length) throw new Error('값 개수 불일치: ' + id);
    edits.push({ from: e.span[0], to: e.span[1], seg });
  }
  edits.sort((a, b) => b.from - a.from);
  for (const ed of edits) src = src.slice(0, ed.from) + ed.seg + src.slice(ed.to);
  fs.writeFileSync(path.join(ROOT, 'Assets', 'Scripts', 'Database', 'EnemyDatabase.cs'), src);
  console.log('\nEnemyDatabase.cs에 새 수치를 썼습니다 (' + edits.length + '종).');
}
