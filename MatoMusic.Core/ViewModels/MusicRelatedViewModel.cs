using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MatoMusic.Core.Interfaces;
using Abp.Dependency;
using Microsoft.Maui.Essentials;
using Abp.Configuration;
using MatoMusic.Core.Settings;
using MatoMusic.Infrastructure.Helper;

namespace MatoMusic.Core.ViewModel
{
    public class MusicRelatedViewModel : ObservableObject, ISingletonDependency
    {

        private bool _isInited = false;

        private bool _isFastSeeking = false;


        

        public bool IsInitFinished = false;

        public event EventHandler OnMusicChanged;
        public static class Properties
        {
            public const string CurrentMusic = "CurrentMusic";
            public const string IsPlaying = "IsPlaying";
            public const string CurrentTime = "CurrentTime";
            public const string Duration = "Duration";
            public const string Musics = "Musics";
            public const string IsRepeatOne = "IsRepeatOne";
            public const string IsShuffle = "IsShuffle";
        }

        public MusicRelatedViewModel(IMusicInfoManager musicInfoManager, 
            
            IMusicSystem musicSystem,
            ISettingManager settingManager
            )
        {
            Device.StartTimer(new TimeSpan(0, 0, 0, 0, 100), DoUpdate);

            this.PlayCommand = new Command(PlayAction, CanPlayExcute);
            this.PreCommand = new Command(PreAction,CanPlayExcute );
            this.NextCommand = new Command(NextAction,CanPlayExcute);
            this.RepeatOneCommand = new Command(RepeatOneAction,CanPlayExcute );
            this.ShuffleCommand = new Command(ShuffleAction,CanPlayExcute );
            this.FavouriteCommand = new Command(FavouriteAction,CanPlayExcute );
            this.PropertyChanged += DetailPageViewModel_PropertyChanged;
            musicSystem.OnPlayFinished += MusicSystem_OnMusicChanged;
            musicSystem.RebuildMusicInfos(MusicSystem_OnRebuildMusicInfosFinished);
            this.musicInfoManager = musicInfoManager;
            this.musicSystem = musicSystem;
            this.settingManager = settingManager;
        }

        private void MusicSystem_OnRebuildMusicInfosFinished()
        {
            //当队列初始化完成时初始化当前曲目
           Device.BeginInvokeOnMainThread(() =>
            {
                InitCurrentMusic();
            });
            musicSystem.SetRepeatOneStatus(IsRepeatOne);
            musicSystem.OnPlayStatusChanged += MusicSystem_OnPlayStatusChanged;
            this._isInited = true;
        }

        private void MusicSystem_OnPlayStatusChanged(object sender, bool e)
        {
            this.IsPlaying = e;
        }

        private void FavouriteAction(object obj)
        {
            CurrentMusic.IsFavourite = !CurrentMusic.IsFavourite;
            if (CurrentMusic.IsFavourite)
            {
                CommonHelper.ShowMsg("我最喜爱");

            }
        }


        public bool CanPlayExcute(object obj)
        {
            var result = this.CurrentMusic != null;
            return result;
        }

        #region methods
        private void MusicSystem_OnMusicChanged(Object sender, bool e)
        {
            NextAction(null);
        }

        private bool DoUpdate()
        {
            this.CurrentTime = GetPlatformSpecificTime(musicSystem.CurrentTime);
            this.Duration = GetPlatformSpecificTime(musicSystem.Duration);

            return true;
        }

        /// <summary>
        /// 获取指定平台的准确时间
        /// </summary>
        /// <param name="originTime"></param>
        /// <returns></returns>
        private double GetPlatformSpecificTime(double originTime)
        {
            double resultTime;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    resultTime = originTime;
                    break;
                case Device.Android:
                    resultTime = originTime / 1000;
                    break;

                case Device.UWP:
                case Device.WPF:
                    resultTime = originTime;
                    break;
                default:
                    resultTime = 0;
                    break;
            }
            return resultTime;

        }

