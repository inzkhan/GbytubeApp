using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using Com.Sothree.Slidinguppanel;
using Newtonsoft.Json;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Payment;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Video;
using PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Com.Github.Library.Bubbleview;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.SettingsPreferences;
using PlayTubeClient.Classes.Payment;
using Exception = System.Exception;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace PlayTube.Activities.Models
{
    public class VideoDataWithEventsLoader : Java.Lang.Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback 
    {
        private readonly TabbedMainActivity ActivityContext;
        private TextView ShareIconView, AddToIconView, ShowMoreDescriptionIconView, TextChannelName, EditIconView, TxtSubscribeCount;
        private ImageView ImageChannelView, LikeIconView, UnLikeIconView, ViewMoreCommentSection;
        private Button SubscribeChannelButton;
        private LinearLayout VideoDescriptionLayout, LikeButton, UnLikeButton, ShareButton, AddToButton, EditButton;
        private TextView VideoTitle, VideoLikeCount, VideoUnLikeCount, VideoChannelViews, VideoPublishDate, VideoCategory, UpNextTextview;
        private AutoLinkTextView VideoDescription;
        private TextView SeeMore;
        private TextSanitizer TextSanitizerAutoLink;
        private BubbleLinearLayout SubscribeCountLayout;
        private VideoObject VideoDataHandler;
        private readonly Activity Context;
        public VideoEnumTypes VideoType; 
        private LinearLayout DonateButton, RentButton;
        private string TypeDialog;
        private Switch AutoNextswitch;
        private FrameLayout VideoButtomLayout;
        private RelativeLayout CommentButtomLayout, CircleLayout;
        private bool IsVideoDescriptionExpended;

        public enum VideoEnumTypes
        {
            Youtube, Normal, DailyMotion, Vimeo, Ok, Twitch, Facebook, AgeRestricted, GeoBlocked
        }

        public VideoDataWithEventsLoader(Activity activityContext)
        {
            Context = activityContext;
            ActivityContext = (TabbedMainActivity)activityContext;
        }

        #region Functions

        public void SetViews()
        {
            try
            {
                IsVideoDescriptionExpended = false;

                LikeIconView = ActivityContext.FindViewById<ImageView>(Resource.Id.Likeicon);
                UnLikeIconView = ActivityContext.FindViewById<ImageView>(Resource.Id.UnLikeicon);
                ShareIconView = ActivityContext.FindViewById<TextView>(Resource.Id.Shareicon);
                AddToIconView = ActivityContext.FindViewById<TextView>(Resource.Id.AddToicon);
                EditIconView = ActivityContext.FindViewById<TextView>(Resource.Id.editIcon);

                ShowMoreDescriptionIconView = ActivityContext.FindViewById<TextView>(Resource.Id.video_ShowDiscription);
                VideoDescriptionLayout = ActivityContext.FindViewById<LinearLayout>(Resource.Id.videoDescriptionLayout);
                ImageChannelView = ActivityContext.FindViewById<ImageView>(Resource.Id.Image_Channel);
                TextChannelName = ActivityContext.FindViewById<TextView>(Resource.Id.ChannelName);
                SubscribeChannelButton = ActivityContext.FindViewById<Button>(Resource.Id.SubcribeButton);

                SubscribeCountLayout = ActivityContext.FindViewById<BubbleLinearLayout>(Resource.Id.bubble_layout);
                TxtSubscribeCount = ActivityContext.FindViewById<TextView>(Resource.Id.subcriberscount);

                LikeButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.LikeButton);
                UnLikeButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.UnLikeButton);
                ShareButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.ShareButton);
                AddToButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.AddToButton);
                EditButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.editButton);

                LikeButton.Tag = "0";
                UnLikeButton.Tag = "0";

                RentButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.RentButton);
                DonateButton = ActivityContext.FindViewById<LinearLayout>(Resource.Id.DonateButton);

                RentButton.Visibility = ViewStates.Gone;
                DonateButton.Visibility = ViewStates.Gone;

                VideoTitle = ActivityContext.FindViewById<TextView>(Resource.Id.video_Titile);
                VideoLikeCount = ActivityContext.FindViewById<TextView>(Resource.Id.LikeNumber);
                VideoUnLikeCount = ActivityContext.FindViewById<TextView>(Resource.Id.UnLikeNumber);
                VideoChannelViews = ActivityContext.FindViewById<TextView>(Resource.Id.Channelviews);
                VideoPublishDate = ActivityContext.FindViewById<TextView>(Resource.Id.videoDate);
                VideoDescription = ActivityContext.FindViewById<AutoLinkTextView>(Resource.Id.videoDescriptionTextview);
                SeeMore = ActivityContext.FindViewById<TextView>(Resource.Id.seeMoreTextView);
                VideoCategory = ActivityContext.FindViewById<TextView>(Resource.Id.videoCategorytextview);

                VideoButtomLayout = ActivityContext.FindViewById<FrameLayout>(Resource.Id.videoButtomLayout);
                CommentButtomLayout = ActivityContext.FindViewById<RelativeLayout>(Resource.Id.CommentButtomLayout);
                UpNextTextview = ActivityContext.FindViewById<TextView>(Resource.Id.UpNextTextview);
                ViewMoreCommentSection = ActivityContext.FindViewById<ImageView>(Resource.Id.viewMoreCommentsection);
                AutoNextswitch = ActivityContext.FindViewById<Switch>(Resource.Id.AutoNextswitch);
                AutoNextswitch.Checked = UserDetails.AutoNext;

                CircleLayout = ActivityContext.FindViewById<RelativeLayout>(Resource.Id.circlelayout);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIconView, IonIconsFonts.ShareAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AddToIconView, IonIconsFonts.AddCircle);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, EditIconView, IonIconsFonts.Create);

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

                TextSanitizerAutoLink = new TextSanitizer(VideoDescription, Context, ActivityContext);
                ToggleVideoDescription();
                FragmentTransaction ftvideo = ActivityContext.SupportFragmentManager.BeginTransaction();
                ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                ftvideo.AddToBackStack(null);
                ftvideo.Add(VideoButtomLayout.Id, ActivityContext.CommentsFragment, null).Hide(ActivityContext.CommentsFragment)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

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
                        SeeMore.Text = ActivityContext.GetText(id);
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
                UpNextTextview.Text = ActivityContext.GetString(Resource.String.Lbl_Comments);
                UpNextTextview.Tag = ActivityContext.GetString(Resource.String.Lbl_Comments);
                ViewMoreCommentSection.Visibility = ViewStates.Visible;
                AutoNextswitch.Visibility = ViewStates.Gone;
                CommentButtomLayout.Visibility = ViewStates.Gone;
                FragmentTransaction ftvideo = ActivityContext.SupportFragmentManager.BeginTransaction();

                if (!ActivityContext.CommentsFragment.IsAdded)
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.AddToBackStack(null);
                    ftvideo.Add(VideoButtomLayout.Id, ActivityContext.CommentsFragment, null)?.Commit();
                }
                else
                {
                    ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                    ftvideo.Hide(ActivityContext.NextToFragment).Show(ActivityContext.CommentsFragment)?.Commit();
                }

                ActivityContext.CommentsFragment.StartApiService(VideoDataHandler.Id, "0");
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
                UpNextTextview.Text = ActivityContext.GetString(Resource.String.Lbl_NextTo);
                UpNextTextview.Tag = ActivityContext.GetString(Resource.String.Lbl_NextTo);
                ViewMoreCommentSection.Visibility = ViewStates.Gone;
                AutoNextswitch.Visibility = ViewStates.Visible;
                CommentButtomLayout.Visibility = ViewStates.Visible;
                FragmentTransaction ftvideo = ActivityContext.SupportFragmentManager.BeginTransaction();
                ftvideo.AddToBackStack(null);
                ftvideo.SetCustomAnimations(Resource.Animation.slide_up, Resource.Animation.slide_down);
                ftvideo.Hide(ActivityContext.CommentsFragment).Show(ActivityContext.NextToFragment)?.Commit();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DonateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(VideoDataHandler.Owner?.OwnerClass?.DonationPaypalEmail))
                {
                    var url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=" + VideoDataHandler.Owner?.OwnerClass?.DonationPaypalEmail + "&lc=US&item_name=Donation+to+" + VideoDataHandler.Owner?.OwnerClass?.Name + "&no_note=0&cn=&currency_code=USD&bn=PP-DonationsBF:btn_donateCC_LG.gif:NonHosted";
                    new IntentController(ActivityContext).OpenBrowserFromApp(url);
                } 
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
                    ActivityContext.PaymentVideoObject = VideoDataHandler;

                    ActivityContext.BillingPayment.SetConnInAppBilling();
                    ActivityContext.BillingPayment.InitInAppBilling("RentVideo");
                }
                else
                {
                    TypeDialog = "Payment_RentVideo";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                    if (AppSettings.ShowPaypal)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_Paypal));

                    if (AppSettings.ShowCreditCard)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CreditCard));

                    if (AppSettings.ShowBankTransfer)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_BankTransfer));

                    if (AppSettings.ShowRazorPay)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_RazorPay));

                    if (AppSettings.ShowPayStack)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_PayStack));

                    if (AppSettings.ShowCashFree)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CashFree));

                    if (AppSettings.ShowPaySera)
                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_PaySera));

                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(ActivityContext.GetString(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
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
                Bundle bundle = new Bundle();
                bundle.PutString("ItemDataVideo", JsonConvert.SerializeObject(VideoDataHandler));

                var editVideoFragment = new EditVideoFragment
                {
                    Arguments = bundle
                };

                ActivityContext.FragmentBottomNavigator.DisplayFragment(editVideoFragment);

                switch (VideoType)
                {
                    case VideoEnumTypes.Normal:
                        ActivityContext.VideoActionsController.SetStopvideo();
                        break;
                    case VideoEnumTypes.Youtube:
                        ActivityContext.YoutubePlayer.Pause();
                        break;
                }

                if (ActivityContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                    ActivityContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
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

                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                var arrayAdapter = new List<string> { ActivityContext.GetString(Resource.String.Lbl_Addto_playlist) };

                var check = ListUtils.WatchLaterVideosList.FirstOrDefault(q => q.Videos?.VideoAdClass.Id == VideoDataHandler.Id);
                arrayAdapter.Add(check == null ? ActivityContext.GetString(Resource.String.Lbl_Addto_watchlater) : ActivityContext.GetString(Resource.String.Lbl_RemoveFromWatchLater));

                dialogList.Title(ActivityContext.GetString(Resource.String.Lbl_Add_To));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(ActivityContext.GetString(Resource.String.Lbl_Close)).OnNegative(this);
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
                ActivityContext?.LibrarySynchronizer?.ShareVideo(VideoDataHandler);
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

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_UnLiked), ToastLength.Short)?.Show();

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
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_Dislike), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
                                ActivityContext.LibrarySynchronizer.AddToLiked(VideoDataHandler);

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Video_Liked), ToastLength.Short)?.Show();

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

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Remove_Video_Liked), ToastLength.Short)?.Show();

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
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_Like), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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
                Bundle bundle = new Bundle();
                bundle.PutString("CatId", VideoDataHandler.CategoryId);
                bundle.PutString("CatName", VideoDataHandler.CategoryName);

                var videoViewerFragment = new VideosByCategoryFragment
                {
                    Arguments = bundle
                };

                ActivityContext.FragmentBottomNavigator.DisplayFragment(videoViewerFragment);


                switch (VideoType)
                {
                    case VideoEnumTypes.Normal:
                        ActivityContext.VideoActionsController.SetStopvideo();
                        break;
                    case VideoEnumTypes.Youtube:
                        ActivityContext.YoutubePlayer.Pause();
                        break;
                }

                if (ActivityContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                    ActivityContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
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
                                //This channel is paid, You must pay to subscribe
                                ActivityContext.OpenDialog(VideoDataHandler.Owner?.OwnerClass);
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
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
                        else
                        {
                            if (SubscribeChannelButton.Tag?.ToString() == "Subscribe")
                            {
                                SubscribeChannelButton.Tag = "Subscribed";
                                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                //Add The Video to  Subcribed Videos Database
                                Events_Insert_SubscriptionsChannel();

                                //Send API Request here for Subcribe
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddSubscribeToChannelAsync(VideoDataHandler.Owner?.OwnerClass?.Id) });


                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribe";
                                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);
                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
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
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, VideoDataHandler, "Login");
                        dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_Subcribed), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void SetSubscribeChannelWithPaid()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    SubscribeChannelButton.Tag = "Subscribed";
                    SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

                    //Color
                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                    //icon
                    Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
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
                            ActivityContext?.RunOnUiThread(() =>
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
                            });
                        }
                    }
                    else Methods.DisplayReportResult(ActivityContext, respond);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ImageChannelViewOnClick(object sender, EventArgs e)
        {
            ActivityContext.ShowUserChannelFragment(VideoDataHandler.Owner?.OwnerClass, VideoDataHandler.Owner?.OwnerClass?.Id);
        }

        private void ShowMoreDescriptionIconViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (VideoDescriptionLayout.Tag?.ToString() == "Open")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);
                    //VideoDescriptionLayout.Visibility = ViewStates.Gone;
                    VideoDescriptionLayout.Tag = "closed";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                    VideoDescriptionLayout.Animate().Alpha(1).SetDuration(400);
                    TextChannelName.Animate().Alpha(1).SetDuration(300);
                    VideoChannelViews.Animate().Alpha(1).SetDuration(300);
                    VideoTitle.SetMaxLines(1);

                    ViewGroup parent = (ViewGroup)VideoDescription.Parent;
                    ViewGroup.LayoutParams par = parent.LayoutParameters;
                    par.Height = 200;
                    VideoDescriptionLayout.LayoutParameters = par;

                }
                else
                {
                    // VideoDescriptionLayout.LayoutParameters = ViewGroup.LayoutParams.WrapContent;
                    //LinearLayout.LayoutParams par = (LinearLayout.LayoutParams)VideoDescriptionLayout.LayoutParameters;
                    ViewGroup parent = (ViewGroup)VideoDescription.Parent;
                    ViewGroup.LayoutParams par = parent.LayoutParameters;
                    par.Height = ViewGroup.LayoutParams.WrapContent;
                    VideoDescriptionLayout.LayoutParameters = par;

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowUp);
                    //VideoDescriptionLayout.Visibility = ViewStates.Visible;
                    VideoDescriptionLayout.Tag = "Open";
                    VideoTitle.Text = Methods.FunString.DecodeString(VideoDataHandler.Title);
                    VideoDescriptionLayout.Animate().Alpha(1).SetDuration(500);
                    TextChannelName.Animate().Alpha(1).SetDuration(300);
                    VideoChannelViews.Animate().Alpha(1).SetDuration(300);
                    VideoTitle.SetMaxLines(4);


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
                    //Send API Request here for WatchLater
                    if (Methods.CheckConnectivity())
                    {
                        var (apiStatus, respond) = await RequestsAsync.Video.AddToWatchLaterVideosAsync(VideoDataHandler.Id);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageCodeObject result)
                            {
                                if (result.SuccessType.Contains("Removed"))
                                {
                                    ActivityContext.LibrarySynchronizer.RemovedFromWatchLater(videoObject);
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_RemovedFromWatchLater), ToastLength.Short)?.Show();
                                }
                                else if (result.SuccessType.Contains("Added"))
                                {
                                    ActivityContext.LibrarySynchronizer.AddToWatchLater(videoObject);
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_AddedToWatchLater), ToastLength.Short)?.Show();
                                }
                            }
                        }
                        else Methods.DisplayReportResult(ActivityContext, respond);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(ActivityContext, videoObject, "Login");
                    dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_WatchLater), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
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
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, videoObject, "PlayList");
                        dialog.ShowPlayListDialog();
                    }
                    else
                    {
                        PopupDialogController dialog = new PopupDialogController(ActivityContext, videoObject, "Login");
                        dialog.ShowNormalDialog(ActivityContext.GetText(Resource.String.Lbl_Warning), ActivityContext.GetText(Resource.String.Lbl_Please_sign_in_playlist), ActivityContext.GetText(Resource.String.Lbl_Yes), ActivityContext.GetText(Resource.String.Lbl_No));
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
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

                ActivityContext.LibrarySynchronizer.AddToSubscriptions(VideoDataHandler.Owner?.OwnerClass);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Fun Video

        private void SetVideoType(VideoObject videoObject)
        {
            var myDetails = ListUtils.MyChannelList?.FirstOrDefault();
            var age = string.IsNullOrWhiteSpace(myDetails?.Age) ? 0 : Convert.ToInt32(myDetails.Age);
            var isBelow18 = age > 0 && age < 18;

            try
            {
                VideoType = VideoEnumTypes.Normal;

                if (videoObject.VideoLocation.Contains("Youtube") || videoObject.VideoLocation.Contains("youtu") || videoObject.VideoType == "VideoObject/youtube")
                    VideoType = VideoEnumTypes.Youtube;
                else if (!string.IsNullOrEmpty(videoObject.Vimeo))
                    VideoType = VideoEnumTypes.Vimeo;
                else if (!string.IsNullOrEmpty(videoObject.Daily))
                    VideoType = VideoEnumTypes.DailyMotion;
                else if (!string.IsNullOrEmpty(videoObject.Ok))
                    VideoType = VideoEnumTypes.Ok;
                else if (!string.IsNullOrEmpty(videoObject.Twitch))
                    VideoType = VideoEnumTypes.Twitch;
                else if (!string.IsNullOrEmpty(videoObject.Facebook))
                    VideoType = VideoEnumTypes.Facebook;
                else if (videoObject.IsOwner != null && videoObject.AgeRestriction == "2" && videoObject.IsOwner.Value == false && isBelow18)
                    VideoType = VideoEnumTypes.AgeRestricted;
                else if (!string.IsNullOrEmpty(videoObject.GeoBlocking) && videoObject.IsOwner == false)
                    VideoType = VideoEnumTypes.GeoBlocked;

                videoObject.VideoType = VideoType.ToString();
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

                GlideImageLoader.LoadImage(Context, videoObject.Owner?.OwnerClass?.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);
                TextChannelName.Text = videoObject.Owner?.OwnerClass?.Username;
                VideoLikeCount.Text = videoObject.LikesPercent;
                VideoUnLikeCount.Text = videoObject.DislikesPercent;
                VideoChannelViews.Text = videoObject.Views + " " + ActivityContext.GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
                VideoPublishDate.Text = ActivityContext.GetText(Resource.String.Lbl_Published_on) + " " + Methods.Time.ConvertToSpanishFormatIfNeeded(videoObject.TimeDate);
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                ActivityContext.VideoChannelText.Text = TextChannelName.Text;
                ActivityContext.VideoTitleText.Text = VideoTitle.Text;

                if (videoObject.Owner?.OwnerClass?.Verified == "1")
                {
                    ActivityContext.VideoChannelText.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);
                    TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.icon_checkmark_small_vector, 0);
                }
                else
                {
                    ActivityContext.VideoChannelText.SetCompoundDrawablesWithIntrinsicBounds(0, 0, 0, 0);
                    TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, 0, 0);
                }

                IsVideoDescriptionExpended = false;
                TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));
                ToggleVideoDescription();

                EditButton.Visibility = videoObject.IsOwner != null && videoObject.IsOwner.Value ? ViewStates.Visible : ViewStates.Gone;

                //Reset Views
                LikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                LikeButton.Tag = "0";
                UnLikeIconView.SetColorFilter(Color.ParseColor("#8e8e8e"));
                UnLikeButton.Tag = "0";
                VideoLikeCount.Text = "0";
                VideoUnLikeCount.Text = "0";

                SubscribeChannelButton.Tag = "Subscribe";
                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);
                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
                icon.Bounds = new Rect(10, 10, 10, 7);
                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                //Close the description panel
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShowMoreDescriptionIconView, IonIconsFonts.ArrowDown);

                //VideoDescriptionLayout.Visibility = ViewStates.Gone;
                VideoDescriptionLayout.Tag = "closed";
                VideoTitle.Text = Methods.FunString.DecodeString(videoObject.Title);

                //Clear all data 
                if (ActivityContext.CommentsFragment != null && ActivityContext.CommentsFragment.MAdapter != null)
                {
                    ActivityContext.CommentsFragment.MAdapter.CommentList?.Clear();
                    ActivityContext.NextToFragment.MAdapter.VideoList?.Clear();
                    ActivityContext.CommentsFragment.MAdapter.NotifyDataSetChanged();
                    ActivityContext.NextToFragment.MAdapter.NotifyDataSetChanged();
                    ActivityContext.CommentsFragment.StartApiService(videoObject.Id, "0");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                VideoChannelViews.Text = videoObject.Views + " " + ActivityContext.GetText(Resource.String.Lbl_Views) + " | " + Methods.Time.ReplaceTime(videoObject.TimeAgo);
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
                VideoPublishDate.Text = ActivityContext.GetText(Resource.String.Lbl_Published_on) + " " + Methods.Time.ConvertToSpanishFormatIfNeeded(videoObject.TimeDate);
                VideoCategory.Text = CategoriesController.GetCategoryName(videoObject);

                //Verified 
                TextChannelName.SetCompoundDrawablesWithIntrinsicBounds(0, 0, videoObject.Owner?.OwnerClass?.Verified == "1" ? Resource.Drawable.icon_checkmark_small_vector : 0, 0);
                TextChannelName.Text = AppTools.GetNameFinal(videoObject.Owner?.OwnerClass);
                GlideImageLoader.LoadImage(ActivityContext, videoObject.Owner?.OwnerClass?.Avatar, ImageChannelView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (videoObject.Owner != null && string.IsNullOrEmpty(videoObject.Owner?.OwnerClass?.SubscribeCount)) 
                        videoObject.Owner.Value.OwnerClass.SubscribeCount = "0";

                TxtSubscribeCount.Text = videoObject.Owner?.OwnerClass?.SubscribeCount;

                IsVideoDescriptionExpended = false;
                TextSanitizerAutoLink.Load(Methods.FunString.DecodeString(videoObject.Description));
                ToggleVideoDescription();

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
                    ActivityContext.VideoActionsController.DownloadIcon.SetImageResource(Resource.Drawable.ic_checked_red);
                    ActivityContext.VideoActionsController.DownloadIcon.Tag = "Downloaded";
                    ActivityContext.LibrarySynchronizer.AddToWatchOffline(videoObject);
                }

                if (videoObject.Owner?.OwnerClass?.Id != UserDetails.UserId)
                {
                    UserDataObject channel = await ApiRequest.GetChannelData(ActivityContext, videoObject.Owner?.OwnerClass?.Id);
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
                                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
                                icon.Bounds = new Rect(10, 10, 10, 7);
                                SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);

                                var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                                var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                                Console.WriteLine(currency);
                                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice;
                            }
                            else
                            {
                                SubscribeChannelButton.Tag = "Subscribed";

                                SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

                                //Color
                                SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                //icon
                                Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
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
                                    SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribed);

                                    //Color
                                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                    //icon
                                    Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribedButton);
                                    icon.Bounds = new Rect(10, 10, 10, 7);
                                    SubscribeChannelButton.SetCompoundDrawablesWithIntrinsicBounds(icon, null, null, null);
                                    break;
                                }
                                case "Subscribe":
                                {
                                    SubscribeChannelButton.Text = ActivityContext.GetText(Resource.String.Lbl_Subscribe);

                                    //Color
                                    SubscribeChannelButton.SetTextColor(Color.ParseColor("#efefef"));
                                    //icon
                                    Drawable icon = ActivityContext.GetDrawable(Resource.Drawable.SubcribeButton);
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
                        ActivityContext.NextToFragment.LoadDataAsync(new ObservableCollection<VideoObject>(result.DataResult.SuggestedVideos));

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
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
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
                    case "AddTo" when itemString == ActivityContext.GetString(Resource.String.Lbl_Addto_playlist):
                        OnMenuAddPlaylistClick(VideoDataHandler);
                        break;
                    case "AddTo":
                    {
                        if (itemString == ActivityContext.GetString(Resource.String.Lbl_Addto_watchlater) || itemString == ActivityContext.GetString(Resource.String.Lbl_RemoveFromWatchLater))
                        {
                            OnMenuAddWatchLaterClick(VideoDataHandler);
                        }

                        break;
                    }
                    case "Payment_RentVideo":
                    {
                        string price = AppSettings.VideoRentalPriceStatic && AppSettings.VideoRentalPrice > 0
                            ? AppSettings.VideoRentalPrice.ToString()
                            : VideoDataHandler.RentPrice;

                        ActivityContext.Price = price;
                        ActivityContext.PayType = "RentVideo";
                        ActivityContext.PaymentVideoObject = VideoDataHandler;

                        if (itemString == ActivityContext.GetString(Resource.String.Lbl_Paypal))
                        {
                            ActivityContext.InitPayPalPayment.BtnPaypalOnClick(price, "RentVideo");
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_CreditCard))
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PaymentCardDetailsActivity));
                            intent.PutExtra("Price", price);
                            intent.PutExtra("payType", "RentVideo");
                            intent.PutExtra("Id", VideoDataHandler.Id);
                            ActivityContext.StartActivity(intent);
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_BankTransfer))
                        {
                            Intent intent = new Intent(ActivityContext, typeof(PaymentLocalActivity));
                            intent.PutExtra("Id", VideoDataHandler.Id);
                            intent.PutExtra("Price", price);
                            intent.PutExtra("payType", "RentVideo");
                            ActivityContext.StartActivity(intent);
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_RazorPay))
                        {
                            ActivityContext.InitRazorPay?.BtnRazorPayOnClick(price, "RentVideo", VideoDataHandler.Id);
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_PayStack))
                        {
                            TypeDialog = "PayStack";

                            var dialogBuilder = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VerificationFailed), ActivityContext.GetText(Resource.String.Lbl_IsEmailValid), ActivityContext.GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await PayStack(s.ToString(), price, "RentVideo", VideoDataHandler.Id);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                            dialogBuilder.PositiveText(ActivityContext.GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                            dialogBuilder.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_CashFree))
                        {
                            OpenCashFreeDialog(price, "RentVideo", VideoDataHandler.Id);
                        }
                        else if (itemString == ActivityContext.GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await PaySera(price, "RentVideo", VideoDataHandler.Id);
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
         
        public void OnPaymentError(int code, string response)
        {
            try
            {
                Console.WriteLine("razorpay : Payment failed: " + code + " " + response);
                Toast.MakeText(ActivityContext, "Payment failed: " + response, ToastLength.Long)?.Show();
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
                        switch (ActivityContext.PayType)
                        {
                            case "RentVideo":
                            {
                                var keyValues = new Dictionary<string, string>
                                {
                                    {"merchant_amount", ActivityContext.Price},
                                    {"video_id", VideoDataHandler.Id},
                                    {"pay_type", "rent"},
                                };

                                var (apiStatus, respond) = await RequestsAsync.Payment.RazorPay(razorpayPaymentId, "buy_video", keyValues);
                                if (apiStatus == 200)
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    //StartApiService(); 
                                }
                                else Methods.DisplayReportResult(ActivityContext, respond);

                                break;
                            }
                            case "SubscriberVideo":
                            case "Subscriber":
                            {
                                var keyValues = new Dictionary<string, string>
                                {
                                    {"merchant_amount",ActivityContext.Price},
                                    {"subscribe_id", VideoDataHandler.Owner?.OwnerClass?.Id},
                                };

                                var (apiStatus, respond) = await RequestsAsync.Payment.RazorPay(razorpayPaymentId, "subscribe", keyValues);
                                if (apiStatus == 200)
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    switch (ActivityContext.PayType)
                                    {
                                        case "SubscriberVideo":
                                            if (GlobalPlayerActivity.OnOpenPage)
                                                GlobalPlayerActivity.GetInstance()?.SetSubscribeChannelWithPaid();
                                            else
                                                ActivityContext?.VideoDataWithEventsLoader.SetSubscribeChannelWithPaid();
                                            break;
                                        case "Subscriber":
                                            ActivityContext?.UserChannelFragment?.SetSubscribeChannelWithPaid();
                                            break;
                                    }
                                }
                                else Methods.DisplayReportResult(ActivityContext, respond);

                                break;
                            }
                            case "purchaseVideo":
                            {
                                var keyValues = new Dictionary<string, string>
                                {
                                    {"merchant_amount", ActivityContext.Price},
                                    {"video_id", VideoDataHandler.Id},
                                };

                                var (apiStatus, respond) = await RequestsAsync.Payment.RazorPay(razorpayPaymentId, "buy_video", keyValues);
                                if (apiStatus == 200)
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                    //StartApiService(); 
                                }
                                else Methods.DisplayReportResult(ActivityContext, respond);

                                break;
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task PayStack(string email, string price, string payType, string id)
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
                                    ActivityContext.PayStackPayment ??= new InitPayStackPayment(ActivityContext);
                                    ActivityContext.PayStackPayment.DisplayPayStackPayment(result.Url, "rent", priceInt.ToString(), id);
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        case "Subscriber":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePayStack("subscribe", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    ActivityContext.PayStackPayment ??= new InitPayStackPayment(ActivityContext);

                                    switch (payType)
                                    {
                                        case "SubscriberVideo":
                                            ActivityContext.PayStackPayment.DisplayPayStackPayment(result.Url, "SubscriberVideo", priceInt.ToString(), id);
                                            break;
                                        case "Subscriber":
                                            ActivityContext.PayStackPayment.DisplayPayStackPayment(result.Url, "Subscriber", priceInt.ToString(), id);
                                            break;
                                    } 
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private EditText TxtName, TxtEmail, TxtPhone;
        public void OpenCashFreeDialog(string price, string payType, string id)
        {
            try
            {
                var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light)
                    .Title(ActivityContext.GetText(Resource.String.Lbl_CashFree))
                    .CustomView(Resource.Layout.CashFreePaymentLayout, true)
                    .PositiveText(ActivityContext.GetText(Resource.String.Lbl_PayNow)).OnPositive(async (materialDialog, action) =>
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short)?.Show();
                                return;
                            }

                            var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                            if (!check)
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VerificationFailed), ActivityContext.GetText(Resource.String.Lbl_IsEmailValid), ActivityContext.GetText(Resource.String.Lbl_Ok));
                                return;
                            }

                            if (string.IsNullOrEmpty(TxtPhone.Text) || string.IsNullOrWhiteSpace(TxtPhone.Text))
                            {
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_enter_your_data), ToastLength.Short)?.Show();
                                return;
                            }

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await CashFree(TxtName.Text, TxtEmail.Text, TxtPhone.Text, price, payType, id);
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    })
                    .NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
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

        public async Task CashFree(string name, string email, string phone, string price, string payType, string id)
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
                                    ActivityContext.CashFreePayment ??= new InitCashFreePayment(ActivityContext);
                                    ActivityContext.CashFreePayment.DisplayCashFreePayment(result, "rent", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        case "Subscriber":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializeCashFree("subscribe", AppSettings.CashFreeCurrency, AppSettings.CashfreeSecretKey, AppSettings.CashfreeMode, keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is CashFreeObject result)
                                {
                                    ActivityContext.CashFreePayment ??= new InitCashFreePayment(ActivityContext);

                                    switch (payType)
                                    {
                                        case "SubscriberVideo":
                                            ActivityContext.CashFreePayment.DisplayCashFreePayment(result, "SubscriberVideo", price, id);
                                            break;
                                        case "Subscriber":
                                            ActivityContext.CashFreePayment.DisplayCashFreePayment(result, "Subscriber", price, id);
                                            break;
                                    }
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async Task PaySera(string price, string payType, string id)
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
                                    ActivityContext.PaySeraPayment ??= new InitPaySeraPayment(ActivityContext);
                                    ActivityContext.PaySeraPayment.DisplayPaySeraPayment(result.Url, "rent", price, id);
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                        case "SubscriberVideo":
                        case "Subscriber":
                        {
                            keyValues.Add("subscribe_id", id);

                            var (apiStatus, respond) = await RequestsAsync.Payment.InitializePaySera("subscribe", keyValues);
                            if (apiStatus == 200)
                            {
                                if (respond is InitializePaymentObject result)
                                {
                                    ActivityContext.PaySeraPayment ??= new InitPaySeraPayment(ActivityContext);

                                    switch (payType)
                                    {
                                        case "SubscriberVideo":
                                            ActivityContext.PaySeraPayment.DisplayPaySeraPayment(result.Url, "SubscriberVideo", price, id);
                                            break;
                                        case "Subscriber":
                                            ActivityContext.PaySeraPayment.DisplayPaySeraPayment(result.Url, "Subscriber", price, id);
                                            break;
                                    } 
                                }
                            }
                            else Methods.DisplayReportResult(ActivityContext, respond);

                            break;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}