namespace EnliStandardAssets.XR
{
    public interface ICoachingProvider
    {
        void ShowHint();
        void HideHint();

        bool IsSupported { get; }
        bool IsCoachingActive { get; }
    }
}