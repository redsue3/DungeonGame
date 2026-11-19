// Unity 없이 게임 코드를 돌리기 위한 최소한의 UnityEngine 대체품.
// BattleSim이 함께 컴파일하는 게임 스크립트(BattleManager, Deck, Enemy, FloorGenerator 등)가 쓰는 것만 흉내 낸다.
// 새 게임 코드가 여기 없는 Unity 기능을 쓰면 컴파일 오류가 나니, 그때 필요한 만큼만 더한다.
using System;
using System.Collections;

namespace UnityEngine
{
    public static class Random
    {
        private static System.Random rng = new System.Random(1);

        public static void InitState(int seed) => rng = new System.Random(seed);

        // Unity와 같이 정수는 max 미포함, 실수는 max 포함
        public static int   Range(int minInclusive, int maxExclusive) => maxExclusive <= minInclusive ? minInclusive : rng.Next(minInclusive, maxExclusive);
        public static float Range(float minInclusive, float maxInclusive) => minInclusive + (float)rng.NextDouble() * (maxInclusive - minInclusive);
        public static float value => (float)rng.NextDouble();
    }

    public static class Mathf
    {
        public static int   Max(int a, int b) => a > b ? a : b;
        public static float Max(float a, float b) => a > b ? a : b;
        public static int   Max(params int[] v)   { int m = v[0];   foreach (int x in v)   if (x > m) m = x; return m; }
        public static float Max(params float[] v) { float m = v[0]; foreach (float x in v) if (x > m) m = x; return m; }
        public static int   Min(int a, int b) => a < b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static int   Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static int   Abs(int v) => v < 0 ? -v : v;
        public static float Abs(float v) => v < 0 ? -v : v;
        // Unity의 RoundToInt도 0.5에서 짝수 쪽으로 반올림한다 (Math.Round 기본값과 같음)
        public static int   RoundToInt(float v) => (int)Math.Round(v);
        public static int   FloorToInt(float v) => (int)Math.Floor(v);
        public static int   CeilToInt(float v) => (int)Math.Ceiling(v);
        public static bool  Approximately(float a, float b) => Math.Abs(a - b) < Math.Max(1e-6f * Math.Max(Math.Abs(a), Math.Abs(b)), 1.1754944E-38f * 8f);
    }

    public static class Debug
    {
        // 전투 한 판에 로그가 수백 줄 나오므로 기본은 끈다. LogError는 항상 센다.
        public static bool Verbose;
        public static int  ErrorCount;
        public static string LastError;

        public static void Log(object message)        { if (Verbose) Console.WriteLine(message); }
        public static void LogWarning(object message) { if (Verbose) Console.WriteLine("[경고] " + message); }
        public static void LogError(object message)
        {
            ErrorCount++;
            LastError = message?.ToString();
            Console.WriteLine("[오류] " + message);
        }
    }

    public class Object { }
    public class Component : Object { }
    public class Behaviour : Component { }

    // 코루틴은 그 자리에서 끝까지 돌린다 (WaitForSeconds 같은 연출 대기는 건너뜀).
    // BattleManager.UseCard/TryMovePlayer가 시간을 흘려보내는 SpendTime이 호출 안에서 바로 끝나므로,
    // 호출이 돌아오면 적 행동까지 다 반영된 상태가 된다.
    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator routine)
        {
            Run(routine);
            return new Coroutine();
        }

        public void StopAllCoroutines() { }

        private static void Run(IEnumerator routine)
        {
            while (routine.MoveNext())
                if (routine.Current is IEnumerator nested) Run(nested);
        }
    }

    public sealed class Coroutine { }
    public class YieldInstruction { }
    public sealed class WaitForSeconds : YieldInstruction { public WaitForSeconds(float seconds) { } }
}
