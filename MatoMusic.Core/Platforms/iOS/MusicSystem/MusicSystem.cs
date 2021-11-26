using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using Foundation;
using System.Threading;
using System.Threading.Tasks;
using MatoMusic.Infrastructure.Helper;
using Abp.Dependency;
using MatoMusic.Core.MusicSystem.Interfaces;


namespace MatoMusic.Core
{
    public partial class MusicSystem : IMusicSystem, ISingletonDependency
    {

        private readonly MusicInfoManager. _musicInfoManager;

        public event EventHandler<bool> OnPlayFinished;

        public event EventHandler OnRebuildMusicInfosFinished;

        public event EventHandler<double> OnProgressChanged;

        public event EventHandler<bool> OnPlayStatusChanged;

        private NSError nserror = new NSError();

        public MusicSystem(MusicInfoManager musicInfoManager)
        {

            _musicInfoManager = musicInfoManager;
        }


        private void OnFinishedPlaying(object sender, AVStatusEventArgs e)
        {
            //另开辟线程完成播放对象的Dispose
            new Thread(new ThreadStart(() =>
              {
                  OnPlayFinished?.Invoke(null, e.Status);
              })).Start();
        }


        private int[] shuffleMap;

        public int[] ShuffleMap
        {
            get
            {
                if (shuffleMap == null || shuffleMap.Length == 0)
                {
                    shuffleMap = CommonHelper.GetRandomArry(0, LastIndex);
                }
                return shuffleMap;
            }
        }
        private AVAudioPlayer CurrentPlayer;

        private List<MusicInfo> musicInfos;

        public List<MusicInfo> MusicInfos
        {
            get
            {
                if (musicInfos == null || musicInfos.Count == 0)
                {
                    RebuildMusicInfos();
                }
                return musicInfos;
            }
        }

        public async void RebuildMusicInfos()
        {
            var task01 = _musicInfoManager.GetQueueEntry();
            musicInfos = await task01;
            Task.WaitAll(task01);
            //this.UpdateShuffleMap();
            OnRebuildMusicInfosFinished?.Invoke(this, EventArgs.Empty);

        }

        public async void RebuildMusicInfos(Action callback)
        {
            var task01 = _musicInfoManager.GetQueueEntry();
            musicInfos = await task01;
            Task.WaitAll(task01);
            //this.UpdateShuffleMap();
            callback?.Invoke();
        }

        public int LastIndex { get { return MusicInfos.FindLastIndex(c => true); } }


        public double Duration
        {
            get
            {
                if (CurrentPlayer == null)
                {

                    return 1.0;
                }
                return CurrentPlayer.Duration;

            }
        }


        public double CurrentTime
        {
            get
            {
                if (CurrentPlayer == null)
                {
                    return default;
                }
                return CurrentPlayer.CurrentTime;
            }
        }


        public bool IsPlaying
        {
            get
            {
                if (CurrentPlayer == null)
                {
                    return default;
                }
                return CurrentPlayer.Playing;
            }
        }

        public bool IsInitFinished => CurrentPlayer != null;

        public void SeekTo(double position)

        {
            if (!IsInitFinished) { return; }
            CurrentPlayer.CurrentTime = position;
        }

        public MusicInfo GetNextMusic(MusicInfo current, bool isShuffle)
        {
            MusicInfo currentMusicInfo = null;
            if (current == null)
            {
                return null;
            }
            var index = GetMusicIndex(current);

            if (!isShuffle)
            {
                if (index + 1 > LastIndex)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
            }
            else
            {
                index = GetShuffleMusicIndex(index, 1);
            }

            if (MusicInfos.Count != 0 && index != -1)
            {
                currentMusicInfo = MusicInfos[index];
            }
            return currentMusicInfo;
        }

