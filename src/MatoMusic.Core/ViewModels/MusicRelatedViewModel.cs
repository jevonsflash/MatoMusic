using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Abp.Dependency;
using Abp.Configuration;
using MatoMusic.Core.Settings;
using MatoMusic.Core.Helper;
using MatoMusic.Core.Services;
using MatoMusic.Core.Interfaces;

namespace MatoMusic.Core.ViewModel
{
    public abstract class MusicRelatedViewModel : ViewModelBase
    {
        public IMusicInfoManager musicInfoManager { get; }

        private readonly IMusicControlService musicSystem;
        private readonly MusicRelatedService ms;

        private bool _isFastSeeking = false;



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

        public MusicRelatedViewModel(
            IMusicInfoManager musicInfoManager, MusicRelatedService musicRelatedService,IMusicControlService musicControlService)
        {

            this.PlayCommand = new Command(PlayAction, CanPlayExcute);
            this.PreCommand = new Command(PreAction, CanPlayExcute);
            this.NextCommand = new Command(NextAction, CanPlayExcute);
            this.RepeatOneCommand = new Command(RepeatOneAction, CanPlayExcute);
            this.ShuffleCommand = new Command(ShuffleAction, CanPlayExcute);
            this.FavouriteCommand = new Command(FavouriteAction, CanPlayExcute);
            this.PropertyChanged += DetailPageViewModel_PropertyChanged;
            musicSystem = musicControlService;
            musicSystem.MusicInfoManager = musicInfoManager;
            musicSystem.OnPlayFinished += MusicControlService_OnMusicChanged;
            this.musicInfoManager = musicInfoManager;
            this.ms = musicRelatedService;
            this.ms.PropertyChanged += this.Delegate_PropertyChanged;
        }

        private void Delegate_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //this.DetailPageViewModel_PropertyChanged(this, e);
            this.RaisePropertyChanged(e.PropertyName);
        }

        public MusicInfo CurrentMusic
        {
            get => ms.CurrentMusic;
            set => ms.CurrentMusic = value;
        }

        public MusicInfo NextMusic
        {
            get => ms.NextMusic;
            set => ms.NextMusic = value;
        }

        public MusicInfo PreviewMusic
        {
            get => ms.PreviewMusic;
            set => ms.PreviewMusic = value;
        }

        public List<MusicInfo> Musics
        {
            get => ms.Musics;
            set => ms.Musics = value;
        }

        public bool Canplay
        {
            get => ms.Canplay;
        }

        public bool IsPlaying
        {
            get => ms.IsPlaying;
            set => ms.IsPlaying = value;
        }

        public bool IsShuffle
        {
            get => ms.IsShuffle;
            set => ms.IsShuffle = value;
        }

        public bool IsRepeatOne
        {
            get => ms.IsRepeatOne;
            set => ms.IsRepeatOne = value;
        }

        public double CurrentTime
        {
            get => ms.CurrentTime;
            set => ms.CurrentTime = value;
        }

        public double Duration
        {
            get => ms.Duration;
            set => ms.Duration = value;
        }

        public async Task RebuildMusicInfos()
        {
            await this.musicSystem.RebuildMusicInfos();
        }

        public async Task RebuildMusicInfos(Action callback)
        {
            await RebuildMusicInfos();
            callback?.Invoke();
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
            var result = ms.Canplay;
            return result;
        }

        public bool CanPlayAllExcute(object obj)
        {
            var result = ms.CanplayAll;
            return result;
        }

        #region methods
        private void MusicControlService_OnMusicChanged(Object sender, bool e)
        {
            NextAction(null);
        }


        private async void DetailPageViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Properties.CurrentMusic)
            {
                if (!Canplay || ms.IsInited == false)
                {
                    return;

                }
                await musicSystem.InitPlayer(CurrentMusic);
                musicSystem.Play(CurrentMusic);
                ms.DoUpdate();
                ms.InitPreviewAndNextMusic();
                OnMusicChanged?.Invoke(this, EventArgs.Empty);
                this.Duration = ms.GetPlatformSpecificTime(musicSystem.Duration());
                this.SettingManager.ChangeSettingForApplication(CommonSettingNames.BreakPointMusicIndex, Musics.IndexOf(CurrentMusic).ToString());
                RaiseCanPlayExecuteChanged();
            }

            else if (e.PropertyName == Properties.IsShuffle)
            {
                this.SettingManager.ChangeSettingForApplication(CommonSettingNames.IsShuffle, this.IsShuffle.ToString());
                if (IsShuffle)
                {
                    await musicSystem.UpdateShuffleMap();
                    ms.InitPreviewAndNextMusic();
                }
                else
                {
                    ms.InitPreviewAndNextMusic();
                }
            }

            else if (e.PropertyName == Properties.IsRepeatOne)
            {
                this.SettingManager.ChangeSettingForApplication(CommonSettingNames.IsRepeatOne, this.IsRepeatOne.ToString());
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
            if (Math.Abs(progress - ms.GetPlatformSpecificTime(musicSystem.CurrentTime())) > 2.0)
            {
                musicSystem.SeekTo(progress);
            }
        }

        public async Task StartFastSeeking(int increment)
        {
            var seekingDuration = 0;
            _isFastSeeking = true;
            await Task.Run((Action)(() =>
            {

                while (_isFastSeeking)
                {
                    var currentTime = this.musicSystem.CurrentTime();

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
        public virtual void ChangeMusic(MusicInfo musicInfo)
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
    
       
        #endregion

        public Command PlayCommand { get; set; }

        public Command PreCommand { get; set; }

        public Command NextCommand { get; set; }

        public Command ShuffleCommand { get; set; }

        public Command RepeatOneCommand { get; set; }

        public Command FavouriteCommand { get; set; }
    }
}

