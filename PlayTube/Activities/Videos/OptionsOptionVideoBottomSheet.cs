using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Upload;
using PlayTube.Helpers.Utils;
using Exception = System.Exception;

namespace PlayTube.Activities.Videos
{
    public class OptionsOptionVideoBottomSheet : BottomSheetDialogFragment 
    {
        #region Variables Basic

        private TabbedMainActivity GlobalContext;

        private RelativeLayout LiveLayout, ImportLayout, UploadLayout;
           
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Base);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater?.Inflate(Resource.Layout.BottomSheetOptionVideoLayout, container, false);  
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                InitComponent(view); 
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                LiveLayout = view.FindViewById<RelativeLayout>(Resource.Id.LiveLayout);
                LiveLayout.Click += LiveLayoutOnClick;

                ImportLayout = view.FindViewById<RelativeLayout>(Resource.Id.ImportLayout);
                ImportLayout.Click += BtnImportOnClick;

                UploadLayout = view.FindViewById<RelativeLayout>(Resource.Id.UploadLayout);
                UploadLayout.Click += UploadLayoutOnClick;

                ImportLayout.Visibility = AppSettings.ShowButtonImport ? ViewStates.Visible : ViewStates.Gone;
                UploadLayout.Visibility = AppSettings.ShowButtonUpload ? ViewStates.Visible : ViewStates.Gone;
                LiveLayout.Visibility = AppSettings.ShowGoLive ? ViewStates.Visible : ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event
             
        //Upload 
        private void UploadLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.ShowButtonUpload)
                {
                    Activity.StartActivityForResult(new Intent(Activity, typeof(VideoUploadActivity)), 3000);
                    GlobalContext.VideoActionsController.SetStopvideo();

                    if (GlobalContext.YoutubePlayer != null && GlobalContext.YouTubePlayerEvents.IsPlaying)
                        GlobalContext.YoutubePlayer?.Pause();
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //VideoObject Import
        private void BtnImportOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (AppSettings.ShowButtonImport)
                { 
                    Activity.StartActivity(new Intent(Activity, typeof(VideoImportActivity)));

                    GlobalContext.VideoActionsController.SetStopvideo();

                    if (GlobalContext.YoutubePlayer != null && GlobalContext.YouTubePlayerEvents.IsPlaying)
                        GlobalContext.YoutubePlayer?.Pause();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Live 
        private void LiveLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.GoLiveOnClick();
                Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
}