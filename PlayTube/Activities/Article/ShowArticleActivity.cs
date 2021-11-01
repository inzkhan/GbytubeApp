using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.RecyclerView.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Newtonsoft.Json;
using PlayTube.Activities.Base;
using PlayTube.Activities.Comments;
using PlayTube.Activities.Comments.Adapters;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Library.Anjo.Share;
using PlayTube.Library.Anjo.Share.Abstractions;
using PlayTubeClient.Classes.Comment;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Article
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ShowArticleActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView ImageBlog;
        private TextView TxtTitle, TxtViews;
        private WebView TxtHtml;
        private ImageButton BtnMore;
        private ArticleObject ArticleData; 
        private CoordinatorLayout RootView;
        private TextView LoadMore, CategoryName, ClockIcon, DateTimeTextView, LikeIcon, LikeCount, UnlikeIcon, UnlikeNumber;
        private EmojiconEditText EmojIconEditTextView;
        private AppCompatImageView EmojIcon;
        private CircleButton SendButton;
        private RecyclerView MRecycler;
        private LinearLayoutManager MLayoutManager;
        public static CommentsAdapter MAdapter;
        private ProgressBar ProgressBarLoader;
        private View Inflated;
        private ViewStub EmptyStateLayout; 
        private RecyclerViewOnScrollListener MainScrollEvent;
        private string ArticleId, DataWebHtml;
     
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                 
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.ArticlesViewLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataArticles();

                StartApiService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.action_share:
                    ShareEvent();
                    break;

                case Resource.Id.action_copy:
                    CopyLinkEvent();
                    break;

            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater?.Inflate(Resource.Menu.MenuArticleShare, menu);
            ChangeMenuIconColor(menu, Color.White);

            return base.OnCreateOptionsMenu(menu);

        }

        private void ChangeMenuIconColor(IMenu menu, Color color)
        {
            for (int i = 0; i < menu.Size(); i++)
            {
                var drawable = menu.GetItem(i).Icon;
                if (drawable == null) continue;
                drawable.Mutate();
                drawable.SetColorFilter(new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop));
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageBlog = FindViewById<ImageView>(Resource.Id.imageBlog);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtHtml = FindViewById<WebView>(Resource.Id.LocalWebView);
                TxtViews = FindViewById<TextView>(Resource.Id.views);
                BtnMore = FindViewById<ImageButton>(Resource.Id.more);

                LoadMore = FindViewById<TextView>(Resource.Id.LoadMore);
                CategoryName = FindViewById<TextView>(Resource.Id.CategoryName);
                ClockIcon = FindViewById<TextView>(Resource.Id.ClockIcon);
                DateTimeTextView = FindViewById<TextView>(Resource.Id.DateTime);
                LikeIcon = FindViewById<TextView>(Resource.Id.LikeIcon);
                LikeCount = FindViewById<TextView>(Resource.Id.LikeCount);
                UnlikeIcon = FindViewById<TextView>(Resource.Id.UnlikeIcon);
                UnlikeNumber = FindViewById<TextView>(Resource.Id.UnlikeNumber);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ClockIcon, IonIconsFonts.Time);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LikeIcon, IonIconsFonts.ThumbsUp);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, UnlikeIcon, IonIconsFonts.ThumbsDown);

                RootView = FindViewById<CoordinatorLayout>(Resource.Id.root);
                EmojIcon = FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojIconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                ProgressBarLoader = FindViewById<ProgressBar>(Resource.Id.sectionProgress);

                ProgressBarLoader.Visibility = ViewStates.Visible;

                EmojIconActions emojis = new EmojIconActions(this, RootView, EmojIconEditTextView, EmojIcon);
                emojis.ShowEmojIcon(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = "";
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new CommentsAdapter(this);
                MLayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.NestedScrollingEnabled = false;
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener recyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
                MainScrollEvent = recyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                MRecycler.AddOnScrollListener(recyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -= 
                if (addEvent)
                {
                    SendButton.Click += SendButton_Click;
                    MAdapter.ReplyClick += CommentsAdapterOnReplyClick;
                    LoadMore.Click += LoadMore_Click;
                    BtnMore.Click += BtnMoreOnClick;
                }
                else
                {
                    SendButton.Click -= SendButton_Click;
                    MAdapter.ReplyClick -= CommentsAdapterOnReplyClick;
                    LoadMore.Click -= LoadMore_Click;
                    BtnMore.Click -= BtnMoreOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Copy));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", ArticleData.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Text_copied), ToastLength.Short)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = ArticleData.Title,
                    Text = " ",
                    Url = ArticleData.Url
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void CommentsAdapterOnReplyClick(object sender, CommentAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    // show dialog :
                    ReplyCommentBottomSheet replyFragment = new ReplyCommentBottomSheet();
                    Bundle bundle = new Bundle();

                    bundle.PutString("Type", "Article");
                    bundle.PutString("Object", JsonConvert.SerializeObject(item));
                    replyFragment.Arguments = bundle;

                    replyFragment.Show(SupportFragmentManager, replyFragment.Tag);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LoadMore_Click(object sender, EventArgs e)
        {
            try
            {
                ProgressBarLoader.Visibility = ViewStates.Visible;
                StartApiService(MAdapter.CommentList.Last().Id.ToString());
                LoadMore.Visibility = ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    if (!string.IsNullOrEmpty(EmojIconEditTextView.Text))
                    {
                        if (Methods.CheckConnectivity())
                        {
                            if (MAdapter.CommentList.Count == 0)
                            {
                                EmptyStateLayout.Visibility = ViewStates.Gone;
                                MRecycler.Visibility = ViewStates.Visible;
                            }

                            //Comment Code
                            string time = Methods.Time.TimeAgo(DateTime.Now , false);
                            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            string time2 = unixTimestamp.ToString();
                            string message = EmojIconEditTextView.Text;
                            var item = MAdapter.CommentList.FirstOrDefault(a => a.PostId == Convert.ToInt32(ArticleId));
                            var postId = item?.PostId ?? 0;
                            var videoId = item?.VideoId ?? 0;

                            CommentObject comment = new CommentObject
                            {
                                Text = message,
                                TextTime = time,
                                UserId = Convert.ToInt32(UserDetails.UserId),
                                Id = Convert.ToInt32(time2),
                                IsCommentOwner = true,
                                VideoId = videoId,
                                CommentUserData = new UserDataObject
                                {
                                    Avatar = UserDetails.Avatar,
                                    Username = UserDetails.Username,
                                    Name = UserDetails.FullName,
                                    Cover = UserDetails.Cover
                                },
                                CommentReplies = new List<ReplyObject>(),
                                DisLikes = 0,
                                IsDislikedComment = 0,
                                IsLikedComment = 0,
                                Likes = 0,
                                Pinned = "",
                                PostId = postId,
                                RepliesCount = 0,
                                Time = unixTimestamp
                            };

                            MAdapter.CommentList.Add(comment);
                            int index = MAdapter.CommentList.IndexOf(comment);
                            MAdapter.NotifyItemInserted(index);
                            MRecycler.ScrollToPosition(MAdapter.CommentList.IndexOf(MAdapter.CommentList.Last()));

                            //Api request
                            Task.Run(async () =>
                            {
                                var (respondCode, respond) = await RequestsAsync.Articles.AddArticlesCommentAsync(ArticleId, message);
                                if (respondCode.Equals(200))
                                {
                                    if (respond is MessageIdObject messageId)
                                    {
                                        var dataComment = MAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
                                        if (dataComment != null)
                                            dataComment.Id = messageId.Id;
                                    }
                                }
                                else Methods.DisplayReportResult(this, respond);
                            });

                            //Hide keyboard
                            EmojIconEditTextView.Text = "";
                            EmojIconEditTextView.ClearFocus();
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning),GetText(Resource.String.Lbl_Please_sign_in_comment),GetText(Resource.String.Lbl_Yes),GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == GetString(Resource.String.Lbl_Copy))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {

                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Code get last id where LoadMore >>
                MainScrollEvent.IsLoading = true;
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "0")
        {
            if (Methods.CheckConnectivity())
            {
                int countList = MAdapter.CommentList.Count;

                var (apiStatus, respond) = await RequestsAsync.Articles.GetArticlesCommentsAsync(ArticleId, "20", offset);
                if (apiStatus != 200 || respond is not GetCommentsObject result || result.ListComments == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.ListComments.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.ListComments let check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                MAdapter.CommentList.Insert(0, item);
                            }

                            RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommentList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.CommentList = new ObservableCollection<CommentObject>(result.ListComments);
                            RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else if (MAdapter.CommentList.Count > 10 && !MRecycler.CanScrollVertically(1))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short)?.Show();
                    }
                }

                MainScrollEvent.IsLoading = false;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout?.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.CommentList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout?.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x?.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null!;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion


        private void GetDataArticles()
        {
            try
            {
                ArticleData = JsonConvert.DeserializeObject<ArticleObject>(Intent?.GetStringExtra("ItemArticle"));
                if (ArticleData != null)
                {
                    ArticleId = ArticleData.Id.ToString();

                    GlideImageLoader.LoadImage(this, ArticleData.Image, ImageBlog, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    SupportActionBar.Title = Methods.FunString.DecodeString(ArticleData.Title);

                    TxtTitle.Text = Methods.FunString.DecodeString(ArticleData.Title);
                    TxtViews.Text = ArticleData.Views + " " + GetText(Resource.String.Lbl_Views);

                    LikeCount.Text = ArticleData.Likes.ToString();
                    UnlikeNumber.Text = ArticleData.Dislikes.ToString();
                    DateTimeTextView.Text = ArticleData.TextTime;

                    string name = Methods.FunString.DecodeString(CategoriesController.ListCategories?.FirstOrDefault(a => a.Id == ArticleData.Category)?.Name);
                    if (string.IsNullOrEmpty(name))
                        name = GetString(Resource.String.Lbl_Unknown);

                    CategoryName.Text = GetText(Resource.String.Lbl_Category) + " : " + name;

                    string style = AppSettings.SetTabDarkTheme ? "<style type='text/css'>body{color: #fff; background-color: #282828;}</style>" : "<style type='text/css'>body{color: #444; background-color: #FFFEFE;}</style>";
                    string imageFullWidthStyle = "<style>img{display: inline;height: auto;max-width: 100%;}</style>";

                    // This method is deprecated but need to use for old os devices
#pragma warning disable CS0618 // Type or member is obsolete
                    string content = Build.VERSION.SdkInt >= BuildVersionCodes.N ? Html.FromHtml(ArticleData.Text, FromHtmlOptions.ModeCompact)?.ToString() : Html.FromHtml(ArticleData.Text)?.ToString();
#pragma warning restore CS0618 // Type or member is obsolete
                     
                    DataWebHtml = "<!DOCTYPE html>";
                    DataWebHtml += "<head><title></title>" + style + imageFullWidthStyle + "</head>";
                    DataWebHtml += "<body>" + content + "</body>";
                    DataWebHtml += "</html>";

                    // This method is deprecated but need to use for old os devices
                    TxtHtml.SetWebViewClient(new MyWebViewClient(this));
                    TxtHtml.Settings.LoadsImagesAutomatically = true;
                    TxtHtml.Settings.JavaScriptEnabled = true;
                    TxtHtml.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    TxtHtml.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                    TxtHtml.Settings.DomStorageEnabled = true;
                    TxtHtml.Settings.AllowFileAccess = true;
                    TxtHtml.Settings.DefaultTextEncodingName = "utf-8";

                    TxtHtml.Settings.UseWideViewPort = true;
                    TxtHtml.Settings.LoadWithOverviewMode = true;

                    TxtHtml.Settings.SetSupportZoom(false);
                    TxtHtml.Settings.BuiltInZoomControls = false;
                    TxtHtml.Settings.DisplayZoomControls = false;

                    //int fontSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 18, Resources?.DisplayMetrics);
                    //TxtHtml.Settings.DefaultFontSize = fontSize;

                    TxtHtml.LoadDataWithBaseURL(null, DataWebHtml, "text/html", "UTF-8", null);
                     
                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Articles.GetArticleAsync(ArticleId) });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyWebViewClient : WebViewClient
        {
            private readonly ShowArticleActivity Activity;
            public MyWebViewClient(ShowArticleActivity mActivity)
            {
                Activity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                new IntentController(Activity).OpenBrowserFromApp(request.Url.ToString());
                view.LoadDataWithBaseURL(null, Activity.DataWebHtml, "text/html", "UTF-8", null);
                return true;
            }
        }

    }
}   