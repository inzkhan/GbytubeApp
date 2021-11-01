using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using TheArtOfDev.Edmodo.Cropper;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using Exception = System.Exception;
using File = Java.IO.File;
using Uri = Android.Net.Uri;
using PlayTube.Activities.Base;
using PlayTube.Activities.Tabbes;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Channel
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditMyChannelActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IListCallbackMultiChoice
    {
        #region Variables Basic
        private PublisherAdView PublisherAdView;
        private TextView NameIcon, GenderIcon, SaveTextView, FacebookIcon, TwitterIcon, GoogleIcon, AgeIcon, CountryIcon;
        private EditText TxtFirstName, TxtLastName, TxtUsername, TxtAboutChannal, TxtEmail, TxtFavCategory;

        private LinearLayout ImageAvatarLiner, ImageCoverLiner;
        private EditText TxtFacebook, TxtTwitter, TxtGoogle;
        private string GenderStatus, ImageAvatar, ImageCover, ImageType, CategoryId, CategoryName, Age, CountryId;
        private List<string> CategorySelect = new List<string>();
        private SelectionType CurrentSelectionType;

        private enum SelectionType
        {
            None,
            Gender,
            Age,
            Country,
        };

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.Edit_MyChannel_layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                Get_Data_User();

                Methods.Path.Chack_MyFolder();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Functions

        private void InitComponent()
        {
            try
            {
                NameIcon = FindViewById<TextView>(Resource.Id.Name_icon);
                TxtFirstName = FindViewById<EditText>(Resource.Id.FirstName_text);
                TxtLastName = FindViewById<EditText>(Resource.Id.LastName_text);


                TxtUsername = FindViewById<EditText>(Resource.Id.username_Edit);

                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);


                TxtAboutChannal = FindViewById<EditText>(Resource.Id.AboutChannal_Edit);


                TxtEmail = FindViewById<EditText>(Resource.Id.email_Edit);


                TxtFavCategory = FindViewById<EditText>(Resource.Id.favCategory_Edit);

                GenderIcon = FindViewById<TextView>(Resource.Id.gender_icon);

                AgeIcon = FindViewById<TextView>(Resource.Id.age_icon);
                CountryIcon = FindViewById<TextView>(Resource.Id.country_icon);
               

                ImageAvatarLiner = FindViewById<LinearLayout>(Resource.Id.ImageAvatarLiner);


                ImageCoverLiner = FindViewById<LinearLayout>(Resource.Id.ImageCoverLiner);


                FacebookIcon = FindViewById<TextView>(Resource.Id.facebook_icon);
                TxtFacebook = FindViewById<EditText>(Resource.Id.facebook_Edit);

                TwitterIcon = FindViewById<TextView>(Resource.Id.twitter_icon);
                TxtTwitter = FindViewById<EditText>(Resource.Id.twitter_Edit);

                GoogleIcon = FindViewById<TextView>(Resource.Id.google_icon);
                TxtGoogle = FindViewById<EditText>(Resource.Id.google_Edit);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, NameIcon, IonIconsFonts.Person);
                NameIcon.SetTextColor(Color.ParseColor("#8c8a8a"));


                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FacebookIcon, IonIconsFonts.LogoFacebook);
                FacebookIcon.SetTextColor(Color.ParseColor("#3b5999"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TwitterIcon, IonIconsFonts.LogoTwitter);
                TwitterIcon.SetTextColor(Color.ParseColor("#55acee"));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, GoogleIcon, IonIconsFonts.LogoGoogle);
                GoogleIcon.SetTextColor(Color.ParseColor("#dd4b39"));

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);

                Methods.SetFocusable(TxtFavCategory);
                Methods.SetFocusable(GenderIcon);
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
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_edit_MyChannal);
                    toolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                    SetSupportActionBar(toolBar);
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

                    ImageAvatarLiner.Click += ImageAvatarLinerOnClick;
                    ImageCoverLiner.Click += ImageCoverLinerOnClick;
                    SaveTextView.Click += SaveTextViewOnClick;
                    TxtFavCategory.Touch += TxtFavCategoryOnTouch;
                    GenderIcon.Touch += GenderIcon_Click;
                    AgeIcon.Touch += AgeIcon_Click;
                    CountryIcon.Touch += CountryIconOnTouch;
                }
                else
                {

                    ImageAvatarLiner.Click -= ImageAvatarLinerOnClick;
                    ImageCoverLiner.Click -= ImageCoverLinerOnClick;
                    SaveTextView.Click -= SaveTextViewOnClick;
                    TxtFavCategory.Touch -= TxtFavCategoryOnTouch;
                    GenderIcon.Touch -= GenderIcon_Click;
                    AgeIcon.Touch -= AgeIcon_Click;
                    CountryIcon.Touch -= CountryIconOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void CountryIconOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                CurrentSelectionType = SelectionType.Country;

                var countriesArray = AppTools.GetCountryList(this);

                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                var arrayAdapter = countriesArray.Select(item => item.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_Country));
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

        private void GenderIcon_Click(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;
            
                CurrentSelectionType = SelectionType.Gender;
                List<string> arrayAdapter = new List<string>();
                MaterialDialog.Builder dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                arrayAdapter.Add(GetText(Resource.String.Radio_Female));

                dialogList.Title(GetText(Resource.String.Lbl_Gender));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Cancel)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void AgeIcon_Click(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;
                
                CurrentSelectionType = SelectionType.Age;
                var arrayAdapter = Enumerable.Range(1, 99).ToList();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                dialogList.Title(GetText(Resource.String.Lbl_Age));
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Cancel)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SaveTextViewOnClick(object sender, EventArgs e)
        {
            SaveDataButtonOnClick();
        }

        #endregion

        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyChannelList?.Count == 0)
                    await ApiRequest.GetChannelData(this, UserDetails.UserId);

                var local = ListUtils.MyChannelList?.FirstOrDefault();
                if (local != null)
                {
                    if (local.Gender == "male" || local.Gender == "Male")
                    { 
                        GenderStatus = "male";
                        GenderIcon.Text = "Male";
                    }
                    else
                    {

                        GenderStatus = "female";
                        GenderIcon.Text = "Female";
                    }

                    TxtUsername.Text = local.Username;
                    TxtEmail.Text = local.Email;
                    TxtFirstName.Text = local.FirstName;
                    TxtLastName.Text = local.LastName;
                    TxtAboutChannal.Text = local.About;
                    TxtFacebook.Text = local.Facebook;
                    TxtGoogle.Text = local.Google;
                    TxtTwitter.Text = local.Twitter;
                     
                    AgeIcon.Text = local.Age == "0" ? GetText(Resource.String.Lbl_Age) : local.Age;
                    Age = local.Age;
                    
                    CountryIcon.Text = local.CountryName;
                    CountryId = local.CountryId;
                    
                    if (local?.FavCategory?.Count > 0)
                    {
                        CategorySelect = local.FavCategory;
                        foreach (var t in local.FavCategory)
                        {
                            CategoryId += t + ",";
                            CategoryName += CategoriesController.ListCategories.FirstOrDefault(q => q.Id == t)?.Name + ",";
                        }

                        TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Events
         
        private async void SaveDataButtonOnClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"settings_type", "general"},
                        {"username", TxtUsername.Text},
                        {"email", TxtEmail.Text},
                        {"first_name", TxtFirstName.Text},
                        {"last_name", TxtLastName.Text},
                        {"about", TxtAboutChannal.Text},
                        {"facebook", TxtFacebook.Text},
                        {"twitter", TxtTwitter.Text},
                        {"google", TxtGoogle.Text},
                        {"gender", GenderStatus},
                        {"age", Age},
                        {"fav_category", CategoryId},
                        {"country", CountryId}
                    };

                    var (apiResult, respond) = await RequestsAsync.Global.UpdateUserDataGeneralAsync(dictionary);
                    if (apiResult == 200)
                    {
                        if (respond is MessageObject result)
                        {
                            Console.WriteLine(result.Message);
                            var local = ListUtils.MyChannelList?.FirstOrDefault();
                            if (local != null)
                            {
                                local.Username = UserDetails.Username = TxtUsername.Text;
                                local.Email = UserDetails.Email = TxtEmail.Text;
                                local.FirstName = TxtFirstName.Text;
                                local.LastName = TxtLastName.Text;
                                local.About = TxtAboutChannal.Text;
                                local.Gender = GenderStatus;
                                local.Facebook = TxtFacebook.Text;
                                local.Twitter = TxtTwitter.Text;
                                local.Google = TxtGoogle.Text;
                                local.FavCategory = CategorySelect;
                                local.Age = Age;
                                local.CountryId = CountryId;
                                local.CountryName = CountryIcon.Text;

                                var database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyChannel(local);
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            Intent intent = new Intent();
                            SetResult(Result.Ok, intent);
                            Finish();
                        }
                    }
                    else
                    {
                        Methods.DisplayAndHudErrorResult(this, respond);
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
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Change Image Cover
        private void ImageCoverLinerOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                OpenDialogGallery("Cover");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Change Image Avatar
        private void ImageAvatarLinerOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Favorite category
        private void TxtFavCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                var arrayIndexAdapter = new int[] { };
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                var arrayAdapter = CategoriesController.ListCategories.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_ChooseFavCategory))
                    .Items(arrayAdapter)
                    .ItemsCallbackMultiChoice(arrayIndexAdapter, this)
                    .AlwaysCallMultiChoiceCallback()
                    .PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this)
                    .Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                //If its from Camera or Gallery  
                if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok && result.IsSuccessful)
                    {
                        var resultUri = result.Uri;

                        if (!string.IsNullOrEmpty(resultUri.Path))
                        { 
                            //Send image function
                            if (Methods.CheckConnectivity())
                            {
                                if (ImageType == "Avatar" || ImageType == "Cover")
                                {
                                    var ch = ListUtils.MyChannelList?.FirstOrDefault();
                                    switch (ImageType)
                                    {
                                        case "Cover":
                                        {
                                            ImageCover = resultUri.Path;
                                            if (ch != null) ch.Cover = ImageCover;
                                            UserDetails.Cover = resultUri.Path;
                                            break;
                                        }
                                        case "Avatar":
                                        {
                                            ImageAvatar = resultUri.Path;
                                            if (ch != null) ch.Avatar = ImageAvatar;
                                            UserDetails.Avatar = resultUri.Path;
                                            break;
                                        }
                                    }

                                    if (ch != null)
                                    {
                                        var sqlEntity = new SqLiteDatabase();
                                        sqlEntity.InsertOrUpdate_DataMyChannel(ch);

                                        TabbedMainActivity.GetInstance()?.MyChannelFragment?.GetDataChannelApi();
                                    }
                                     
                                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.UpdateUserDataImageAsync(resultUri.Path, ImageType.ToLower()) });

                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Image_changed_successfully), ToastLength.Short)?.Show();

                                    Intent intent = new Intent();
                                    SetResult(Result.Ok, intent);
                                    Finish();
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Short)?.Show();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 108) //Image Picker
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open Image 
                        OpenDialogGallery(ImageType);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OpenDialogGallery(string type)
        {
            try
            {
                ImageType = type;
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                }
                else
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                            CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                        {
                            Methods.Path.Chack_MyFolder();

                            //Open Image 
                            var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                            CropImage.Activity()
                                .SetInitialCropWindowPaddingRatio(0)
                                .SetAutoZoomEnabled(true)
                                .SetMaxZoom(4)
                                .SetGuidelines(CropImageView.Guidelines.On)
                                .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                                .SetOutputUri(myUri).Start(this);
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(108);
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

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (CurrentSelectionType)
                {
                    case SelectionType.Gender:
                        GenderIcon.Text = itemString;
                        GenderStatus = position == 0 ? "male" : "female";
                        break;
                    case SelectionType.Age:
                        AgeIcon.Text = itemString;
                        Age = itemString;
                        break;
                    case SelectionType.Country:
                        var countriesArray = AppTools.GetCountryList(this);
                        var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
                        if (check != null)
                        {
                            CountryId = check;
                        }

                        CountryIcon.Text = itemString; 
                        break;
                }

                CurrentSelectionType = SelectionType.None;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
        {
            try
            {
                CategoryId = "";
                CategoryName = "";
                CategorySelect = new List<string>();

                foreach (var t in which)
                {
                    CategoryId += CategoriesController.ListCategories[t].Id + ",";
                    CategoryName += CategoriesController.ListCategories[t].Name + ",";

                    CategorySelect.Add(CategoryId);
                }

                TxtFavCategory.Text = CategoryName.Remove(CategoryName.Length - 1, 1);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
            return true;
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

        #endregion

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
    }
}