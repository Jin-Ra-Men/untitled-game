using UntitledGame.Negotiation.Localization;

namespace UntitledGame.Negotiation
{
    public enum InputKind
    {
        Choice,
        Silence,
        Interrupt
    }

    /// <summary>Juusei-style Emotion bar is 10 discrete cells in the UI.</summary>
    public static class EmotionBar
    {
        public const int Segments = 10;

        public static int FilledCells(float emotion01) =>
            System.Math.Clamp((int)System.Math.Round(emotion01 * Segments), 0, Segments);
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
        /// <summary>Resources/Portraits/{PortraitId} without extension. Empty = hide portrait.</summary>
        public string PortraitId { get; set; } = "";
        /// <summary>
        /// Portrait pose suffix: "" (auto look-at partner), "front", "left", "right", "chat" (typing / texting).
        /// Loads Resources/Portraits/{id}_{pose} when set / auto.
        /// </summary>
        public string PortraitPose { get; set; } = "";
        /// <summary>Who the intern is addressing (drives the partner-side portrait). Empty = keep current partner.</summary>
        public string ListenerPortraitId { get; set; } = "";
        /// <summary>Resources/Backgrounds/{id} for intern side (usually company office).</summary>
        public string LeftBackgroundId { get; set; } = "office_hr";
        /// <summary>Resources/Backgrounds/{id} for partner side (cafe, manager desk, …).</summary>
        public string RightBackgroundId { get; set; } = "cafe";
        /// <summary>Line is a messenger message (bubble), not spoken aloud.</summary>
        public bool IsChat { get; set; }
        /// <summary>Messenger window is on screen.</summary>
        public bool MessengerOpen { get; set; }
        /// <summary>Changes when the chat log should start over (e.g. retry).</summary>
        public int ChatSession { get; set; }
        /// <summary>Partner has read the intern's pending messages (clears the unread "1").</summary>
        public bool ChatSeen { get; set; }
        public ChoiceOption[] Choices { get; set; } = System.Array.Empty<ChoiceOption>();
        public bool AllowSilence { get; set; }
        public float SilenceFailSeconds { get; set; } = 14f;
        /// <summary>Timed cut-in while the prompt is live (Juusei-style interrupt).</summary>
        public bool AllowInterrupt { get; set; }
        public float InterruptWindowSeconds { get; set; } = 6f;
        /// <summary>When true, UI shows a POSITIVE cue — interrupt tends to help.</summary>
        public bool InterruptIsPositive { get; set; }
        /// <summary>After the line finishes (typewriter / read), auto-advance without a Continue choice.</summary>
        public bool AutoAdvance { get; set; }
        public bool IsOutcome { get; set; }
        public EpisodeOutcome Outcome { get; set; }
        public string OutcomeTitle { get; set; } = "";
        public string OutcomeBody { get; set; } = "";
    }

    public sealed class NegotiationState
    {
        public int Cooperation { get; set; }
        public int Pride { get; set; }
        /// <summary>0..1 partner agitation / heat. UI shows as 10 cells.</summary>
        public float Emotion01 { get; set; } = 0.4f;
        public float MinutesLeft { get; set; } = 14f;
        public DeadlinePressure Pressure { get; set; } = DeadlinePressure.Relaxed;
        public bool TriggerFired { get; set; }
        /// <summary>Partner already knows how to issue it online (link / simple auth).</summary>
        public bool KnowsOnlineRoute { get; set; }
        /// <summary>Intern prepared the issuing guide before messaging.</summary>
        public bool GuidePrepared { get; set; }
        public bool ManagerEyeRaised { get; set; }
        /// <summary>Partner complained about privacy.</summary>
        public bool PrivacyRaised { get; set; }
        public bool PayHoldThreatUsed { get; set; }
        public bool TeamLeadPingShown { get; set; }

        public void Clamp()
        {
            if (Cooperation < 0) Cooperation = 0;
            if (Cooperation > 3) Cooperation = 3;
            if (Pride < -2) Pride = -2;
            if (Pride > 2) Pride = 2;
            if (Emotion01 < 0f) Emotion01 = 0f;
            if (Emotion01 > 1f) Emotion01 = 1f;
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
            var pressure = Pressure switch
            {
                DeadlinePressure.Tight => Loc.T("status.pressure.tight"),
                DeadlinePressure.Critical => Loc.T("status.pressure.critical"),
                _ => Loc.T("status.pressure.relaxed")
            };
            var pride = Pride.ToString("+0;-0;0");
            var emo = EmotionBar.FilledCells(Emotion01);
            return Loc.F("status.debug", Cooperation, pride, MinutesLeft, pressure, emo);
        }
    }
}
