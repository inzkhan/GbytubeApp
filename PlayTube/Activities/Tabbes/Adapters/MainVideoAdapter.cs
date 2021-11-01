using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Util;
using PlayTube.Activities.Models;
using PlayTube.Activities.Search;
using PlayTube.Activities.Videos;
using PlayTube.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using IList = System.Collections.IList;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using PlayTube.Activities.Live.Page;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Newtonsoft.Json;
using PlayTube.Activities.Stock;
using PlayTube.Library.Anjo.IntegrationRecyclerView;

namespace PlayTube.Activities.Tabbes.Adapters
{
    public class MainVideoAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<MainVideoAdapterClickEventArgs> ItemClick;
        public event EventHandler<MainVideoAdapterClickEventArgs> ItemLongClick;

        public event EventHandler<VideoAdapterClickEventArgs> OtherVideosItemClick;
        public event EventHandler<VideoAdapterClickEventArgs> OtherVideosItemLongClick;
         
        private readonly Activity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        private CategoryIconAdapter CategoryIconAdapter;
        private VideoHorizontalAdapter TopVideosAdapter, LatestVideosAdapter, FavVideosAdapter, LiveVideosAdapter, StockVideosAdapter; 

        public ObservableCollection<Classes.MainVideoClass> MainVideoList = new ObservableCollection<Classes.MainVideoClass>();

        private readonly LibrarySynchronizer LibrarySynchronizer;

