//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PlayTube 12/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using PlayTube.Helpers.Utils;
using MaterialDialogsCore;
using PlayTube.Activities.Base;
using PlayTube.Activities.Channel;
using PlayTube.Activities.Models;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Upgrade;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTubeClient;
using PlayTubeClient.RestCalls;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.SettingsPreferences
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SettingsActivity : BaseActivity, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private RelativeLayout InfoUserContainer;
        private ImageView ImageUser;
        private TextView ChannelName, IconVerified, CountSubscribers;

        private FrameLayout EditChannalLayout, WithdrawalsLayout, GoProLayout, VerificationLayout, BlockedUsersLayout;
        private FrameLayout PasswordLayout, TwoFactorLayout, ManageSessionsLayout;
        private FrameLayout ThemeLayout, PictureInPictureLayout;

        private Switch SwitchPip;

        private FrameLayout ClearHistoryLayout, ClearCacheLayout;
        private FrameLayout RateOurAppLayout, AboutAppLayout, TermsOfUseLayout, HelpLayout;
        private FrameLayout DeleteAccountLayout, LogoutLayout;

        private string OpenEvent;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Settings_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                LoadDataSettings();
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

        protected override void OnDestroy()
        {
            try
            {
                if (OpenEvent == "Profile")
                {
                    TabbedMainActivity globalContext = TabbedMainActivity.GetInstance();
                    globalContext.MyChannelFragment = new MyChannelFragment();
                    globalContext?.FragmentBottomNavigator.DisplayFragment(globalContext?.MyChannelFragment); 
                }

                base.OnDestroy();
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
                InfoUserContainer = FindViewById<RelativeLayout>(Resource.Id.info_container);
                ImageUser = FindViewById<ImageView>(Resource.Id.image);
                ChannelName = FindViewById<TextView>(Resource.Id.ChannelName);
                IconVerified = FindViewById<TextView>(Resource.Id.IconVerified);
                CountSubscribers = FindViewById<TextView>(Resource.Id.countSubscribers);

                EditChannalLayout = FindViewById<FrameLayout>(Resource.Id.layoutEditChannal);
                WithdrawalsLayout = FindViewById<FrameLayout>(Resource.Id.layoutWithdrawals);
                GoProLayout = FindViewById<FrameLayout>(Resource.Id.layoutGoPro);
                VerificationLayout = FindViewById<FrameLayout>(Resource.Id.layoutVerification);
                BlockedUsersLayout = FindViewById<FrameLayout>(Resource.Id.layoutBlockedUsers);

                PasswordLayout = FindViewById<FrameLayout>(Resource.Id.layoutPassword);
                TwoFactorLayout = FindViewById<FrameLayout>(Resource.Id.layoutTwoFactor);
                ManageSessionsLayout = FindViewById<FrameLayout>(Resource.Id.layoutManageSessions);

                ThemeLayout = FindViewById<FrameLayout>(Resource.Id.layoutTheme);
                PictureInPictureLayout = FindViewById<FrameLayout>(Resource.Id.layoutPictureInPicture);
                SwitchPip = FindViewById<Switch>(Resource.Id.SwitchPIP);

                ClearHistoryLayout = FindViewById<FrameLayout>(Resource.Id.layoutClearHistory);
                ClearCacheLayout = FindViewById<FrameLayout>(Resource.Id.layoutClearCache);

                RateOurAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutRateOurApp);
                AboutAppLayout = FindViewById<FrameLayout>(Resource.Id.layoutAboutApp);
                TermsOfUseLayout = FindViewById<FrameLayout>(Resource.Id.layoutTermsOfUse);
                HelpLayout = FindViewById<FrameLayout>(Resource.Id.layoutHelp);

                DeleteAccountLayout = FindViewById<FrameLayout>(Resource.Id.layoutDeleteAccount);
                LogoutLayout = FindViewById<FrameLayout>(Resource.Id.layoutLogout);
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
                    toolbar.Title = GetString(Resource.String.Lbl_Settings);
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
                    InfoUserContainer.Click += InfoUserContainerOnClick;
                    EditChannalLayout.Click += EditChannalLayoutOnClick;
                    WithdrawalsLayout.Click += WithdrawalsLayoutOnClick;
                    GoProLayout.Click += GoProLayoutOnClick;
                    VerificationLayout.Click += VerificationLayoutOnClick;
                    BlockedUsersLayout.Click += BlockedUsersLayoutOnClick;
                    PasswordLayout.Click += PasswordLayoutOnClick;
                    TwoFactorLayout.Click += TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click += ManageSessionsLayoutOnClick;
                    ThemeLayout.Click += ThemeLayoutOnClick;
                    SwitchPip.CheckedChange += SwitchPIPOnCheckedChange;
                    ClearHistoryLayout.Click += ClearHistoryLayoutOnClick;
                    ClearCacheLayout.Click += ClearCacheLayoutOnClick;
                    RateOurAppLayout.Click += RateOurAppLayoutOnClick;
                    AboutAppLayout.Click += AboutAppLayoutOnClick;
                    TermsOfUseLayout.Click += TermsOfUseLayoutOnClick;
                    HelpLayout.Click += HelpLayoutOnClick;
                    DeleteAccountLayout.Click += DeleteAccountLayoutOnClick;
                    LogoutLayout.Click += LogoutLayoutOnClick;
                }
                else
                {
                    InfoUserContainer.Click -= InfoUserContainerOnClick;
                    EditChannalLayout.Click -= EditChannalLayoutOnClick;
                    WithdrawalsLayout.Click -= WithdrawalsLayoutOnClick;
                    GoProLayout.Click -= GoProLayoutOnClick;
                    VerificationLayout.Click -= VerificationLayoutOnClick;
                    BlockedUsersLayout.Click -= BlockedUsersLayoutOnClick;
                    PasswordLayout.Click -= PasswordLayoutOnClick;
                    TwoFactorLayout.Click -= TwoFactorLayoutOnClick;
                    ManageSessionsLayout.Click -= ManageSessionsLayoutOnClick;
                    ThemeLayout.Click -= ThemeLayoutOnClick;
                    SwitchPip.CheckedChange -= SwitchPIPOnCheckedChange;
                    ClearHistoryLayout.Click -= ClearHistoryLayoutOnClick;
                    ClearCacheLayout.Click -= ClearCacheLayoutOnClick;
                    RateOurAppLayout.Click -= RateOurAppLayoutOnClick;
                    AboutAppLayout.Click -= AboutAppLayoutOnClick;
                    TermsOfUseLayout.Click -= TermsOfUseLayoutOnClick;
                    HelpLayout.Click -= HelpLayoutOnClick;
                    DeleteAccountLayout.Click -= DeleteAccountLayoutOnClick;
                    LogoutLayout.Click -= LogoutLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void InfoUserContainerOnClick(object sender, EventArgs e)
        {
            try
            { 
                if (UserDetails.IsLogin)
                {
                    OpenEvent = "Profile";
                    Finish();
                }
                else
                {
                    PopupDialogController dialog = new PopupDialogController(this, null, "Login");
                    dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Start_signin), GetText(Resource.String.Lbl_Yes), GetText(Resource.String.Lbl_No));
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EditChannalLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(EditMyChannelActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void WithdrawalsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(MonetizationActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void GoProLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(GoProActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void VerificationLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(VerificationActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BlockedUsersLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(BlockedUsersActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void PasswordLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(PasswordActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TwoFactorLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(TwoFactorAuthActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ManageSessionsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ManageSessionsActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ThemeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                dialogList.Title(Resource.String.Lbl_Night_Mode);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                if ((int)Build.VERSION.SdkInt >= 29)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SwitchPIPOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                UserDetails.PipIsChecked = e.IsChecked;

                MainSettings.SharedData?.Edit()?.PutBoolean("picture_in_picture_key", UserDetails.PipIsChecked)?.Commit();

                if (!AppTools.CheckPictureInPictureAllowed(this) && UserDetails.PipIsChecked)
                {
                    var intent = new Intent("android.settings.PICTURE_IN_PICTURE_SETTINGS", Android.Net.Uri.Parse("package:" + PackageName));
                    StartActivityForResult(intent, 8520);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ClearHistoryLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                new LibrarySynchronizer(this).RemoveRecentlyWatched();

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Video.DeleteHistoryVideosAsync() });
                else
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();

                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long)?.Show();

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ClearCacheLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                dialog.Title(GetText(Resource.String.Lbl_Warning));
                dialog.Content(GetText(Resource.String.Lbl_TheFilesWillBeDeleted));
                dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_FilesAreNowDeleted), ToastLength.Long)?.Show();

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var dirPath = CacheDir;
                            dirPath.Delete();

                            string path = Methods.Path.FolderDcimMyApp;
                            if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }

                            Methods.Path.Chack_MyFolder();
                        }
                        catch (Exception e)
                        {
                            Methods.DisplayReportResultTrack(e);
                        }
                    });
                });
                dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(new MyMaterialDialog());
                dialog.AlwaysCallSingleChoiceCallback();
                dialog.Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void RateOurAppLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StoreReviewApp store = new StoreReviewApp();
                store.OpenStoreReviewPage(PackageName);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AboutAppLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/terms/about-us");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_about));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TermsOfUseLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/terms/terms");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_Termsofuse));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void HelpLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializePlayTube.WebsiteUrl + "/contact-us");
                intent.PutExtra("Type", GetText(Resource.String.Lbl_Help));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DeleteAccountLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                PopupDialogController dialog = new PopupDialogController(this, null, "DeleteAcount");
                dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Are_you_sure_you_went_to_delete_account) + " " + AppSettings.ApplicationName, GetText(Resource.String.Lbl_Ok), GetText(Resource.String.Lbl_Cancel));

            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LogoutLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                PopupDialogController dialog = new PopupDialogController(this, null, "Logout");
                dialog.ShowNormalDialog(GetText(Resource.String.Lbl_Warning), GetText(Resource.String.Lbl_Are_you_sure_you_went_to_logout), GetText(Resource.String.Lbl_Ok), GetText(Resource.String.Lbl_Cancel));

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

                string getValue = MainSettings.SharedData?.GetString("Night_Mode_key", string.Empty);

                if (text == GetString(Resource.String.Lbl_Light) && getValue != MainSettings.LightMode)
                {
                    //Set Light Mode   
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_Light);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.LightMode)?.Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(TabbedMainActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();
                    OverridePendingTransition(0, 0);
                    StartActivity(intent);
                }
                else if (text == GetString(Resource.String.Lbl_Dark) && getValue != MainSettings.DarkMode)
                {
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_Dark);

                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DarkMode)?.Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(TabbedMainActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.NoAnimation);
                    FinishAffinity();
                    OverridePendingTransition(0, 0);
                    StartActivity(intent);
                }
                else if (text == GetString(Resource.String.Lbl_SetByBattery) && getValue != MainSettings.DefaultMode)
                {
                    //NightMode.Summary = this.GetString(Resource.String.Lbl_SetByBattery);
                    MainSettings.SharedData?.Edit()?.PutString("Night_Mode_key", MainSettings.DefaultMode)?.Commit();

                    if ((int)Build.VERSION.SdkInt >= 29)
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                        var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                        switch (currentNightMode)
                        {
                            case UiMode.NightNo:
                                // Night mode is not active, we're using the light theme
                                AppSettings.SetTabDarkTheme = false;
                                break;
                            case UiMode.NightYes:
                                // Night mode is active, we're using dark theme
                                AppSettings.SetTabDarkTheme = true;
                                break;
                        }
                    }
                    else
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAutoBattery;

                        var currentNightMode = Resources?.Configuration?.UiMode & UiMode.NightMask;
                        switch (currentNightMode)
                        {
                            case UiMode.NightNo:
                                // Night mode is not active, we're using the light theme
                                AppSettings.SetTabDarkTheme = false;
                                break;
                            case UiMode.NightYes:
                                // Night mode is active, we're using dark theme
                                AppSettings.SetTabDarkTheme = true;
                                break;
                        }

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            Window?.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window?.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(this, typeof(SplashScreenActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        intent.AddFlags(ActivityFlags.NoAnimation);
                        FinishAffinity();
                        OverridePendingTransition(0, 0);
                        StartActivity(intent);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }
         
        #endregion

        private void LoadDataSettings()
        {
            try
            {
                var dataUser = ListUtils.MyChannelList?.FirstOrDefault();
                if (dataUser != null)
                {
                    GlideImageLoader.LoadImage(this, dataUser.Avatar, ImageUser, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                    ChannelName.Text = AppTools.GetNameFinal(dataUser);
                    //Verified 
                    if (dataUser.Verified == "1")
                    {
                        IconVerified.Visibility = ViewStates.Visible;
                        FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconVerified, IonIconsFonts.CheckmarkCircle);
                    }
                    else
                    {
                        IconVerified.Visibility = ViewStates.Gone;
                    }

                    CountSubscribers.Text = dataUser.SubscribeCount;
                }

                //Delete Preference
                //============== Account_Profile ===================

                if (!AppSettings.ShowMonetization)
                    WithdrawalsLayout.Visibility = ViewStates.Gone;

                var isPro = dataUser?.IsPro ?? "0";
                if (!AppSettings.ShowGoPro || isPro != "0")
                    GoProLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowVerification)
                    VerificationLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowBlockedUsers)
                    BlockedUsersLayout.Visibility = ViewStates.Gone;

                //============== SecurityAccount ===================

                if (!AppSettings.ShowEditPassword)
                    PasswordLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsTwoFactor)
                    TwoFactorLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowSettingsManageSessions)
                    ManageSessionsLayout.Visibility = ViewStates.Gone;

                //============== CategoryGeneral ===================

                if ((int)Build.VERSION.SdkInt <= 23)
                    PictureInPictureLayout.Visibility = ViewStates.Gone;

                //============== History_Privacy ===================

                if (!AppSettings.ShowClearHistory)
                    ClearHistoryLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowClearCache)
                    ClearCacheLayout.Visibility = ViewStates.Gone;

                //============== Support ===================

                if (!AppSettings.ShowSettingsRateApp)
                    RateOurAppLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowAbout)
                    AboutAppLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowHelp)
                    HelpLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowTermsOfUse)
                    TermsOfUseLayout.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowDeleteAccount)
                    DeleteAccountLayout.Visibility = ViewStates.Gone;

                SwitchPip.Checked = UserDetails.PipIsChecked;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}