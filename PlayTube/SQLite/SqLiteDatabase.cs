//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using PlayTube.Activities.Chat;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.OneSignal;
using PlayTubeClient;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Messages;
using SQLite;

namespace PlayTube.SQLite
{
    public class SqLiteDatabase
    {
        //############# DON'T MODIFY HERE #############

        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = System.IO.Path.Combine(Folder, AppSettings.DatabaseName + "_.db");
         
        //############# CONNECTION #############

        #region DataBase Functions

        private SQLiteConnection OpenConnection()
        {
            try
            {
               var connection = new SQLiteConnection(PathCombine, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex);
                return connection;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    OpenConnection();
                else
                    Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public void Connect()
        {
            try
            {
                using var connection = OpenConnection();
                connection?.CreateTable<DataTables.MySettingsTb>();
                connection?.CreateTable<DataTables.LoginTb>();
                connection?.CreateTable<DataTables.ChannelTb>();
                connection?.CreateTable<DataTables.WatchOfflineVideosTb>();
                connection?.CreateTable<DataTables.SubscriptionsChannelTb>();
                connection?.CreateTable<DataTables.LibraryItemTb>();
                connection?.CreateTable<DataTables.SharedVideosTb>();
                connection?.CreateTable<DataTables.NotInterestedTb>();
                connection?.CreateTable<DataTables.LastChatTb>();
                connection?.CreateTable<DataTables.MessageTb>();

                //Connection?.Dispose();
                //Connection?.Close();
            }
            catch (SQLiteException e)
            {
                if (e.Message.Contains("database is locked"))
                    Connect();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
          
        //Delete table 
        public void DropAll()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DropTable<DataTables.MySettingsTb>();
                connection.DropTable<DataTables.LoginTb>();
                connection.DropTable<DataTables.ChannelTb>();
                connection.DropTable<DataTables.WatchOfflineVideosTb>();
                connection.DropTable<DataTables.SubscriptionsChannelTb>();
                connection.DropTable<DataTables.LibraryItemTb>();
                connection.DropTable<DataTables.SharedVideosTb>();
                connection.DropTable<DataTables.NotInterestedTb>();
                connection.DropTable<DataTables.LastChatTb>();
                connection.DropTable<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //############# CONNECTION #############

        #region Settings

        //Insert data Settings
        public void InsertOrUpdate_Settings(GetSettingsObject.SiteSettings data)
        {
            try
            {
                using var connection = OpenConnection();
                var resultChannelTb = connection.Table<DataTables.MySettingsTb>().FirstOrDefault();
                if (resultChannelTb == null)
                {
                    resultChannelTb = ClassMapper.Mapper?.Map<DataTables.MySettingsTb>(data);
                    if (resultChannelTb != null)
                    {
                        resultChannelTb.CurrencyArray = JsonConvert.SerializeObject(data.CurrencyArray);
                        resultChannelTb.CurrencySymbolArray = JsonConvert.SerializeObject(data.CurrencySymbolArray);
                        resultChannelTb.Continents = JsonConvert.SerializeObject(data.Continents);
                        resultChannelTb.Categories = JsonConvert.SerializeObject(data.Categories);
                        if (data.SubCategories != null)
                            resultChannelTb.SubCategories = JsonConvert.SerializeObject(data.SubCategories?.SubCategoriessList);
                        resultChannelTb.MoviesCategories = JsonConvert.SerializeObject(data.MoviesCategories);
                        resultChannelTb.ImportSystem = data.ImportSystem;
                        resultChannelTb.UploadSystem = data.UploadSystem;

                        connection.Insert(resultChannelTb);
                    }
                }
                else
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.MySettingsTb>(data);
                    if (db != null)
                    {
                        db.CurrencyArray = JsonConvert.SerializeObject(data.CurrencyArray);
                        db.CurrencySymbolArray = JsonConvert.SerializeObject(data.CurrencySymbolArray);
                        db.Continents = JsonConvert.SerializeObject(data.Continents);
                        db.Categories = JsonConvert.SerializeObject(data.Categories);
                        if (data.SubCategories != null)
                            db.SubCategories = JsonConvert.SerializeObject(data.SubCategories?.SubCategoriessList);
                        db.MoviesCategories = JsonConvert.SerializeObject(data.MoviesCategories);
                        db.ImportSystem = data.ImportSystem;
                        db.UploadSystem = data.UploadSystem;

                        connection.Update(db);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_Settings(data);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data setting 
        public GetSettingsObject.SiteSettings Get_Settings()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.MySettingsTb>().FirstOrDefault();
                if (data != null)
                {
                    var db = ClassMapper.Mapper?.Map<GetSettingsObject.SiteSettings>(data);
                    if (db != null)
                    {
                        var asd = db;
                        asd.CurrencyArray = new List<string>();
                        asd.CurrencySymbolArray = new GetSettingsObject.CurrencySymbolArray();
                        asd.Continents = new List<string>();
                        asd.Categories = new Dictionary<string, string>();
                        asd.SubCategories = new GetSettingsObject.SubCategoriesUnion();
                        asd.MoviesCategories = new Dictionary<string, string>();

                        if (!string.IsNullOrEmpty(data.CurrencyArray))
                            asd.CurrencyArray = JsonConvert.DeserializeObject<List<string>>(data.CurrencyArray);

                        if (!string.IsNullOrEmpty(data.CurrencySymbolArray))
                            asd.CurrencySymbolArray = JsonConvert.DeserializeObject<GetSettingsObject.CurrencySymbolArray>(data.CurrencySymbolArray);

                        if (!string.IsNullOrEmpty(data.Continents))
                            asd.Continents = JsonConvert.DeserializeObject<List<string>>(data.Continents);

                        if (!string.IsNullOrEmpty(data.Categories))
                            asd.Categories = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.Categories);

                        if (!string.IsNullOrEmpty(data.SubCategories))
                            asd.SubCategories = new GetSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriessList = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(data.SubCategories),
                            };

                        if (!string.IsNullOrEmpty(data.MoviesCategories))
                            asd.MoviesCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.MoviesCategories);

                        //Categories
                        var listCategories = db.Categories.Select(cat => new Classes.Category
                        {
                            Id = cat.Key,
                            Name = Methods.FunString.DecodeString(cat.Value),
                            Color = "#212121",
                            Image = CategoriesController.GetImageCategory(cat.Value),
                            SubList = new List<Classes.Category>()
                        }).ToList();

                        CategoriesController.ListCategories.Clear();
                        CategoriesController.ListCategories = new ObservableCollection<Classes.Category>(listCategories);

                        if (db.SubCategories?.SubCategoriessList?.Count > 0)
                        {
                            //Sub Categories
                            foreach (var sub in db.SubCategories?.SubCategoriessList)
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

                        //Movies Categories
                        var listMovies = db.MoviesCategories.Select(cat => new Classes.Category
                        {
                            Id = cat.Key,
                            Name = Methods.FunString.DecodeString(cat.Value),
                            Color = "#212121",
                            SubList = new List<Classes.Category>()
                        }).ToList();

                        CategoriesController.ListCategoriesMovies.Clear();
                        CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Category>(listMovies);

                        AppSettings.Lang = data.Language;

                        AppSettings.OneSignalAppId = data.PushId;
                        //AppSettings.ShowButtonImport = string.IsNullOrWhiteSpace(data.ImportSystem) ? AppSettings.ShowButtonImport : data.ImportSystem == "on";
                        //AppSettings.ShowButtonUpload = string.IsNullOrWhiteSpace(data.UploadSystem) ? AppSettings.ShowButtonUpload : data.UploadSystem == "on";
                        OneSignalNotification.RegisterNotificationDevice();

                        return db;
                    }

                    return null!;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_Settings();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region Login

        //Get data Login
        public DataTables.LoginTb Get_data_Login()
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    UserDetails.Username = dataUser.Username;
                    UserDetails.FullName = dataUser.Username;
                    UserDetails.Password = dataUser.Password;
                    UserDetails.UserId = InitializePlayTube.UserId = dataUser.UserId;
                    UserDetails.AccessToken = Current.AccessToken = dataUser.AccessToken;
                    UserDetails.Status = dataUser.Status;
                    UserDetails.Cookie = dataUser.Cookie;
                    UserDetails.Email = dataUser.Email;
                    AppSettings.Lang = dataUser.Lang;
                         
                    ListUtils.DataUserLoginList.Add(dataUser);

                    return dataUser;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.UserId = UserDetails.UserId;
                    dataUser.AccessToken = UserDetails.AccessToken;
                    dataUser.Cookie = UserDetails.Cookie;
                    dataUser.Username = UserDetails.Username;
                    dataUser.Password = UserDetails.Password;
                    dataUser.Status = UserDetails.Status;
                    dataUser.Lang = AppSettings.Lang;
                    dataUser.DeviceId = UserDetails.DeviceId;
                    dataUser.Email = UserDetails.Email;

                    connection.Update(dataUser);
                }
                else
                {
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin_Credentials(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region MyChannel

        //Insert Or Update data MyChannel

        public void InsertOrUpdate_DataMyChannel(UserDataObject channel)
        {
            try
            {
                using var connection = OpenConnection();
                var resultChannelTb = connection.Table<DataTables.ChannelTb>().FirstOrDefault();
                if (resultChannelTb != null)
                {
                    resultChannelTb = ClassMapper.Mapper?.Map<DataTables.ChannelTb>(channel);
                    if (resultChannelTb != null)
                    {
                        UserDetails.Avatar = resultChannelTb.Avatar;
                        UserDetails.Cover = resultChannelTb.Cover;
                        UserDetails.Username = resultChannelTb.Username;
                        UserDetails.FullName = AppTools.GetNameFinal(resultChannelTb);

                        ListUtils.MyChannelList?.Clear();
                        ListUtils.MyChannelList?.Add(resultChannelTb);

                        connection.Update(resultChannelTb);
                    }
                }
                else
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.ChannelTb>(channel);
                    if (db != null)
                    {
                        connection.Insert(db);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_DataMyChannel(channel);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Data My Channel
        public UserDataObject GetDataMyChannel()
        {
            try
            {
                using var connection = OpenConnection();
                var channel = connection.Table<DataTables.ChannelTb>().FirstOrDefault();
                if (channel != null)
                {
                    var db = ClassMapper.Mapper?.Map<UserDataObject>(channel);
                    if (db != null)
                    {
                        UserDetails.Avatar = db.Avatar;
                        UserDetails.Cover = db.Cover;
                        UserDetails.Username = db.Username;
                        UserDetails.FullName = AppTools.GetNameFinal(db);

                        ListUtils.MyChannelList?.Clear();
                        ListUtils.MyChannelList?.Add(db);

                        return channel;
                    }

                    return null!;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetDataMyChannel();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region SubscriptionsChannel Videos

        //Insert SubscriptionsChannel Videos
        public void Insert_One_SubscriptionChannel(UserDataObject channel)
        {
            try
            {
                using var connection = OpenConnection();
                if (channel != null)
                {
                    var select = connection.Table<DataTables.SubscriptionsChannelTb>().FirstOrDefault(a => a.Id == channel.Id);
                    if (select == null)
                    {
                        var db = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(channel);
                        if (db != null) connection.Insert(db);
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(channel);
                        if (select != null) connection.Update(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_One_SubscriptionChannel(channel);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Insert SubscriptionsChannel Videos
        public void InsertAllSubscriptionsChannel(ObservableCollection<UserDataObject> channelsList)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();

                var list = new List<DataTables.SubscriptionsChannelTb>();
                foreach (var info in channelsList)
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.SubscriptionsChannelTb>(info);
                    if (db != null) list.Add(db);

                    var update = result.FirstOrDefault(a => a.Id == info.Id);
                    if (update != null)
                    {
                        update = db;
                        connection.Update(update);
                    }
                }

                if (list.Count <= 0) return;

                connection.BeginTransaction();
                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                if (newItemList.Count > 0)
                    connection.InsertAll(newItemList);

                result = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                if (deleteItemList.Count > 0)
                    foreach (var delete in deleteItemList)
                        connection.Delete(delete);

                connection?.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertAllSubscriptionsChannel(channelsList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove SubscriptionsChannel Videos
        public void RemoveSubscriptionsChannel(string subscriptionsChannelId)
        {
            try
            {
                using var connection = OpenConnection();
                if (!string.IsNullOrEmpty(subscriptionsChannelId))
                {
                    var select = connection.Table<DataTables.SubscriptionsChannelTb>().FirstOrDefault(a => a.Id == subscriptionsChannelId);
                    if (select != null)
                    {
                        connection.Delete(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    RemoveSubscriptionsChannel(subscriptionsChannelId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get SubscriptionsChannel Videos
        public ObservableCollection<UserDataObject> GetSubscriptionsChannel()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.SubscriptionsChannelTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new ObservableCollection<UserDataObject>();
                    foreach (var item in select)
                    {
                        var db = ClassMapper.Mapper?.Map<UserDataObject>(item);
                        if (db != null) list.Add(db);
                    }

                    return list;
                }

                return new ObservableCollection<UserDataObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSubscriptionsChannel();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<UserDataObject>();
                } 
            }
        }

        #endregion

        #region WatchOffline Videos

        //Insert WatchOffline Videos
        public void Insert_WatchOfflineVideos(VideoObject video)
        {
            try
            {
                using var connection = OpenConnection();
                if (video != null)
                {
                    var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.VideoId == video.VideoId);
                    if (select == null)
                    {
                        var db = ClassMapper.Mapper?.Map<DataTables.WatchOfflineVideosTb>(video);
                        db.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Insert(db);
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.WatchOfflineVideosTb>(video);
                        select.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Update(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_WatchOfflineVideos(video);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove WatchOffline Videos
        public void Remove_WatchOfflineVideos(string watchOfflineVideosId)
        {
            try
            {
                using var connection = OpenConnection();
                if (!string.IsNullOrEmpty(watchOfflineVideosId))
                {
                    var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.VideoId == watchOfflineVideosId);
                    if (select != null)
                    {
                        connection.Delete(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Remove_WatchOfflineVideos(watchOfflineVideosId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get WatchOffline Videos
        public ObservableCollection<VideoObject> Get_WatchOfflineVideos()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.WatchOfflineVideosTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new ObservableCollection<VideoObject>();
                    foreach (var item in select)
                    {
                        var db = new VideoObject
                        {
                            Id = item.Id,
                            VideoId = item.VideoId,
                            UserId = item.UserId,
                            ShortId = item.ShortId,
                            Title = item.Title,
                            Description = item.Description,
                            Thumbnail = item.Thumbnail,
                            VideoLocation = item.VideoLocation,
                            Youtube = item.Youtube,
                            Vimeo = item.Vimeo,
                            Daily = item.Daily,
                            Facebook = item.Facebook,
                            Time = item.Time,
                            TimeDate = item.TimeDate,
                            Active = item.Active,
                            Tags = item.Tags,
                            Duration = item.Duration,
                            Size = item.Size,
                            Converted = item.Converted,
                            CategoryId = item.CategoryId,
                            Views = item.Views,
                            Featured = item.Featured,
                            Registered = item.Registered,
                            Type = item.Type,
                            Approved = item.Approved,
                            OrgThumbnail = item.OrgThumbnail,
                            VideoType = item.VideoType,
                            Source = item.Source,
                            Url = item.Url,
                            EditDescription = item.EditDescription,
                            MarkupDescription = item.MarkupDescription,
                            IsLiked = item.IsLiked,
                            IsDisliked = item.IsDisliked,
                            IsOwner = item.IsOwner,
                            TimeAlpha = item.TimeAlpha,
                            TimeAgo = item.TimeAgo,
                            CategoryName = item.CategoryName,
                            Likes = item.Likes,
                            Dislikes = item.Dislikes,
                            LikesPercent = item.LikesPercent,
                            DislikesPercent = item.DislikesPercent,
                            AgeRestriction = item.AgeRestriction,
                            Country = item.Country,
                            Demo = item.Demo,
                            GeoBlocking = item.GeoBlocking,
                            Gif = item.Gif,
                            IsMovie = item.IsMovie,
                            IsPurchased = item.IsPurchased,
                            MovieRelease = item.MovieRelease,
                            Ok = item.Ok,
                            PlaylistLink = item.PlaylistLink,
                            Privacy = item.Privacy,
                            Producer = item.Producer,
                            Quality = item.Quality,
                            Rating = item.Rating,
                            RentPrice = item.RentPrice,
                            SellVideo = item.SellVideo,
                            Stars = item.Stars,
                            SubCategory = item.SubCategory,
                            TrId = item.TrId,
                            Twitch = item.Twitch,
                            TwitchType = item.TwitchType,
                            DataVideoId = item.DataVideoId,
                            IsSubscribed = item.IsSubscribed,
                            Monetization = item.Monetization,
                            The1080P = item.The1080P,
                            The2048P = item.The2048P,
                            The240P = item.The240P,
                            The360P = item.The360P,
                            The4096P = item.The4096P,
                            The480P = item.The480P,
                            The720P = item.The720P,
                            AgoraResourceId = item.AgoraResourceId,
                            AgoraSid = item.AgoraSid,
                            HistoryId = item.HistoryId,
                            IsStock = item.IsStock,
                            License = item.License,
                            LiveEnded = item.LiveEnded,
                            LiveTime = item.LiveTime, 
                            QualityVideoSelect = item.QualityVideoSelect,
                            StreamName = item.StreamName,
                            Video1080P = item.Video1080P,
                            Video2048P = item.Video2048P,
                            Video240P = item.Video240P,
                            Video360P = item.Video360P,
                            Video4096P = item.Video4096P,
                            Video480P = item.Video480P,
                            Video720P = item.Video720P,
                            VideoAuto = item.VideoAuto,
                            Owner = new UserDataObject(),
                        };

                        if (!string.IsNullOrEmpty(item.Owner))
                            db.Owner = new OwnerUnion
                            {
                                OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
                            };

                        list.Add(db);
                    }

                    return list;
                }

                return new ObservableCollection<VideoObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_WatchOfflineVideos();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<VideoObject>();
                } 
            }
        }

        public void Update_WatchOfflineVideos(string videoid, string videopath)
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.VideoId == videoid);
                if (select != null)
                {
                    //select.VideoName = videoid + ".mp4";
                    select.VideoLocation = videopath;
                    connection.Update(select);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Update_WatchOfflineVideos(videoid, videopath);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Library Item

        //Insert data LibraryItem
        public void InsertLibraryItem(Classes.LibraryItem libraryItem)
        {
            try
            {
                using var connection = OpenConnection();
                if (libraryItem == null)
                    return;
                var select = connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
                if (select != null)
                {
                    select.VideoCount = libraryItem.VideoCount;
                    select.BackgroundImage = libraryItem.BackgroundImage;
                    connection.Update(select);
                }
                else
                {
                    var item = new DataTables.LibraryItemTb
                    {
                        SectionId = libraryItem.SectionId,
                        SectionText = libraryItem.SectionText,
                        VideoCount = libraryItem.VideoCount,
                        BackgroundImage = libraryItem.BackgroundImage
                    };
                    connection.Insert(item);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertLibraryItem(libraryItem);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Insert data LibraryItem
        public void InsertLibraryItem(ObservableCollection<Classes.LibraryItem> libraryList)
        {
            try
            {
                using var connection = OpenConnection();
                if (libraryList?.Count == 0)
                    return;
                if (libraryList != null)
                {
                    foreach (var libraryItem in libraryList)
                    {
                        var select = connection.Table<DataTables.LibraryItemTb>().FirstOrDefault(a => a.SectionId == libraryItem.SectionId);
                        if (select != null)
                        {
                            select.SectionId = libraryItem.SectionId;
                            select.SectionText = libraryItem.SectionText;
                            select.VideoCount = libraryItem.VideoCount;
                            select.BackgroundImage = libraryItem.BackgroundImage;

                            connection.Update(select);
                        }
                        else
                        {
                            var item = new DataTables.LibraryItemTb
                            {
                                SectionId = libraryItem.SectionId,
                                SectionText = libraryItem.SectionText,
                                VideoCount = libraryItem.VideoCount,
                                BackgroundImage = libraryItem.BackgroundImage
                            };
                            connection.Insert(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertLibraryItem(libraryList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data LibraryItem
        public ObservableCollection<DataTables.LibraryItemTb> Get_LibraryItem()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.LibraryItemTb>().OrderBy(a => a.SectionId).ToList();
                if (select.Count > 0)
                {
                    return new ObservableCollection<DataTables.LibraryItemTb>(select);
                }

                return new ObservableCollection<DataTables.LibraryItemTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_LibraryItem();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<DataTables.LibraryItemTb>();
                } 
            }
        }

        #endregion

        #region Shared Videos

        //Insert Shared Videos
        public void Insert_SharedVideos(VideoObject video)
        {
            try
            {
                using var connection = OpenConnection();
                if (video != null)
                {
                    var select = connection.Table<DataTables.SharedVideosTb>().FirstOrDefault(a => a.VideoId == video.VideoId);
                    if (select == null)
                    {
                        var db = ClassMapper.Mapper?.Map<DataTables.SharedVideosTb>(video);
                        db.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Insert(db);
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.SharedVideosTb>(video);
                        select.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Update(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_SharedVideos(video);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Shared Videos
        public ObservableCollection<VideoObject> Get_SharedVideos()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.SharedVideosTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new ObservableCollection<VideoObject>();
                    foreach (var item in select)
                    {
                        var db = new VideoObject
                        {
                            Id = item.Id,
                            VideoId = item.VideoId,
                            UserId = item.UserId,
                            ShortId = item.ShortId,
                            Title = item.Title,
                            Description = item.Description,
                            Thumbnail = item.Thumbnail,
                            VideoLocation = item.VideoLocation,
                            Youtube = item.Youtube,
                            Vimeo = item.Vimeo,
                            Daily = item.Daily,
                            Facebook = item.Facebook,
                            Time = item.Time,
                            TimeDate = item.TimeDate,
                            Active = item.Active,
                            Tags = item.Tags,
                            Duration = item.Duration,
                            Size = item.Size,
                            Converted = item.Converted,
                            CategoryId = item.CategoryId,
                            Views = item.Views,
                            Featured = item.Featured,
                            Registered = item.Registered,
                            Type = item.Type,
                            Approved = item.Approved,
                            OrgThumbnail = item.OrgThumbnail,
                            VideoType = item.VideoType,
                            Source = item.Source,
                            Url = item.Url,
                            EditDescription = item.EditDescription,
                            MarkupDescription = item.MarkupDescription,
                            IsLiked = item.IsLiked,
                            IsDisliked = item.IsDisliked,
                            IsOwner = item.IsOwner,
                            TimeAlpha = item.TimeAlpha,
                            TimeAgo = item.TimeAgo,
                            CategoryName = item.CategoryName,
                            Likes = item.Likes,
                            Dislikes = item.Dislikes,
                            LikesPercent = item.LikesPercent,
                            DislikesPercent = item.DislikesPercent,
                            AgeRestriction = item.AgeRestriction,
                            Country = item.Country,
                            Demo = item.Demo,
                            GeoBlocking = item.GeoBlocking,
                            Gif = item.Gif,
                            IsMovie = item.IsMovie,
                            IsPurchased = item.IsPurchased,
                            MovieRelease = item.MovieRelease,
                            Ok = item.Ok,
                            PlaylistLink = item.PlaylistLink,
                            Privacy = item.Privacy,
                            Producer = item.Producer,
                            Quality = item.Quality,
                            Rating = item.Rating,
                            RentPrice = item.RentPrice,
                            SellVideo = item.SellVideo,
                            Stars = item.Stars,
                            SubCategory = item.SubCategory,
                            TrId = item.TrId,
                            Twitch = item.Twitch,
                            TwitchType = item.TwitchType,
                            DataVideoId = item.DataVideoId,
                            IsSubscribed = item.IsSubscribed,
                            Monetization = item.Monetization,
                            The1080P = item.The1080P,
                            The2048P = item.The2048P,
                            The240P = item.The240P,
                            The360P = item.The360P,
                            The4096P = item.The4096P,
                            The480P = item.The480P,
                            The720P = item.The720P,
                            AgoraResourceId = item.AgoraResourceId,
                            AgoraSid = item.AgoraSid,
                            HistoryId = item.HistoryId,
                            IsStock = item.IsStock,
                            License = item.License,
                            LiveEnded = item.LiveEnded,
                            LiveTime = item.LiveTime,
                            QualityVideoSelect = item.QualityVideoSelect,
                            StreamName = item.StreamName,
                            Video1080P = item.Video1080P,
                            Video2048P = item.Video2048P,
                            Video240P = item.Video240P,
                            Video360P = item.Video360P,
                            Video4096P = item.Video4096P,
                            Video480P = item.Video480P,
                            Video720P = item.Video720P,
                            VideoAuto = item.VideoAuto,
                            Owner = new UserDataObject(),
                        };

                        if (!string.IsNullOrEmpty(item.Owner))
                            db.Owner = new OwnerUnion
                            {
                                OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
                            };

                        list.Add(db);
                    }

                    return list;
                }

                return new ObservableCollection<VideoObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_SharedVideos();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<VideoObject>();
                } 
            }
        }

        #endregion

        #region Not Interested Videos

        //Insert NotInterested Videos
        public void Insert_NotInterestedVideos(VideoObject video)
        {
            try
            {
                using var connection = OpenConnection();
                if (video != null)
                {
                    var select = connection.Table<DataTables.NotInterestedTb>().FirstOrDefault(a => a.VideoId == video.VideoId);
                    if (select == null)
                    {
                        var db = ClassMapper.Mapper?.Map<DataTables.NotInterestedTb>(video);
                        db.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Insert(db);
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.NotInterestedTb>(video);
                        select.Owner = JsonConvert.SerializeObject(video.Owner?.OwnerClass);
                        connection.Update(select);
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_NotInterestedVideos(video);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get NotInterested Videos
        public ObservableCollection<VideoObject> Get_NotInterestedVideos()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.NotInterestedTb>().ToList();
                if (select.Count > 0)
                {
                    var list = new ObservableCollection<VideoObject>();
                    foreach (var item in select)
                    {
                        var db = new VideoObject
                        {
                            Id = item.Id,
                            VideoId = item.VideoId,
                            UserId = item.UserId,
                            ShortId = item.ShortId,
                            Title = item.Title,
                            Description = item.Description,
                            Thumbnail = item.Thumbnail,
                            VideoLocation = item.VideoLocation,
                            Youtube = item.Youtube,
                            Vimeo = item.Vimeo,
                            Daily = item.Daily,
                            Facebook = item.Facebook,
                            Time = item.Time,
                            TimeDate = item.TimeDate,
                            Active = item.Active,
                            Tags = item.Tags,
                            Duration = item.Duration,
                            Size = item.Size,
                            Converted = item.Converted,
                            CategoryId = item.CategoryId,
                            Views = item.Views,
                            Featured = item.Featured,
                            Registered = item.Registered,
                            Type = item.Type,
                            Approved = item.Approved,
                            OrgThumbnail = item.OrgThumbnail,
                            VideoType = item.VideoType,
                            Source = item.Source,
                            Url = item.Url,
                            EditDescription = item.EditDescription,
                            MarkupDescription = item.MarkupDescription,
                            IsLiked = item.IsLiked,
                            IsDisliked = item.IsDisliked,
                            IsOwner = item.IsOwner,
                            TimeAlpha = item.TimeAlpha,
                            TimeAgo = item.TimeAgo,
                            CategoryName = item.CategoryName,
                            Likes = item.Likes,
                            Dislikes = item.Dislikes,
                            LikesPercent = item.LikesPercent,
                            DislikesPercent = item.DislikesPercent,
                            AgeRestriction = item.AgeRestriction,
                            Country = item.Country,
                            Demo = item.Demo,
                            GeoBlocking = item.GeoBlocking,
                            Gif = item.Gif,
                            IsMovie = item.IsMovie,
                            IsPurchased = item.IsPurchased,
                            MovieRelease = item.MovieRelease,
                            Ok = item.Ok,
                            PlaylistLink = item.PlaylistLink,
                            Privacy = item.Privacy,
                            Producer = item.Producer,
                            Quality = item.Quality,
                            Rating = item.Rating,
                            RentPrice = item.RentPrice,
                            SellVideo = item.SellVideo,
                            Stars = item.Stars,
                            SubCategory = item.SubCategory,
                            TrId = item.TrId,
                            Twitch = item.Twitch,
                            TwitchType = item.TwitchType,
                            DataVideoId = item.DataVideoId,
                            IsSubscribed = item.IsSubscribed,
                            Monetization = item.Monetization,
                            The1080P = item.The1080P,
                            The2048P = item.The2048P,
                            The240P = item.The240P,
                            The360P = item.The360P,
                            The4096P = item.The4096P,
                            The480P = item.The480P,
                            The720P = item.The720P,
                            AgoraResourceId = item.AgoraResourceId,
                            AgoraSid = item.AgoraSid,
                            HistoryId = item.HistoryId,
                            IsStock = item.IsStock,
                            License = item.License,
                            LiveEnded = item.LiveEnded,
                            LiveTime = item.LiveTime,
                            QualityVideoSelect = item.QualityVideoSelect,
                            StreamName = item.StreamName,
                            Video1080P = item.Video1080P,
                            Video2048P = item.Video2048P,
                            Video240P = item.Video240P,
                            Video360P = item.Video360P,
                            Video4096P = item.Video4096P,
                            Video480P = item.Video480P,
                            Video720P = item.Video720P,
                            VideoAuto = item.VideoAuto,
                            Owner = new UserDataObject(),
                        };

                        if (!string.IsNullOrEmpty(item.Owner))
                            db.Owner = new OwnerUnion
                            {
                                OwnerClass = JsonConvert.DeserializeObject<UserDataObject>(item.Owner)
                            };

                        list.Add(db);
                    }

                    return list;
                }

                return new ObservableCollection<VideoObject>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_NotInterestedVideos();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<VideoObject>();
                } 
            }
        }

        #endregion

        #region Last Chat

        //Insert data To Last Chat Table
        public void InsertOrReplaceLastChatTable(ObservableCollection<GetChatsObject.Data> usersContactList)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.LastChatTb>().ToList();
                List<DataTables.LastChatTb> list = new List<DataTables.LastChatTb>();
                foreach (var info in usersContactList)
                {
                    var user = new DataTables.LastChatTb
                    {
                        Id = info.Id,
                        UserOne = info.UserOne,
                        UserTwo = info.UserTwo,
                        Time = info.Time,
                        TextTime = info.TextTime,
                        GetCountSeen = info.GetCountSeen,
                        UserDataJson = JsonConvert.SerializeObject(info.User),
                        GetLastMessageJson = JsonConvert.SerializeObject(info.GetLastMessage),
                    };

                    list.Add(user);

                    var update = result.FirstOrDefault(a => a.Id == info.Id);
                    if (update != null)
                    {
                        update = user;
                        connection.Update(update);
                    }
                }

                if (list.Count > 0)
                {
                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (newItemList.Count > 0)
                    {
                        connection.InsertAll(newItemList);
                    }

                    var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (deleteItemList.Count > 0)
                    {
                        foreach (var delete in deleteItemList)
                        {
                            connection.Delete(delete);
                        }
                    }

                    connection?.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceLastChatTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To LastChat Table
        public ObservableCollection<GetChatsObject.Data> GetAllLastChat()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.LastChatTb>().ToList();
                if (select.Count > 0)
                {
                    var list = select.Select(user => new GetChatsObject.Data
                    {
                        Id = user.Id,
                        UserOne = user.UserOne,
                        UserTwo = user.UserTwo,
                        Time = user.Time,
                        TextTime = user.TextTime,
                        GetCountSeen = user.GetCountSeen,
                        User = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
                        GetLastMessage =
                            JsonConvert.DeserializeObject<GetChatsObject.GetLastMessage>(user.GetLastMessageJson),
                    }).ToList();

                    return new ObservableCollection<GetChatsObject.Data>(list);
                }

                return new ObservableCollection<GetChatsObject.Data>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetAllLastChat();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<GetChatsObject.Data>();
                } 
            }
        }

        // Get data To LastChat Table By Id >> Load More
        public ObservableCollection<GetChatsObject.Data> GetLastChatById(int id, int nSize)
        {
            try
            {
                using var connection = OpenConnection();
                var query = connection.Table<DataTables.LastChatTb>().Where(w => w.AutoIdLastChat >= id)
                    .OrderBy(q => q.AutoIdLastChat).Take(nSize).ToList();
                if (query.Count > 0)
                {
                    var list = query.Select(user => new GetChatsObject.Data
                    {
                        Id = user.Id,
                        UserOne = user.UserOne,
                        UserTwo = user.UserTwo,
                        Time = user.Time,
                        TextTime = user.TextTime,
                        GetCountSeen = user.GetCountSeen,
                        User = JsonConvert.DeserializeObject<UserDataObject>(user.UserDataJson),
                        GetLastMessage =
                            JsonConvert.DeserializeObject<GetChatsObject.GetLastMessage>(user.GetLastMessageJson),
                    }).ToList();

                    if (list.Count > 0)
                        return new ObservableCollection<GetChatsObject.Data>(list);
                    return null!;
                }

                return new ObservableCollection<GetChatsObject.Data>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetLastChatById(id, nSize);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<GetChatsObject.Data>();
                } 
            }
        }

        //Remove data To LastChat Table
        public void DeleteUserLastChat(string userId)
        {
            try
            {
                using var connection = OpenConnection();
                var user = connection.Table<DataTables.LastChatTb>()
                    .FirstOrDefault(c => c.UserTwo.ToString() == userId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteUserLastChat(userId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Clear All data LastChat
        public void ClearLastChat()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DeleteAll<DataTables.LastChatTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearLastChat();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void InsertOrReplaceMessages(ObservableCollection<GetUserMessagesObject.Message> messageList)
        {
            try
            {
                using var connection = OpenConnection();
                var listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                // get data from database
                var resultMessage = connection.Table<DataTables.MessageTb>().ToList();
                var listAllMessage = resultMessage.Select(messages => new GetUserMessagesObject.Message
                {
                    Id = messages.Id,
                    FromId = messages.FromId,
                    ToId = messages.ToId,
                    Text = messages.Text,
                    Seen = messages.Seen,
                    Time = messages.Time,
                    FromDeleted = messages.FromDeleted,
                    ToDeleted = messages.ToDeleted,
                    TextTime = messages.TextTime,
                    Position = messages.Position,
                }).ToList();

                foreach (var messages in messageList)
                {
                    var maTb = new DataTables.MessageTb
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        FromDeleted = messages.FromDeleted,
                        ToDeleted = messages.ToDeleted,
                        TextTime = messages.TextTime,
                        Position = messages.Position,
                    };

                    var dataCheck = listAllMessage.FirstOrDefault(a => a.Id == messages.Id);
                    if (dataCheck != null)
                    {
                        var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                        if (checkForUpdate != null)
                        {
                            checkForUpdate.Id = messages.Id;
                            checkForUpdate.FromId = messages.FromId;
                            checkForUpdate.ToId = messages.ToId;
                            checkForUpdate.Text = messages.Text;
                            checkForUpdate.Seen = messages.Seen;
                            checkForUpdate.Time = messages.Time;
                            checkForUpdate.FromDeleted = messages.FromDeleted;
                            checkForUpdate.ToDeleted = messages.ToDeleted;
                            checkForUpdate.TextTime = messages.TextTime;
                            checkForUpdate.Position = messages.Position;

                            connection.Update(checkForUpdate);
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }
                    else
                    {
                        listOfDatabaseForInsert.Add(maTb);
                    }
                }

                connection.BeginTransaction();

                //Bring new  
                if (listOfDatabaseForInsert.Count > 0)
                {
                    connection.InsertAll(listOfDatabaseForInsert);
                }

                connection?.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceMessages(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Messages Table
        public void InsertOrUpdateToOneMessages(GetUserMessagesObject.Message messages)
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == messages.Id);
                if (data != null)
                {
                    data.Id = messages.Id;
                    data.FromId = messages.FromId;
                    data.ToId = messages.ToId;
                    data.Text = messages.Text;
                    data.Seen = messages.Seen;
                    data.Time = messages.Time;
                    data.FromDeleted = messages.FromDeleted;
                    data.ToDeleted = messages.ToDeleted;
                    data.TextTime = messages.TextTime;
                    data.Position = messages.Position;

                    connection.Update(data);
                }
                else
                {
                    var mdb = new DataTables.MessageTb
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        FromDeleted = messages.FromDeleted,
                        ToDeleted = messages.ToDeleted,
                        TextTime = messages.TextTime,
                        Position = messages.Position,
                    };

                    //Insert  one Messages Table
                    connection.Insert(mdb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateToOneMessages(messages);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To Messages
        public string GetMessagesList(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                var beforeQ = "";
                if (beforeMessageId != "0")
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query = connection.Query<DataTables.MessageTb>(
                    "SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +
                    toId + " and ToId=" + fromId + ")) " + beforeQ);
                var queryList = query
                    .Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
                    .OrderBy(q => q.Time).TakeLast(35).ToList();
                if (queryList.Count > 0)
                {
                    foreach (var messages in queryList)
                    {
                        var m = new GetUserMessagesObject.Message
                        {
                            Id = messages.Id,
                            FromId = messages.FromId,
                            ToId = messages.ToId,
                            Text = messages.Text,
                            Seen = messages.Seen,
                            Time = messages.Time,
                            FromDeleted = messages.FromDeleted,
                            ToDeleted = messages.ToDeleted,
                            TextTime = messages.TextTime,
                            Position = messages.Position,
                        };

                        if (beforeMessageId == "0")
                        {
                            MessagesBoxActivity.MAdapter?.Add(m);
                        }
                        else
                        {
                            MessagesBoxActivity.MAdapter?.Insert(m, beforeMessageId);
                        }
                    }

                    return "1";
                }

                return "0";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessagesList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return "0";
                }  
            }
        }

        //Get data To where first Messages >> load more
        public List<DataTables.MessageTb> GetMessageList(string fromId, string toId, string beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                var beforeQ = "";
                if (beforeMessageId != "0")
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query = connection.Query<DataTables.MessageTb>(
                    "SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +
                    toId + " and ToId=" + fromId + ")) " + beforeQ);
                var queryList = query
                    .Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
                    .OrderBy(q => q.Time).TakeLast(35).ToList();
                return queryList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessageList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(string messageId)
        {
            try
            {
                using var connection = OpenConnection();
                var user = connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using var connection = OpenConnection();
                var query = connection.Query<DataTables.MessageTb>(
                    "Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" +
                    toId + " and ToId=" + fromId + "))");
                Console.WriteLine(query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMessagesUser(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Messages Table
        public void ClearAll_Messages()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DeleteAll<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Messages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}