using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Java.Lang;
using Newtonsoft.Json;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Messages;
using PlayTubeClient.RestCalls;
using Exception = System.Exception;

namespace PlayTube.Activities.Chat.Service
{
    [Service(Exported = false)]
    public class ChatApiService : JobIntentService 
    {
        public override IBinder OnBind(Intent intent)
        {
            return null!;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();
                //Toast.MakeText(Application.Context, "OnCreate", ToastLength.Short)?.Show();
                new Handler(Looper.MainLooper).PostDelayed(new ApiPostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnHandleWork(Intent p0)
        {
            try
            {
                //Toast.MakeText(Application.Context, "OnHandleWork", ToastLength.Short)?.Show();
                new Handler(Looper.MainLooper).PostDelayed(new ApiPostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                base.OnStartCommand(intent, flags, startId);

                new Handler(Looper.MainLooper).PostDelayed(new ApiPostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);

                //Toast.MakeText(Application.Context, "OnStartCommand", ToastLength.Short)?.Show();

                return StartCommandResult.Sticky;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return StartCommandResult.NotSticky;
            }
        }
    }

    public class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private static Handler MainHandler;

        public ApiPostUpdaterHelper(Handler mainHandler)
        {
            MainHandler = mainHandler;
        }

        public async void Run()
        {
            try
            {
                if (string.IsNullOrEmpty(Methods.AppLifecycleObserver.AppState))
                    Methods.AppLifecycleObserver.AppState = "Background";

                //Toast.MakeText(Application.Context, "Started", ToastLength.Short)?.Show(); 

                if (Methods.AppLifecycleObserver.AppState == "Background" && string.IsNullOrEmpty(Current.AccessToken))
                {
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    var login = dbDatabase.Get_data_Login();
                    Console.WriteLine(login);
                }

                if (string.IsNullOrEmpty(Current.AccessToken) || !UserDetails.IsLogin)
                    return;

                var (apiStatus, respond) = await RequestsAsync.Messages.GetChats("15", "0");
                if (apiStatus != 200 || respond is not GetChatsObject result || result.data == null)
                {
                    // Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    //Toast.MakeText(Application.Context, "ResultSender 1 \n" + data, ToastLength.Short)?.Show();
                    result.data.RemoveAll(a => a.Id == null || a.User == null);

                    if (result.data.Count > 0)
                    {
                        if (Methods.AppLifecycleObserver.AppState == "Foreground")
                        {
                            TabbedMainActivity.GetInstance()?.OnReceiveResult(JsonConvert.SerializeObject(result));
                        }
                        else
                        {
                            ListUtils.ChatList = new ObservableCollection<GetChatsObject.Data>(result.data);
                            //Insert All data users to database
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                        } 
                    }
                }

                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ApiPostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds); 
            }
            catch (Exception e)
            {
                //Toast.MakeText(Application.Context, "ResultSender failed", ToastLength.Short)?.Show();
                MainHandler ??= new Handler(Looper.MainLooper);
                MainHandler?.PostDelayed(new ApiPostUpdaterHelper(new Handler(Looper.MainLooper)), AppSettings.RefreshChatActivitiesSeconds);
                Methods.DisplayReportResultTrack(e); 
            }
        }
    }
} 