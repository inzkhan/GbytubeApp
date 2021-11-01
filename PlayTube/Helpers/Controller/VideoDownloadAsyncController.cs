//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Widget;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using Uri = Android.Net.Uri;

namespace PlayTube.Helpers.Controller
{ 
    public class VideoDownloadAsyncController
    {
        private readonly DownloadManager Downloadmanager;
        private readonly DownloadManager.Request Request; 
        private readonly string FilePath = Methods.Path.FolderDcimVideo;
        private readonly string Filename;
        private long DownloadId;
        private string FromActivity;
        private  VideoObject Video;
        private readonly Activity ActivityContext;
         
        public VideoDownloadAsyncController(string url, string filename, Activity contextActivity , string fromActivity)
        {
            try
            {
                switch (fromActivity)
                {
                    case "Main":
                        ActivityContext = TabbedMainActivity.GetInstance();
                        break;
                    case "GlobalPlayer":
                        ActivityContext = GlobalPlayerActivity.GetInstance();
                        break;
                    default:
                        ActivityContext = contextActivity;
                        break;
                }
               
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);

                if (!filename.Contains(".mp4") || !filename.Contains(".Mp4") || !filename.Contains(".MP4"))
                    Filename = filename + ".mp4";
               
                Downloadmanager = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
                Request = new DownloadManager.Request(Uri.Parse(url));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StartDownloadManager(string title, VideoObject video, string fromActivity)
        {
            try
            {
                if (video != null && !string.IsNullOrEmpty(title))
                {
                    Video = video;
                    FromActivity = fromActivity;

                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Insert_WatchOfflineVideos(Video);

                    Request.SetTitle(title);
                    Request.SetAllowedNetworkTypes(DownloadNetwork.Mobile | DownloadNetwork.Wifi);
                    Request.SetDestinationInExternalPublicDir("/" + AppSettings.ApplicationName + "/Video/", Filename);
                    Request.SetNotificationVisibility(DownloadVisibility.Visible);
                    Request.SetAllowedOverRoaming(true);
                    DownloadId = Downloadmanager.Enqueue(Request);
                     
                    var onDownloadComplete = new OnDownloadComplete
                    {
                        ActivityContext = ActivityContext, TypeActivity = fromActivity, Video = Video
                    };

                    ActivityContext.ApplicationContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));  
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Download_faileds), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopDownloadManager()
        {
            try
            {
                Downloadmanager.Remove(DownloadId);
                RemoveDiskVideoFile(Filename);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public static bool RemoveDiskVideoFile(string filename)
        {
            try
            {
                string path = Methods.Path.FolderDcimVideo + "/" + filename;
                if (File.Exists(path))
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.Remove_WatchOfflineVideos(filename.Replace(".mp4", ""));
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public bool CheckDownloadLinkIfExits()
        {
            try
            {
                if (File.Exists(FilePath + Filename))
                    return true;

                return false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return false;
            }
        }

        public static string GetDownloadedDiskVideoUri(string url)
        {
            try
            {
                string filename = url.Split('/').Last();

                var fullpaths = "file://" + Uri.Parse(Methods.Path.FolderDcimVideo + "/" + filename + ".mp4");
                if (File.Exists(fullpaths))
                    return fullpaths;

                var fullpaths2 = Methods.Path.FolderDcimVideo + "/" + filename + ".mp4";
                if (File.Exists(fullpaths2))
                    return fullpaths2; 
                
                return "";
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return "";
            }
        }

        [BroadcastReceiver]
        [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
        public class OnDownloadComplete : BroadcastReceiver
        {
            public Context ActivityContext;
            public string TypeActivity;
            public VideoObject Video;
             
            public override void OnReceive(Context context, Intent intent)
            {
                try
                { 
                    if (intent.Action == DownloadManager.ActionDownloadComplete )
                    {
                        if (ActivityContext == null)
                            return;

                        DownloadManager downloadManagerExcuter = (DownloadManager)ActivityContext.GetSystemService(Context.DownloadService);
                        long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                        DownloadManager.Query query = new DownloadManager.Query();
                        query.SetFilterById(downloadId);
                        ICursor c = downloadManagerExcuter.InvokeQuery(query);
                        var sqlEntity = new SqLiteDatabase();

                        if (c.MoveToFirst())
                        {
                            int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
                            if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
                            {
                                string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

                                ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
                                ActivityManager.GetMyMemoryState(appProcessInfo);
                                if (appProcessInfo.Importance == Importance.Foreground ||  appProcessInfo.Importance == Importance.Background)
                                {
                                    sqlEntity.Update_WatchOfflineVideos(Video.VideoId, downloadedPath);
                                    switch (TypeActivity)
                                    {
                                        case "Main":
                                            TabbedMainActivity.GetInstance()?.VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                                            TabbedMainActivity.GetInstance().VideoActionsController.DownloadIcon.Tag = "Downloaded";
                                            TabbedMainActivity.GetInstance()?.LibrarySynchronizer.AddToWatchOffline(Video);
                                            break;
                                        case "GlobalPlayer":
                                            GlobalPlayerActivity.GetInstance()?.VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                                            GlobalPlayerActivity.GetInstance().VideoActionsController.DownloadIcon.Tag = "Downloaded";
                                            GlobalPlayerActivity.GetInstance()?.LibrarySynchronizer.AddToWatchOffline(Video);
                                            break;
                                        default:
                                        {
                                            if (ActivityContext is FullScreenVideoActivity fullScreen)
                                            {
                                                fullScreen.VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                                                fullScreen.VideoActionsController.DownloadIcon.Tag = "Downloaded";
                                                //fullScreen tabbedMain.LibrarySynchronizer.OfflineVideoList.Add(Video);
                                            }

                                            break;
                                        }
                                    }
                                }
                                else
                                { 
                                    sqlEntity.Update_WatchOfflineVideos(Video.VideoId, downloadedPath); 
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }
    }
}