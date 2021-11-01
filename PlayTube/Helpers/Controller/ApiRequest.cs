using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Widget;
using Bumptech.Glide;
using Java.IO;
using Java.Lang;
using PlayTube.Activities.Chat.Service;
using PlayTube.Activities.Default;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.OneSignal;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Playlist;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Exception = System.Exception;
using Thread = System.Threading.Thread;

namespace PlayTube.Helpers.Controller
{
    internal static class ApiRequest
    {
        //Get Settings Api
        public static async Task GetSettings_Api(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                if (UserDetails.IsLogin)
                    await SetLangUserAsync();

                //site_settings
                var (apiStatus, respond) = await Current.GetSettingsAsync();
                if (apiStatus == 200)
                {
                    if (respond is GetSettingsObject result)
                    {
                        var settings = result.Data.SiteSettings;

                        ListUtils.MySettingsList = settings;

                        AppSettings.OneSignalAppId = settings.PushId;
                        //AppSettings.ShowButtonImport = string.IsNullOrWhiteSpace(settings.ImportSystem) ? AppSettings.ShowButtonImport : settings.ImportSystem == "on";
                        //AppSettings.ShowButtonUpload = string.IsNullOrWhiteSpace(settings.UploadSystem) ? AppSettings.ShowButtonUpload : settings.UploadSystem == "on";

                        OneSignalNotification.RegisterNotificationDevice();

                        if (settings.Categories?.Count > 0)
                        {
                            //Categories >> New V.1.6
                            var listCategories = settings.Categories.Select(cat => new Classes.Category
                            {
                                Id = cat.Key,
                                Name = Methods.FunString.DecodeString(cat.Value),
                                Color = "#212121",
                                Image = CategoriesController.GetImageCategory(cat.Value),
                                SubList = new List<Classes.Category>()
                            }).ToList();

                            CategoriesController.ListCategories.Clear();
                            CategoriesController.ListCategories = new ObservableCollection<Classes.Category>(listCategories);
                        }
                        else
                        {
                            //Categories >> Old V.1.5
                            var listCategories = result.Data.Categories.Select(cat => new Classes.Category
                            {
                                Id = cat.Key,
                                Name = Methods.FunString.DecodeString(cat.Value),
                                Color = "#212121",
                                Image = CategoriesController.GetImageCategory(cat.Value),
                                SubList = new List<Classes.Category>()
                            }).ToList();

                            CategoriesController.ListCategories.Clear();
                            CategoriesController.ListCategories = new ObservableCollection<Classes.Category>(listCategories);
                        }

                        if (settings.SubCategories?.SubCategoriessList?.Count > 0)
                        {
                            //Sub Categories
                            foreach (var sub in settings.SubCategories?.SubCategoriessList)
                            {
                                var subCategories = ListUtils.MySettingsList?.SubCategories?.SubCategoriessList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                if (subCategories?.Count > 0)
                                {
                                    var cat = CategoriesController.ListCategories.FirstOrDefault(a => a.Id == sub.Key);
                                    if (cat != null)
                                    {
                                        foreach (var pairs in subCategories.SelectMany(pairs => pairs))
                                        {
                                            cat.SubList.Add(new Classes.Category
                                            {
                                                Id = pairs.Key,
                                                Name = Methods.FunString.DecodeString(pairs.Value),
                                                Color = "#212121",
                                            });
                                        }
                                    }
                                }
                            }
                        }

                        if (settings.MoviesCategories?.Count > 0)
                        {
                            //Movies Categories
                            var listMovies = settings.MoviesCategories.Select(cat => new Classes.Category
                            {
                                Id = cat.Key,
                                Name = Methods.FunString.DecodeString(cat.Value),
                                Color = "#212121",
                                SubList = new List<Classes.Category>()
                            }).ToList();

                            CategoriesController.ListCategoriesMovies.Clear();
                            CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Category>(listMovies);
                        }

                        //Insert MySettings in Database
                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.InsertOrUpdate_Settings(settings);
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
        }

        private static async Task SetLangUserAsync()
        {
            try
            {
                string lang;
                if (AppSettings.Lang.Contains("en"))
                    lang = "english";
                else if (AppSettings.Lang.Contains("ar"))
                    lang = "arabic";
                else if (AppSettings.Lang.Contains("de"))
                    lang = "german";
                else if (AppSettings.Lang.Contains("el"))
                    lang = "greek";
                else if (AppSettings.Lang.Contains("es"))
                    lang = "spanish";
                else if (AppSettings.Lang.Contains("fr"))
                    lang = "french";
                else if (AppSettings.Lang.Contains("it"))
                    lang = "italian";
                else if (AppSettings.Lang.Contains("ja"))
                    lang = "japanese";
                else if (AppSettings.Lang.Contains("nl"))
                    lang = "dutch";
                else if (AppSettings.Lang.Contains("pt"))
                    lang = "portuguese";
                else if (AppSettings.Lang.Contains("ro"))
                    lang = "romanian";
                else if (AppSettings.Lang.Contains("ru"))
                    lang = "russian";
                else if (AppSettings.Lang.Contains("sq"))
                    lang = "albanian";
                else if (AppSettings.Lang.Contains("sr"))
                    lang = "serbian";
                else if (AppSettings.Lang.Contains("tr"))
                    lang = "turkish";
                else
                    lang = string.IsNullOrEmpty(UserDetails.LangName) ? UserDetails.LangName : "english";

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateLangAsync(lang) });
                else
                    Toast.MakeText(Application.Context, Application.Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();

                await Task.Delay(0);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async Task<UserDataObject> GetChannelData(Activity activity, string userId)
        {
            if (!Methods.CheckConnectivity()) return null!;
            var (apiStatus, respond) = await RequestsAsync.Global.GetChannelInfoAsync(userId);
            if (apiStatus == 200)
            {
                if (respond is GetChannelInfoObject result)
                {
                    if (userId == UserDetails.UserId)
                    {
                        UserDetails.Avatar = result.DataResult.Avatar;
                        UserDetails.Cover = result.DataResult.Cover;
                        UserDetails.Username = result.DataResult.Username;
                        UserDetails.FullName = AppTools.GetNameFinal(result.DataResult);

                        ListUtils.MyChannelList?.Clear();
                        ListUtils.MyChannelList?.Add(result.DataResult);

                        try
                        {
                            var profileImage = TabbedMainActivity.GetInstance()?.MainVideoFragment?.ProfileButton;
                            if (profileImage != null)
                                GlideImageLoader.LoadImage(activity, UserDetails.Avatar, profileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }


                        var sqlEntity = new SqLiteDatabase();
                        sqlEntity.InsertOrUpdate_DataMyChannel(result.DataResult);

                        return result.DataResult;
                    }

                    return result.DataResult;
                }
            }
            else Methods.DisplayReportResult(activity, respond);
            return null!;
        }

        //Get PlayLists Videos in API
        public static async Task PlayListsVideosApi(Activity activity, string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Playlist.GetMyPlaylistAsync(UserDetails.UserId, offset, "10");
                if (apiStatus == 200)
                {
                    if (respond is GetPlaylistObject result)
                    {
                        if (result.AllPlaylist.Count > 0)
                        {
                            ListUtils.PlayListVideoObjectList = new ObservableCollection<PlayListVideoObject>(result.AllPlaylist);
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
        }

        //Get WatchLater Videos in API
        public static async Task WatchLaterVideosApi(Activity activity)
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Video.WatchLaterVideosAsync("25");
                if (apiStatus == 200)
                {
                    if (respond is WatchLaterVideosListObject result)
                    {
                        if (result.Data.Count > 0)
                        {
                            ListUtils.WatchLaterVideosList = new ObservableCollection<DataWatchLaterVideos>(result.Data);
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
        }

        //Get Ads Videos in API
        public static async Task AdsVideosApi(Activity activity, string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Video.GetAdsAsync("10", offset);
                if (apiStatus == 200)
                {
                    if (respond is GetAdsVideosListObject result)
                    {
                        if (result.VideoList.Count > 0)
                        {
                            ListUtils.AdsVideoList = new ObservableCollection<VideoAdDataObject>(result.VideoList);
                        }
                    }
                }
                else Methods.DisplayReportResult(activity, respond);
            }
        }

        //======================================== 
        private static bool RunLogout;

        public static async Task Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context?.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_FolderUser();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DropAll();

                        Runtime.GetRuntime()?.RunFinalization();
                        Runtime.GetRuntime()?.Gc();
                        TrimCache(context);

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.Connect();

                        context.StopService(new Intent(context, typeof(ChatApiService)));

                        MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                        MainSettings.AutoNext?.Edit()?.Clear()?.Commit();
                        MainSettings.InAppReview?.Edit()?.Clear()?.Commit();

                        Intent intent = new Intent(context, typeof(LoginActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish(); 
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    context?.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_FolderUser();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.DropAll();

                        Runtime.GetRuntime()?.RunFinalization();
                        Runtime.GetRuntime()?.Gc();
                        TrimCache(context);

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.Connect();

                        context.StopService(new Intent(context, typeof(ChatApiService)));

                        MainSettings.SharedData?.Edit()?.Clear()?.Commit();
                        MainSettings.AutoNext?.Edit()?.Clear()?.Commit();
                        MainSettings.InAppReview?.Edit()?.Clear()?.Commit();

                        Intent intent = new Intent(context, typeof(LoginActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                File dir = context.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context?.DeleteDatabase(AppSettings.DatabaseName + "_.db");
                context?.DeleteDatabase(SqLiteDatabase.PathCombine);

                if (context?.IsDestroyed != false)
                {
                    return;
                }

                Glide.Get(context)?.ClearMemory();

                new Thread(() =>
                {
                    try
                    {
                        Glide.Get(context)?.ClearDiskCache();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir != null && dir.IsDirectory)
                {
                    string[] children = dir.List();
                    if (children != null)
                    {
                        if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                        {
                            return false;
                        } 
                    }

                    // The directory is now empty so delete it
                    return dir.Delete();
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                switch (type)
                {
                    case "Logout":
                    {
                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RequestsAsync.Auth.UserLogoutAsync });
                        }

                        break;
                    }
                    case "Delete":
                    {
                        Methods.Path.DeleteAll_FolderUser();

                        if (Methods.CheckConnectivity())
                        {
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Auth.DeleteUserAsync(UserDetails.UserId, UserDetails.Password) });
                        }

                        break;
                    }
                }

                await Task.Delay(500);

                if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleSignInClient != null)
                    if (Auth.GoogleSignInApi != null)
                    {
                        LoginActivity.MGoogleSignInClient.SignOut();
                        LoginActivity.MGoogleSignInClient = null!;
                    }

                if (AppSettings.ShowFacebookLogin)
                {
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }
                }

                UserDetails.IsLogin = false;

                OneSignalNotification.UnRegisterNotificationDevice();

                UserDetails.ClearAllValueUserDetails();

                GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}