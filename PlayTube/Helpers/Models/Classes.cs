//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System.Collections.Generic;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Video;

namespace PlayTube.Helpers.Models
{   
    public class Classes
    {
        public class Category
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Image { get; set; }
            public string Color { get; set; } 
            public List<Category> SubList { get; set; }
        }


        public class TrendingClass
        {
            public int Id { get; set; }
            public ItemType Type { get; set; }
            public List<GetPopularChannelsObject.Channel> ChannelList { get; set; } 
            public VideoObject VideoData { get; set; } 
        }

        public class MainVideoClass
        {
            public int Id { get; set; }
            public ItemType Type { get; set; }
            public List<Category> CategoryList { get; set; } 
            public List<VideoObject> TopVideoList { get; set; } 
            public List<VideoObject> LatestVideoList { get; set; } 
            public List<VideoObject> FavVideoList { get; set; } 
            public List<VideoObject> StockVideoList { get; set; } 
            public VideoObject VideoData { get; set; }
            public List<VideoObject> LiveVideoList { get; set; }
            public List<VideoObject> SliderVideoList { get; set; }
        }

        public class SubscriptionsClass
        {
            public int Id { get; set; }
            public ItemType Type { get; set; }
            public List<UserDataObject> ChannelList { get; set; }
            public VideoObject VideoData { get; set; }
        }

        public class LibraryItem
        {
            public string SectionId { get; set; }
            public string SectionText { get; set; }
            public int VideoCount { get; set; }
            public string BackgroundImage { get; set; }
        }
    }
     
    public enum ItemType
    {
        RowVideo = 100,  BigVideo = 500, Channel = 200, EmptyPage = 300 , TopVideos = 101, LatestVideos = 102, FavVideos = 103, OtherVideos = 104 , Category = 400, LiveVideos = 105, StockVideos = 106 , SliderHome = 107  
        , AdMob1 = 601, AdMob2 = 602, AdMob3 = 603,
    }
}