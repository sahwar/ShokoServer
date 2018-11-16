﻿using System;
using Shoko.Commons.Queue;
using Shoko.Models.Queue;
using Shoko.Server.Providers.WebCache;

namespace Shoko.Server.CommandQueue.Commands.WebCache
{
    public class CmdWebCacheDeleteXRefAniDBTvDB : BaseCommand, ICommand
    {
        public int AnimeID { get; set; }
        public int AniDBStartEpisodeType { get; set; }
        public int AniDBStartEpisodeNumber { get; set; }
        public int TvDBID { get; set; }
        public int TvDBSeasonNumber { get; set; }
        public int TvDBStartEpisodeNumber { get; set; }

        public string ParallelTag { get; set; } = WorkTypes.WebCache;
        public int ParallelMax { get; set; } = 2;
        public int Priority { get; set; } = 10;
        public string WorkType => WorkTypes.WebCache;

        public string Id => $"WebCacheDeleteXRefAniDBTvDB_{AnimeID}";

        public QueueStateStruct PrettyDescription => new QueueStateStruct
        {
            QueueState = QueueStateEnum.WebCacheDeleteXRefAniDBTvDB,
            ExtraParams = new[] {AnimeID.ToString(), AniDBStartEpisodeType.ToString(), AniDBStartEpisodeNumber.ToString(), TvDBID.ToString(),TvDBSeasonNumber.ToString(), TvDBStartEpisodeNumber.ToString()}
        };



        public CmdWebCacheDeleteXRefAniDBTvDB(string str) : base(str)
        {
        }

        public CmdWebCacheDeleteXRefAniDBTvDB(int animeID, int aniDBStartEpisodeType,
            int aniDBStartEpisodeNumber,
            int tvDBID,
            int tvDBSeasonNumber, int tvDBStartEpisodeNumber)
        {
            AnimeID = animeID;
            AniDBStartEpisodeType = aniDBStartEpisodeType;
            AniDBStartEpisodeNumber = aniDBStartEpisodeNumber;
            TvDBID = tvDBID;
            TvDBSeasonNumber = tvDBSeasonNumber;
            TvDBStartEpisodeNumber = tvDBStartEpisodeNumber;
        }

        public override void Run(IProgress<ICommand> progress = null)
        {
            try
            {
                ReportInit(progress);
                WebCacheAPI.Delete_CrossRefAniDBTvDB(AnimeID, AniDBStartEpisodeType, AniDBStartEpisodeNumber, TvDBID,
                    TvDBSeasonNumber, TvDBStartEpisodeNumber);
                ReportFinish(progress);
            }
            catch (Exception ex)
            {
                ReportError(progress, $"Error processing WebCacheDeleteXRefAniDBTvDB: {AnimeID} - {TvDBID} - {ex}", ex);
            }
        }

    }
}