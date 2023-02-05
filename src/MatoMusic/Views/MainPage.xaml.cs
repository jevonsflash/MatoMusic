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
        this.NowPlayingPageShellContent.Content = nowPlayingPage;
        this.QueuePageShellContent.Content = queuePage;
        this.PlaylistPageShellContent.Content = playlistPage;

        var musicPage = iocManager.Resolve<MusicPage>();
        var albumPage = iocManager.Resolve<AlbumPage>();
        var artistPage = iocManager.Resolve<ArtistPage>();

        this.MusicPageShellContent.Content = musicPage;
        this.ArtistPageShellContent.Content = artistPage;
        this.AlbumPageShellContent.Content = albumPage;
    }

}