        public MusicInfo GetPreMusic(MusicInfo current, bool isShuffle)
        {
            MusicInfo currentMusicInfo = null;

            if (current == null)
            {
                return null;
            }
            var index = GetMusicIndex(current);
            if (!isShuffle)
            {
                if (index - 1 < 0)
                {
                    index = LastIndex;
                }
                else
                {
                    index--;
                }
            }
            else
            {
                index = GetShuffleMusicIndex(index, -1);
            }

            if (MusicInfos.Count != 0 && index != -1)
            {
                currentMusicInfo = MusicInfos[index];
            }

            return currentMusicInfo;
        }

        public int GetMusicIndex(MusicInfo musicInfo)
        {
            var current = MusicInfos;
            var result = current.IndexOf(current.FirstOrDefault(c => c.Id == musicInfo.Id));
            return result;
        }

        public MusicInfo GetMusicByIndex(int index)
        {
            var result = MusicInfos[index];
            return result;
        }

        public void InitPlayer(MusicInfo currentMusic)
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSessionCategory.Playback);
            AVAudioSession.SharedInstance().SetActive(true);
            if (CurrentPlayer != null)
            {
                Stop();
                CurrentPlayer.Dispose();
            }

            try
            {
                CurrentPlayer = new AVAudioPlayer(new NSUrl(currentMusic.Url), "", out nserror);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                CurrentPlayer = null;
                return;
            }
            //注册完成播放事件
            CurrentPlayer.FinishedPlaying -= new EventHandler<AVStatusEventArgs>(OnFinishedPlaying);
            CurrentPlayer.FinishedPlaying += new EventHandler<AVStatusEventArgs>(OnFinishedPlaying);


        }

        public void Play(MusicInfo currentMusic)
        {
            if (!IsInitFinished) { return; }
            CurrentPlayer?.Play();
            OnPlayStatusChanged?.Invoke(this, true);
        }

        public void Stop()
        {
            if (!IsInitFinished) { return; }

            if (CurrentPlayer.Playing)
            {
                CurrentPlayer.Stop();
                OnPlayStatusChanged?.Invoke(this, false);

            }
        }

        public void PauseOrResume()
        {
            if (!IsInitFinished) { return; }

            var status = CurrentPlayer.Playing;
            PauseOrResume(status);
        }

        public void PauseOrResume(bool status)
        {
            if (!IsInitFinished) { return; }

            if (status)
            {
                CurrentPlayer.Pause();
                OnPlayStatusChanged?.Invoke(this, false);
            }
            else
            {
                CurrentPlayer.Play();
                OnPlayStatusChanged?.Invoke(this, true);

            }

        }

        private int GetShuffleMusicIndex(int originItem, int increment)
        {
            var originItemIndex = 0;

            foreach (var item in ShuffleMap)
            {
                if (originItem == item)
                {
                    break;
                }
                originItemIndex++;
            }
            var newItemIndex = originItemIndex + increment;
            if (newItemIndex < 0)
            {
                newItemIndex = LastIndex;
            }
            if (newItemIndex > LastIndex)
            {
                newItemIndex = 0;
            }
            var shuffleMapCount = shuffleMap.Count();

            var musicInfosCount = MusicInfos.Count();

            if (shuffleMapCount != musicInfosCount)
            {
                shuffleMap = CommonHelper.GetRandomArry(0, LastIndex);
                shuffleMapCount = shuffleMap.Count();
            }

            if (shuffleMapCount > 0 && newItemIndex < shuffleMapCount)
            {
                var resultContent = ShuffleMap[newItemIndex];
                return resultContent;
            }
            else
            {
                return -1;
            }



        }

        public Task UpdateShuffleMap()
        {
            return Task.Run(() =>
            {
                shuffleMap = CommonHelper.GetRandomArry(0, LastIndex);
                return;
            });

        }

        public void SetRepeatOneStatus(bool isRepeatOne)
        {
            if (!IsInitFinished) { return; }
            CurrentPlayer.NumberOfLoops = isRepeatOne ? nint.MaxValue : 0;
        }
    }
}