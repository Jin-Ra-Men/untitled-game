using UntitledGame.Negotiation;
using UntitledGame.Negotiation.Episodes;
using UntitledGame.Negotiation.Localization;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var lang = "ko";
for (var i = 0; i < args.Length - 1; i++)
{
    if (args[i] is "--lang" or "-l")
        lang = args[i + 1];
}

LocLoader.LoadLanguage(LocLoader.FindDefaultLocRoot(), lang);
Console.Title = Loc.T("playtest.title");

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
    Console.WriteLine(frame.IsChat ? $"[DM · {frame.Speaker}]" : $"[{frame.Speaker}]");
    Console.WriteLine(frame.Line);
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine(frame.StatusLine);
    if (frame.AllowSilence && frame.AllowInterrupt)
        Console.WriteLine(Loc.F("playtest.hint_both", frame.SilenceFailSeconds, frame.InterruptWindowSeconds));
    else if (frame.AllowSilence)
        Console.WriteLine(Loc.F("playtest.hint_silence", frame.SilenceFailSeconds));
    else if (frame.AllowInterrupt)
        Console.WriteLine(Loc.F("playtest.hint_interrupt", frame.InterruptWindowSeconds));
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

    if (frame.AutoAdvance)
    {
        episode.Advance();
        continue;
    }

    var input = ReadInput(episode, frame);
    if (input == null) continue;

    if (input == "quit")
    {
        Console.WriteLine(Loc.T("playtest.bye"));
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
    var ok = SmokeRoute(EpisodeOutcome.MinSuccess, "B", "rule", "online", "now", "stall");
    ok &= SmokeRoute(EpisodeOutcome.Ambiguous, "A", "family", "snark", "payhold", "honest");
    ok &= SmokeRoute(EpisodeOutcome.Escalate, "C", "money", "push", "mask", "panic");
    return ok ? 0 : 2;
}

static bool SmokeRoute(EpisodeOutcome expected, params string[] choices)
{
    var ep = new FamilyCertificateEpisode();
    ep.Start();
    ContinueIfNeeded(ep);
    foreach (var id in choices)
    {
        ep.SubmitChoice(id);
        ContinueIfNeeded(ep);
    }

    var got = ep.CurrentFrame.Outcome;
    Console.WriteLine($"{string.Join(" > ", choices)} => {got} (expected {expected})");
    return got == expected;
}

static void ContinueIfNeeded(FamilyCertificateEpisode ep)
{
    while (ep.CurrentFrame.AutoAdvance)
        ep.Advance();
    while (ep.CurrentFrame.Choices.Length == 1 && ep.CurrentFrame.Choices[0].Id == "continue")
        ep.SubmitChoice("continue");
}

static void PrintHelp()
{
    Console.WriteLine(Loc.T("playtest.help_1"));
    Console.WriteLine(Loc.T("playtest.help_2"));
    Console.WriteLine();
    Console.WriteLine(Loc.T("playtest.help_3"));
    Console.WriteLine(Loc.T("playtest.help_4"));
    Console.WriteLine();
}

static string? ReadInput(FamilyCertificateEpisode episode, NegotiationFrame frame)
{
    var silenceBudget = frame.AllowSilence ? frame.SilenceFailSeconds : 0f;
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
            return "__interrupt__";

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
