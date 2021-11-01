using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Globalization;
using System.Linq;
using MaterialDialogsCore;
using Android.Graphics;
using Google.Android.Material.BottomSheet;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using Exception = System.Exception;

namespace PlayTube.Activities.Movies
{
    public class MoviesFilterDialogFragment : BottomSheetDialogFragment, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private TextView IconBack, IconSearchTerm, IconRating, IconRelease, IconCountry, IconCategory;
        private EditText TxtSearchTerm, TxtRating, TxtRelease, TxtCountry, TxtCategory;
        private Button BtnApply;
        private string TypeDialog, CountryId, CategoryId;
        private readonly MoviesFragment ContextMovies;

        #endregion

        public MoviesFilterDialogFragment(MoviesFragment moviesActivity)
        {
            ContextMovies = moviesActivity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.MoviesFilterLayout, container, false);

                InitComponent(view);

                IconBack.Click += IconBackOnClick;
                BtnApply.Click += BtnApplyOnClick;
                TxtCountry.Touch += TxtCountryOnTouch;
                TxtCategory.Touch += TxtCategoryOnTouch;
                TxtRating.Touch += TxtRatingOnTouch;

                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                IconBack = view.FindViewById<TextView>(Resource.Id.IconBack);

                IconSearchTerm = view.FindViewById<TextView>(Resource.Id.IconSearchTerm);
                TxtSearchTerm = view.FindViewById<EditText>(Resource.Id.SearchTermEditText);

                IconRating = view.FindViewById<TextView>(Resource.Id.IconRating);
                TxtRating = view.FindViewById<EditText>(Resource.Id.RatingEditText);

                IconRelease = view.FindViewById<TextView>(Resource.Id.IconRelease);
                TxtRelease = view.FindViewById<EditText>(Resource.Id.ReleaseEditText);

                IconCountry = view.FindViewById<TextView>(Resource.Id.IconCountry);
                TxtCountry = view.FindViewById<EditText>(Resource.Id.CountryEditText);

                IconCategory = view.FindViewById<TextView>(Resource.Id.IconCategory);
                TxtCategory = view.FindViewById<EditText>(Resource.Id.CategoryEditText);
                 
                BtnApply = view.FindViewById<Button>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowBack);

                IconSearchTerm.Visibility = ViewStates.Gone; 
                IconRating.Visibility = ViewStates.Gone;
                IconRelease.Visibility = ViewStates.Gone;
                IconCountry.Visibility = ViewStates.Gone;
                IconCategory.Visibility = ViewStates.Gone; 

                Methods.SetColorEditText(TxtSearchTerm, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRating, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtRelease, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCountry, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                
                Methods.SetFocusable(TxtCountry);
                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtRating);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Save data 
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            { 
                UserDetails.MoviesSearchTerm = TxtSearchTerm.Text;
                UserDetails.MoviesRating = TxtRating.Text;
                UserDetails.MoviesRelease = TxtRelease.Text;
                UserDetails.MoviesCountry = TxtCountry.Text;
                UserDetails.MoviesCategory = TxtCategory.Text;
                 
                ContextMovies.MAdapter.MoviesList.Clear();
                ContextMovies.MAdapter.NotifyDataSetChanged();

                ContextMovies.SwipeRefreshLayout.Refreshing = true;
                ContextMovies.SwipeRefreshLayout.Enabled = true;

                ContextMovies.MainScrollEvent.IsLoading = false;
                ContextMovies.CountOffset = 1; 

                ContextMovies.MRecycler.Visibility = ViewStates.Visible;
                ContextMovies.EmptyStateLayout.Visibility = ViewStates.Gone;

                ContextMovies.StartApiService();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtCountryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Country";

                var countriesArray = AppTools.GetCountryList(Activity);

                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

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
        
        private void TxtCategoryOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                if (CategoriesController.ListCategoriesMovies.Count > 0)
                { 
                    TypeDialog = "Category";

                    var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                    var arrayAdapter = CategoriesController.ListCategoriesMovies.Select(item => Methods.FunString.DecodeString(item.Name)).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_Category));
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(Activity, "Not have List Categories Movies");
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private RatingBar RatingBar;
        private void TxtRatingOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                var dialog = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light)
                    .Title(GetText(Resource.String.Lbl_Rate)).TitleColorRes(Resource.Color.primary)
                    .CustomView(Resource.Layout.DialogRateLayout, true)
                    .PositiveText(GetText(Resource.String.Lbl_Save)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            switch (RatingBar.Rating)
                            {
                                case <= 0:
                                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Please_select_Rating), ToastLength.Short)?.Show();
                                    return;
                            }

                            TxtRating.Text = RatingBar.Rating.ToString(CultureInfo.InvariantCulture);
                        }
                        catch (Exception exception)
                        {
                            Methods.DisplayReportResultTrack(exception);
                        }
                    })
                    .NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog())
                    .Build();

                RatingBar = dialog.CustomView.FindViewById<RatingBar>(Resource.Id.ratingBar);
                RatingBar.NumStars = 5;
                 
                dialog.Show();
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
                if (TypeDialog == "Category")
                {
                    CategoryId = CategoriesController.ListCategoriesMovies.FirstOrDefault(categories => categories.Name == itemString)?.Id;
                    TxtCategory.Text = text;
                }
                else if (TypeDialog == "Country")
                {
                    var countriesArray = AppTools.GetCountryList(Activity);
                    var check = countriesArray.FirstOrDefault(a => a.Value == itemString).Key;
                    if (check != null)
                    {
                        CountryId = check;
                    }

                    TxtCountry.Text = itemString;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}