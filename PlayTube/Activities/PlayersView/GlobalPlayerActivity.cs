using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using MaterialDialogsCore; 
using PlayTube.Activities.Comments;
using PlayTube.Activities.Models;
using PlayTube.Helpers.Controller;
using PlayTubeClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;
using Com.Luseen.Autolinklibrary;
using Newtonsoft.Json;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.MediaPlayers;
using PlayTube.Payment;
using PlayTube.PaymentGoogle;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Math = System.Math;
using Google.Android.Material.AppBar;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Lifecycle;
using Com.Github.Library.Bubbleview;
using Com.Razorpay;
using PlayTube.Activities.Base;
using PlayTubeClient.Classes.Payment;
using YouTubePlayerAndroidX.Player;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace PlayTube.Activities.PlayersView
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode , LaunchMode = LaunchMode.SingleInstance ,ResizeableActivity = true, SupportsPictureInPicture = true)]
    public class GlobalPlayerActivity : BaseActivity, IPaymentResultListener, AppBarLayout.IOnOffsetChangedListener, IYouTubePlayerInitListener, IYouTubePlayerFullScreenListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView ShareIconView, AddToIconView, ShowMoreDescriptionIconView, TextChannelName, EditIconView, TxtSubscribeCount;
        private ImageView ImageChannelView, LikeIconView, UnLikeIconView, ViewMoreCommentSection;
        private Button SubscribeChannelButton;
        private LinearLayout VideoDescriptionLayout, LikeButton, UnLikeButton, ShareButton, AddToButton, EditButton;
        private TextView VideoTitle, VideoLikeCount, VideoUnLikeCount, VideoChannelViews, VideoPublishDate, VideoCategory, UpNextTextview;
        private AutoLinkTextView VideoDescription;
        private TextView SeeMore;
        private TextSanitizer TextSanitizerAutoLink;
        private BubbleLinearLayout SubscribeCountLayout;
        public VideoObject VideoDataHandler; 
        private VideoDataWithEventsLoader.VideoEnumTypes VideoType;
        private LinearLayout DonateButton, RentButton;
        private string TypeDialog, PayType, Price;
        private Switch AutoNextswitch;
        private FrameLayout VideoButtomLayout;
        private RelativeLayout CommentButtomLayout, CircleLayout;
        private bool IsVideoDescriptionExpended;
         
        public CommentsFragment CommentsFragment;
        private NextToFragment NextToFragment;
        public IYouTubePlayer YoutubePlayer { get; private set; }
        public LibrarySynchronizer LibrarySynchronizer;
        public VideoController VideoActionsController;
        private AppBarLayout AppBarLayoutView;
        private CoordinatorLayout CoordinatorLayoutView;
        public YouTubePlayerView TubePlayerView;
        public RestrictedVideoFragment RestrictedVideoPlayerFragment;
        private ThirdPartyPlayersFragment ThirdPartyPlayersFragment;
        private string VideoIdYoutube; 
       
        private static GlobalPlayerActivity Instance;
        private bool OnStopCalled;
        public static bool OnOpenPage;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;
        private InitRazorPayPayment InitRazorPay;
        private InitPayStackPayment PayStackPayment;
        private InitCashFreePayment CashFreePayment;
        private InitPaySeraPayment PaySeraPayment;
         
        private FrameLayout MainVideoRoot;

        public YouTubePlayerEvents YouTubePlayerEvents;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.VideoSliderLayout);

                Instance = this;
                OnStopCalled = false;
                OnOpenPage = true;

                SetVideoPlayerFragmentAdapters();

                var videoObject = Intent?.GetStringExtra("VideoObject");
                if (!string.IsNullOrEmpty(videoObject))
                    VideoDataHandler = JsonConvert.DeserializeObject<VideoObject>(videoObject);

                InitComponent();
                  
                if (VideoDataHandler == null)
                    return;

                LoadVideoData(VideoDataHandler); 
                StartPlayVideo(VideoDataHandler);
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

        protected override void OnStop()
        {
            try
            {
                OnStopCalled = true;
                base.OnStop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                VideoActionsController?.ReleaseVideo();

                TubePlayerView?.Release();

                OnOpenPage = false;

                if (AppSettings.ShowInAppBilling && InitializePlayTube.IsExtended)
                    BillingPayment?.DisconnectInAppBilling();

                if (AppSettings.ShowPaypal)
                    InitPayPalPayment?.StopPayPalService();

                if (AppSettings.ShowRazorPay)
                    InitRazorPay?.StopRazorPay();

                if (AppSettings.ShowPayStack)
                    PayStackPayment?.StopPayStack();

                if (AppSettings.ShowCashFree)
                    CashFreePayment?.StopCashFree();

                if (AppSettings.ShowPaySera)
                    PaySeraPayment?.StopPaySera();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            try
            {
                base.OnNewIntent(intent);

                //Called onNewIntent
                VideoActionsController.SetStopvideo();

                if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                    YoutubePlayer?.Pause();

                var videoObject = Intent?.GetStringExtra("VideoObject");
                if (!string.IsNullOrEmpty(videoObject))
                    VideoDataHandler = JsonConvert.DeserializeObject<VideoObject>(videoObject);

                InitComponent();
                InitBuy();

                if (VideoDataHandler == null)
                    return;

                LoadVideoData(VideoDataHandler);
                StartPlayVideo(VideoDataHandler);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Back Pressed

        private bool IsPipModeEnabled = true;
        public override void OnBackPressed()
        {
            try
            { 
                if (TubePlayerView.FullScreen)
                {
                    TubePlayerView?.ExitFullScreen(); 
                    return;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture) && IsPipModeEnabled)
                {
                    switch (VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            EnterPipMode();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            EnterPipMode();
                            //base.OnBackPressed();
                            break;
                    }
                }
                else
                {
                    base.OnBackPressed();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                base.OnBackPressed();
            }
        }

        private void EnterPipMode()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N && PackageManager.HasSystemFeature(PackageManager.FeaturePictureInPicture))
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        Rational rational = new Rational(450, 250);
                        PictureInPictureParams.Builder builder = new PictureInPictureParams.Builder();
                        builder.SetAspectRatio(rational);
                        EnterPictureInPictureMode(builder.Build());
                    }
                    else
                    {
                        var param = new PictureInPictureParams.Builder().Build();
                        EnterPictureInPictureMode(param);
                    }

                    new Handler(Looper.MainLooper).PostDelayed(CheckPipPermission, 30);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CheckPipPermission()
        {
            IsPipModeEnabled = IsInPictureInPictureMode;
            if (!IsInPictureInPictureMode)
            {
                OnBackPressed();
            }
        }

        private void BackIcon_Click(object sender, EventArgs e)
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var param = new PictureInPictureParams.Builder().Build();
                            EnterPictureInPictureMode(param);
                        }
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        FinishActivityAndTask();
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainVideoRoot = FindViewById<FrameLayout>(Resource.Id.Mainroot);

                CoordinatorLayoutView = FindViewById<CoordinatorLayout>(Resource.Id.parent);
                AppBarLayoutView = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayoutView.AddOnOffsetChangedListener(this);

                LikeIconView = FindViewById<ImageView>(Resource.Id.Likeicon);
                UnLikeIconView = FindViewById<ImageView>(Resource.Id.UnLikeicon);
                ShareIconView = FindViewById<TextView>(Resource.Id.Shareicon);
                AddToIconView = FindViewById<TextView>(Resource.Id.AddToicon);
                EditIconView = FindViewById<TextView>(Resource.Id.editIcon);

                ShowMoreDescriptionIconView = FindViewById<TextView>(Resource.Id.video_ShowDiscription);
                VideoDescriptionLayout = FindViewById<LinearLayout>(Resource.Id.videoDescriptionLayout);
                ImageChannelView = FindViewById<ImageView>(Resource.Id.Image_Channel);
                TextChannelName = FindViewById<TextView>(Resource.Id.ChannelName);
                SubscribeChannelButton = FindViewById<Button>(Resource.Id.SubcribeButton);

                SubscribeCountLayout = FindViewById<BubbleLinearLayout>(Resource.Id.bubble_layout);
                TxtSubscribeCount = FindViewById<TextView>(Resource.Id.subcriberscount);

                LikeButton = FindViewById<LinearLayout>(Resource.Id.LikeButton);
                UnLikeButton = FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
                ShareButton = FindViewById<LinearLayout>(Resource.Id.ShareButton);
                AddToButton = FindViewById<LinearLayout>(Resource.Id.AddToButton);
                EditButton = FindViewById<LinearLayout>(Resource.Id.editButton);

                LikeButton.Tag = "0";
                UnLikeButton.Tag = "0";

                RentButton = FindViewById<LinearLayout>(Resource.Id.RentButton);
                DonateButton = FindViewById<LinearLayout>(Resource.Id.DonateButton);

                RentButton.Visibility = ViewStates.Gone;
                DonateButton.Visibility = ViewStates.Gone;

                VideoTitle = FindViewById<TextView>(Resource.Id.video_Titile);
                VideoLikeCount = FindViewById<TextView>(Resource.Id.LikeNumber);
                VideoUnLikeCount = FindViewById<TextView>(Resource.Id.UnLikeNumber);
                VideoChannelViews = FindViewById<TextView>(Resource.Id.Channelviews);
                VideoPublishDate = FindViewById<TextView>(Resource.Id.videoDate);
                VideoDescription = FindViewById<AutoLinkTextView>(Resource.Id.videoDescriptionTextview);
                SeeMore = FindViewById<TextView>(Resource.Id.seeMoreTextView);
                VideoCategory = FindViewById<TextView>(Resource.Id.videoCategorytextview);

                VideoButtomLayout = FindViewById<FrameLayout>(Resource.Id.videoButtomLayout);
                CommentButtomLayout = FindViewById<RelativeLayout>(Resource.Id.CommentButtomLayout);
                UpNextTextview = FindViewById<TextView>(Resource.Id.UpNextTextview);
                ViewMoreCommentSection = FindViewById<ImageView>(Resource.Id.viewMoreCommentsection);
                AutoNextswitch = FindViewById<Switch>(Resource.Id.AutoNextswitch);
                AutoNextswitch.Checked = UserDetails.AutoNext;

                CircleLayout = FindViewById<RelativeLayout>(Resource.Id.circlelayout);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIconView, IonIconsFonts.ShareAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AddToIconView, IonIconsFonts.AddCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EditIconView, IonIconsFonts.Create);

                TextSanitizerAutoLink = new TextSanitizer(VideoDescription, this);

                VideoActionsController = new VideoController(this, "GlobalPlayer");
                VideoActionsController.ExoBackButton.Click += BackIcon_Click;

                LibrarySynchronizer = new LibrarySynchronizer(this); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitBuy()
        {
            try
            {
                if (AppSettings.ShowInAppBilling && InitializePlayTube.IsExtended)
                    BillingPayment = new InitInAppBillingPayment(this);

                if (AppSettings.ShowPaypal)
                    InitPayPalPayment = new InitPayPalPayment(this);

                if (AppSettings.ShowRazorPay)
                    InitRazorPay = new InitRazorPayPayment(this);

                if (AppSettings.ShowPayStack)
                    PayStackPayment = new InitPayStackPayment(this);

                if (AppSettings.ShowCashFree)
                    CashFreePayment = new InitCashFreePayment(this);

                if (AppSettings.ShowPaySera)
                    PaySeraPayment = new InitPaySeraPayment(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static GlobalPlayerActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    LikeButton.Click += LikeButtonOnClick;
                    UnLikeButton.Click += UnLikeButtonOnClick;
                    ShareButton.Click += ShareButtonOnClick;
                    AddToButton.Click += AddToButtonOnClick;
                    SubscribeChannelButton.Click += SubscribeChannelButtonOnClick;
                    TextChannelName.Click += ImageChannelViewOnClick;
                    VideoCategory.Click += VideoCategoryOnClick;
                    ImageChannelView.Click += ImageChannelViewOnClick;
                    ShowMoreDescriptionIconView.Click += ShowMoreDescriptionIconViewOnClick;
                    EditButton.Click += EditButtonOnClick;
                    RentButton.Click += RentButtonOnClick;
                    DonateButton.Click += DonateButtonOnClick;
                    CommentButtomLayout.Click += CommentButtomLayout_Click;
                    ViewMoreCommentSection.Click += ViewMoreCommentSectionClick;
                    UpNextTextview.Click += ViewMoreCommentSectionClick;
                    AutoNextswitch.CheckedChange += AutoNextswitchOnCheckedChange;
                    SeeMore.Click += OnVideoDescriptionClick; 
                }
                else
                {
                    LikeButton.Click -= LikeButtonOnClick;
                    UnLikeButton.Click -= UnLikeButtonOnClick;
                    ShareButton.Click -= ShareButtonOnClick;
                    AddToButton.Click -= AddToButtonOnClick;
                    SubscribeChannelButton.Click -= SubscribeChannelButtonOnClick;
                    TextChannelName.Click -= ImageChannelViewOnClick;
                    VideoCategory.Click -= VideoCategoryOnClick;
                    ImageChannelView.Click -= ImageChannelViewOnClick;
                    ShowMoreDescriptionIconView.Click -= ShowMoreDescriptionIconViewOnClick;
                    EditButton.Click -= EditButtonOnClick;
                    RentButton.Click -= RentButtonOnClick;
                    DonateButton.Click -= DonateButtonOnClick;
                    CommentButtomLayout.Click -= CommentButtomLayout_Click;
                    ViewMoreCommentSection.Click -= ViewMoreCommentSectionClick;
                    UpNextTextview.Click -= ViewMoreCommentSectionClick;
                    AutoNextswitch.CheckedChange -= AutoNextswitchOnCheckedChange;
                    SeeMore.Click -= OnVideoDescriptionClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
         
        private void OnVideoDescriptionClick(object sender, EventArgs e)
        {
            try
            {
                IsVideoDescriptionExpended = !IsVideoDescriptionExpended;
                ToggleVideoDescription();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ToggleVideoDescription()
        {
            try
            {
                if (IsVideoDescriptionExpended)
                {
                    VideoDescription.SetSingleLine(false);
                    VideoDescription.Ellipsize = null!;
                }
                else
                {
                    VideoDescription.SetLines(2);
                    VideoDescription.Ellipsize = TextUtils.TruncateAt.End;
                }

                ChangeSeeMoreOption();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ChangeSeeMoreOption()
        {
            try
            {
                VideoDescription?.Post(() =>
                {
                    try
                    {
                        var lineCount = VideoDescription.LineCount;
                        SeeMore.Visibility = lineCount > 2 ? ViewStates.Visible : ViewStates.Gone;

                        var id = IsVideoDescriptionExpended ? Resource.String.Lbl_See_Less : Resource.String.Lbl_See_More;
                        SeeMore.Text = GetText(id);
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra("Open", "EditVideo");
                intent.PutExtra("ItemDataVideo", JsonConvert.SerializeObject(VideoDataHandler));
                SetResult(Result.Ok, intent);

                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        VideoActionsController.SetStopvideo();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        YoutubePlayer.Pause();
                        break;
                }

                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AutoNextswitchOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.AutoNext = AutoNextswitch.Checked;
                MainSettings.AutoNext?.Edit()?.PutBoolean(MainSettings.PrefKeyAutoNext, UserDetails.AutoNext)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentButtomLayout_Click(object sender, EventArgs e)
        {
            try
            {
                UpNextTextview.Text = GetString(Resource.String.Lbl_Comments);
                UpNextTextview.Tag = GetString(Resource.String.Lbl_Comments);
                ViewMoreCommentSection.Visibility = ViewStates.Visible;
                AutoNextswitch.Visibility = ViewStates.Gone;
                CommentButtomLayout.Visibility = ViewStates.Gone;
                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();

                if (!CommentsFragment.IsAdded)
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.AddToBackStack(null);
                    ftvideo.Add(VideoButtomLayout.Id, CommentsFragment, null)?.Commit();
                }
                else
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.Hide(NextToFragment).Show(CommentsFragment)?.Commit();
                }

                CommentsFragment.StartApiService(VideoDataHandler.Id, "0");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ViewMoreCommentSectionClick(object sender, EventArgs e)
        {
            try
            {
                HideCommentsAndShowNextTo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void HideCommentsAndShowNextTo()
        {
            try
            {
                UpNextTextview.Text = GetString(Resource.String.Lbl_NextTo);
                UpNextTextview.Tag = GetString(Resource.String.Lbl_NextTo);
                ViewMoreCommentSection.Visibility = ViewStates.Gone;
                AutoNextswitch.Visibility = ViewStates.Visible;
                CommentButtomLayout.Visibility = ViewStates.Visible;
                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
                ftvideo.AddToBackStack(null);
                ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                ftvideo.Hide(CommentsFragment).Show(NextToFragment)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private void AddToButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "AddTo";

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                var arrayAdapter = new List<string> { GetString(Resource.String.Lbl_Addto_playlist) };

                var check = ListUtils.WatchLaterVideosList.FirstOrDefault(q => q.Videos?.VideoAdClass.Id == VideoDataHandler.Id);
                arrayAdapter.Add(check == null ? GetString(Resource.String.Lbl_Addto_watchlater) : GetString(Resource.String.Lbl_RemoveFromWatchLater));

                dialogList.Title(GetString(Resource.String.Lbl_Add_To));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShareButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                LibrarySynchronizer?.ShareVideo(VideoDataHandler);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UnLikeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        if (UnLikeButton.Tag?.ToString() == "0")
                        {
                            UnLikeButton.Tag = "1";
                            UnLikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                            LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));

                            if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                            {
                                var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                x++;
                                VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                            }

                            if (LikeButton.Tag?.ToString() == "1")
                            {
                                LikeButton.Tag = "0";
                                if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                    if (x > 0)
                                    {
                                        x--;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }

                                    VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                }
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short)?.Show();

                            //Send API Request here for dislike
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoDataHandler.Id, "dislike") });
                        }
                        else
                        {
                            UnLikeButton.Tag = "0";


                            UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                            var x = Convert.ToDouble(VideoUnLikeCount.Text);
                            if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                            {
                                if (x > 0)
                                {
                                    x--;
                                }
                                else
                                {
                                    x = 0;
                                }
                                VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                            }

                            //Send API Request here for Remove UNLike
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoDataHandler.Id, "dislike") });

                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Dislike), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        try
                        {
                            //If User Liked
                            if (LikeButton.Tag?.ToString() == "0")
                            {
                                LikeButton.Tag = "1";
                                LikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));


                                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                if (!VideoLikeCount.Text.Contains("K") && !VideoLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoLikeCount.Text);
                                    x++;
                                    VideoLikeCount.Text = x.ToString(CultureInfo.InvariantCulture);
                                }

                                if (UnLikeButton.Tag?.ToString() == "1")
                                {
                                    UnLikeButton.Tag = "0";
                                    if (!VideoUnLikeCount.Text.Contains("K") && !VideoUnLikeCount.Text.Contains("M"))
                                    {
                                        var x = Convert.ToDouble(VideoUnLikeCount.Text);
                                        if (x > 0)
                                        {
                                            x--;
                                        }
                                        else
                                        {
                                            x = 0;
                                        }
                                        VideoUnLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                    }
                                }


                                //AddToLiked
                                LibrarySynchronizer.AddToLiked(VideoDataHandler);

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short)?.Show();

                                //Send API Request here for Like
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoDataHandler.Id, "like") });

                            }
                            else
                            {
                                LikeButton.Tag = "0";

                                LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                                if (!VideoLikeCount.Text.Contains("K") && !VideoLikeCount.Text.Contains("M"))
                                {
                                    var x = Convert.ToDouble(VideoLikeCount.Text);
                                    if (x > 0)
                                    {
                                        x--;
                                    }
                                    else
                                    {
                                        x = 0;
                                    }

                                    VideoLikeCount.Text = x.ToString(CultureInfo.CurrentCulture);
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short)?.Show();

                                //Send API Request here for Remove UNLike
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.AddLikeDislikeVideosAsync(VideoDataHandler.Id, "like") });
                            }
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Like), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VideoCategoryOnClick(object sender, EventArgs e)
        {
            try
            {

                Intent intent = new Intent();
                intent.PutExtra("Open", "VideosByCategory");
                intent.PutExtra("CatId", VideoDataHandler.CategoryId);
                intent.PutExtra("CatName", VideoDataHandler.CategoryName);
                SetResult(Result.Ok, intent);

                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        VideoActionsController.SetStopvideo();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        YoutubePlayer.Pause();
                        break;
                }
                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SubscribeChannelButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        if (!string.IsNullOrEmpty(VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice) && VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice != "0")
                        {
                            if (SubscribeChannelButton.Tag?.ToString() == "PaidSubscribe")
                            {
                                TypeDialog = "PaidSubscribe";

                                //This channel is paid, You must pay to subscribe
                                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                                dialog.Title(Resource.String.Lbl_Warning);
                                dialog.Content(GetText(Resource.String.Lbl_ChannelIsPaid));
                                dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                                dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                                dialog.AlwaysCallSingleChoiceCallback();
                                dialog.Build().Show();
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Remove The Video to Subscribed Videos Database
                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.RemoveSubscriptionsChannel(VideoDataHandler.Owner?.OwnerClass?.Id);

                                //Send API Request here for UnSubscribed
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoDataHandler.Owner?.OwnerClass?.Id) });

                                // Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
                            }
                        }
                        else
                        {
                            if (SubscribeChannelButton.Tag?.ToString() == "Subscribe")
                            {
                                SubscribeChannelButton.Tag = "Subscribed";
                                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Add The Video to  Subcribed Videos Database
                                Events_Insert_SubscriptionsChannel();

                                //Send API Request here for Subcribe
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoDataHandler.Owner?.OwnerClass?.Id) });

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Remove The Video to Subcribed Videos Database
                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.RemoveSubscriptionsChannel(VideoDataHandler.Owner?.OwnerClass?.Id);

                                //Send API Request here for UnSubcribe
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoDataHandler.Owner?.OwnerClass?.Id) });

                                // Toast.MakeText(this, this.GetText(Resource.String.Lbl_Channel_Removed_successfully, ToastLength.Short)?.Show();
                            }
                        }
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_Subcribed), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ImageChannelViewOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent();
                intent.PutExtra("Open", "UserProfile");
                intent.PutExtra("UserObject", JsonConvert.SerializeObject(VideoDataHandler.Owner));
                SetResult(Result.Ok, intent);

                FinishActivityAndTask();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShowMoreDescriptionIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (VideoDescriptionLayout.Tag?.ToString() == "Open")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);
                    VideoDescriptionLayout.Visibility = ViewStates.Gone;
                    VideoDescriptionLayout.Tag = "closed";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                }
                else
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowUp);
                    VideoDescriptionLayout.Visibility = ViewStates.Visible;
                    VideoDescriptionLayout.Tag = "Open";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> WatchLater
        private async void OnMenuAddWatchLaterClick(VideoObject videoObject)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Video.AddToWatchLaterVideosAsync(VideoDataHandler.Id);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageCodeObject result)
                            {
                                if (result.SuccessType.Contains("Removed"))
                                {
                                    LibrarySynchronizer.RemovedFromWatchLater(videoObject);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_RemovedFromWatchLater), ToastLength.Short)?.Show();
                                }
                                else if (result.SuccessType.Contains("Added"))
                                {
                                    LibrarySynchronizer.AddToWatchLater(videoObject);
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_AddedToWatchLater), ToastLength.Short)?.Show();
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, videoObject, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_WatchLater), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Event Menu >> Playlist
        private void OnMenuAddPlaylistClick(VideoObject videoObject)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (UserDetails.IsLogin)
                    {
                        PopupDialogController dialog = new PopupDialogController(this, videoObject, "PlayList");
                        dialog.ShowPlayListDialog();
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(this, videoObject, "Login");
                        dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Please_sign_in_playlist), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void Events_Insert_SubscriptionsChannel()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                if (VideoDataHandler.Owner != null)
                    sqlEntity.Insert_One_SubscriptionChannel(VideoDataHandler.Owner?.OwnerClass);

                LibrarySynchronizer.AddToSubscriptions(VideoDataHandler.Owner?.OwnerClass);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Rent
        private void RentButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.ShowInAppBilling && InitializePlayTube.IsExtended && AppSettings.VideoRentalPriceStatic)
                {
                    BillingPayment?.SetConnInAppBilling();
                    BillingPayment?.InitInAppBilling("RentVideo");
                }
                else
                {
                    TypeDialog = "Payment_RentVideo";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                    if (AppSettings.ShowPaypal)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_Paypal));

                    if (AppSettings.ShowCreditCard)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                    if (AppSettings.ShowBankTransfer)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                    if (AppSettings.ShowRazorPay)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));

                    if (AppSettings.ShowPayStack)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));

                    if (AppSettings.ShowCashFree)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));

                    if (AppSettings.ShowPaySera)
                        arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));

                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Donate
        private void DonateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(VideoDataHandler.Owner?.OwnerClass?.DonationPaypalEmail))
                {
                    var url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=" + VideoDataHandler.Owner?.OwnerClass?.DonationPaypalEmail + "&lc=US&item_name=Donation+to+" + VideoDataHandler.Owner?.OwnerClass?.Name + "&no_note=0&cn=&currency_code=USD&bn=PP-DonationsBF:btn_donateCC_LG.gif:NonHosted";
                    new IntentController(this).OpenBrowserFromApp(url);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region PictureInPictur

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                var percentage = (float)Math.Abs(verticalOffset) / appBarLayout.TotalScrollRange;
                Console.WriteLine(percentage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPictureInPictureModeChanged(bool isInPictureInPictureMode, Configuration newConfig)
        {
            try
            {
                CoordinatorLayoutView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;

                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (VideoActionsController?.ControlView != null)
                            VideoActionsController.ControlView.Visibility = isInPictureInPictureMode ? ViewStates.Gone : ViewStates.Visible;
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        if (isInPictureInPictureMode)
                            TubePlayerView?.PlayerUiController.ShowUi(false);
                        else
                            TubePlayerView.PlayerUiController.ShowUi(true);
                        break;
                }
                 
                if (isInPictureInPictureMode)
                {
                    // ...
                    switch (VideoType)
                    {
                        case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                            //VideoActionsController?.SetStopvideo();
                            break;
                        case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                            //YoutubePlayer.Play();
                            break;
                    }
                }
                else
                {
                    if (OnStopCalled)
                    {
                        switch (VideoType)
                        {
                            case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                                VideoActionsController?.SetStopvideo();
                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                                YoutubePlayer.Pause();
                                TubePlayerView.PlayerUiController.ShowUi(true);
                                break;
                        }

                        FinishActivityAndTask();
                    }
                }

                base.OnPictureInPictureModeChanged(isInPictureInPictureMode, newConfig);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnUserLeaveHint()
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        {
                            var param = new PictureInPictureParams.Builder().Build();
                            EnterPictureInPictureMode(param);
                        }
                        base.OnUserLeaveHint();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region YouTube Player

        //public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult p1)
        //{
        //    try
        //    {
        //        if (AppSettings.DisableYouTubeInitializationFailureMessages)
        //            return;

        //        if (p1.IsUserRecoverableError)
        //            p1.GetErrorDialog(this, 1).Show();
        //        else
        //            Toast.MakeText(this, p1.ToString(), ToastLength.Short)?.Show();
        //    }
        //    catch (Exception e)
        //    {
        //        Methods.DisplayReportResultTrack(e);
        //    }
        //}

        public void OnInitSuccess(IYouTubePlayer player)
        {
            try
            {
                if (YoutubePlayer == null)
                {
                    YoutubePlayer = player;
                    YouTubePlayerEvents = new YouTubePlayerEvents(player, VideoIdYoutube);
                    YoutubePlayer.AddListener(YouTubePlayerEvents);
                    TubePlayerView.AddFullScreenListener(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerEnterFullScreen()
        {
            try
            {
                Intent intent = new Intent(this, typeof(YouTubePlayerFullScreenActivity)); 
                intent.PutExtra("VideoIdYoutube", VideoIdYoutube);
                intent.PutExtra("CurrentSecond", YouTubePlayerEvents.CurrentSecond);
                StartActivityForResult(intent, 2100);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnYouTubePlayerExitFullScreen()
        {
            try
            {

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Fun Video 

        public readonly List<Fragment> VideoFrameLayoutFragments = new List<Fragment>();

        private void SetVideoPlayerFragmentAdapters()
        {
            try
            {
                CommentsFragment = new CommentsFragment();
                NextToFragment = new NextToFragment();

                FragmentTransaction ftvideo = SupportFragmentManager.BeginTransaction();
                ftvideo.Add(Resource.Id.videoButtomLayout, NextToFragment, NextToFragment.Tag)?.Commit();

                TubePlayerView = FindViewById<YouTubePlayerView>(Resource.Id.youtube_player_view);
                if (TubePlayerView != null)
                {
                    TubePlayerView.Visibility = ViewStates.Gone;

                    // The player will automatically release itself when the activity is destroyed.
                    // The player will automatically pause when the activity is paused
                    // If you don't add YouTubePlayerView as a lifecycle observer, you will have to release it manually.
                    Lifecycle.AddObserver(TubePlayerView);

                    TubePlayerView.PlayerUiController.ShowMenuButton(false);
                    //TubePlayerView.PlayerUiController.Menu.AddItem(new MenuItem("example", Resource.Drawable.icon_settings_vector, (view)->Toast.makeText(this, "item clicked", Toast.LENGTH_SHORT).show()));
                    TubePlayerView.Initialize(this, true);
                }
                if (ThirdPartyPlayersFragment == null)
                {
                    FragmentTransaction ft1 = SupportFragmentManager.BeginTransaction();
                    ThirdPartyPlayersFragment = new ThirdPartyPlayersFragment();
                    ft1.Add(Resource.Id.root, ThirdPartyPlayersFragment, ThirdPartyPlayersFragment.Tag)?.Commit();

                    if (!VideoFrameLayoutFragments.Contains(ThirdPartyPlayersFragment))
                        VideoFrameLayoutFragments.Add(ThirdPartyPlayersFragment);
                }
                if (RestrictedVideoPlayerFragment == null)
                {
                    FragmentTransaction ft2 = SupportFragmentManager.BeginTransaction();
                    RestrictedVideoPlayerFragment = new RestrictedVideoFragment();
                    ft2.Add(Resource.Id.root, RestrictedVideoPlayerFragment, RestrictedVideoPlayerFragment.Tag)?.Commit();

                    if (!VideoFrameLayoutFragments.Contains(RestrictedVideoPlayerFragment))
                        VideoFrameLayoutFragments.Add(RestrictedVideoPlayerFragment);
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadVideoData(VideoObject videoObject)
        {
            try
            {
                if (videoObject == null)
                    return;

                VideoDataHandler = videoObject;
                SetVideoType(VideoDataHandler);

                //Run fast data fetch from the server 
                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetVideosInfoAsJson(videoObject.VideoId) });

                GlideImageLoader.LoadImage(this, videoObject.Owner?.OwnerClass?.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);
                TextChannelName.Text = videoObject.Owner?.OwnerClass?.Username;
                VideoLikeCount.Text = videoObject.LikesPercent;
                VideoUnLikeCount.Text = videoObject.DislikesPercent;
                VideoChannelViews.Text = videoObject.Views + " " + GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
                VideoPublishDate.Text = GetText(Resource.String.Lbl_Published_on) + " " + Methods.Time.ConvertToSpanishFormatIfNeeded(videoObject.TimeDate);
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);

                if (videoObject.Owner != null && string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscribeCount))
                    videoObject.Owner.Value.OwnerClass.SubscribeCount = "0";

                TxtSubscribeCount.Text = videoObject.Owner?.OwnerClass?.SubscribeCount;

                TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));

                EditButton.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Visible : ViewStates.Gone;

                //Reset Views
                LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                LikeButton.Tag = "0";
                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                UnLikeButton.Tag = "0";
                VideoLikeCount.Text = "0";
                VideoUnLikeCount.Text = "0";

                SubscribeChannelButton.Tag = "Subscribe";
                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);
                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                icon.Bounds = new Rect(10, 10, 10, 7);
                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                //Close the description panel
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);

                VideoDescriptionLayout.Tag = "closed";
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);

                //Clear all data 
                if (CommentsFragment != null && CommentsFragment.MAdapter != null)
                {
                    CommentsFragment.MAdapter.CommentList?.Clear();
                    NextToFragment.MAdapter.VideoList?.Clear();
                    CommentsFragment.MAdapter.NotifyDataSetChanged();
                    NextToFragment.MAdapter.NotifyDataSetChanged();
                    CommentsFragment.StartApiService(videoObject.Id, "0");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetVideoType(VideoObject videoObject)
        {
            try
            {
                VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Normal;
                var myDetails = ListUtils.MyChannelList?.FirstOrDefault();
                var age = string.IsNullOrWhiteSpace(myDetails?.Age) ? 0 : Convert.ToInt32(myDetails.Age);
                var isBelow18 = age > 0 && age < 18;

                if (videoObject.VideoLocation.Contains("Youtube") || videoObject.VideoLocation.Contains("youtu") || videoObject.VideoType == "VideoObject/youtube")
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Youtube;
                else if (!string.IsNullOrEmpty(videoObject.Vimeo))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Vimeo;
                else if (!string.IsNullOrEmpty(videoObject.Daily))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.DailyMotion;
                else if (!string.IsNullOrEmpty(videoObject.Ok))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Ok;
                else if (!string.IsNullOrEmpty(videoObject.Twitch))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Twitch;
                else if (!string.IsNullOrEmpty(videoObject.Facebook))
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.Facebook;
                else if (videoObject.IsOwner != null && videoObject.AgeRestriction == "2" && videoObject.IsOwner.Value == false && isBelow18)
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.AgeRestricted;
                else if (!string.IsNullOrEmpty(videoObject.GeoBlocking) && videoObject.IsOwner == false)
                    VideoType = VideoDataWithEventsLoader.VideoEnumTypes.GeoBlocked;

                videoObject.VideoType = VideoType.ToString();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task GetVideosInfoAsJson(string videoId)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Video.GetVideosDetailsAsync(videoId, UserDetails.AndroidId);
                if (apiStatus == 200)
                {
                    if (respond is GetVideosDetailsObject result)
                    {
                        var updater = ListUtils.GlobalViewsVideosList.FirstOrDefault(a => a.VideoId == videoId);
                        if (updater != null)
                        {
                            ListUtils.GlobalViewsVideosList.Add(updater);
                            SetNewDataVideo(updater);
                        }
                        else
                        {
                            ListUtils.GlobalViewsVideosList.Add(result.DataResult);
                            SetNewDataVideo(result.DataResult);
                        }

                        result.DataResult.SuggestedVideos = AppTools.ListFilter(result.DataResult.SuggestedVideos);
                        ListUtils.ArrayListPlay = new ObservableCollection<VideoObject>(result.DataResult.SuggestedVideos);
                        NextToFragment.LoadDataAsync(new ObservableCollection<VideoObject>(result.DataResult.SuggestedVideos));

                        if (ListUtils.AdsVideoList.Count > 0)
                        {
                            if (result.DataResult.VideoAd.VideoAdClass != null)
                                ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
                        }
                        else
                        {
                            ListUtils.AdsVideoList = new ObservableCollection<VideoAdDataObject>();

                            if (result.DataResult.VideoAd.VideoAdClass != null)
                                ListUtils.AdsVideoList.Add(result.DataResult.VideoAd.VideoAdClass);
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void SetNewDataVideo(VideoObject videoObject)
        {
            try
            {
                if (videoObject == null)
                    return;

                VideoDataHandler = videoObject;
                SetVideoType(VideoDataHandler);

                VideoChannelViews.Text = videoObject.Views + " " + GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);

                if (videoObject.IsLiked == "1") // true
                {

                    LikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    LikeButton.Tag = "1";
                }
                else
                {

                    LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    LikeButton.Tag = "0";
                }

                if (videoObject.IsDisliked == "1") // true
                {
                    LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    UnLikeIconView.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    UnLikeButton.Tag = "1";
                }
                else
                {
                    UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                    UnLikeButton.Tag = "0";
                }


                var isOwner = videoObject.IsOwner != null && videoObject.IsOwner.Value;
                SubscribeChannelButton.Visibility = isOwner && AppSettings.HideSubscribeForOwner ? ViewStates.Invisible : ViewStates.Visible;
                SubscribeCountLayout.Visibility = isOwner && AppSettings.HideSubscribeForOwner ? ViewStates.Invisible : ViewStates.Visible;
                EditButton.Visibility = isOwner ? ViewStates.Visible : ViewStates.Gone;

                VideoLikeCount.Text = videoObject.Likes;
                VideoUnLikeCount.Text = videoObject.Dislikes;
                VideoPublishDate.Text = GetText(Resource.String.Lbl_Published_on) + " " + Methods.Time.ConvertToSpanishFormatIfNeeded(videoObject.TimeDate);
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                //Verified 
                TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                TextChannelName.Text = AppTools.GetNameFinal(videoObject.Owner?.OwnerClass);
                GlideImageLoader.LoadImage(this, videoObject.Owner?.OwnerClass?.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (videoObject.Owner != null && string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscribeCount))
                    videoObject.Owner.Value.OwnerClass.SubscribeCount = "0";

                TxtSubscribeCount.Text = videoObject.Owner?.OwnerClass?.SubscribeCount;

                //Rent
                if (!string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem)
                {
                    RentButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    RentButton.Visibility = ViewStates.Gone;
                }

                //Donate
                if (!string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.DonationPaypalEmail) && AppSettings.DonateVideosSystem)
                {
                    DonateButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    DonateButton.Visibility = ViewStates.Gone;
                }

                var file = VideoDownloadAsyncController.GetDownloadedDiskVideoUri(videoObject.VideoId);
                if (!string.IsNullOrEmpty(file))
                {
                    VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                    VideoActionsController.DownloadIcon.Tag = "Downloaded";
                    LibrarySynchronizer.AddToWatchOffline(videoObject);
                }

                if (videoObject.Owner?.OwnerClass?.Id != UserDetails.UserId)
                {
                    UserDataObject channel = await ApiRequest.GetChannelData(this, videoObject.Owner?.OwnerClass?.Id);
                    if (channel != null)
                    {
                        videoObject.Owner = channel;

                        if (!string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscriberPrice) && videoObject.Owner?.OwnerClass?.SubscriberPrice != "0")
                        {
                            if (videoObject.Owner?.OwnerClass?.IsSubscribedToChannel == "0")
                            {
                                //This channel is paid, You must pay to subscribe
                                SubscribeChannelButton.Tag = "PaidSubscribe";

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                                var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                                Console.WriteLine(currency);
                                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice;
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribed";

                                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                            }
                        }
                        else
                        {
                            SubscribeChannelButton.Tag = videoObject.Owner?.OwnerClass?.IsSubscribedToChannel == "0" ? "Subscribe" : "Subscribed";

                            switch (SubscribeChannelButton.Tag?.ToString())
                            {
                                case "Subscribed":
                                {
                                    SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

                                    //Color
                                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                    //icon
                                    Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                                    icon.Bounds = new Rect(10, 10, 10, 7);
                                    SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                                    break;
                                }
                                case "Subscribe":
                                {
                                    SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribe);

                                    //Color
                                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                    //icon
                                    Drawable icon = GetDrawable(Resource.Drawable.SubcribeButton);
                                    icon.Bounds = new Rect(10, 10, 10, 7);
                                    SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                                    break;
                                }
                            }
                        }

                        //Verified 
                        TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                    }
                }
                else
                {
                    UserDataObject channel = ListUtils.MyChannelList?.FirstOrDefault();
                    if (channel == null) return;
                    videoObject.Owner = channel;
                    //Verified 
                    TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void StartPlayVideo(VideoObject videoObject)
        {
            try
            {
                await Task.Delay(1500);

                RestrictedVideoPlayerFragment?.HideRestrictedInfo(true);
                UpdateMainRootDefaultSize();

                if (videoObject != null)
                {
                    VideoDataHandler = videoObject;

                    if (ListUtils.ArrayListPlay.Count > 0)
                        ListUtils.ArrayListPlay.Remove(videoObject);

                    LoadVideoData(videoObject);
                    HideCommentsAndShowNextTo();

                    VideoActionsController.ExoBackButton.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                    VideoActionsController.ExoBackButton.Tag = "Open";

                    var userNotVideoOwner = videoObject.IsOwner != null && !videoObject.IsOwner.Value;
                    if (userNotVideoOwner &&
                        (!string.IsNullOrEmpty(videoObject.SellVideo) && videoObject.SellVideo != "0"
                         || !string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem
                         || !string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscriberPrice) && videoObject.Owner?.OwnerClass?.SubscriberPrice != "0" & videoObject.Owner?.OwnerClass?.IsSubscribedToChannel == "0"))
                    {
                        GlobalVideosRelease("All");
                        CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, SupportFragmentManager, VideoFrameLayoutFragments);

                        if (!string.IsNullOrEmpty(videoObject.SellVideo) && videoObject.SellVideo != "0")
                            RestrictedVideoPlayerFragment.LoadRestriction("purchaseVideo", videoObject.Thumbnail, videoObject);
                        else if (!string.IsNullOrEmpty(videoObject.RentPrice) && videoObject.RentPrice != "0" && AppSettings.RentVideosSystem)
                            RestrictedVideoPlayerFragment.LoadRestriction("RentVideo", videoObject.Thumbnail, videoObject);
                        else if (!string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscriberPrice) && videoObject.Owner?.OwnerClass?.SubscriberPrice != "0" & videoObject.Owner?.OwnerClass?.IsSubscribedToChannel == "0")
                            RestrictedVideoPlayerFragment.LoadRestriction("Subscriber", videoObject.Thumbnail, videoObject);
                    }
                    else
                    {
                        switch (VideoType)
                        {
                            case VideoDataWithEventsLoader.VideoEnumTypes.AgeRestricted:
                                GlobalVideosRelease("All");
                                CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                                RestrictedVideoPlayerFragment.LoadRestriction("AgeRestriction", videoObject.Thumbnail, videoObject);
                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.GeoBlocked:
                                GlobalVideosRelease("All");
                                CustomNavigationController.BringFragmentToTop(RestrictedVideoPlayerFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                                RestrictedVideoPlayerFragment.LoadRestriction("GeoRestriction", videoObject.Thumbnail, videoObject);
                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                                VideoIdYoutube = videoObject.VideoLocation.Split(new[] { "v=" }, StringSplitOptions.None).LastOrDefault();
                                GlobalVideosRelease("Youtube");
                                CustomNavigationController.BringFragmentToTop(null, SupportFragmentManager, VideoFrameLayoutFragments);

                                if (Lifecycle.CurrentState == Lifecycle.State.Resumed)
                                    YoutubePlayer.LoadVideo(VideoIdYoutube, 0);
                                else
                                    YoutubePlayer.CueVideo(VideoIdYoutube, 0);

                                break;
                            case VideoDataWithEventsLoader.VideoEnumTypes.Facebook:
                            case VideoDataWithEventsLoader.VideoEnumTypes.Twitch:
                            case VideoDataWithEventsLoader.VideoEnumTypes.DailyMotion:
                            case VideoDataWithEventsLoader.VideoEnumTypes.Ok:
                            case VideoDataWithEventsLoader.VideoEnumTypes.Vimeo:
                                GlobalVideosRelease("All");
                                CustomNavigationController.BringFragmentToTop(ThirdPartyPlayersFragment, SupportFragmentManager, VideoFrameLayoutFragments);
                                ThirdPartyPlayersFragment.SetVideoIframe(videoObject);
                                break;
                            default:
                                GlobalVideosRelease("exo");
                                CustomNavigationController.BringFragmentToTop(null, SupportFragmentManager, VideoFrameLayoutFragments);
                                VideoActionsController.PlayVideo(videoObject.VideoLocation, videoObject, RestrictedVideoPlayerFragment, this);
                                break;
                        }
                    }

                    TabbedMainActivity.GetInstance()?.SetOnWakeLock();
                    LibrarySynchronizer.AddToRecentlyWatched(videoObject);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void UpdateMainRootDefaultSize()
        {
            try
            {
                if (AppSettings.ShowVideoWithDynamicHeight)
                {
                    var p = MainVideoRoot.LayoutParameters;
                    p.Width = GetScreenWidth();
                    p.Height = VideoActionsController.IsPortraitVideo && VideoActionsController.PortraitHeight > 0 ? VideoActionsController.PortraitHeight : CovertDpToPixel(220);
                    MainVideoRoot.RequestLayout();
                    VideoActionsController?.FullWidthSetting();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void UpdateMainRootDefaultLandscapeSize()
        {
            try
            {
                var p = MainVideoRoot.LayoutParameters;
                p.Width = GetScreenWidth();
                p.Height = CovertDpToPixel(220);
                MainVideoRoot.RequestLayout();
                VideoActionsController?.SetFullWidthDefaultSetting();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private int GetScreenWidth()
        {
            try
            {
                DisplayMetrics displayMetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
                int width = displayMetrics.WidthPixels;
                return width;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        public int CovertDpToPixel(int dp)
        {
            try
            {
                var displayMetrics = new DisplayMetrics();
                WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
                return (int)(dp * displayMetrics.Density);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        private void GlobalVideosRelease(string exepttype)
        {
            try
            {
                switch (exepttype)
                {
                    case "exo":
                    {
                        if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                            YoutubePlayer?.Pause();

                        if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Gone;

                        if (VideoActionsController.SimpleExoPlayerView.Visibility == ViewStates.Gone)
                            VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Visible;
                        break;
                    }
                    case "Youtube":
                    {
                        VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                        VideoActionsController.ReleaseVideo();

                        if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Visible;

                        break;
                    }
                    case "All":
                    {
                        if (YoutubePlayer != null && YouTubePlayerEvents.IsPlaying)
                            YoutubePlayer?.Pause();
                        if (TubePlayerView != null) TubePlayerView.Visibility = ViewStates.Gone;

                        VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Gone;
                        VideoActionsController.ReleaseVideo();
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "AddTo" when itemString == GetString(Resource.String.Lbl_Addto_playlist):
                        OnMenuAddPlaylistClick(VideoDataHandler);
                        break;
                    case "AddTo":
                    {
                        if (itemString == GetString(Resource.String.Lbl_Addto_watchlater) || itemString == GetString(Resource.String.Lbl_RemoveFromWatchLater))
                        {
                            OnMenuAddWatchLaterClick(VideoDataHandler);
                        }

                        break;
                    }
                    case "Payment_RentVideo":
                    {
                        PayType = "RentVideo";

                        Price = AppSettings.VideoRentalPriceStatic && AppSettings.VideoRentalPrice > 0
                            ? AppSettings.VideoRentalPrice.ToString()
                            : VideoDataHandler.RentPrice;

                        if (itemString == GetString(Resource.String.Lbl_Paypal))
                        {
                            InitPayPalPayment?.BtnPaypalOnClick(Price, "RentVideo");
                        }
                        else if (itemString == GetString(Resource.String.Lbl_CreditCard))
                        {
                            Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                            intent.PutExtra("Price", Price);
                            intent.PutExtra("payType", "RentVideo");
                            intent.PutExtra("Id", VideoDataHandler.Id);
                            StartActivity(intent);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_BankTransfer))
                        {
                            Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                            intent.PutExtra("Id", VideoDataHandler.Id);
                            intent.PutExtra("Price", Price);
                            intent.PutExtra("payType", "RentVideo");
                            StartActivity(intent);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_RazorPay))
                        {
                            InitRazorPay?.BtnRazorPayOnClick(Price, "RentVideo", VideoDataHandler.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PayStack))
                        {
                            TypeDialog = "PayStack";

                            var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await PayStack(s.ToString(), Price, "RentVideo", VideoDataHandler.Id);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                            dialogBuilder.PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                            dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        }
                        else if (itemString == GetString(Resource.String.Lbl_CashFree))
                        {
                            OpenCashFreeDialog(Price, "RentVideo", VideoDataHandler.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await PaySera(Price, "RentVideo", VideoDataHandler.Id);
                        }

                        break;
                    } 
                    case "Payment_PaidSubscribe":
                    {
                        PayType = "SubscriberVideo";
                        Price = VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice;

                        if (itemString == GetString(Resource.String.Lbl_Paypal))
                        {
                            InitPayPalPayment?.BtnPaypalOnClick(VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice, "SubscriberVideo");
                        }
                        else if (itemString == GetString(Resource.String.Lbl_CreditCard))
                        {
                            Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                            intent.PutExtra("Price", VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice);
                            intent.PutExtra("payType", "SubscriberVideo");
                            StartActivity(intent);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_BankTransfer))
                        {
                            Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                            intent.PutExtra("Id", VideoDataHandler.Owner?.OwnerClass?.Id);
                            intent.PutExtra("Price", VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice);
                            intent.PutExtra("payType", "SubscriberVideo");
                            StartActivity(intent);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_RazorPay))
                        {
                            InitRazorPay?.BtnRazorPayOnClick(VideoDataHandler.Owner?.OwnerClass?.SubscriberPrice, "SubscriberVideo", VideoDataHandler.Owner?.OwnerClass?.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PayStack))
                        {
                            TypeDialog = "PayStack";

                            var dialogBuilder = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await PayStack(s.ToString(), Price, "SubscriberVideo", VideoDataHandler.Owner?.OwnerClass?.Id);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                            dialogBuilder.PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                            dialogBuilder.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        }
                        else if (itemString == GetString(Resource.String.Lbl_CashFree))
                        {
                            OpenCashFreeDialog(Price, "SubscriberVideo", VideoDataHandler.Owner?.OwnerClass?.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await PaySera(Price, "SubscriberVideo", VideoDataHandler.Owner?.OwnerClass?.Id);
                        }

                        break;
                    }
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
                if (TypeDialog == "PaidSubscribe")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        TypeDialog = "Payment_PaidSubscribe";

                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                        if (AppSettings.ShowPaypal)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_Paypal));

                        if (AppSettings.ShowCreditCard)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                        if (AppSettings.ShowBankTransfer)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                        if (AppSettings.ShowRazorPay)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_RazorPay));

                        if (AppSettings.ShowPayStack)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_PayStack));

                        if (AppSettings.ShowCashFree)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CashFree));

                        if (AppSettings.ShowPaySera)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_PaySera));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetString(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnPaymentError(int code, string response)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                Toast.MakeText(this, "Payment failed: " + response, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnPaymentSuccess(string razorpayPaymentId)
        {
            try
            {
                Console.WriteLine("razorpay : Payment Successful:" + razorpayPaymentId);

                if (!string.IsNullOrEmpty(razorpayPaymentId))
                {
                    if (Methods.CheckConnectivity())
                    {
                        switch (PayType)
                        {
                            case "RentVideo":
                            {
                                var keyValues = new Dictionary<string, string>
                                {
                                    {"merchant_amount", Price},
                                    {"video_id", VideoDataHandler.Id},
                                    {"pay_type", "rent"},
                                };

                                var (apiStatus, respond) = await RequestsAsync.Payment.RazorPay(razorpayPaymentId, "buy_video", keyValues);
                                if (apiStatus == 200)
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    //StartApiService(); 
                                }
                                else Methods.DisplayReportResult(this, respond);

                                break;
                            }
                            case "SubscriberVideo":
                            {
                                var keyValues = new Dictionary<string, string>
                                {
                                    {"merchant_amount",Price},
                                    {"subscribe_id", VideoDataHandler.Owner?.OwnerClass?.Id},
                                };

                                var (apiStatus, respond) = await RequestsAsync.Payment.RazorPay(razorpayPaymentId, "subscribe", keyValues);
                                if (apiStatus == 200)
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    //StartApiService(); 
                                }
                                else Methods.DisplayReportResult(this, respond);

                                break;
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string email, string price, string payType, string id)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var priceInt = Convert.ToInt32(price) * 100;

                    var keyValues = new Dictionary<string, string>
                    {
                        {"email", email},
                        {"amount", priceInt.ToString()},
                    };

                    switch (payType)
                    {
                        case "RentVideo":
                        {
                            keyValues.Add("video_id", id);
                            keyValues.Add("pay_type", "rent");

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePayStack("buy_video", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    PayStackPayment ??= new InitPayStackPayment(this);
                                    PayStackPayment.DisplayPayStackPayment(result.Url, "rent", priceInt.ToString(), id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePayStack("subscribe", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    PayStackPayment ??= new InitPayStackPayment(this);
                                    PayStackPayment.DisplayPayStackPayment(result.Url, "check_subscribe", priceInt.ToString(), id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private EditText TxtName, TxtEmail, TxtPhone;
        private void OpenCashFreeDialog(string price, string payType, string id)
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light)
                    .Title(GetText(Resource.String.Lbl_CashFree))
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                                return;
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text, price, payType, id);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    })
                    .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                    .Build();

                var iconName = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconName);
                TxtName = dialog.CustomView.FindViewById<EditText>(Resource.Id.NameEditText);

                var iconEmail = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = dialog.CustomView.FindViewById<EditText>(Resource.Id.EmailEditText);

                var iconPhone = dialog.CustomView.FindViewById<TextView>(Resource.Id.IconPhone);
                TxtPhone = dialog.CustomView.FindViewById<EditText>(Resource.Id.PhoneEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconName, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconEmail, FontAwesomeIcon.PaperPlane);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, iconPhone, FontAwesomeIcon.Mobile);

                Methods.SetColorEditText(TxtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPhone, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var local = ListUtils.MyChannelList?.FirstOrDefault();
                if (local != null)
                {
                    TxtName.Text = AppTools.GetNameFinal(local);
                    TxtEmail.Text = local.Email;
                    TxtPhone.Text = local.PhoneNumber;
                }

                dialog.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(string name, string email, string phone, string price, string payType, string id)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"name", name},
                        {"phone", phone},
                        {"email", email},
                        {"amount", price},
                    };

                    switch (payType)
                    {
                        case "RentVideo":
                        {
                            keyValues.Add("video_id", id);
                            keyValues.Add("pay_type", "rent");

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializeCashFree("buy_video", AppSettings.CashFreeCurrency, AppSettings.CashfreeSecretKey, AppSettings.CashfreeMode, keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is CashFreeObject result)
                                {
                                    CashFreePayment ??= new InitCashFreePayment(this);
                                    CashFreePayment.DisplayCashFreePayment(result, "rent", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializeCashFree("subscribe", AppSettings.CashFreeCurrency, AppSettings.CashfreeSecretKey, AppSettings.CashfreeMode, keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is CashFreeObject result)
                                {
                                    CashFreePayment ??= new InitCashFreePayment(this);
                                    CashFreePayment.DisplayCashFreePayment(result, "check_subscribe", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PaySera(string price, string payType, string id)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"amount", price},
                    };

                    switch (payType)
                    {
                        case "RentVideo":
                        {
                            keyValues.Add("video_id", id);
                            keyValues.Add("pay_type", "rent");

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePaySera("buy_video", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    PaySeraPayment ??= new InitPaySeraPayment(this);
                                    PaySeraPayment.DisplayPaySeraPayment(result.Url, "rent", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePaySera("subscribe", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    PaySeraPayment ??= new InitPaySeraPayment(this);
                                    PaySeraPayment.DisplayPaySeraPayment(result.Url, "subscribe", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    case 2000 when resultCode == Result.Ok:
                    {
                        VideoActionsController.RestartPlayAfterShrinkScreen();
                        break;
                    }
                    case 2100 when resultCode == Result.Ok:
                    {
                        TubePlayerView?.ExitFullScreen();
                        break;
                    }
                    case InitPayPalPayment.PayPalDataRequestCode:
                        switch (resultCode)
                        {
                            case Result.Ok:
                                var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                                PaymentConfirmation configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                                if (configuration != null)
                                {
                                    switch (PayType)
                                    {
                                        //string createTime = configuration.ProofOfPayment.CreateTime;
                                        //string intent = configuration.ProofOfPayment.Intent;
                                        //string paymentId = configuration.ProofOfPayment.PaymentId;
                                        //string state = configuration.ProofOfPayment.State;
                                        //string transactionId = configuration.ProofOfPayment.TransactionId;
                                        case "SubscriberVideo" when Methods.CheckConnectivity():
                                            SetSubscribeChannelWithPaid();
                                            break;
                                        case "SubscriberVideo":
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                            break;
                                        case "RentVideo" when Methods.CheckConnectivity():
                                        {
                                            var (apiStatus, respond) = await RequestsAsync.Video.RentVideoAsync(VideoDataHandler.Id);
                                            if (apiStatus == 200)
                                            {
                                                Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();

                                            }
                                            else Methods.DisplayReportResult(this, respond);

                                            break;
                                        }
                                        case "RentVideo":
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                            break;
                                        case "DonateVideo":
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_DonatedSuccessfully), ToastLength.Long)?.Show();
                                            break;
                                    }
                                }
                                break;
                            case Result.Canceled:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                                break;
                        }
                        break;
                    default:
                        switch (requestCode)
                        {
                            case PaymentActivity.ResultExtrasInvalid:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long)?.Show();
                                break;
                            case 1001 when resultCode == Result.Ok:
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    var (apiStatus, respond) = await RequestsAsync.Video.RentVideoAsync(VideoDataHandler.Id);
                                    if (apiStatus == 200)
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }

                                break;
                            }
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        public async void SetSubscribeChannelWithPaid()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    SubscribeChannelButton.Tag = "Subscribed";
                    SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Subscribed);

                    //Color
                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                    //icon
                    Drawable icon = GetDrawable(Resource.Drawable.SubcribedButton);
                    icon.Bounds = new Rect(10, 10, 10, 7);
                    SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                    //Add The Video to  Subscribed Videos Database
                    Events_Insert_SubscriptionsChannel();

                    //Send API Request here for Subscribed
                    var (apiStatus, respond) = await RequestsAsync.Global.AddSubscribeToChannelAsync(VideoDataHandler.Owner?.OwnerClass?.Id, "paid");
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void FinishActivityAndTask()
        {
            try
            {
                switch (VideoType)
                {
                    case VideoDataWithEventsLoader.VideoEnumTypes.Normal:
                        VideoActionsController.ReleaseVideo();
                        //MoveTaskToBack(true);
                        FinishAndRemoveTask();
                        Finish();
                        break;
                    case VideoDataWithEventsLoader.VideoEnumTypes.Youtube:
                        YoutubePlayer?.Pause();
                        Finish();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
    }
}