        private async void DetailPageViewModel_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Properties.CurrentMusic)
            {
                Canplay = CanPlayExcute(null);
                if (CurrentMusic == null || _isInited == false)
                {
                    return;

                }
                musicSystem.InitPlayer(CurrentMusic);
                musicSystem.Play(CurrentMusic);
                DoUpdate();
                InitPreviewAndNextMusic();
                OnMusicChanged?.Invoke(this, EventArgs.Empty);
                this.Duration = GetPlatformSpecificTime(musicSystem.Duration);
                this.settingManager.ChangeSettingForApplication(CommonSettingNames.BreakPointMusicIndex, Musics.IndexOf(CurrentMusic).ToString());
                RaiseCanPlayExecuteChanged();
            }

            else if (e.PropertyName == Properties.IsShuffle)
            {
                this.settingManager.ChangeSettingForApplication(CommonSettingNames.IsShuffle, this.IsShuffle.ToString());
                if (IsShuffle)
                {
                    await musicSystem.UpdateShuffleMap();
                    InitPreviewAndNextMusic();
                }
                else
                {
                    InitPreviewAndNextMusic();
                }
            }

            else if (e.PropertyName == Properties.IsRepeatOne)
            {
                this.settingManager.ChangeSettingForApplication(CommonSettingNames.IsRepeatOne, this.IsRepeatOne.ToString());
                musicSystem.SetRepeatOneStatus(this.IsRepeatOne);
            }
        }

        /// <summary>
        /// 可播放状态改变通知
        /// </summary>
        private void RaiseCanPlayExecuteChanged()
        {
            PlayCommand.ChangeCanExecute();
            PreCommand.ChangeCanExecute();
            NextCommand.ChangeCanExecute();
            ShuffleCommand.ChangeCanExecute();
            RepeatOneCommand.ChangeCanExecute();
            FavouriteCommand.ChangeCanExecute();
        }

        /// <summary>
        /// 初始化下一首/上一首曲目
        /// </summary>
        public void InitPreviewAndNextMusic()
        {
            this.PreviewMusic = musicSystem.GetPreMusic(this.CurrentMusic, IsShuffle);
            this.NextMusic = musicSystem.GetNextMusic(this.CurrentMusic, IsShuffle);
        }

        /// <summary>
        /// 下一曲
        /// </summary>
        /// <param name="obj"></param>
        public void NextAction(object obj)
        {
            var next = musicSystem.GetNextMusic(this.CurrentMusic, IsShuffle);
            if (next != null)
            {
                this.CurrentMusic = next;

            }
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        /// <param name="obj"></param>
        public void PreAction(object obj)
        {
            var pre = musicSystem.GetPreMusic(this.CurrentMusic, IsShuffle);
            if (pre != null)
            {
                this.CurrentMusic = pre;

            }
        }

        /// <summary>
        /// 播放/暂停
        /// </summary>
        /// <param name="obj"></param>
        public void PlayAction(object obj)
        {
            musicSystem.PauseOrResume();
        }

        /// <summary>
        /// 随机/顺序播放
        /// </summary>
        /// <param name="obj"></param>
        public void ShuffleAction(object obj)
        {
            this.IsShuffle = !this.IsShuffle;
        }

        /// <summary>
        /// 单曲循环/全列表播放
        /// </summary>
        /// <param name="obj"></param>
        public void RepeatOneAction(object obj)
        {
            this.IsRepeatOne = !this.IsRepeatOne;

        }

        /// <summary>
        /// 更改当前进度
        /// </summary>
        /// <param name="progress"></param>
        public void ChangeProgess(double progress)
        {
            if (Math.Abs(progress - GetPlatformSpecificTime(musicSystem.CurrentTime)) > 2.0)
            {
                musicSystem.SeekTo(progress);
            }
        }

        public async void StartFastSeeking(int increment)
        {
            var seekingDuration = 0;
            _isFastSeeking = true;
            await Task.Run((Action)(() =>
            {

                while (_isFastSeeking)
                {
                    var currentTime = this.musicSystem.CurrentTime;

                    var seekingSpan = 0;

                    if (seekingDuration >= 0 && seekingDuration < 6)
                    {
                        seekingSpan = increment;
                    }
                    else if (seekingDuration >= 6 && seekingDuration < 12)
                    {
                        seekingSpan = increment * 4;
                    }
                    else if (seekingDuration >= 12)
                    {
                        seekingSpan = increment * 8;
                    }
                    this.musicSystem.SeekTo(currentTime + seekingSpan);
                    Task.Delay(500).Wait();
                    Debug.WriteLine((string)("Fastseeking:" + currentTime + "\n SeekingDuration:" + seekingDuration));
                    seekingDuration++;
                }
            }));



        }

        public void EndFastSeeking()
        {
            this._isFastSeeking = false;
        }


        /// <summary>
        /// 以MusicInfo对象切歌
        /// </summary>
        /// <param name="musicInfo"></param>
        public void ChangeMusic(MusicInfo musicInfo)
        {
            this.CurrentMusic = musicInfo;
        }

        /// <summary>
        /// 以歌曲标题切歌
        /// </summary>
        /// <param name="title"></param>
        public void ChangeMusic(string title)
        {
            this.CurrentMusic = Musics.FirstOrDefault(c => c.Title == title);
        }

        private bool _canPlay;

        /// <summary>
        /// 是否可播放
        /// </summary>
        public bool Canplay
        {
            get { return _canPlay; }
            set
            {
                _canPlay = value;
                RaisePropertyChanged();
            }
        }


        private List<MusicInfo> _musics;

        /// <summary>
        /// 当前播放列表
        /// </summary>
        public List<MusicInfo> Musics
        {
            get
            {

                _musics = musicSystem.MusicInfos;
                return _musics;
            }
            set
            {
                _musics = value;
                RaisePropertyChanged();
            }
        }

        private MusicInfo _currentMusic;

        /// <summary>
        /// 当前曲目
        /// </summary>
        public MusicInfo CurrentMusic
        {
            get
            {
                return _currentMusic;
            }
            set
            {
                _currentMusic = value;
                RaisePropertyChanged();
            }
        }

        public void InitCurrentMusic()
        {           
            var musicIndex = int.Parse(this.settingManager.GetSettingValue(CommonSettingNames.BreakPointMusicIndex));
            if (Musics.Count > 0)
            {
                if (musicIndex >= 0 && musicIndex <= Musics.Count - 1)
                {
                    CurrentMusic = Musics[musicIndex];
                }
                else
                {
                    CurrentMusic = Musics[0];
                }
                musicSystem.InitPlayer(CurrentMusic);

                this.Duration = GetPlatformSpecificTime(musicSystem.Duration);
            }
            else
            {
                this.Duration = 0;
                this.CurrentTime = 0;
            }
        }

        private bool _isPlaying;

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;

                    RaisePropertyChanged();

                }
            }
        }

        private double _currentTime;

        /// <summary>
        /// 当前进度
        /// </summary>
        public double CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                RaisePropertyChanged();
            }
        }

        private double _duration;

        /// <summary>
        /// 当前曲目总时间
        /// </summary>
        public double Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;

                RaisePropertyChanged();
            }
        }

        private MusicInfo _previewMusic;

        /// <summary>
        /// 上一曲目
        /// </summary>
        public MusicInfo PreviewMusic
        {
            get
            {
                return _previewMusic;
            }
            set
            {
                _previewMusic = value;
                RaisePropertyChanged();
            }
        }
        private MusicInfo _nextMusic;

        /// <summary>
        /// 下一曲目
        /// </summary>
        public MusicInfo NextMusic
        {
            get
            {
                return _nextMusic;
            }
            set
            {
                _nextMusic = value;
                RaisePropertyChanged();
            }
        }

        private bool _isShuffle;

        /// <summary>
        /// 是否随机播放
        /// </summary>
        public bool IsShuffle
        {
            get
            {
                if (!IsInitFinished)
                {
                    this._isShuffle =bool.Parse(this.settingManager.GetSettingValue(CommonSettingNames.IsShuffle));
                    IsInitFinished = true;
                }

                return _isShuffle;
            }
            set
            {
                if (value != _isShuffle)
                {
                    _isShuffle = value;
                    RaisePropertyChanged();

                }
            }
        }

        /// <summary>
        /// 是否单曲循环
        /// </summary>
        private bool _isRepeatOne;
        private readonly IMusicInfoManager musicInfoManager;
        private readonly IMusicSystem musicSystem;
        private readonly ISettingManager settingManager;

        public bool IsRepeatOne
        {
            get
            {
                if (!IsInitFinished)
                {
                    this._isRepeatOne = bool.Parse(this.settingManager.GetSettingValue(CommonSettingNames.IsRepeatOne));
                    IsInitFinished = true;
                }
                return _isRepeatOne;
            }
            set
            {
                if (value != _isRepeatOne)
                {
                    _isRepeatOne = value;
                    RaisePropertyChanged();

                }
            }
        }
        #endregion

        public Command PlayCommand { get; set; }

        public Command PreCommand { get; set; }

        public Command NextCommand { get; set; }

        public Command ShuffleCommand { get; set; }

        public Command RepeatOneCommand { get; set; }

        public Command FavouriteCommand { get; set; }


    }
}

