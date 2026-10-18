using UntitledGame.Negotiation;
using UntitledGame.Negotiation.Episodes;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.Title = "미제 — 가족관계증명서 교섭 프로토타입";

if (args.Contains("--smoke", StringComparer.OrdinalIgnoreCase))
{
    Environment.Exit(RunSmoke());
    return;
}

var episode = new FamilyCertificateEpisode();
episode.Start();

PrintHelp();

while (true)
{
    var frame = episode.CurrentFrame;
    Console.WriteLine();
    Console.WriteLine(new string('─', 60));
    Console.WriteLine($"[{frame.Speaker}]");
    Console.WriteLine(frame.Line);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(frame.StatusLine);
    if (frame.AllowInterrupt)
        Console.WriteLine($"끼어들기 창 ~{frame.InterruptWindowSeconds:0.0}s (키 I) · 침묵 자동 {frame.SilenceFailSeconds:0.0}s 또는 키 S");
    else if (frame.AllowSilence)
        Console.WriteLine($"침묵 자동 {frame.SilenceFailSeconds:0.0}s 또는 키 S");
    Console.ResetColor();

    if (frame.Choices.Length > 0)
    {
        Console.WriteLine();
        for (var i = 0; i < frame.Choices.Length; i++)
        {
            var c = frame.Choices[i];
            var hint = string.IsNullOrEmpty(c.Hint) ? "" : $"  ({c.Hint})";
            Console.WriteLine($"  [{i + 1}] {c.Label}{hint}");
        }
    }

    if (frame.IsOutcome && frame.OutcomeTitle.Length > 0)
    {
        // already printed in Line
    }

    var input = ReadInput(episode, frame);
    if (input == null) continue;

    if (input == "quit")
    {
        Console.WriteLine("종료.");
        break;
    }

    switch (input)
    {
        case "__silence__":
            episode.SubmitSilence();
            break;
        case "__interrupt__":
            episode.SubmitInterrupt();
            break;
        default:
            episode.SubmitChoice(input);
            break;
    }
}

static int RunSmoke()
{
    var ep = new FamilyCertificateEpisode();
    ep.Start();
    ep.SubmitChoice("B");
    ep.SubmitInterrupt();
    ContinueIfNeeded(ep);
    ep.SubmitInterrupt();
    ContinueIfNeeded(ep);
    ep.SubmitChoice("practical");
    ContinueIfNeeded(ep);
    ep.SubmitChoice("stall");

    Console.WriteLine(ep.CurrentFrame.OutcomeTitle);
    Console.WriteLine(ep.CurrentFrame.Outcome);
    return ep.CurrentFrame.Outcome == EpisodeOutcome.MinSuccess ? 0 : 2;
}

static void ContinueIfNeeded(FamilyCertificateEpisode ep)
{
    while (ep.CurrentFrame.Choices.Length == 1 && ep.CurrentFrame.Choices[0].Id == "continue")
        ep.SubmitChoice("continue");
}

static void PrintHelp()
{
    Console.WriteLine("미제 프로토타입 · 테스트 에피소드: 가족관계증명서");
    Console.WriteLine("근거: docs/test-episode-family-certificate.md");
    Console.WriteLine();
    Console.WriteLine("조작: 숫자 = 선택 · S = 침묵 · I = 끼어들기 · Q = 종료 · H = 도움말");
    Console.WriteLine("실시간: 입력 없이 기다리면 침묵이 자동으로 들어간다.");
    Console.WriteLine();
}

static string? ReadInput(FamilyCertificateEpisode episode, NegotiationFrame frame)
{
    var silenceBudget = frame.AllowSilence ? frame.SilenceFailSeconds : 0f;
    var interruptBudget = frame.AllowInterrupt ? frame.InterruptWindowSeconds : 0f;
    var started = DateTime.UtcNow;

    while (true)
    {
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(50);
            var elapsed = (float)(DateTime.UtcNow - started).TotalSeconds;
            episode.TickRealtime(0.05f);

            if (frame.AllowSilence && elapsed >= silenceBudget)
                return "__silence__";

            // refresh interrupt availability via episode tick only
            if (Console.KeyAvailable) break;
        }

        var key = Console.ReadKey(intercept: true);

        if (key.Key == ConsoleKey.H)
        {
            PrintHelp();
            return null;
        }

        if (key.Key == ConsoleKey.Q)
            return "quit";

        if (key.Key == ConsoleKey.S && frame.AllowSilence)
            return "__silence__";

        if (key.Key == ConsoleKey.I && frame.AllowInterrupt)
        {
            // Late interrupt still attempted; episode gates by window.
            return "__interrupt__";
        }

        if (key.Key == ConsoleKey.Enter && frame.Choices.Length == 1)
            return frame.Choices[0].Id;

        if (char.IsDigit(key.KeyChar))
        {
            var index = key.KeyChar - '1';
            if (index >= 0 && index < frame.Choices.Length)
                return frame.Choices[index].Id;
        }
    }
}
