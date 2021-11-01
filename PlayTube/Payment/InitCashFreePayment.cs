using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Webkit;
using Android.Widget;
using AndroidHUD;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.Classes.Payment;
using PlayTubeClient.RestCalls;

namespace PlayTube.Payment
{
    public class InitCashFreePayment
    {
        private readonly Activity ActivityContext;
        private Dialog CashFreeWindow;
        private WebView HybridView;
        private string Price, PayType, Id;
        private CashFreeObject CashFreeObject;

        public InitCashFreePayment(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayCashFreePayment(CashFreeObject cashFreeObject, string payType, string price, string id)
        {
            try
            {
                CashFreeObject = cashFreeObject;
                Price = price;
                PayType = payType;
                Id = id;

                CashFreeWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                CashFreeWindow?.SetContentView(Resource.Layout.PaymentWebViewLayout);

                var title = (TextView)CashFreeWindow?.FindViewById(Resource.Id.toolbar_title);
                if (title != null)
                    title.Text = ActivityContext.GetText(Resource.String.Lbl_PayWith) + " " + ActivityContext.GetText(Resource.String.Lbl_CashFree);

                var closeButton = (TextView)CashFreeWindow?.FindViewById(Resource.Id.toolbar_close);
                if (closeButton != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, closeButton, IonIconsFonts.Close);

                    closeButton.SetTextSize(ComplexUnitType.Sp, 20f);
                    closeButton.Click += CloseButtonOnClick;
                }

                HybridView = CashFreeWindow?.FindViewById<WebView>(Resource.Id.LocalWebView);

                //Set WebView
                HybridView.SetWebViewClient(new MyWebViewClient(this));
                if (HybridView.Settings != null)
                {
                    HybridView.Settings.LoadsImagesAutomatically = true;
                    HybridView.Settings.JavaScriptEnabled = true;
                    HybridView.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                    HybridView.Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.TextAutosizing);
                    HybridView.Settings.DomStorageEnabled = true;
                    HybridView.Settings.AllowFileAccess = true;
                    HybridView.Settings.DefaultTextEncodingName = "utf-8";

                    HybridView.Settings.UseWideViewPort = true;
                    HybridView.Settings.LoadWithOverviewMode = true;

                    HybridView.Settings.SetSupportZoom(false);
                    HybridView.Settings.BuiltInZoomControls = false;
                    HybridView.Settings.DisplayZoomControls = false;

                    if (!string.IsNullOrEmpty(CashFreeObject.JsonForm))
                    {
                        //Load url to be rendered on WebView
                        HybridView.LoadUrl(CashFreeObject.JsonForm);

                        CashFreeWindow?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CloseButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                CashFreeWindow?.Hide();
                CashFreeWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopCashFree()
        {
            try
            {
                if (CashFreeWindow != null)
                {
                    CashFreeWindow?.Hide();
                    CashFreeWindow?.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task CashFree(CashFreeGetStatusObject statusObject, string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"txStatus", statusObject.TxStatus},
                        {"orderAmount", statusObject.OrderAmount},
                        {"referenceId", statusObject.ReferenceId},
                        {"paymentMode", statusObject.PaymentMode},
                        {"txMsg", statusObject.TxMsg},
                        {"txTime", statusObject.TxTime},

                        {"signature", CashFreeObject.Signature},
                        {"orderId", CashFreeObject.OrderId},
                    };

                    switch (request)
                    {
                        case "pro_paid": 
                            break;
                        case "buy_video":
                            keyValues.Add("pay_type" , "rent");
                            keyValues.Add("video_id", Id);
                            break;
                        case "check_subscribe":
                            keyValues.Add("subscribe_id", Id);
                            break;
                    }

                    var (apiStatus, respond) = await RequestsAsync.Payment.CashFree(request, keyValues);
                    if (apiStatus == 200)
                    {
                        AndHUD.Shared.Dismiss(ActivityContext);

                        switch (request)
                        {
                            case "pro_paid":
                                var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.IsPro = "1";

                                    var sqlEntity = new SqLiteDatabase();
                                    sqlEntity.InsertOrUpdate_DataMyChannel(dataUser); 
                                }

                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Upgraded), ToastLength.Long)?.Show();
                                break;
                            case "buy_video": 
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_VideoSuccessfullyPaid), ToastLength.Long)?.Show();
                                break;
                            case "check_subscribe":
                                switch (PayType)
                                {
                                    case "SubscriberVideo":
                                        if (GlobalPlayerActivity.OnOpenPage)
                                            GlobalPlayerActivity.GetInstance()?.SetSubscribeChannelWithPaid();
                                        else
                                            TabbedMainActivity.GetInstance()?.VideoDataWithEventsLoader.SetSubscribeChannelWithPaid();
                                        break;
                                    case "Subscriber":
                                        TabbedMainActivity.GetInstance()?.UserChannelFragment?.SetSubscribeChannelWithPaid();
                                        break;
                                }
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Short)?.Show();
                                break;
                        }

                        StopCashFree();
                    }
                    else Methods.DisplayAndHudErrorResult(ActivityContext, respond);
                }
                else
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                AndHUD.Shared.Dismiss(ActivityContext);
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MyWebViewClient : WebViewClient
        {
            private readonly InitCashFreePayment MActivity;
            public MyWebViewClient(InitCashFreePayment mActivity)
            {
                MActivity = mActivity;
            }

            public override async void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    if (url.Contains("requests.php?f=cashfree"))
                    {
                        //Show a progress
                        AndHUD.Shared.Show(MActivity.ActivityContext, MActivity.ActivityContext.GetText(Resource.String.Lbl_Processing));

                        var (apiStatus, respond) = await RequestsAsync.Payment.CashFreeGetStatus(MActivity.CashFreeObject.AppId, AppSettings.CashfreeSecretKey, MActivity.CashFreeObject.OrderId, AppSettings.CashfreeMode);
                        if (apiStatus == 200)
                        {
                            if (respond is CashFreeGetStatusObject result)
                            {
                                switch (MActivity.PayType)
                                {
                                    case "pro":
                                        await MActivity.CashFree(result, "pro_paid");
                                        break;
                                    case "rent":
                                        await MActivity.CashFree(result, "buy_video");
                                        break;
                                    case "subscribe":
                                    case "SubscriberVideo":
                                    case "Subscriber":
                                    case "check_subscribe":
                                        await MActivity.CashFree(result, "check_subscribe");
                                        break;
                                }
                            }
                        }
                        else Methods.DisplayReportResult(MActivity.ActivityContext, respond);
                    }
                    else
                    {
                        base.OnPageStarted(view, url, favicon);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }
    }
}