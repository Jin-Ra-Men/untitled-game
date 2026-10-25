namespace UntitledGame.Negotiation
{
    public interface INegotiationEpisode
    {
        string Title { get; }
        NegotiationState State { get; }
        NegotiationFrame CurrentFrame { get; }
        bool IsFinished { get; }

        void Start();
        void SubmitChoice(string choiceId);
        void SubmitSilence();
        void SubmitInterrupt();
        /// <summary>Advances AutoAdvance dialogue bridges (no Continue button).</summary>
        void Advance();
        void TickRealtime(float deltaSeconds);
        void RestartFromNegotiation();
    }
}
