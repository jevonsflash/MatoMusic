using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatoMusic.Core;
using MatoMusic.Infrastructure.Helper;
using Microsoft.Maui.Controls;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace MatoMusic.Core
{
    public class MusicSystem : IMusicSystem
    {
        public event EventHandler<bool> OnPlayFinished;

        public event EventHandler OnRebuildMusicInfosFinished;
        public event EventHandler<double> OnProgressChanged;
        public event EventHandler<bool> OnPlayStatusChanged;

        private IMusicInfoManager MusicInfoManager => DependencyService.Get<IMusicInfoManager>();

        public MusicSystem()
        {

            InitializeAudioListener();
        }

        private void InitializeAudioListener()
        {

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

        private MediaPlayer _currentPlayer;

        private MediaPlayer CurrentPlayer
        {

            set { _currentPlayer = value; }
            get
            {
                if (_currentPlayer == null)
                {

                    _currentPlayer = new MediaPlayer();



                }
                return _currentPlayer;
            }
        }

        private void Cl_OnComplete(object sender, MediaPlayer e)
        {
            OnPlayFinished?.Invoke(null, true);
        }

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
            musicInfos = await MusicInfoManager.GetQueueEntry();
            OnRebuildMusicInfosFinished?.Invoke(this, EventArgs.Empty);
        }

        public async void RebuildMusicInfos(Action callback)
        {
            musicInfos = await MusicInfoManager.GetQueueEntry();
            callback?.Invoke();
        }
        public int LastIndex { get { return MusicInfos.FindLastIndex(c => true); } }


        public double Duration { get { return CurrentPlayer.PlaybackSession.NaturalDuration.TotalSeconds; } }


        public double CurrentTime { get { return CurrentPlayer.PlaybackSession.Position.TotalSeconds; } }


        public bool IsPlaying { get { return GetIsPlaying(CurrentPlayer.PlaybackSession.PlaybackState); } }


        private bool GetIsPlaying(MediaPlaybackState status)
        {
            var result = false;
            switch (status)
            {
                case MediaPlaybackState.None:
                case MediaPlaybackState.Opening:
                case MediaPlaybackState.Buffering:
                case MediaPlaybackState.Paused:
                    result = false;
                    break;
                case MediaPlaybackState.Playing:
                    result = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
            return result;
        }

        public bool IsInitFinished { get { return true; } }


        public void SeekTo(double position)

        {
            CurrentPlayer.Position = new TimeSpan(0, 0, 0, (int)position);

        }

        public MusicInfo GetNextMusic(MusicInfo current, bool isShuffle)
        {
            MusicInfo currentMusicInfo = null;
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

            if (MusicInfos.Count != 0)
            {
                currentMusicInfo = MusicInfos[index];
            }
            return currentMusicInfo;
        }

        public MusicInfo GetPreMusic(MusicInfo current, bool isShuffle)
        {
            MusicInfo currentMusicInfo = null;
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

            if (MusicInfos.Count != 0)
            {
                currentMusicInfo = MusicInfos[index];
            }

            return currentMusicInfo;
        }

        public int GetMusicIndex(MusicInfo musicInfo)
        {
            var result = MusicInfos.IndexOf(MusicInfos.FirstOrDefault(c => c.Id == musicInfo.Id));
            return result;
        }

        public MusicInfo GetMusicByIndex(int index)
        {
            var result = MusicInfos[index];
            return result;
        }

        public async void InitPlayer(MusicInfo musicInfo)
        {
            CurrentPlayer.CurrentStateChanged -= CurrentPlayer_CurrentStateChanged;

            CurrentPlayer.Dispose();
            CurrentPlayer = new MediaPlayer();

            var results = StorageFile.GetFileFromPathAsync(musicInfo.Url);

            StorageFile file = await results;
            while (results.Status != Windows.Foundation.AsyncStatus.Completed)
            {

            }
            CurrentPlayer.Source =
                MediaSource.CreateFromStream(await file.OpenAsync(FileAccessMode.Read), file.ContentType);
            CurrentPlayer.CurrentStateChanged += CurrentPlayer_CurrentStateChanged; ;


        }

        private void CurrentPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {

        }

        public void Play(MusicInfo currentMusic)
        {
            if (currentMusic != null)
            {
                CurrentPlayer?.Play();
            }
        }

        public void Stop()
        {
            if (GetIsPlaying(CurrentPlayer.PlaybackSession.PlaybackState))
            {
                CurrentPlayer.Dispose();
            }
        }

        public void PauseOrResume()
        {

            var status = GetIsPlaying(CurrentPlayer.PlaybackSession.PlaybackState);
            PauseOrResume(status);
        }

        public void PauseOrResume(bool status)
        {

            if (status)
            {
                CurrentPlayer.Pause();

            }
            else
            {
                CurrentPlayer.Play();
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
            try
            {
                var resultContent = ShuffleMap[newItemIndex];
                return resultContent;

            }
            catch (Exception e)
            {
                return ShuffleMap[0];
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
            if (CurrentPlayer != null)
            {
                CurrentPlayer.IsLoopingEnabled = isRepeatOne;
            }
        }

    }
}
