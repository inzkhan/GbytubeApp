//###############################################################
// Author >> Elin Doughouz
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================
 
namespace PlayTube
{
    internal static class AppSettings
    {
        /// <summary>
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">playtubescript.com</string>
        /// </summary>
        /// 
        ///public static string TripleDesAppServiceProvider = "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";
        /// public static string TripleDesAppServiceProvider = "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";
        /// public static string TripleDesAppServiceProvider = "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";	
        public static string TripleDesAppServiceProvider = "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";
        //Main Settings >>>>> 
        //********************************************************* 
        public static string ApplicationName = "GBYTUBE";
        public static string DatabaseName = "GBYTUBEVideo";//New
        public static string Version = "2.4";

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#4ca5ff";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //true = Show Username ,  false = Show Full name
        //*********************************************************
        public static bool ShowUserPlayListVideoObject = true;

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "b8f1fcb3-2588-46c7-a62d-41fed45fe573";

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = true;
        public static bool ShowAdMobInterstitial = true;
        public static bool ShowAdMobRewardVideo = true;
        public static bool ShowAdMobNative = true;
        public static bool ShowAdMobAppOpen = true; //#New
        public static bool ShowAdMobRewardedInterstitial = true; //#New 

        public static string AdInterstitialKey = "ca-app-pub-6864724618922968/8344551078";
        public static string AdRewardVideoKey = "ca-app-pub-6864724618922968/1779142724";
        public static string AdAdMobNativeKey = "ca-app-pub-6864724618922968/2787110246";
        public static string AdAdMobAppOpenKey = "ca-app-pub-6864724618922968/2787110246"; //#New
        public static string AdRewardedInterstitialKey = "ca-app-pub-6864724618922968/9358249847"; //#New

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        public static int ShowAdMobNativeCount = 5; //#New
        public static int ShowAdMobAppOpenCount = 2; //#New
        public static int ShowAdMobRewardedInterstitialCount = 3; //#New

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false; 
        public static bool ShowFbRewardVideoAds = false; 
        public static bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828"; 
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492";  
        public static string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static bool ShowColonyBannerAds = false; //#New
        public static bool ShowColonyInterstitialAds = false; //#New
        public static bool ShowColonyRewardAds = false; //#New

        public static string AdsColonyAppId = "app6fa8d492324841b9b5"; //#New
        public static string AdsColonyBannerId = "vz8f1309aa856842248e"; //#New
        public static string AdsColonyInterstitialId = "vzd4f625080a1b4bc0be"; //#New
        public static string AdsColonyRewardedId = "vzb00816befb614810ac"; //#New

        //*********************************************************   

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml  
        //Google >> ../Properties/AndroidManifest.xml .. line 27
        //*********************************************************
        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true; 
        public static bool ShowWoWonderLogin = true; //#New

        public static readonly string AppNameWoWonder = "WoWonder";//#New
        public static readonly string WoWonderDomainUri = "https://demo.wowonder.com";//#New
        public static readonly string WoWonderAppKey = "131c471cf7adf3c3d7365838b9";//#New

       // public static readonly string ClientId = "738059222411-rt9bj4d6oa7klbjq96ai72m1cju2miol.apps.googleusercontent.com";
        public static readonly string ClientId = "738059222411-onol7b07o7gpe819rmrdaekijrfjb3dp.apps.googleusercontent.com";


        //First Page
        //*********************************************************
        public static bool ShowSkipButton = true; 

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;
        public static bool EnablePictureToPictureMode = true; 

        //Data Channal Users >> About
        //*********************************************************
        public static bool ShowEmailAccount = true;
        public static bool ShowActivities = true; 

        //Tab >> 
        //*********************************************************
        public static bool ShowArticle = true;
        public static bool ShowMovies = true;

        //Offline Watched Videos >>  
        //*********************************************************
        public static bool AllowOfflineDownload = true;
        public static bool AllowDownloadProUser = true;

        //Import && Upload Videos >>  
        //*********************************************************
        public static bool ShowButtonImport { get; set; } = true;
        public static bool ShowButtonUpload { get; set; } = true;

        //Last_Messages Page >>
        ///********************************************************* 
        public static bool RunSoundControl = true;
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds

        public static int ShowButtonSkip = 6; // 6 Seconds 

