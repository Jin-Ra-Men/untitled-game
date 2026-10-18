namespace UntitledGame.Negotiation
{
    public enum InputKind
    {
        Choice,
        Silence,
        Interrupt
    }

    public enum DeadlinePressure
    {
        Relaxed,
        Tight,
        Critical
    }

    public enum EpisodePhase
    {
        Opening,
        PreChoice,
        Beat1,
        Beat2,
        Beat3,
        Beat4,
        Outcome,
        Done
    }

    public enum EpisodeOutcome
    {
        None,
        MinSuccess,
        Ambiguous,
        Failure,
        Escalate
    }

    public readonly struct ChoiceOption
    {
        public ChoiceOption(string id, string label, string? hint = null)
        {
            Id = id;
            Label = label;
            Hint = hint;
        }

        public string Id { get; }
        public string Label { get; }
        public string? Hint { get; }
    }

    public sealed class NegotiationFrame
    {
        public EpisodePhase Phase { get; set; }
        public string Speaker { get; set; } = "";
        public string Line { get; set; } = "";
        public string StatusLine { get; set; } = "";
        public ChoiceOption[] Choices { get; set; } = System.Array.Empty<ChoiceOption>();
        public bool AllowSilence { get; set; }
        public bool AllowInterrupt { get; set; }
        public float InterruptWindowSeconds { get; set; }
        public float SilenceFailSeconds { get; set; } = 2.5f;
        public bool IsOutcome { get; set; }
        public EpisodeOutcome Outcome { get; set; }
        public string OutcomeTitle { get; set; } = "";
        public string OutcomeBody { get; set; } = "";
    }

    public sealed class NegotiationState
    {
        public int Cooperation { get; set; }
        public int Pride { get; set; }
        public float MinutesLeft { get; set; } = 14f;
        public DeadlinePressure Pressure { get; set; } = DeadlinePressure.Relaxed;
        public bool TriggerFired { get; set; }
        public bool GotPhoneInfo { get; set; }
        public bool SlackPrepared { get; set; }
        public bool ManagerEyeRaised { get; set; }
        public bool BadgeThreatUsed { get; set; }
        public bool ParentRouteUsed { get; set; }
        public bool TeamLeadPingShown { get; set; }

        public void Clamp()
        {
            if (Cooperation < 0) Cooperation = 0;
            if (Cooperation > 3) Cooperation = 3;
            if (Pride < -2) Pride = -2;
            if (Pride > 2) Pride = 2;
            if (MinutesLeft < 0f) MinutesLeft = 0f;

            if (MinutesLeft <= 3f)
                Pressure = DeadlinePressure.Critical;
            else if (MinutesLeft <= 8f)
                Pressure = DeadlinePressure.Tight;
            else
                Pressure = DeadlinePressure.Relaxed;
        }

        public string DebugSummary()
        {
            return $"협조 {Cooperation}/3 · 자존심 {Pride:+0;-0;0} · 남은 {MinutesLeft:0.0}분 · {Pressure}";
        }
    }
}
