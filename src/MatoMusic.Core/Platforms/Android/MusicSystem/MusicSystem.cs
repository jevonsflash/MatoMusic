using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using MatoMusic.Infrastructure.Helper;

namespace MatoMusic.Core
{
    public class CompleteListener : Service, MediaPlayer.IOnCompletionListener
    {
        public event EventHandler<MediaPlayer> OnComplete;

        public void OnCompletion(MediaPlayer mp)
        {
            OnComplete?.Invoke(this, mp);

        }

        public override IBinder OnBind(Intent intent)
        {
            return null;

        }
    }
    public partial class MusicSystem : IMusicSystem
    {
        public IMusicInfoManager MusicInfoManager { get; set; }

        public event EventHandler<bool> OnPlayFinished;

        public event EventHandler OnRebuildMusicInfosFinished;

        public event EventHandler<double> OnProgressChanged;

        public event EventHandler<bool> OnPlayStatusChanged;

        public MusicSystem()
        {

        }
        public MusicSystem(IMusicInfoManager musicInfoManager)
        {

            MusicInfoManager = musicInfoManager;
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
                    var cl = new CompleteListener();
                    _currentPlayer.SetOnCompletionListener(cl);
                    cl.OnComplete += Cl_OnComplete;
                }
                return _currentPlayer;
            }
        }

        private void Cl_OnComplete(object sender, MediaPlayer e)
        {
            if (!CurrentPlayer.Looping && e.Duration > 0.0)
            {
                OnPlayFinished?.Invoke(null, true);

            }
        }

        private List<MusicInfo> musicInfos;

        public List<MusicInfo> MusicInfos
        {
            get
            {
                if (musicInfos == null || musicInfos.Count == 0)
                {
                    musicInfos = new List<MusicInfo>();
                }
                return musicInfos;
            }
        }


        public async Task RebuildMusicInfos()
        {
            musicInfos = await MusicInfoManager.GetQueueEntry();
            OnRebuildMusicInfosFinished?.Invoke(this, EventArgs.Empty);

        }

        public async Task RebuildMusicInfos(Action callback)
        {
            await RebuildMusicInfos();
            callback?.Invoke();
        }

        public int LastIndex { get { return MusicInfos.FindLastIndex(c => true); } }


        public double Duration { get { return CurrentPlayer.Duration; } }


        public double CurrentTime { get { return CurrentPlayer.CurrentPosition; } }


        public bool IsPlaying { get { return CurrentPlayer.IsPlaying; } }


        public bool IsInitFinished { get { return true; } }


        public void SeekTo(double position)

        {
            CurrentPlayer.SeekTo((int)position * 1000);

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
            var result = MusicInfos.IndexOf(MusicInfos.FirstOrDefault(c => c.Id == musicInfo.Id));
            return result;
        }

        public MusicInfo GetMusicByIndex(int index)
        {
            var result = MusicInfos[index];
            return result;
        }

        public void InitPlayer(MusicInfo musicInfo)
        {
            CurrentPlayer.Reset();
            CurrentPlayer.SetDataSource(musicInfo.Url);

            CurrentPlayer.Prepare();
        }

        public void Play(MusicInfo currentMusic)
        {
            if (currentMusic != null)
            {
                CurrentPlayer?.Start();
            }
        }

        public void Stop()
        {
            if (CurrentPlayer.IsPlaying)
            {
                CurrentPlayer.SeekTo(0);
                CurrentPlayer.Pause();

            }
        }

        public void PauseOrResume()
        {

            var status = CurrentPlayer.IsPlaying;
            PauseOrResume(status);
        }

        public void PauseOrResume(bool status)
        {

            if (status)
            {
                CurrentPlayer.Pause();
                OnPlayStatusChanged?.Invoke(this, false);
            }
            else
            {
                CurrentPlayer.Start();
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
            if (CurrentPlayer != null)
            {
                CurrentPlayer.Looping = isRepeatOne;
            }
        }
    }
}
