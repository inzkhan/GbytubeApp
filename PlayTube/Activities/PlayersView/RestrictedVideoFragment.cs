using System;
using System.Collections.Generic;
using MaterialDialogsCore;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.Payment;
using PlayTubeClient.Classes.Global;
using Exception = System.Exception;

namespace PlayTube.Activities.PlayersView
{
    public class RestrictedVideoFragment : Fragment, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallback
    {
        public TextView RestrictedTextView;
        public ImageView ImageVideo, RestrictedIcon;
        public Button PurchaseButton;
        private VideoObject VideoObject;
        private string RestrictionText, ImageUrl;
        private TabbedMainActivity GlobalContext;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            GlobalContext = TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater?.Inflate(Resource.Layout.RestrictedVideoLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                //Get Value And Set Toolbar
                InitComponent(view);

                if (!string.IsNullOrWhiteSpace(RestrictionText) && VideoObject != null)
                    LoadRestriction(RestrictionText, ImageUrl, VideoObject); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent(View view)
        {
            try
            {
                GlobalContext = TabbedMainActivity.GetInstance();
                ImageVideo = (ImageView)view.FindViewById(Resource.Id.Imagevideo);
                RestrictedIcon = (ImageView)view.FindViewById(Resource.Id.restrictedIcon);
                RestrictedTextView = (TextView)view.FindViewById(Resource.Id.restrictedTextview);
                PurchaseButton = (Button)view.FindViewById(Resource.Id.purchaseButton);
                PurchaseButton.Visibility = ViewStates.Gone;

                PurchaseButton.Click += PurchaseButtonOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PurchaseButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (UserDetails.IsLogin)
                {
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                    if (AppSettings.ShowPaypal)
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_Paypal));

                    if (AppSettings.ShowCreditCard)
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_CreditCard));