        //CategoriesVideoList
        //*********************************************************
        public static bool CategoriesVideoStyleImage = false; // Style 1 
        public static bool CategoriesVideoStyleText = true; // Style 2 

        //Set Theme App >> Color - Tab
        //*********************************************************
        public static bool SetTabDarkTheme = false;

        public static bool SetYoutubeTypeBadgeIcon = true;
        public static bool SetVimeoTypeBadgeIcon = true;
        public static bool SetDailyMotionTypeBadgeIcon = true;
        public static bool SetTwichTypeBadgeIcon = true;
        public static bool SetOkTypeBadgeIcon = true;
        public static bool SetFacebookTypeBadgeIcon = true;

        //Bypass Web Errors 
        ///*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //*********************************************************
        public static bool RenderPriorityFastPostLoad = true;

        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false;

        public static bool CompressImage = false;
        public static int AvatarSize = 60; 
        public static int ImageSize = 400;   

        public static int CountVideosTop = 13;  
        public static int CountVideosLatest = 13;  
        public static int CountVideosFav = 13;
        public static int CountVideosLive = 13;
        public static int CountVideosStock= 13;

        public static bool ShowGoLive = true; 
        public static string AppIdAgoraLive = "31ee15cd26414e689a12a8e5c23bf10e"; 
         
        //Settings 
        //*********************************************************
        public static bool ShowEditPassword = true; 
        public static bool ShowMonetization = true; //(Withdrawals)
        public static bool ShowVerification = true; 
        public static bool ShowBlockedUsers = true; 
        public static bool ShowSettingsTwoFactor = true;   
        public static bool ShowSettingsManageSessions = true;   

        public static bool ShowSettingsRateApp = true;  
        public static int ShowRateAppCount = 5;  
         
        public static bool ShowSettingsUpdateManagerApp = false; 

        public static bool ShowGoPro = true; 
        public static int PricePro = 10;  

        public static bool ShowClearHistory = true; 
        public static bool ShowClearCache = true; 
         
        public static bool ShowHelp = true; 
        public static bool ShowTermsOfUse = true; 
        public static bool ShowAbout = true; 
        public static bool ShowDeleteAccount = true;

        //********************************************************* 
        public static bool ImageCropping = true;  

        //*********************************************************
        /// <summary>
        /// Currency
        /// CurrencyStatic = true : get currency from app not api 
        /// CurrencyStatic = false : get currency from api (default)
        /// </summary>
        public static readonly bool CurrencyStatic = false; 
        public static readonly string CurrencyIconStatic = "$"; 
        public static readonly string CurrencyCodeStatic = "USD"; 

        //********************************************************* 
        public static bool RentVideosSystem = true; 
        /// <summary>
        /// RentVideos
        /// VideoRentalPriceStatic = true : Video rent becomes a fixed rate in the app >> #Compatible with InAppBilling 
        /// VideoRentalPriceStatic = false : Video rent price from api (default)       >> #Not compatible with InAppBilling just Paypal and CreditCard
        /// 
        /// VideoRentalPrice = 0.0 USD : The fixed value of the video rental price can be determined
        /// </summary> 
        public static bool VideoRentalPriceStatic = false;  
        public static int VideoRentalPrice = 50;  

        //*********************************************************  
        public static bool DonateVideosSystem = true;  

        public static bool ShowCategoriesInHome = true; 
         
        //*********************************************************  
        public static bool ShowPaypal = true;
        public static bool ShowCreditCard = true;
        public static bool ShowBankTransfer = true;  

        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false;

        //*********************************************************   
        public static bool ShowCashFree = false;   
        /// <summary>
        /// SandBox , Live
        /// </summary>
        public static string CashfreeMode = "SandBox";
        public static string CashfreeSecretKey = "";

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static string CashFreeCurrency = "INR";  

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static string RazorPayCurrency = "USD"; 

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 18 
        /// </summary>
        public static bool ShowRazorPay = false;  

        public static bool ShowPayStack = false;  
        public static bool ShowPaySera = false;   

        //*********************************************************  

        public static bool HideSubscribeForOwner = false;
        public static bool DisableYouTubeInitializationFailureMessages = true;   
        public static bool ShowVideoWithDynamicHeight = true;

        //********************************************************* 
        public static bool ShowTextWithSpace = true;//#New

    }
}