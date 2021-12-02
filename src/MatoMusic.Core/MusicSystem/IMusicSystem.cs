using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;

namespace MatoMusic.Core
{
    public interface IMusicSystem
    {
        event EventHandler<bool> OnPlayFinished;
        event EventHandler OnRebuildMusicInfosFinished;
        event EventHandler<double> OnProgressChanged;
        event EventHandler<bool> OnPlayStatusChanged;
        public IMusicInfoManager MusicInfoManager { get; set; }

        int[] ShuffleMap { get; }
        List<MusicInfo> MusicInfos { get; }
        int LastIndex { get; }
        double Duration { get; }
        double CurrentTime { get; }
        bool IsPlaying { get; }
        bool IsInitFinished { get; }
        Task RebuildMusicInfos();
        Task RebuildMusicInfos(Action callback);
        void SeekTo(double position);
        MusicInfo GetNextMusic(MusicInfo current, bool isShuffle);
        MusicInfo GetPreMusic(MusicInfo current, bool isShuffle);
        int GetMusicIndex(MusicInfo musicInfo);
        MusicInfo GetMusicByIndex(int index);
        void InitPlayer(MusicInfo musicInfo);
        void Play(MusicInfo currentMusic);
        void Stop();
        void PauseOrResume();
        void PauseOrResume(bool status);
        Task UpdateShuffleMap();
        void SetRepeatOneStatus(bool isRepeatOne);
    }
}
