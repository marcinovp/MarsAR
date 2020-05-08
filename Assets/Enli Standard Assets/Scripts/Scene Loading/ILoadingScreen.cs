namespace EnliStandardAssets
{
    public interface ILoadingScreen
    {
        void ShowLoading(bool animate, System.Action onActionFinished);
        void HideLoading(bool animate, System.Action onActionFinished);
        void SetProgress(float progress);
    }
}