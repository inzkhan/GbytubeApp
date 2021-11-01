using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Util;
using PlayTube.Activities.Channel.ChannelPopular;
using PlayTube.Activities.Models;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.IntegrationRecyclerView;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Video;
using IList = System.Collections.IList;

namespace PlayTube.Activities.Tabbes.Adapters
{ 
    public class TrendingAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<TrendingAdapterClickEventArgs> ItemClick;
        public event EventHandler<TrendingAdapterClickEventArgs> ItemLongClick;

        public event EventHandler<VideoRowAdapterClickEventArgs> VideoItemClick;
        public event EventHandler<VideoRowAdapterClickEventArgs> VideoItemLongClick;

        public event EventHandler<VideoAdapterClickEventArgs> BigVideoItemClick;
        public event EventHandler<VideoAdapterClickEventArgs> BigVideoItemLongClick;

        private readonly Activity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        private PopularChannelAdapter PopularChannelAdapter;
        public ObservableCollection<Classes.TrendingClass> TrendingList = new ObservableCollection<Classes.TrendingClass>();
        private readonly LibrarySynchronizer LibrarySynchronizer;

        public TrendingAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                GlobalContext = TabbedMainActivity.GetInstance();
                LibrarySynchronizer = new LibrarySynchronizer(context);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case (int)ItemType.Channel:
                    {
                        //Setup your layout here >> TemplateRecyclerViewLayout
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                        var vh = new TrendingAdapterViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    }
                    case (int)ItemType.RowVideo:
                    {
                        //Setup your layout here >> Video_Row_View
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Video_Row_View, parent, false);
                        var vh = new VideoRowAdapterViewHolder(itemView, VideoOnClick, VideoOnLongClick);
                        return vh;
                    }
                    case (int)ItemType.BigVideo:
                    {
                        //Setup your layout here >> Video_Big_View
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Video_Big_View, parent, false);
                        var vh = new VideoBigAdapterViewHolder(itemView, BigVideoOnClick, BigVideoOnLongClick);
                        return vh;
                    }
                    case (int)ItemType.AdMob1:
                    {
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob, parent, false);
                        var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, ActivityContext);
                        return vh;
                    }
                    case (int)ItemType.AdMob2:
                    {
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob2, parent, false);
                        var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, ActivityContext);
                        return vh;
                    }
                    case (int)ItemType.AdMob3:
                    {
                        var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_AdMob3, parent, false);
                        var vh = new AdapterHolders.AdMob3AdapterViewHolder(itemView);
                        return vh;
                    }  
                    case (int)ItemType.EmptyPage:
                    {
                        //Setup your layout here >> EmptyStateLayout
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.EmptyStateLayout, parent, false);
                        var vh = new AdapterHolders.EmptyStateViewHolder(itemView);
                        return vh;
                    }
                    default:
                        return null!;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                var item = TrendingList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case ItemType.Channel:
                        {
                            if (viewHolder is TrendingAdapterViewHolder holder)
                            {
                                if (PopularChannelAdapter == null)
                                {
                                    PopularChannelAdapter = new PopularChannelAdapter(ActivityContext)
                                    {
                                        ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>()
                                    };

                                    LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                    holder.MRecycler.SetLayoutManager(layoutManager);
                                    holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                    var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                    var preLoader = new RecyclerViewPreloader<GetPopularChannelsObject.Channel>(ActivityContext, PopularChannelAdapter, sizeProvider, 10);
                                    holder.MRecycler.AddOnScrollListener(preLoader);
                                    holder.MRecycler.SetAdapter(PopularChannelAdapter);
                                    PopularChannelAdapter.OnItemClick += PopularChannelAdapterOnOnItemClick;
                                }

                                if (item.ChannelList.Count > 0)
                                {
                                    if (PopularChannelAdapter.ChannelList.Count == 0)
                                    {
                                        PopularChannelAdapter.ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>(item.ChannelList);
                                        PopularChannelAdapter.NotifyDataSetChanged();
                                    }
                                    else if (PopularChannelAdapter.ChannelList != null && PopularChannelAdapter.ChannelList.Count != item.ChannelList.Count)
                                    {
                                        PopularChannelAdapter.ChannelList = new ObservableCollection<GetPopularChannelsObject.Channel>(item.ChannelList);
                                        PopularChannelAdapter.NotifyDataSetChanged();
                                    }
                                }

                                holder.MainLinear.Visibility = ViewStates.Visible;
                                holder.MoreText.Visibility = PopularChannelAdapter.ChannelList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_PopularChannels);

                            }

                            break;
                        }
                        case ItemType.RowVideo:
                        {
                            if (viewHolder is VideoRowAdapterViewHolder videoRow)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Thumbnail, videoRow.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                                videoRow.TxtDuration.Text = Methods.Time.SplitStringDuration(item.VideoData.Duration);
                                videoRow.TxtTitle.Text = Methods.FunString.DecodeString(item.VideoData.Title);

                                videoRow.TxtChannelName.Text = AppTools.GetNameFinal(item.VideoData.Owner?.OwnerClass);

                                videoRow.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.VideoData.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoRow.MenueView, IonIconsFonts.More);

                                //Verified 
                                if (item.VideoData.Owner?.OwnerClass?.Verified == "1")
                                {
                                    videoRow.IconVerified.Visibility = ViewStates.Visible;
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoRow.IconVerified, IonIconsFonts.CheckmarkCircle);
                                }
                                else
                                {
                                    videoRow.IconVerified.Visibility = ViewStates.Gone; 
                                }
                             
                                if (!videoRow.MenueView.HasOnClickListeners)
                                    videoRow.MenueView.Click += (sender, args) =>
                                    {
                                        ShowPopup(videoRow.MenueView, item.VideoData); 
                                    };

                                //Set Badge on videos
                                AppTools.ShowGlobalBadgeSystem(videoRow.VideoType, item.VideoData);
                            }

                            break;
                        }
                        case ItemType.BigVideo:
                        {
                            if (viewHolder is VideoBigAdapterViewHolder videoRow)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Thumbnail, videoRow.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                                GlideImageLoader.LoadImage(ActivityContext, item.VideoData.Owner?.OwnerClass?.Avatar, videoRow.ChannelImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                videoRow.TxtDuration.Text = Methods.Time.SplitStringDuration(item.VideoData.Duration);
                                videoRow.TxtTitle.Text = Methods.FunString.DecodeString(item.VideoData.Title);

                                videoRow.TxtChannelName.Text = AppTools.GetNameFinal(item.VideoData.Owner?.OwnerClass);

                                videoRow.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.VideoData.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

                                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoRow.MenueView, IonIconsFonts.More);

                                //Verified 
                                if (item.VideoData.Owner?.OwnerClass?.Verified == "1")
                                {
                                    videoRow.IconVerified.Visibility = ViewStates.Visible;
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, videoRow.IconVerified, IonIconsFonts.CheckmarkCircle);
                                }
                                else
                                {
                                    videoRow.IconVerified.Visibility = ViewStates.Gone; 
                                }
                             
                                if (!videoRow.MenueView.HasOnClickListeners)
                                    videoRow.MenueView.Click += (sender, args) =>
                                    {
                                        ShowPopup(videoRow.MenueView, item.VideoData); 
                                    };

                                //Set Badge on videos
                                AppTools.ShowGlobalBadgeSystem(videoRow.VideoType, item.VideoData);
                            }

                            break;
                        }
                        case ItemType.EmptyPage:
                        {
                            if (viewHolder is AdapterHolders.EmptyStateViewHolder holder)
                            {
                                //FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EmptyStateIcon, IonIconsFonts.Camera);
                                holder.EmptyStateIcon.SetImageResource(Resource.Drawable.icon_fire_vector);

                                holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
                                holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_Start_Trending);
                                holder.EmptyStateButton.Visibility = ViewStates.Gone;
                            }
                            break;
                        }
                    } 
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void ShowPopup(View v, VideoObject item)
        {
            try
            {
                LayoutInflater layoutInflater = (LayoutInflater)ActivityContext.GetSystemService(Context.LayoutInflaterService);
                View popupView = layoutInflater.Inflate(Resource.Layout.PopupVideoMoreLayout, null);

                int px = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 200, ActivityContext.Resources.DisplayMetrics);
                var popupWindow = new PopupWindow(popupView, px, ViewGroup.LayoutParams.WrapContent);

                var menuAddWatchLater = popupView.FindViewById<TextView>(Resource.Id.menu_AddWatchLater);
                var menuAddPlaylist = popupView.FindViewById<TextView>(Resource.Id.menu_AddPlaylist);
                var menuRemove = popupView.FindViewById<TextView>(Resource.Id.menu_Remove);
                var menuShare = popupView.FindViewById<TextView>(Resource.Id.menu_Share);
                var menuReport = popupView.FindViewById<TextView>(Resource.Id.menu_Report);

                if (item.IsOwner != null && item.IsOwner.Value)
                {
                    menuRemove.Visibility = ViewStates.Gone;
                    menuReport.Visibility = ViewStates.Invisible;
                }

                menuAddWatchLater.Click += (sender, args) =>
                {
                    try
                    {
                        LibrarySynchronizer.AddToWatchLater(item);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuAddPlaylist.Click += (sender, args) =>
                {
                    try
                    {
                        LibrarySynchronizer.AddToPlaylist(item);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuRemove.Click += (sender, args) =>
                {
                    try
                    {
                        var index = TrendingList.IndexOf(TrendingList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId));
                        if (index != -1)
                        {
                            TrendingList.RemoveAt(index);
                            NotifyItemRemoved(index);

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

                            var data = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.VideoId == item.VideoId);
                            if (data == null)
                            {
                                ListUtils.GlobalNotInterestedList.Add(item);
                            }

                            var sqlEntity = new SqLiteDatabase();
                            sqlEntity.Insert_NotInterestedVideos(item);
                            popupWindow.Dismiss();
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuShare.Click += (sender, args) =>
                {
                    try
                    {
                        LibrarySynchronizer.ShareVideo(item);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                menuReport.Click += (sender, args) =>
                {
                    try
                    {
                        LibrarySynchronizer.AddReportVideo(item);
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.SetBackgroundDrawable(new ColorDrawable()); 
                popupWindow.Focusable = true;
                popupWindow.ClippingEnabled = true;
                popupWindow.OutsideTouchable = false;
                popupWindow.DismissEvent += delegate (object sender, EventArgs args) {
                    try
                    {
                        popupWindow.Dismiss();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                };

                popupWindow.ShowAsDropDown(v);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

         
        private void PopularChannelAdapterOnOnItemClick(object sender, PopularChannelAdapterClickEventArgs e)
        {
            try
            {
                if(e.Position <= -1) return;

                var item = PopularChannelAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.ShowUserChannelFragment(item.UserData, item.UserData.Id);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => TrendingList?.Count ?? 0;
     
        public Classes.TrendingClass GetItem(int position)
        {
            return TrendingList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = TrendingList[position];
                if (item != null)
                {
                    return item.Type switch
                    {
                        ItemType.BigVideo => (int)ItemType.BigVideo,
                        ItemType.RowVideo => (int)ItemType.RowVideo,
                        ItemType.Channel => (int)ItemType.Channel,
                        ItemType.AdMob1 => (int)ItemType.AdMob1,
                        ItemType.AdMob2 => (int)ItemType.AdMob2,
                        ItemType.AdMob3 => (int)ItemType.AdMob3,
                        ItemType.EmptyPage => (int) ItemType.EmptyPage, 
                        _ => (int)ItemType.EmptyPage
                    };
                }
               
                return (int)ItemType.EmptyPage;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        void OnClick(TrendingAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
        void OnLongClick(TrendingAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

        void VideoOnClick(VideoRowAdapterClickEventArgs args) => VideoItemClick?.Invoke(this, args);
        void VideoOnLongClick(VideoRowAdapterClickEventArgs args) => VideoItemLongClick?.Invoke(this, args);

        void BigVideoOnClick(VideoAdapterClickEventArgs args) => BigVideoItemClick?.Invoke(this, args);
        void BigVideoOnLongClick(VideoAdapterClickEventArgs args) => BigVideoItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = TrendingList[p0];

                if (item?.VideoData == null)
                    return Collections.SingletonList(p0);

                if (!string.IsNullOrEmpty(item.VideoData?.Thumbnail))
                {
                    d.Add(item.VideoData.Thumbnail);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString()).Apply(new RequestOptions().CenterCrop());
        }
         
    }

    public class TrendingAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public RelativeLayout MainLinear { get; private set; }
        public TextView TitleText { get; private set; } 
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public TrendingAdapterViewHolder(View itemView, Action<TrendingAdapterClickEventArgs> clickListener, Action<TrendingAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            { 
                MainView = itemView;

                MainLinear = MainView.FindViewById<RelativeLayout>(Resource.Id.mainLinear);
                TitleText = MainView.FindViewById<TextView>(Resource.Id.textTitle); 
                MoreText = MainView.FindViewById<TextView>(Resource.Id.textMore);
                MRecycler = MainView.FindViewById<RecyclerView>(Resource.Id.recyler);
                 
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                 
                //Create an Event
                MainLinear.Click += MoreTextOnClick;
                //itemView.Click += (sender, e) => clickListener(new TrendingAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new TrendingAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreTextOnClick(object sender, EventArgs e)
        {
            try
            {
               var globalContext = TabbedMainActivity.GetInstance();
               if (globalContext?.TrendingFragment != null)
               {
                   globalContext.TrendingFragment.AllChannelPopularFragment = new AllChannelPopularFragment();
                   globalContext.FragmentBottomNavigator.DisplayFragment(globalContext.TrendingFragment.AllChannelPopularFragment);
               } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
     

    public class TrendingAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}