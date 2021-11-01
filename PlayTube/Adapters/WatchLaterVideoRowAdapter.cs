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
using Bumptech.Glide;
using Bumptech.Glide.Request;
using PlayTube.Activities.Models;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using Exception = System.Exception;
using Java.Util;
using PlayTubeClient.Classes.Video;
using IList = System.Collections.IList;
using AndroidX.RecyclerView.Widget;

namespace PlayTube.Adapters
{
    public class WatchLaterVideoRowAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<WatchLaterVideoRowAdapterClickEventArgs> ItemClick;
        public event EventHandler<WatchLaterVideoRowAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<DataWatchLaterVideos> VideoList = new ObservableCollection<DataWatchLaterVideos>();
        private readonly LibrarySynchronizer LibrarySynchronizer;
        private readonly string Type;

        public WatchLaterVideoRowAdapter(Activity context , string type = "Normal")
        {
            HasStableIds = true;
            ActivityContext = context;
            LibrarySynchronizer = new LibrarySynchronizer(context);
            Type = type;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Video_Row_View
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Video_Row_View, parent, false);
                var vh = new WatchLaterVideoRowAdapterViewHolder(itemView, OnClick, OnLongClick);

                return vh;
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
                if (viewHolder is WatchLaterVideoRowAdapterViewHolder holder)
                {
                    var item = VideoList[position];
                    if (item?.Videos != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Videos?.VideoAdClass.Thumbnail, holder.VideoImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                          
                        holder.TxtDuration.Text = Methods.Time.SplitStringDuration(item.Videos?.VideoAdClass.Duration);
                        holder.TxtTitle.Text = Methods.FunString.DecodeString(item.Videos?.VideoAdClass.Title);

                        holder.TxtChannelName.Text = AppTools.GetNameFinal(item.Videos?.VideoAdClass.Owner?.OwnerClass);

                        holder.TxtViewsCount.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(item.Videos?.VideoAdClass.Views)) + " " + ActivityContext.GetText(Resource.String.Lbl_Views);

                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.MenueView, IonIconsFonts.More);
                             
                        //Verified 
                        if (item.Videos?.VideoAdClass.Owner?.OwnerClass?.Verified == "1")
                        {
                            holder.IconVerified.Visibility = ViewStates.Visible;
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.IconVerified, IonIconsFonts.CheckmarkCircle);
                        }
                        else
                        {
                            holder.IconVerified.Visibility = ViewStates.Gone;
                        }

                        if (!holder.MenueView.HasOnClickListeners)
                            holder.MenueView.Click += (sender, args) =>
                            {
                                ShowPopup(holder.MenueView, item);
                            };

                        //Set Badge on videos
                        AppTools.ShowGlobalBadgeSystem(holder.VideoType, item.Videos?.VideoAdClass);

                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void ShowPopup(View v, DataWatchLaterVideos item)
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

                if (item.Videos.Value.VideoAdClass.IsOwner != null && item.Videos.Value.VideoAdClass.IsOwner.Value)
                {
                    menuRemove.Visibility = ViewStates.Gone;
                    menuReport.Visibility = ViewStates.Invisible;
                }

                menuAddWatchLater.Click += (sender, args) =>
                {
                    try
                    {
                        LibrarySynchronizer.AddToWatchLater(item.Videos.Value.VideoAdClass);
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
                        LibrarySynchronizer.AddToPlaylist(item.Videos.Value.VideoAdClass);
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
                        var index = VideoList.IndexOf(VideoList.FirstOrDefault(a => a.VideoId == item.VideoId));
                        if (index != -1)
                        {
                            VideoList.Remove(item);
                            NotifyItemRemoved(index);

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Removed), ToastLength.Short)?.Show();

                            var data = ListUtils.GlobalNotInterestedList.FirstOrDefault(a => a.VideoId == item.VideoId);
                            if (data == null)
                            {
                                ListUtils.GlobalNotInterestedList.Add(item.Videos?.VideoAdClass);
                            }

                            var sqlEntity = new SqLiteDatabase();
                            sqlEntity.Insert_NotInterestedVideos(item.Videos?.VideoAdClass);
                        }
                        popupWindow.Dismiss();
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
                        LibrarySynchronizer.ShareVideo(item.Videos?.VideoAdClass);
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
                        LibrarySynchronizer.AddReportVideo(item.Videos?.VideoAdClass);
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
         
       public override int ItemCount => VideoList?.Count ?? 0;
        
        public DataWatchLaterVideos GetItem(int position)
        {
            return VideoList[position];
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
                return position;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }
        void OnClick(WatchLaterVideoRowAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(WatchLaterVideoRowAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (ActivityContext?.IsDestroyed != false)
                    return;

                if (holder is WatchLaterVideoRowAdapterViewHolder viewHolder)
                {
                    Glide.With(ActivityContext).Clear(viewHolder.VideoImage);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = VideoList[p0];

                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Videos?.VideoAdClass.Thumbnail != "")
                {
                    d.Add(item.Videos?.VideoAdClass.Thumbnail);
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
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CenterCrop());
        }
    }

    public class WatchLaterVideoRowAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView VideoImage { get; private set; }
        public TextView TxtDuration { get; private set; }
        public TextView TxtTitle { get; private set; }
        public TextView TxtChannelName { get; private set; }
        public TextView TxtViewsCount { get; private set; }
        public TextView IconVerified { get; private set; }
        public TextView MenueView { get; private set; }

        public TextView VideoType { get; private set; }

        #endregion

        public WatchLaterVideoRowAdapterViewHolder(View itemView, Action<WatchLaterVideoRowAdapterClickEventArgs> clickListener,Action<WatchLaterVideoRowAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                VideoImage = (ImageView)MainView.FindViewById(Resource.Id.Imagevideo);
                TxtDuration = MainView.FindViewById<TextView>(Resource.Id.duration);
                TxtTitle = MainView.FindViewById<TextView>(Resource.Id.Title);
                TxtChannelName = MainView.FindViewById<TextView>(Resource.Id.ChannelName);
                TxtViewsCount = MainView.FindViewById<TextView>(Resource.Id.Views_Count);
                IconVerified = MainView.FindViewById<TextView>(Resource.Id.IconVerified);
                MenueView = MainView.FindViewById<TextView>(Resource.Id.videoMenue);

                VideoType = MainView.FindViewById<TextView>(Resource.Id.videoType);
                //Create an Event
                itemView.Click += (sender, e) => clickListener(new WatchLaterVideoRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new WatchLaterVideoRowAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class WatchLaterVideoRowAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}