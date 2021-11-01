using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;

namespace PlayTube.Adapters
{
    public class LibraryAdapter : RecyclerView.Adapter
    {
        public event EventHandler<LibraryAdapterClickEventArgs> ItemClick;
        public event EventHandler<LibraryAdapterClickEventArgs> ItemLongClick;
        private readonly Activity ActivityContext;
        public ObservableCollection<Classes.LibraryItem> LibraryList = new ObservableCollection<Classes.LibraryItem>();

        public LibraryAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                AddLibrarySectionViews();
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
                View itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Library_view, parent, false);
                var vh = new LibraryAdapterViewHolder(itemView, OnClick, OnLongClick);

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
                if (position >= 0)
                {
                    if (viewHolder is LibraryAdapterViewHolder holder)
                    {

                        var item = LibraryList[position];
                        if (item != null)
                        {
                            switch (item.SectionId)
                            {
                                // Subscriptions
                                case "1":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.Checkmark);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Subscriptions);

                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =
                                            item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Watch Later
                                case "2":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.IosClock);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_WatchLater);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Recently Watched 
                                case "3":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.Calendar);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_RecentlyWatched);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Watch Offline 
                                case "4":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.IosCloudDownload);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_WatchOffline);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text = item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // PlayLists
                                case "5":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.IosFilm);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_PlayLists);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Liked
                                case "6":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.HeartEmpty);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Liked);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Shared
                                case "7":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.Share);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Shared);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                                // Paid
                                case "8":
                                {
                                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.SectionIconView, IonIconsFonts.LogoUsd);
                                    holder.SectionTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Paid);
                                    if (item.VideoCount == 0)
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Gone;
                                    }
                                    else
                                    {
                                        holder.SectionVideosCounTextView.Visibility = ViewStates.Visible;
                                        holder.SectionVideosCounTextView.Text =item.VideoCount + " " + ActivityContext.GetText(Resource.String.Lbl_Videos);
                                    }

                                    break;
                                }
                            }

                            if(item.BackgroundImage ==null)
                                return;

                            GlideImageLoader.LoadImage(ActivityContext, item.BackgroundImage, holder.BacgroundImageview, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void Add(Classes.LibraryItem item)
        {
            try
            {
                var check = LibraryList.FirstOrDefault(a => a.SectionId == item.SectionId);
                if (check == null)
                {
                    LibraryList.Add(item);
                    NotifyItemInserted(LibraryList.IndexOf(LibraryList.Last()));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

         
        public Classes.LibraryItem GetItem(int position)
        {
            return LibraryList[position];
        }

         public override int ItemCount => LibraryList?.Count ?? 0;
 

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

        void OnClick(LibraryAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(LibraryAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public void AddLibrarySectionViews()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();
                var check = sqlEntity.Get_LibraryItem();

                if (check != null && check.Count > 0)
                {
                    foreach (var all in check)
                    {
                        Classes.LibraryItem item = new Classes.LibraryItem
                        {
                            SectionId = all.SectionId,
                            SectionText = all.SectionText,
                            VideoCount = all.VideoCount,
                            BackgroundImage = all.BackgroundImage
                        };

                        Add(item);
                        NotifyDataSetChanged();
                    }
                }
                else
                {
                    //translate text in the adapter
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "1",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Subscriptions),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "2",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_WatchLater),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "3",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_RecentlyWatched),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    if (AppSettings.AllowOfflineDownload)
                    {
                        LibraryList.Add(new Classes.LibraryItem
                        {
                            SectionId = "4",
                            SectionText = ActivityContext.GetText(Resource.String.Lbl_WatchOffline),
                            VideoCount = 0,
                            BackgroundImage = "blackdefault"
                        });
                    }
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "5",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_PlayLists),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "6",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Liked),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "7",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Shared),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });
                    LibraryList.Add(new Classes.LibraryItem
                    {
                        SectionId = "8",
                        SectionText = ActivityContext.GetText(Resource.String.Lbl_Paid),
                        VideoCount = 0,
                        BackgroundImage = "blackdefault"
                    });

                    NotifyDataSetChanged();
                    sqlEntity.InsertLibraryItem(LibraryList);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        } 
    }

    public class LibraryAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView BacgroundImageview { get; private set; }
        public TextView SectionTextView { get; private set; }
        public TextView SectionVideosCounTextView { get; private set; }
        public TextView SectionIconView { get; private set; }

        #endregion
        
        public LibraryAdapterViewHolder(View itemView, Action<LibraryAdapterClickEventArgs> clickListener, Action<LibraryAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                BacgroundImageview = (ImageView)MainView.FindViewById(Resource.Id.Imagelibraryvideo);
                SectionTextView = MainView.FindViewById<TextView>(Resource.Id.libraryText);
                SectionIconView = MainView.FindViewById<TextView>(Resource.Id.libraryicon);
                SectionVideosCounTextView = MainView.FindViewById<TextView>(Resource.Id.LibraryVideosCount);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new LibraryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new LibraryAdapterClickEventArgs { View = itemView, Position = BindingAdapterPosition });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }

    public class LibraryAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}