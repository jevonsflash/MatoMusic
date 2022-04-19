using Abp.Dependency;
using MatoMusic.Core.Services;

namespace MatoMusic;

public partial class MainPage : Shell, ITransientDependency
{
    private readonly IocManager iocManager;

    public MainPage(IocManager iocManager)
	{
		InitializeComponent();
        this.iocManager = iocManager;
        this.Init();
        Loaded += MainPage_Loaded;
    }

    private async void MainPage_Loaded(object sender, EventArgs e)
    {
        var musicRelatedViewModel = iocManager.Resolve<MusicRelatedService>();
        await musicRelatedViewModel.InitAll();
    }

    private void Init()
    {
        var nowPlayingPage = iocManager.Resolve<NowPlayingPage>();
        var queuePage = iocManager.Resolve<QueuePage>();
        var playlistPage = iocManager.Resolve<PlaylistPage>();
        var libraryMainPage = iocManager.Resolve<LibraryMainPage>();
        this.NowPlayingPageShellContent.Content = nowPlayingPage;
        this.QueuePageShellContent.Content = queuePage;
        this.LibraryMainPageShellContent.Content = libraryMainPage;
        this.PlaylistPageShellContent.Content = playlistPage;
    }

}