                    if (AppSettings.ShowBankTransfer)
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_BankTransfer));
                
                    var type = PurchaseButton.Tag?.ToString();
                    if (AppSettings.ShowRazorPay && (type == "RentVideo" || type == "Subscriber")) //wael add purchaseVideo on the api
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_RazorPay));

                    if (AppSettings.ShowPayStack && (type == "RentVideo" || type == "Subscriber"))
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_PayStack));

                    if (AppSettings.ShowCashFree && (type == "RentVideo" || type == "Subscriber"))
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_CashFree));

                    if (AppSettings.ShowPaySera && (type == "RentVideo" || type == "Subscriber"))
                        arrayAdapter.Add(Activity.GetString(Resource.String.Lbl_PaySera));

                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(Activity.GetString(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(Activity, VideoObject, "Login");
                    dialog.ShowNormalDialog(Activity.GetText(Resource.String.Lbl_Warning), Activity.GetText(Resource.String.Lbl_Please_sign_in_Paid), Activity.GetText(Resource.String.Lbl_Yes), Activity.GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void HideRestrictedInfo(bool hide)
        {
            try
            {
                if (RestrictedTextView != null)
                    RestrictedTextView.Visibility = hide ? ViewStates.Gone : ViewStates.Visible;
               
                if (RestrictedIcon != null)
                    RestrictedIcon.Visibility = hide ? ViewStates.Gone : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            } 
        }

        public void UpdateRestrictionData(string strtext, string imageUrl, VideoObject videoObject)
        {
            try
            {
                RestrictionText = strtext;
                ImageUrl = imageUrl;
                VideoObject = videoObject;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void LoadRestriction(string strtext, string imageUrl, VideoObject videoObject)
        {
            try
            {
                HideRestrictedInfo(false);
                VideoObject = videoObject;
                switch (strtext)
                {
                    case "AgeRestriction":
                    {
                        RestrictedIcon.SetImageResource(Resource.Drawable.icon_18plus);
                        RestrictedTextView.Text = GetText(Resource.String.Lbl_AgeRestricted);
                        if (!string.IsNullOrEmpty(imageUrl))
                            GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        break;
                    }
                    case "GeoRestriction":
                    {
                        RestrictedIcon.SetImageResource(Resource.Drawable.ic_GeoRestict);
                        RestrictedTextView.Text = GetText(Resource.String.Lbl_GEORestricted);
                        if (!string.IsNullOrEmpty(imageUrl))
                            GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                        break;
                    }
                    case "purchaseVideo":
                    {
                        RestrictedIcon.SetImageResource(Resource.Drawable.ic_action_dollars);
                        RestrictedTextView.Text = GetText(Resource.String.Lbl_purchaseVideo);
                        if (!string.IsNullOrEmpty(imageUrl))
                            GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                        var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                        Console.WriteLine(currency);
                        PurchaseButton.Visibility = ViewStates.Visible;
                        PurchaseButton.Text = GetText(Resource.String.Lbl_Purchase) + " " + currencyIcon + videoObject.SellVideo;
                        PurchaseButton.Tag = "purchaseVideo";
                        break;
                    }
                    case "RentVideo":
                    {
                        RestrictedIcon.SetImageResource(Resource.Drawable.ic_action_dollars);
                        RestrictedTextView.Text = GetText(Resource.String.Lbl_RentVideo);
                        if (!string.IsNullOrEmpty(imageUrl))
                            GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                        var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                        Console.WriteLine(currency);
                        PurchaseButton.Visibility = ViewStates.Visible;
                        PurchaseButton.Text = GetText(Resource.String.Lbl_Rent) + " " + currencyIcon + videoObject.RentPrice;
                        PurchaseButton.Tag = "RentVideo";
                        break;
                    }
                    // SubscribeChannel
                    case "Subscriber":
                    {
                        RestrictedIcon.SetImageResource(Resource.Drawable.ic_action_dollars);

                        var idCurrency = ListUtils.MySettingsList?.PaymentCurrency;
                        var (currency, currencyIcon) = AppTools.GetCurrency(idCurrency);
                        Console.WriteLine(currency);
                     
                        RestrictedTextView.Text = Activity.GetText(Resource.String.Lbl_SubscribeFor) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice + " " + Activity.GetText(Resource.String.Lbl_AndUnlockAllTheVideos);
                        if (!string.IsNullOrEmpty(imageUrl))
                            GlideImageLoader.LoadImage(Activity, imageUrl, ImageVideo, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        PurchaseButton.Visibility = ViewStates.Visible;
                        PurchaseButton.Text = Activity.GetText(Resource.String.Lbl_Subscribe) + " " + currencyIcon + videoObject.Owner?.OwnerClass?.SubscriberPrice;
                        PurchaseButton.Tag = "Subscriber";
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        #region MaterialDialog

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

        public async void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                var type = PurchaseButton.Tag?.ToString();

                switch (type)
                {
                    case "purchaseVideo":
                    {
                        GlobalContext.StopFragmentVideo();
                        GlobalContext.PayType = "purchaseVideo";
                        GlobalContext.Price = VideoObject.SellVideo;
                        GlobalContext.PaymentVideoObject = VideoObject;

                        if (text == Activity.GetString(Resource.String.Lbl_Paypal))
                        {
                            GlobalContext.InitPayPalPayment.BtnPaypalOnClick(VideoObject.SellVideo, "purchaseVideo");
                        }
                        else if (text == Activity.GetString(Resource.String.Lbl_CreditCard))
                        {
                            OpenIntentCreditCard();
                        }
                        else if (text == Activity.GetString(Resource.String.Lbl_BankTransfer))
                        {
                            OpenIntentBankTransfer();
                        }
                        else if (text == GetString(Resource.String.Lbl_RazorPay))
                        {
                            GlobalContext.InitRazorPay?.BtnRazorPayOnClick(VideoObject.RentPrice, "purchaseVideo", VideoObject.Id);
                        }
                        else if (text == GetString(Resource.String.Lbl_PayStack))
                        {
                            var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_VerificationFailed), Activity.GetText(Resource.String.Lbl_IsEmailValid), Activity.GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await GlobalContext.VideoDataWithEventsLoader.PayStack(s.ToString(), VideoObject.RentPrice, "purchaseVideo", VideoObject.Id);
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
                        else if (text == GetString(Resource.String.Lbl_CashFree))
                        {
                            GlobalContext.VideoDataWithEventsLoader.OpenCashFreeDialog(VideoObject.RentPrice, "purchaseVideo", VideoObject.Id);
                        }
                        else if (text == GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await GlobalContext.VideoDataWithEventsLoader.PaySera(VideoObject.RentPrice, "purchaseVideo", VideoObject.Id);
                        }

                        break;
                    }
                    case "RentVideo":
                    {
                        GlobalContext.StopFragmentVideo();
                        GlobalContext.PayType = "RentVideo";
                        GlobalContext.Price = VideoObject.RentPrice;
                        GlobalContext.PaymentVideoObject = VideoObject;

                        if (text == Activity.GetString(Resource.String.Lbl_Paypal))
                        {
                            GlobalContext.InitPayPalPayment.BtnPaypalOnClick(VideoObject.SellVideo, "RentVideo");
                        }
                        else if (text == Activity.GetString(Resource.String.Lbl_CreditCard))
                        {
                            OpenIntentCreditCard();
                        }
                        else if (text == Activity.GetString(Resource.String.Lbl_BankTransfer))
                        {
                            OpenIntentBankTransfer();
                        }
                        else if (text == GetString(Resource.String.Lbl_RazorPay))
                        {
                            GlobalContext.InitRazorPay?.BtnRazorPayOnClick(VideoObject.RentPrice, "RentVideo", VideoObject.Id);
                        }
                        else if (text == GetString(Resource.String.Lbl_PayStack))
                        {
                            var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_VerificationFailed), Activity.GetText(Resource.String.Lbl_IsEmailValid), Activity.GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await GlobalContext.VideoDataWithEventsLoader.PayStack(s.ToString(), VideoObject.RentPrice, "RentVideo", VideoObject.Id);
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
                        else if (text == GetString(Resource.String.Lbl_CashFree))
                        {
                            GlobalContext.VideoDataWithEventsLoader.OpenCashFreeDialog(VideoObject.RentPrice, "RentVideo", VideoObject.Id);
                        }
                        else if (text == GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                            await GlobalContext.VideoDataWithEventsLoader.PaySera(VideoObject.RentPrice, "RentVideo", VideoObject.Id);
                        }

                        break;
                    }
                    case "Subscriber":
                    {
                        GlobalContext.StopFragmentVideo();

                        GlobalContext.Price = VideoObject.Owner?.OwnerClass?.SubscriberPrice;
                        GlobalContext.PayType = "Subscriber";

                        if (text == Activity.GetString(Resource.String.Lbl_Paypal))
                        {
                            GlobalContext.InitPayPalPayment.BtnPaypalOnClick(VideoObject.Owner?.OwnerClass?.SubscriberPrice, "Subscriber");
                        }
                        else if (text == Activity.GetString(Resource.String.Lbl_CreditCard))
                        {
                            OpenIntentCreditCard();
                        }
                        else if (itemString == GetString(Resource.String.Lbl_BankTransfer))
                        {
                            OpenIntentBankTransfer(); 
                        }
                        else if (itemString == GetString(Resource.String.Lbl_RazorPay))
                        {
                            GlobalContext.InitRazorPay?.BtnRazorPayOnClick(VideoObject.Owner?.OwnerClass?.SubscriberPrice, "Subscriber", VideoObject.Owner?.OwnerClass?.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PayStack))
                        {
                            var dialogBuilder = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);
                            dialogBuilder.Title(Resource.String.Lbl_PayStack);
                            dialogBuilder.Input(Resource.String.Lbl_Email, 0, false, async (materialDialog, s) =>
                            {
                                try
                                {
                                    if (s.Length <= 0) return;

                                    var check = Methods.FunString.IsEmailValid(s.ToString().Replace(" ", ""));
                                    if (!check)
                                    {
                                        Methods.DialogPopup.InvokeAndShowDialog(Activity, Activity.GetText(Resource.String.Lbl_VerificationFailed), Activity.GetText(Resource.String.Lbl_IsEmailValid), Activity.GetText(Resource.String.Lbl_Ok));
                                        return;
                                    }

                                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();

                                    await GlobalContext.VideoDataWithEventsLoader.PayStack(s.ToString(), VideoObject.Owner?.OwnerClass?.SubscriberPrice, "Subscriber", VideoObject.Owner?.OwnerClass?.Id);
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            });
                            dialogBuilder.InputType(InputTypes.TextVariationEmailAddress);
                            dialogBuilder.PositiveText(Activity.GetText(Resource.String.Lbl_PayNow)).OnPositive(this);
                            dialogBuilder.NegativeText(Activity.GetText(Resource.String.Lbl_Cancel)).OnNegative(new MyMaterialDialog());
                            dialogBuilder.AlwaysCallSingleChoiceCallback();
                            dialogBuilder.Build().Show();
                        }
                        else if (itemString == GetString(Resource.String.Lbl_CashFree))
                        {
                            GlobalContext.VideoDataWithEventsLoader.OpenCashFreeDialog(VideoObject.Owner?.OwnerClass?.SubscriberPrice, "Subscriber", VideoObject.Owner?.OwnerClass?.Id);
                        }
                        else if (itemString == GetString(Resource.String.Lbl_PaySera))
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Please_wait), ToastLength.Short)?.Show();
                            await GlobalContext.VideoDataWithEventsLoader.PaySera(VideoObject.Owner?.OwnerClass?.SubscriberPrice, "Subscriber", VideoObject.Owner?.OwnerClass?.Id);
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

        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(PaymentCardDetailsActivity));

                switch (PurchaseButton.Tag?.ToString())
                {
                    case "purchaseVideo":
                        intent.PutExtra("Id", VideoObject.Id);
                        intent.PutExtra("Price", VideoObject.SellVideo);
                        intent.PutExtra("payType", "purchaseVideo");
                        break;
                    case "RentVideo":
                        intent.PutExtra("Id", VideoObject.Id);
                        intent.PutExtra("Price", VideoObject.RentPrice);
                        intent.PutExtra("payType", "RentVideo");
                        break;
                    case "Subscriber":
                        intent.PutExtra("Id", VideoObject.Owner?.OwnerClass?.Id);
                        intent.PutExtra("Price", VideoObject.Owner?.OwnerClass?.SubscriberPrice);
                        intent.PutExtra("payType", "Subscriber");
                        break;
                }
                
                Activity.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(Activity, typeof(PaymentLocalActivity));
                switch (PurchaseButton.Tag?.ToString())
                {
                    case "purchaseVideo":
                        intent.PutExtra("Id", VideoObject.Id);
                        intent.PutExtra("Price", VideoObject.SellVideo);
                        intent.PutExtra("payType", "purchaseVideo");
                        break;
                    case "RentVideo":
                        intent.PutExtra("Id", VideoObject.Id);
                        intent.PutExtra("Price", VideoObject.RentPrice);
                        intent.PutExtra("payType", "RentVideo");
                        break;
                    case "Subscriber":
                        intent.PutExtra("Id", VideoObject.Owner?.OwnerClass?.Id);
                        intent.PutExtra("Price", VideoObject.Owner?.OwnerClass?.SubscriberPrice);
                        intent.PutExtra("payType", "Subscriber");
                        break;
                }

                Activity.StartActivity(intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
    }
}