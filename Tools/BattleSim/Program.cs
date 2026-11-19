// 던전 게임 전투 시뮬레이터 - 사용법은 README.md
using System;
using System.Globalization;

public class Options
{
    public string          Mode = "balance";
    public int             Battles = 200;
    public int             Seed = 20261119;
    public string          SnapshotPath;
    public CharacterClass? Class;
    public int?            Layer;

    public static Options Parse(string[] args)
    {
        var o = new Options();
        for (int i = 0; i < args.Length; i++)
        {
            string a = args[i];
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException(a + " 뒤에 값이 필요합니다");
            float F() => float.Parse(Next(), CultureInfo.InvariantCulture);
            switch (a)
            {
                case "balance": case "check": o.Mode = a; break;
                case "--battles":   o.Battles = int.Parse(Next()); break;
                case "--seed":      o.Seed = int.Parse(Next()); break;
                case "--snapshots": o.SnapshotPath = Next(); break;
                case "--layer":     o.Layer = int.Parse(Next()); break;
                case "--class":     o.Class = ParseClass(Next()); break;
                case "--verbose":   UnityEngine.Debug.Verbose = true; break;
                case "--kite":      Arena.Kite = true; break;
                // 시간 규칙을 코드 수정 없이 바꿔 보기 (BattleManager의 static 값)
                case "--move":          BattleManager.MoveTime = F(); break;
                case "--card":          BattleManager.CardTime = F(); break;
                case "--recovery":      BattleManager.RecoveryDuration = F(); break;
                case "--recovery-move": BattleManager.RecoveryMoveTime = F(); break;
                default: throw new ArgumentException("모르는 옵션: " + a);
            }
        }
        return o;
    }

    private static CharacterClass ParseClass(string s) => s switch
    {
        "전사" => CharacterClass.Warrior,
        "도적" => CharacterClass.Rogue,
        "마법사" => CharacterClass.Mage,
        "성기사" => CharacterClass.Paladin,
        _ => (CharacterClass)Enum.Parse(typeof(CharacterClass), s, true),
    };
}

public static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Options opt;
        try { opt = Options.Parse(args); }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("사용법: balance [--battles N] [--layer 1~3] [--class 전사] [--seed N] [--move 0.5 --card 0.25 --recovery 0.5 --recovery-move 1] | check");
            return 2;
        }
        return opt.Mode == "check" ? FlowChecks.Run() : Balance.Run(opt);
    }
}
