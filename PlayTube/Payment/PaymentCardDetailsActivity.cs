using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Com.Stripe.Android.View;
using PlayTube.Activities.Base;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using Exception = System.Exception; 
using Math = System.Math;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Payment
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentCardDetailsActivity : BaseActivity, ITokenCallback
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtName;
        private Button BtnApply;
        private CardMultilineWidget MultilineWidget;

        private Stripe Stripe; 

        private string Price, PayType, Id, TokenId;
        private TabbedMainActivity GlobalContext;

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
                SetContentView(Resource.Layout.PaymentCardDetailsLayout);

                Id = Intent?.GetStringExtra("Id") ?? ""; // VideoObject.Id
                Price = Intent?.GetStringExtra("Price") ?? "";
                PayType = Intent?.GetStringExtra("payType") ?? "";  

                GlobalContext = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitWalletStripe();
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
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardExpire = (TextView)FindViewById(Resource.Id.card_expire);
                CardCvv = (TextView)FindViewById(Resource.Id.card_cvv);
                CardName = (TextView)FindViewById(Resource.Id.card_name);

                MultilineWidget = (CardMultilineWidget)FindViewById(Resource.Id.card_multiline_widget);

                EtName = (EditText)FindViewById(Resource.Id.et_name);
                BtnApply = (Button)FindViewById(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_CreditCard);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MultilineWidget.CvcComplete += MultilineWidgetOnCvcComplete;
                    EtName.TextChanged += EtNameOnTextChanged;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    MultilineWidget.CvcComplete -= MultilineWidgetOnCvcComplete;
                    EtName.TextChanged -= EtNameOnTextChanged;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MultilineWidgetOnCvcComplete(object sender, EventArgs e)
        {
            try
            {
                if (MultilineWidget.Card != null && MultilineWidget.Card.ValidateCard() && MultilineWidget.ValidateAllFields())
                {
                    if (MultilineWidget.Card.Number.Trim().Length == 0)
                    {
                        CardNumber.Text = "**** **** **** ****";
                    }
                    else
                    {
                        string number = InsertPeriodically(MultilineWidget.Card.Number.Trim(), " ", 4);
                        CardNumber.Text = number;
                    }

                    if (MultilineWidget.Card.ExpMonth.ToString().Trim().Length == 0 && MultilineWidget.Card.ExpYear.ToString().Trim().Length == 0)
                    {
                        CardExpire.Text = "MM/YY";
                    }
                    else
                    {
                        CardExpire.Text = MultilineWidget.Card.ExpMonth + "/" + MultilineWidget.Card.ExpYear;
                    }

                    CardCvv.Text = MultilineWidget.Card.CVC.Trim().Length == 0 ? "***" : MultilineWidget.Card.CVC.Trim();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EtNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardName.Text = e.Text.ToString().Trim().Length == 0 ? GetString(Resource.String.Lbl_YourName) : e.Text.ToString().Trim();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Stripe
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            { 
                if (MultilineWidget.Card.ValidateCard() && !string.IsNullOrEmpty(EtName.Text))
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    Card card = MultilineWidget.Card;
                    Stripe.CreateToken(card, PaymentConfiguration.Instance.PublishableKey, this);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseVerifyDataCard), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string InsertPeriodically(string text, string insert, int period)
        {
            try
            {
                var parts = SplitInParts(text, period);
                string formatted = string.Join(insert, parts);
                return formatted;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        #endregion
          
        #region Stripe

        private void InitWalletStripe()
        {
            try
            {
                var stripePublishableKey = ListUtils.MySettingsList?.StripeId ?? "";
                if (!string.IsNullOrEmpty(stripePublishableKey))
                {
                    PaymentConfiguration.Init(stripePublishableKey);
                    Stripe = new Stripe(this, stripePublishableKey);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long)?.Show();
                    Finish();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(Java.Lang.Exception error)
        {
            try
            {
                AndHUD.Shared.Dismiss(this);
                Toast.MakeText(this, error.Message, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public async void OnSuccess(Token token)
        {
            try
            {
                // Send token to your own web service
                //var stripeBankAccount = token.BankAccount;
                //var stripeCard = token.Card;
                //var stripeCreated = token.Created;
                TokenId = token.Id;
                //var stripeLiveMode = token.Livemode;
                //var stripeType = token.Type;
                //var stripeUsed = token.Used;
                 
                switch (PayType)
                {
                    //send api  
                    case "purchaseVideo" when Methods.CheckConnectivity():
                    { 
                            var (apiStatus, respond) = await RequestsAsync.Payment.StripeAsync(TokenId, "buy_video", Id);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    if (GlobalPlayerActivity.OnOpenPage)
                                    {
                                       var globalPlayerContext = GlobalPlayerActivity.GetInstance();

                                        if (globalPlayerContext != null)
                                        {
                                            var ft = globalPlayerContext.SupportFragmentManager.BeginTransaction();

                                            if (globalPlayerContext.RestrictedVideoPlayerFragment != null && globalPlayerContext.RestrictedVideoPlayerFragment.IsAdded)
                                                ft.Hide(globalPlayerContext.RestrictedVideoPlayerFragment);
                                             
                                            if (globalPlayerContext.TubePlayerView != null)
                                            {
                                                if (globalPlayerContext.YoutubePlayer != null && globalPlayerContext.YouTubePlayerEvents.IsPlaying)
                                                    globalPlayerContext.YoutubePlayer?.Pause();

                                                globalPlayerContext.TubePlayerView.Visibility = ViewStates.Gone;

                                                if (globalPlayerContext.VideoActionsController.SimpleExoPlayerView.Visibility == ViewStates.Gone)
                                                    globalPlayerContext.VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Visible;
                                            }
                                               
                                            globalPlayerContext.VideoDataHandler.IsPurchased = "1";
                                            globalPlayerContext.StartPlayVideo(globalPlayerContext.VideoDataHandler);

                                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                                        } 
                                    }
                                    else
                                    {
                                        if (GlobalContext != null)
                                        {
                                            var ft = GlobalContext.SupportFragmentManager.BeginTransaction();

                                            if (GlobalContext.RestrictedVideoPlayerFragment != null && GlobalContext.RestrictedVideoPlayerFragment.IsAdded)
                                                ft.Hide(GlobalContext.RestrictedVideoPlayerFragment);

                                            if (GlobalContext.TubePlayerView != null)
                                            {
                                                if (GlobalContext.YoutubePlayer != null && GlobalContext.YouTubePlayerEvents.IsPlaying)
                                                    GlobalContext.YoutubePlayer?.Pause();

                                                GlobalContext.TubePlayerView.Visibility = ViewStates.Gone;

                                                if (GlobalContext.VideoActionsController.SimpleExoPlayerView.Visibility == ViewStates.Gone)
                                                    GlobalContext.VideoActionsController.SimpleExoPlayerView.Visibility = ViewStates.Visible;
                                            }
                                             
                                            GlobalContext.PaymentVideoObject.IsPurchased = "1";
                                            GlobalContext.StartPlayVideo(GlobalContext.PaymentVideoObject);

                                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                                        }
                                    }
                                    
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);
                            break;
                        }
                    case "Subscriber" when Methods.CheckConnectivity():
                    {
                            var (apiStatus, respond) = await RequestsAsync.Payment.StripeAsync(TokenId, "subscribe", Id);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    GlobalContext?.UserChannelFragment?.SetSubscribeChannelWithPaid();
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);
                        }
                        
                        break; 
                    case "SubscriberVideo" when Methods.CheckConnectivity():
                        {
                            var (apiStatus, respond) = await RequestsAsync.Payment.StripeAsync(TokenId, "subscribe", Id);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    if (GlobalPlayerActivity.OnOpenPage)
                                    {
                                        GlobalPlayerActivity.GetInstance()?.SetSubscribeChannelWithPaid();
                                    }
                                    else
                                    {
                                        GlobalContext?.VideoDataWithEventsLoader.SetSubscribeChannelWithPaid();
                                    }
                                }
                            }
                            else Methods.DisplayReportResult(this, respond); 
                            break;
                        } 
                    case "RentVideo" when Methods.CheckConnectivity():
                        {
                            var (apiStatus, respond) = await RequestsAsync.Payment.StripeAsync(TokenId, "rent_video", Id);
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                }
                            }
                            else Methods.DisplayReportResult(this, respond); 
                            break;
                        } 
                    case "DonateVideo":
                        Toast.MakeText(this, GetText(Resource.String.Lbl_DonatedSuccessfully), ToastLength.Long)?.Show();
                        break;
                    case "GoPro" when Methods.CheckConnectivity():
                        {
                            var (apiStatus, respond) = await RequestsAsync.Payment.StripeAsync(TokenId, "pro", "");
                            if (apiStatus == 200)
                            {
                                if (respond is MessageObject result)
                                {
                                    var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
                                    if (dataUser != null)
                                    {
                                        dataUser.IsPro = "1";

                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.InsertOrUpdate_DataMyChannel(dataUser);
                                    }

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();
                                }
                            }
                            else Methods.DisplayReportResult(this, respond); 
                            break;
                        } 
                }

                AndHUD.Shared.Dismiss(this);
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion
          
    }
}