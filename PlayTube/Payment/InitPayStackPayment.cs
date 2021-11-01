using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Webkit;
using Android.Widget;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.RestCalls;

namespace PlayTube.Payment
{
    public class InitPayStackPayment
    {
        private readonly Activity ActivityContext;
        private Dialog PayStackWindow;
        private WebView HybridView;
        private string Url, Price, PayType, Id;
        public InitPayStackPayment(Activity context)
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

        public void DisplayPayStackPayment(string url, string payType, string price, string id)
        {
            try
            {
                Url = url;
                Price = price;
                PayType = payType;
                Id = id;

                PayStackWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                PayStackWindow?.SetContentView(Resource.Layout.PaymentWebViewLayout);

                var title = (TextView)PayStackWindow?.FindViewById(Resource.Id.toolbar_title);
                if (title != null)
                    title.Text = ActivityContext.GetText(Resource.String.Lbl_PayWith) + " " + ActivityContext.GetText(Resource.String.Lbl_PayStack);

                var closeButton = (TextView)PayStackWindow?.FindViewById(Resource.Id.toolbar_close);
                if (closeButton != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, closeButton, IonIconsFonts.Close);

                    closeButton.SetTextSize(ComplexUnitType.Sp, 20f);
                    closeButton.Click += CloseButtonOnClick;
                }

                HybridView = PayStackWindow?.FindViewById<WebView>(Resource.Id.LocalWebView);

                //Set WebView
                if (HybridView != null)
                {
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
                    }

                    //Load url to be rendered on WebView
                    HybridView.LoadUrl(Url);
                }

                PayStackWindow?.Show();
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
                PayStackWindow?.Hide();
                PayStackWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopPayStack()
        {
            try
            {
                if (PayStackWindow != null)
                {
                    PayStackWindow?.Hide();
                    PayStackWindow?.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PayStack(string reference, string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>
                    {
                        {"reference", reference},
                    };

                    switch (request)
                    { 
                        case "pro_paid": 
                            break;
                        case "buy_video":
                            //keyValues.Add("amount", Price);
                            keyValues.Add("video_id", Id);
                            break;
                        case "check_subscribe":
                            //keyValues.Add("merchant_amount", Price);
                            keyValues.Add("subscribe_id", Id);
                            break;
                    }

                    var (apiStatus, respond) = await RequestsAsync.Payment.PayStack(request, keyValues);
                    if (apiStatus == 200)
                    {
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
                                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Subscribed_successfully), ToastLength.Long)?.Show();
                                break;
                        }
                        ActivityContext.Finish();
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

        private class MyWebViewClient : WebViewClient
        {
            private readonly InitPayStackPayment MActivity;
            public MyWebViewClient(InitPayStackPayment mActivity)
            {
                MActivity = mActivity;
            }

            public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
            {
                try
                {
                    if (string.IsNullOrEmpty(request?.Url?.ToString()))
                        return false;

                    if (request.Url.ToString() == MActivity.Url)
                    {
                        view.LoadUrl(request.Url.ToString());
                    }
                    else if (request.Url.ToString().Contains("reference"))
                    {
                        //https://demo.wowonder.com/requests.php?f=paystack&s=fund&amount=12&fund_id=403&trxref=5f3f88e2a43c7&reference=5f3f88e2a43c7

                        var reference = request.Url.ToString()?.Split("&reference=")?.Last();
                        if (string.IsNullOrEmpty(reference))
                            return false;

                        switch (MActivity.PayType)
                        {
                            case "pro":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PayStack(reference, "pro_paid") });
                                break;
                            case "rent":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PayStack(reference, "buy_video") });
                                break;
                            case "check_subscribe":
                            case "subscribe":
                            case "SubscriberVideo":
                            case "Subscriber":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PayStack(reference, "check_subscribe") });
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
                return false;
            }

            public override void OnPageStarted(WebView view, string url, Bitmap favicon)
            {
                try
                {
                    base.OnPageStarted(view, url, favicon);

                    if (view.Settings != null)
                    {
                        view.Settings.JavaScriptEnabled = true;
                        view.Settings.DomStorageEnabled = true;
                        view.Settings.AllowFileAccess = true;
                        view.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                        view.Settings.UseWideViewPort = true;
                        view.Settings.LoadWithOverviewMode = true;
                        view.Settings.SetSupportZoom(false);
                        view.Settings.BuiltInZoomControls = false;
                        view.Settings.DisplayZoomControls = false;
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