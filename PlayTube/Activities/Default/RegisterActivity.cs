using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTube.SQLite;
using PlayTubeClient;
using PlayTubeClient.Classes.Auth;
using PlayTubeClient.Classes.Global;
using PlayTubeClient.RestCalls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Text.Method;
using AndroidHUD;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
 
namespace PlayTube.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : SocialLoginBaseActivity, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private EditText TxtUsername, TxtEmail, TxtGender, TxtPassword, TxtConfirmPassword;
        private CheckBox ChkTermsOfUse;
        private TextView TxtTermsOfService;
        private Button BtnSignUp;
        private ImageView ImageShowPass, ImageShowConPass;
        private string GenderStatus = "male";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                Window?.SetSoftInputMode(SoftInput.AdjustResize);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Register_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitSocialLogins();
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

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                TxtUsername = FindViewById<EditText>(Resource.Id.etUsername);
                TxtEmail = FindViewById<EditText>(Resource.Id.etEmail);
                TxtPassword = FindViewById<EditText>(Resource.Id.etPassword);
                TxtConfirmPassword = FindViewById<EditText>(Resource.Id.etConfirmPassword);
                TxtGender = FindViewById<EditText>(Resource.Id.etGender);

                ImageShowPass = FindViewById<ImageView>(Resource.Id.imageShowPass);
                ImageShowConPass = FindViewById<ImageView>(Resource.Id.imageShowConPass);

                ChkTermsOfUse = FindViewById<CheckBox>(Resource.Id.terms_of_use);
                TxtTermsOfService = FindViewById<TextView>(Resource.Id.terms_of_service);

                BtnSignUp = FindViewById<Button>(Resource.Id.bntSignUp);

                ToggleVisibility(false);
                Methods.SetFocusable(TxtGender);

                Methods.SetColorEditText(TxtUsername, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtConfirmPassword, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtGender, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = " ";

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
                    BtnSignUp.Click += BtnSignUpOnClick;
                    ImageShowPass.Touch += ImageShowPassOnTouch;
                    ImageShowConPass.Touch += ImageShowConPassOnTouch;
                    TxtGender.Touch += TxtGenderOnTouch;
                    TxtTermsOfService.Click += TxtTermsOfServiceOnClick;
                }
                else
                {
                    BtnSignUp.Click -= BtnSignUpOnClick;
                    ImageShowPass.Touch -= ImageShowPassOnTouch;
                    ImageShowConPass.Touch -= ImageShowConPassOnTouch;
                    TxtGender.Touch -= TxtGenderOnTouch;
                    TxtTermsOfService.Click -= TxtTermsOfServiceOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                TxtUsername = null!;
                TxtEmail = null!;
                TxtGender = null!;
                TxtPassword = null!;
                TxtConfirmPassword = null!;
                ChkTermsOfUse = null!;
                TxtTermsOfService = null!;
                BtnSignUp = null!;
                GenderStatus = "male";
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Show Con Password 
        private void ImageShowConPassOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                switch (e.Event?.Action)
                {
                    case MotionEventActions.Up: // hide password
                        TxtConfirmPassword.TransformationMethod = PasswordTransformationMethod.Instance;
                        break;
                    case MotionEventActions.Down: // show password
                        TxtConfirmPassword.TransformationMethod = HideReturnsTransformationMethod.Instance;
                        break;
                    default:
                        return;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Show Password 
        private void ImageShowPassOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                switch (e.Event?.Action)
                {
                    case MotionEventActions.Up: // hide password
                        TxtPassword.TransformationMethod = PasswordTransformationMethod.Instance;
                        break;
                    case MotionEventActions.Down: // show password
                        TxtPassword.TransformationMethod = HideReturnsTransformationMethod.Instance;
                        break;
                    default:
                        return;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //start login 
        private async void BtnSignUpOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!ChkTermsOfUse.Checked)
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_You_can_not_access_your_disapproval), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (string.IsNullOrEmpty(TxtUsername.Text.Replace(" ", "")) || string.IsNullOrEmpty(TxtEmail.Text.Replace(" ", "")) || string.IsNullOrEmpty(GenderStatus) || string.IsNullOrEmpty(TxtPassword.Text) || string.IsNullOrEmpty(TxtConfirmPassword.Text))
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                var check = Methods.FunString.IsEmailValid(TxtEmail.Text.Replace(" ", ""));
                if (!check)
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                if (TxtPassword.Text != TxtConfirmPassword.Text)
                {
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Your_password_dont_match), GetText(Resource.String.Lbl_Ok));
                    return;
                }

                HideKeyboard();

                ToggleVisibility(true);

                var (apiStatus, respond) = await RequestsAsync.Auth.RegistrationAsync(TxtEmail.Text.Replace(" ", ""), TxtUsername.Text.Replace(" ", ""), TxtPassword.Text, TxtConfirmPassword.Text, GenderStatus, UserDetails.DeviceId);
                switch (apiStatus)
                {
                    case 200 when respond is RegisterObject result:

                        SetDataLogin(result, TxtPassword.Text);

                        UserDetails.IsLogin = true;

                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));

                        ToggleVisibility(false);
                        Finish();
                        break;
                    case 200:
                        {
                            if (respond is MessageObject message)
                            {
                                ToggleVisibility(false);
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), message.Message.Contains("We have sent you an email") ? GetString(Resource.String.Lbl_VerifyRegistration) : message.Message, GetText(Resource.String.Lbl_Ok));
                            }

                            break;
                        }
                    case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                ToggleVisibility(false);
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.errors.ErrorText, GetText(Resource.String.Lbl_Ok));
                            }

                            break;
                        }
                    default:
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                        break;
                }

            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Terms Of Service
        private void TxtTermsOfServiceOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = InitializePlayTube.WebsiteUrl + "/terms/terms";
                new IntentController(this).OpenBrowserFromApp(url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Radio_Male));
                arrayAdapter.Add(GetText(Resource.String.Radio_Female));

                dialogList.Title(GetText(Resource.String.Lbl_Gender));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                if (itemString == GetText(Resource.String.Radio_Male))
                {
                    TxtGender.Text = GetText(Resource.String.Radio_Male);
                    GenderStatus = "male";
                }
                else if (itemString == GetText(Resource.String.Radio_Female))
                {
                    TxtGender.Text = GetText(Resource.String.Radio_Female);
                    GenderStatus = "female";
                }
                else
                {
                    TxtGender.Text = GetText(Resource.String.Radio_Male);
                    GenderStatus = "male";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion


        protected override void ToggleVisibility(bool isLoginProgress)
        {
            try
            {
                if (isLoginProgress)
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                else
                    AndHUD.Shared.Dismiss(this);

                BtnSignUp.Visibility = isLoginProgress ? ViewStates.Invisible : ViewStates.Visible;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void SetDataLogin(RegisterObject auth, string password)
        {
            try
            {
                UserDetails.Username = TxtEmail.Text;
                UserDetails.FullName = TxtEmail.Text;
                UserDetails.Password = password;
                UserDetails.AccessToken = Current.AccessToken = auth.User.S;
                UserDetails.UserId = InitializePlayTube.UserId = auth.User.UserId.ToString();
                UserDetails.Status = "Active";
                UserDetails.Cookie = auth.User.Cookie;
                UserDetails.Email = TxtEmail.Text;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId,
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = TxtEmail.Text,
                    Password = password,
                    Status = "Active",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                };

                ListUtils.DataUserLoginList.Clear();
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetChannelData(this, UserDetails.UserId) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}