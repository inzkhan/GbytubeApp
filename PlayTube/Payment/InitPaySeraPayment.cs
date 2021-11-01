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
    public class InitPaySeraPayment
    {
        private readonly Activity ActivityContext;
        private Dialog PaySeraWindow;
        private WebView HybridView;
        private string Url, Price, PayType, Id;
        public InitPaySeraPayment(Activity context)
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

        public void DisplayPaySeraPayment(string url, string payType, string price, string id)
        {
            try
            {
                Url = url;
                Price = price;
                PayType = payType;
                Id = id;

                PaySeraWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                PaySeraWindow?.SetContentView(Resource.Layout.PaymentWebViewLayout);

                var title = (TextView)PaySeraWindow?.FindViewById(Resource.Id.toolbar_title);
                if (title != null)
                    title.Text = ActivityContext.GetText(Resource.String.Lbl_PayWith) + " " + ActivityContext.GetText(Resource.String.Lbl_PaySera);

                var closeButton = (TextView)PaySeraWindow?.FindViewById(Resource.Id.toolbar_close);
                if (closeButton != null)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, closeButton, IonIconsFonts.Close);

                    closeButton.SetTextSize(ComplexUnitType.Sp, 20f);
                    closeButton.Click += CloseButtonOnClick;
                }

                HybridView = PaySeraWindow?.FindViewById<WebView>(Resource.Id.LocalWebView);

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

                PaySeraWindow?.Show();
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
                PaySeraWindow?.Hide();
                PaySeraWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void StopPaySera()
        {
            try
            {
                if (PaySeraWindow != null)
                {
                    PaySeraWindow?.Hide();
                    PaySeraWindow?.Dismiss();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task PaySera(string request)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var keyValues = new Dictionary<string, string>();

                    switch (request)
                    {
                        case "pro":
                            break;
                        case "buy_video":
                            keyValues.Add("pay_type", "rent");
                            keyValues.Add("video_id", Id);
                            break;
                        case "subscribe":
                            keyValues.Add("subscribe_id", Id);
                            break;
                    } 

                    var (apiStatus, respond) = await RequestsAsync.Payment.PaySera(request, keyValues);
                    if (apiStatus == 200)
                    {
                        switch (request)
                        { 
                            case "pro":
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
                            case "subscribe":
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

                        StopPaySera();
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
            private readonly InitPaySeraPayment MActivity;
            public MyWebViewClient(InitPaySeraPayment mActivity)
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
                        //https://demo.wowonder.com/requests.php?f=PaySera&s=fund&amount=12&fund_id=403&trxref=5f3f88e2a43c7&reference=5f3f88e2a43c7

                        var reference = request.Url.ToString()?.Split("&reference=")?.Last();
                        if (string.IsNullOrEmpty(reference))
                            return false;

                        switch (MActivity.PayType)
                        {
                            case "pro":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PaySera("pro") });
                                break;
                            case "rent":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PaySera("buy_video") });
                                break; 
                            case "subscribe":
                            case "SubscriberVideo":
                            case "Subscriber":
                            case "check_subscribe":
                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MActivity.PaySera("subscribe") });
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