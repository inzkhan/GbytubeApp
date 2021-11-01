using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Widget;
using InAppBilling.Lib;
using PlayTube.Helpers.Utils;

namespace PlayTube.PaymentGoogle
{
    public class InitInAppBillingPayment : BillingProcessor.IBillingHandler
    {
        private readonly Activity ActivityContext;
        private string PayType;
        public BillingProcessor Handler;
        private List<SkuDetails> Products;
        private bool ReadyToPurchase = false;

        public InitInAppBillingPayment(Activity activity)
        {
            try
            {
                ActivityContext = activity;

                if (!BillingProcessor.IsIabServiceAvailable(activity, AppSettings.TripleDesAppServiceProvider))
                {
                    Console.WriteLine("In-app billing service is unavailable, please upgrade Android Market/Play to version >= 3.9.16");
                    return;
                }

                SetConnInAppBilling();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region In-App Billing Google

        public void SetConnInAppBilling()
        {
            try
            {
                if (Handler == null)
                {
                    Handler = new BillingProcessor(ActivityContext, InAppBillingGoogle.ProductId, AppSettings.TripleDesAppServiceProvider, this);
                    Handler.Initialize();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisconnectInAppBilling()
        {
            try
            {
                Handler?.Release();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitInAppBilling(string payType)
        {
            PayType = payType;
              
            if (Methods.CheckConnectivity())
            {
                if (!ReadyToPurchase || !Handler.IsInitialized())
                {
                    return;
                }

                try
                {
                    Products = Handler.GetPurchaseListingDetails(InAppBillingGoogle.ListProductSku);
                    switch (Products.Count)
                    {
                        case > 0:
                            {
                                var membership = Products.FirstOrDefault(a => a.ProductId == "membership");
                                var rentVideo = Products.FirstOrDefault(a => a.ProductId == "rentvideo");

                                switch (PayType)
                                {
                                    //membership
                                    case "membership":
                                        Handler.Purchase(ActivityContext, membership?.ProductId);
                                        break;
                                    case "RentVideo":
                                        Handler.Purchase(ActivityContext, rentVideo?.ProductId);
                                        break;
                                } 
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    Console.WriteLine("Issue connecting: " + ex);
                    Toast.MakeText(ActivityContext, "Issue connecting: " + ex, ToastLength.Long)?.Show();
                }
            }
            else
            {
                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            } 
        }

        #endregion

        public void OnProductPurchased(string productId, TransactionDetails details)
        {
            Console.WriteLine("onProductPurchased: " + productId);
        }

        public void OnPurchaseHistoryRestored()
        {
            Console.WriteLine("onPurchaseHistoryRestored");

            foreach (var sku in Handler.ListOwnedProducts())
                Console.WriteLine("Owned Managed Product: " + sku);

            //foreach (var sku in Handler.ListOwnedSubscriptions())
            //    Console.WriteLine("Owned Subscription: " + sku); 
        }

        public void OnBillingError(int errorCode, Exception error)
        {
            Console.WriteLine("onBillingError: " + errorCode + " " + error.Message);
        }

        public void OnBillingInitialized()
        {
            Console.WriteLine("onBillingInitialized");
            ReadyToPurchase = true;
        }
    }
}