        public MainVideoAdapter(Activity context)
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
                    case (int)ItemType.SliderHome:
                    {
                        //Setup your layout here >> SliderHomePageLayout
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.SliderHomePageLayout, parent, false);
                        var vh = new SliderHomePageViewHolder(itemView, OtherVideosOnClick, OtherVideosOnLongClick);
                        return vh;
                    }
                    case (int)ItemType.LiveVideos:
                    case (int)ItemType.Category:
                    case (int)ItemType.TopVideos:
                    case (int)ItemType.LatestVideos:
                    case (int)ItemType.FavVideos:
                    case (int)ItemType.StockVideos:
                    {
                        //Setup your layout here >> TemplateRecyclerViewLayout 
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.TemplateRecyclerViewLayout, parent, false);
                        var vh = new MainVideoAdapterViewHolder(itemView, OnClick, OnLongClick);
                        return vh;
                    }
                    case (int)ItemType.OtherVideos:
                    {
                        //Setup your layout here >> Video_Big_View
                        View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Video_Big_View, parent, false);
                        var vh = new VideoBigAdapterViewHolder(itemView, OtherVideosOnClick, OtherVideosOnLongClick);
                        return vh;
                    }
                    case (int) ItemType.EmptyPage:
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
                var item = MainVideoList[position];
                if (item != null)
                {
                    switch (item.Type)
                    {
                        case ItemType.SliderHome:
                        {
                            if (viewHolder is SliderHomePageViewHolder holder)
                            {
                                if (holder.ViewPagerView.Adapter == null)
                                {
                                    holder.ViewPagerView.Adapter = new ImageCoursalViewPager(ActivityContext, ListUtils.FeaturedVideosList);
                                    holder.ViewPagerView.CurrentItem = 0;
                                }
                                holder.ViewPagerView.Adapter.NotifyDataSetChanged(); 
                            }
                            break;
                        }
                        case ItemType.LiveVideos:
                        case ItemType.Category:
                        case ItemType.TopVideos:
                        case ItemType.LatestVideos:
                        case ItemType.FavVideos:
                        case ItemType.StockVideos:
                        {
                            if (viewHolder is MainVideoAdapterViewHolder holder)
                            {
                                switch (item.Type)
                                {
                                    case ItemType.Category:
                                    {
                                        if (CategoryIconAdapter == null)
                                        {
                                            CategoryIconAdapter = new CategoryIconAdapter(ActivityContext) { CategoryList = new ObservableCollection<Classes.Category>() };
                                            CategoryIconAdapter.ItemClick += CategoryIconAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            holder.MRecycler.SetAdapter(CategoryIconAdapter);
                                        }

                                        if (item.CategoryList.Count > 0)
                                        {
                                            if (CategoryIconAdapter.CategoryList.Count == 0)
                                            {
                                                CategoryIconAdapter.CategoryList = new ObservableCollection<Classes.Category>(item.CategoryList);
                                                CategoryIconAdapter.NotifyDataSetChanged();
                                            }
                                            else if (CategoryIconAdapter.CategoryList != null && CategoryIconAdapter.CategoryList.Count != item.CategoryList.Count)
                                            {
                                                CategoryIconAdapter.CategoryList = new ObservableCollection<Classes.Category>(item.CategoryList);
                                                CategoryIconAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += CategoryMainLinearOnClick;
                                 
                                        holder.MoreText.Visibility = CategoryIconAdapter.CategoryList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Category);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_cat_vector, 0, 0, 0);
                                        break;
                                    }
                                    case ItemType.TopVideos:
                                    {
                                        if (TopVideosAdapter == null)
                                        {
                                            TopVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.TopVideo) { VideoList = new ObservableCollection<VideoObject>() };
                                            TopVideosAdapter.ItemClick += TopVideosAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                            var preLoader = new RecyclerViewPreloader<VideoObject>(ActivityContext, TopVideosAdapter, sizeProvider, 10);
                                            holder.MRecycler.AddOnScrollListener(preLoader);
                                            holder.MRecycler.SetAdapter(TopVideosAdapter);
                                        }
                                 
                                        if (item.TopVideoList.Count > 0)
                                        {
                                            if (TopVideosAdapter.VideoList.Count == 0)
                                            {
                                                TopVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.TopVideoList);
                                                TopVideosAdapter.NotifyDataSetChanged();
                                            }
                                            else if (TopVideosAdapter.VideoList != null && TopVideosAdapter.VideoList.Count != item.TopVideoList.Count)
                                            {
                                                TopVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.TopVideoList);
                                                TopVideosAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += TopVideosMainLinearOnClick;
                                     
                                        holder.MoreText.Visibility = TopVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Top_videos);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_bar_chart_vector, 0, 0, 0);
                                        break;
                                    }
                                    case ItemType.LatestVideos:
                                    {
                                        if (LatestVideosAdapter == null)
                                        {
                                            LatestVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.LatestVideo) { VideoList = new ObservableCollection<VideoObject>() };
                                            LatestVideosAdapter.ItemClick += LatestVideosAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                            var preLoader = new RecyclerViewPreloader<VideoObject>(ActivityContext, LatestVideosAdapter, sizeProvider, 10);
                                            holder.MRecycler.AddOnScrollListener(preLoader);
                                            holder.MRecycler.SetAdapter(LatestVideosAdapter);
                                        }

                                        if (item.LatestVideoList.Count > 0)
                                        {
                                            if (LatestVideosAdapter.VideoList.Count == 0)
                                            {
                                                LatestVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.LatestVideoList);
                                                LatestVideosAdapter.NotifyDataSetChanged();
                                            }
                                            else if (LatestVideosAdapter.VideoList != null && LatestVideosAdapter.VideoList.Count != item.LatestVideoList.Count)
                                            {
                                                LatestVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.LatestVideoList);
                                                LatestVideosAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += LatestVideosMainLinearOnClick;
                                 
                                        holder.MoreText.Visibility = LatestVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Latest_videos);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_video_camera_vector, 0, 0, 0);
                                        break;
                                    }
                                    case ItemType.FavVideos:
                                    {
                                        if (FavVideosAdapter == null)
                                        {
                                            FavVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.FavVideo) { VideoList = new ObservableCollection<VideoObject>() };
                                            FavVideosAdapter.ItemClick += FavVideosAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                            var preLoader = new RecyclerViewPreloader<VideoObject>(ActivityContext, FavVideosAdapter, sizeProvider, 10);
                                            holder.MRecycler.AddOnScrollListener(preLoader);
                                            holder.MRecycler.SetAdapter(FavVideosAdapter);
                                        }

                                        if (item.FavVideoList.Count > 0)
                                        {
                                            if (FavVideosAdapter.VideoList.Count == 0)
                                            {
                                                FavVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.FavVideoList);
                                                FavVideosAdapter.NotifyDataSetChanged();
                                            }
                                            else if (FavVideosAdapter.VideoList != null && FavVideosAdapter.VideoList.Count != item.FavVideoList.Count)
                                            {
                                                FavVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.FavVideoList);
                                                FavVideosAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += FavVideosMainLinearOnClick;

                                        holder.MoreText.Visibility = FavVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Fav_videos);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_star_vector, 0, 0, 0);
                                        break;
                                    }
                                    case ItemType.LiveVideos:
                                    {
                                        if (LiveVideosAdapter == null)
                                        {
                                            LiveVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.LiveVideo) { VideoList = new ObservableCollection<VideoObject>() };
                                            LiveVideosAdapter.ItemClick += LiveVideosAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                            var preLoader = new RecyclerViewPreloader<VideoObject>(ActivityContext, LiveVideosAdapter, sizeProvider, 10);
                                            holder.MRecycler.AddOnScrollListener(preLoader);
                                            holder.MRecycler.SetAdapter(LiveVideosAdapter);
                                        }

                                        if (item.LiveVideoList.Count > 0)
                                        {
                                            if (LiveVideosAdapter.VideoList.Count == 0)
                                            {
                                                LiveVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.LiveVideoList);
                                                LiveVideosAdapter.NotifyDataSetChanged();
                                            }
                                            else if (LiveVideosAdapter.VideoList != null && LiveVideosAdapter.VideoList.Count != item.LiveVideoList.Count)
                                            {
                                                LiveVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.LiveVideoList);
                                                LiveVideosAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += LiveVideosMainLinearOnClick;

                                        holder.MoreText.Visibility = LiveVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_Live);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_live_vector, 0, 0, 0);
                                        holder.TitleText.CompoundDrawableTintList= ColorStateList.ValueOf(Color.Red);
                                        break;
                                    }  
                                    case ItemType.StockVideos:
                                    {
                                        if (StockVideosAdapter == null)
                                        {
                                            StockVideosAdapter = new VideoHorizontalAdapter(ActivityContext, VideoAdapterClickEventArgs.VideoType.StockVideo) { VideoList = new ObservableCollection<VideoObject>() };
                                            StockVideosAdapter.ItemClick += StockVideosAdapterOnItemClick;

                                            LinearLayoutManager layoutManager = new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false);
                                            holder.MRecycler.SetLayoutManager(layoutManager);
                                            holder.MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                                            var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                                            var preLoader = new RecyclerViewPreloader<VideoObject>(ActivityContext, StockVideosAdapter, sizeProvider, 10);
                                            holder.MRecycler.AddOnScrollListener(preLoader);
                                            holder.MRecycler.SetAdapter(StockVideosAdapter);
                                        }

                                        if (item.StockVideoList.Count > 0)
                                        {
                                            if (StockVideosAdapter.VideoList.Count == 0)
                                            {
                                                StockVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.StockVideoList);
                                                StockVideosAdapter.NotifyDataSetChanged();
                                            }
                                            else if (StockVideosAdapter.VideoList != null && StockVideosAdapter.VideoList.Count != item.StockVideoList.Count)
                                            {
                                                StockVideosAdapter.VideoList = new ObservableCollection<VideoObject>(item.StockVideoList);
                                                StockVideosAdapter.NotifyDataSetChanged();
                                            }
                                        }

                                        holder.MainLinear.Visibility = ViewStates.Visible;

                                        if (!holder.MainLinear.HasOnClickListeners)
                                            holder.MainLinear.Click += StockVideosMainLinearOnClick;

                                        holder.MoreText.Visibility = StockVideosAdapter.VideoList?.Count >= 5 ? ViewStates.Visible : ViewStates.Invisible;
                                        holder.TitleText.Text = ActivityContext.GetText(Resource.String.Lbl_StockVideo);
                                        holder.TitleText.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_stock_vector, 0, 0, 0); 
                                        break;
                                    }
                                }
                            }

                            break;
                        }
                        case ItemType.OtherVideos:
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
                                holder.EmptyStateIcon.SetImageResource(Resource.Drawable.ic_no_video_vector);
                                holder.TitleText.Text = Application.Context.GetText(Resource.String.Lbl_Emptyvideos);
                                holder.DescriptionText.Text = Application.Context.GetText(Resource.String.Lbl_No_videos_found_for_now);
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
                        var index = MainVideoList.IndexOf(MainVideoList.FirstOrDefault(a => a.VideoData?.VideoId == item.VideoId));
                        if (index != -1)
                        {
                            MainVideoList.RemoveAt(index);
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


        private void LiveVideosMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("Type", "Live");

                VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
                GlobalContext?.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void StockVideosMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                StockVideosFragment stockVideosFragment = new StockVideosFragment();
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(stockVideosFragment);
                GlobalContext?.VideoActionsController.SetStopvideo(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void FavVideosMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("Type", "Fav");

                VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
                GlobalContext?.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LatestVideosMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("Type", "Latest");

                VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
                GlobalContext?.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TopVideosMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("Type", "Top");

                VideosByTypeFragment videoViewerFragment = new VideosByTypeFragment
                {
                    Arguments = bundle
                };
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
                GlobalContext?.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CategoryMainLinearOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchFragment searchFragment = new SearchFragment();
                GlobalContext?.FragmentBottomNavigator.DisplayFragment(searchFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Open Video from Fav
        private void FavVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = FavVideosAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.StartPlayVideo(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Video from Latest
        private void LatestVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = LatestVideosAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.StartPlayVideo(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void StockVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = StockVideosAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.StartPlayVideo(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        private void LiveVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
        {
            try
            {
                var item = LiveVideosAdapter.VideoList[e.Position];
                if (item?.LiveTime != null && item.LiveTime > 0)
                {
                    //Owner >> ClientRoleBroadcaster , Users >> ClientRoleAudience
                    Intent intent = new Intent(ActivityContext, typeof(LiveStreamingActivity));
                    intent.PutExtra(Constants.KeyClientRole, DT.Xamarin.Agora.Constants.ClientRoleAudience);
                    intent.PutExtra("VideoId", item.Id);
                    intent.PutExtra("StreamName", item.StreamName);
                    intent.PutExtra("PostLiveStream", JsonConvert.SerializeObject(item));
                    ActivityContext.StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Video from Top
        private void TopVideosAdapterOnItemClick(object sender, VideoAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = TopVideosAdapter.GetItem(e.Position);
                if (item == null) return;

                GlobalContext.StartPlayVideo(item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //get video by Category
        private void CategoryIconAdapterOnItemClick(object sender, CategoryIconAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = CategoryIconAdapter.GetItem(e.Position);
                if (item == null) return;

                Bundle bundle = new Bundle();
                bundle.PutString("CatId", item.Id);
                bundle.PutString("CatName", item.Name);

                VideosByCategoryFragment videoViewerFragment = new VideosByCategoryFragment
                {
                    Arguments = bundle
                };

                GlobalContext.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override int ItemCount => MainVideoList?.Count ?? 0;

        public Classes.MainVideoClass GetItem(int position)
        {
            return MainVideoList[position];
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
                var item = MainVideoList[position];
                if (item != null)
                {
                    return item.Type switch
                    {
                        ItemType.SliderHome => (int)ItemType.SliderHome,
                        ItemType.Category => (int)ItemType.Category,
                        ItemType.TopVideos => (int)ItemType.TopVideos,
                        ItemType.LatestVideos => (int)ItemType.LatestVideos,
                        ItemType.LiveVideos => (int)ItemType.LiveVideos,
                        ItemType.FavVideos => (int)ItemType.FavVideos,
                        ItemType.OtherVideos => (int)ItemType.OtherVideos,
                        ItemType.StockVideos => (int)ItemType.StockVideos,
                        ItemType.EmptyPage => (int)ItemType.EmptyPage,  
                        _ => (int)ItemType.EmptyPage
                    };
                }

                return (int)ItemType.EmptyPage;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return (int)ItemType.EmptyPage;
            }
        }

        void OnClick(MainVideoAdapterClickEventArgs args) => ItemClick?.Invoke(ActivityContext, args);
        void OnLongClick(MainVideoAdapterClickEventArgs args) => ItemLongClick?.Invoke(ActivityContext, args);

        void OtherVideosOnClick(VideoAdapterClickEventArgs args) => OtherVideosItemClick?.Invoke(this, args);
        void OtherVideosOnLongClick(VideoAdapterClickEventArgs args) => OtherVideosItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = MainVideoList[p0];

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

    public class MainVideoAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public RelativeLayout MainLinear { get; private set; }
        public TextView TitleText { get; private set; }
        public TextView MoreText { get; private set; }
        public RecyclerView MRecycler { get; private set; }

        #endregion

        public MainVideoAdapterViewHolder(View itemView, Action<MainVideoAdapterClickEventArgs> clickListener, Action<MainVideoAdapterClickEventArgs> longClickListener) : base(itemView)
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
                //itemView.Click += (sender, e) => clickListener(new MainVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new MainVideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }


    public class SliderHomePageViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; set; }
        public ViewPager ViewPagerView { get; private set; }


        #endregion

        public SliderHomePageViewHolder(View itemView, Action<VideoAdapterClickEventArgs> clickListener, Action<VideoAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                ViewPagerView = itemView.FindViewById<ViewPager>(Resource.Id.viewpager2);

                //Create an Event
                //itemView.Click += (sender, e) => clickListener(new VideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition, VideoStyle = VideoAdapterClickEventArgs.VideoType.BigVideo });
                //itemView.LongClick += (sender, e) => longClickListener(new VideoAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }



    public class MainVideoAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}