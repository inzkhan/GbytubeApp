using System;
using System.Linq;
using MaterialDialogsCore;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using Exception = System.Exception;

namespace PlayTube.Activities.Stock
{
    public class StockFilterDialogFragment : BottomSheetDialogFragment, MaterialDialog.IListCallback 
    {
        #region Variables Basic

        private TextView IconBack, IconSearchTerm, IconLicenseType, IconPrice;
        private EditText TxtSearchTerm, TxtLicenseType, TxtPriceMin, TxtPriceMax;
        private Button BtnApply;
        private string TypeDialog, LicenseTypeId;
        private readonly StockVideosFragment ContextStock;

        #endregion

        public StockFilterDialogFragment(StockVideosFragment stockActivity)
        {
            ContextStock = stockActivity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.StockFilterLayout, container, false);

                InitComponent(view);

                IconBack.Click += IconBackOnClick;
                BtnApply.Click += BtnApplyOnClick;
                TxtLicenseType.Touch += TxtLicenseTypeOnTouch;

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
                
                IconLicenseType = view.FindViewById<TextView>(Resource.Id.IconLicenseType);
                TxtLicenseType = view.FindViewById<EditText>(Resource.Id.LicenseTypeEditText);

                IconPrice = view.FindViewById<TextView>(Resource.Id.IconPrice);
                TxtPriceMin = view.FindViewById<EditText>(Resource.Id.MinimumEditText);
                TxtPriceMax = view.FindViewById<EditText>(Resource.Id.MaximumEditText);

                BtnApply = view.FindViewById<Button>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.ArrowBack);

                IconSearchTerm.Visibility = ViewStates.Gone;
                IconLicenseType.Visibility = ViewStates.Gone;
                IconPrice.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(TxtSearchTerm, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLicenseType, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMin, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPriceMax, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtLicenseType);
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
                UserDetails.StockSearchTerm = TxtSearchTerm.Text;
                UserDetails.StockLicenseType = LicenseTypeId;
                UserDetails.StockPriceMin = TxtPriceMin.Text;
                UserDetails.StockPriceMax = TxtPriceMax.Text;

                ContextStock.MAdapter.VideoList.Clear();
                ContextStock.MAdapter.NotifyDataSetChanged();

                ContextStock.SwipeRefreshLayout.Refreshing = true;
                ContextStock.SwipeRefreshLayout.Enabled = true;

                ContextStock.MainScrollEvent.IsLoading = false;

                ContextStock.MRecycler.Visibility = ViewStates.Visible;
                ContextStock.EmptyStateLayout.Visibility = ViewStates.Gone;

                ContextStock.StartApiService();

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void TxtLicenseTypeOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "LicenseType";

                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsCore.Theme.Dark : MaterialDialogsCore.Theme.Light);

                var arrayAdapter = AppTools.GetLicenseTypeStockList(Activity).Select(cat => cat.Value).ToList();

                dialogList.Title(GetText(Resource.String.Lbl_LicenseType));
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

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString; 
                if (TypeDialog == "LicenseType")
                {
                    LicenseTypeId = AppTools.GetLicenseTypeStockList(Activity).FirstOrDefault(a => a.Value == text).Key;
                    TxtLicenseType.Text = text;
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