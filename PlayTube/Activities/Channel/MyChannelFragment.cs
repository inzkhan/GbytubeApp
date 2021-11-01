using System;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App; 
using AndroidX.ViewPager2.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using PlayTube.Activities.Channel.Tab;
using PlayTube.Activities.SettingsPreferences;
using PlayTube.Activities.Tabbes;
using PlayTube.Adapters;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.CacheLoaders;
using PlayTube.Helpers.Controller;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Channel
{
    public class MyChannelFragment : Fragment, TabLayoutMediator.ITabConfigurationStrategy 
    {
        #region Variables Basic

        public ChPlayListFragment PlayListFragment;
        public ChVideosFragment VideosFragment;
        public ChActivitiesFragment ActivitiesFragment;
        private AppBarLayout AppBarLayout;
        private TabLayout Tabs;
        private ViewPager2 ViewPagerView;
        private Toolbar MainToolbar;
        private ImageView ImageChannel, ImageCoverChannel, IconSettings;
        private CollapsingToolbarLayout CollapsingToolbar;
        private TextView ChannelNameText, ChannelVerifiedText ;
        private TabbedMainActivity MainContext;
        private Button SubscribeChannelButton;
        private MainTabAdapter Adapter;
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext = (TabbedMainActivity)Activity;
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater?.Inflate(Resource.Layout.MyChannelFragment_Layout, container, false);
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
                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                InitTab();
                GetDataChannelApi();

                SubscribeChannelButton.Click += SubscribeChannelButtonOnClick;
                IconSettings.Click += IconSettingsOnClick;

                AdsGoogle.Ad_Interstitial(MainContext);
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

        #endregion
  
        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ImageCoverChannel = view.FindViewById<ImageView>(Resource.Id.myImagevideo);
                ImageChannel = view.FindViewById<ImageView>(Resource.Id.myChannelImage);
                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.mycollapsingToolbar);
                ChannelNameText = view.FindViewById<TextView>(Resource.Id.myChannelName);
                ChannelVerifiedText = view.FindViewById<TextView>(Resource.Id.myChannelVerifiedText);
                IconSettings = view.FindViewById<ImageView>(Resource.Id.mySettings_icon);
                SubscribeChannelButton = view.FindViewById<Button>(Resource.Id.mySubcribeChannelButton);

                Tabs = view.FindViewById<TabLayout>(Resource.Id.mychanneltabs);
                ViewPagerView = view.FindViewById<ViewPager2>(Resource.Id.myChannelviewpager);
                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mymainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                SubscribeChannelButton.Text = GetText(Resource.String.Lbl_Edit);

                ChannelVerifiedText.Visibility = ViewStates.Gone;
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ChannelVerifiedText,IonIconsFonts.CheckmarkCircle);
               
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                MainToolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(MainToolbar, " ");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitTab()
        {
            try
            {
                ViewPagerView.OffscreenPageLimit = 3;
                SetUpViewPager(ViewPagerView);
                new TabLayoutMediator(Tabs, ViewPagerView, this).Attach();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        #endregion

        #region Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                PlayListFragment = new ChPlayListFragment();
                VideosFragment = new ChVideosFragment();
                ChAboutFragment aboutFragment = new ChAboutFragment();
                ActivitiesFragment = new ChActivitiesFragment();
                 
                Bundle bundle = new Bundle(); 
                bundle.PutString("ChannelId", UserDetails.UserId);

                PlayListFragment.Arguments = bundle;
                VideosFragment.Arguments = bundle;
                ActivitiesFragment.Arguments = bundle;
                aboutFragment.Arguments = bundle;

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(VideosFragment, GetText(Resource.String.Lbl_Videos));
                Adapter.AddFragment(PlayListFragment, GetText(Resource.String.Lbl_PlayLists));

                if (AppSettings.ShowActivities)
                    Adapter.AddFragment(ActivitiesFragment, GetText(Resource.String.Lbl_Activities));

                Adapter.AddFragment(aboutFragment, GetText(Resource.String.Lbl_AboutChannal));

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));  
                viewPager.Adapter = Adapter;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly MyChannelFragment Fragment;

            public MyOnPageChangeCallback(MyChannelFragment fragment)
            {
                try
                {
                   Fragment = fragment;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    var p = position;
                    switch (p)
                    {
                        case 0:
                            break;
                        case 1:
                            Fragment?.PlayListFragment?.StartApiService();
                            break;
                        case 2:
                            break;
                        case 3:
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        #endregion

        #region Get Data Channel

        public async void GetDataChannelApi()
        {
            try
            {
                if (ListUtils.MyChannelList?.Count == 0)
                    await ApiRequest.GetChannelData(Activity, UserDetails.UserId);

                var dataChannel = ListUtils.MyChannelList?.FirstOrDefault();
                if (dataChannel != null)
                {
                    var name = AppTools.GetNameFinal(dataChannel);

                    CollapsingToolbar.Title = name;
                    ChannelNameText.Text = name;
                     
                    GlideImageLoader.LoadImage(Activity, dataChannel.Avatar, ImageChannel, ImageStyle.CircleCrop, ImagePlaceholders.Drawable); 
                    Glide.With(Activity).Load(dataChannel.Cover).Apply(new RequestOptions().FitCenter()).Into(ImageCoverChannel);

                    if (dataChannel.Verified == "1")
                        ChannelVerifiedText.Visibility = ViewStates.Visible;

                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Events

        private void SubscribeChannelButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(EditMyChannelActivity));
                Activity.StartActivityForResult(intent,252);

                MainContext.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void IconSettingsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(SettingsActivity));
                Activity.StartActivity(intent);
                MainContext.VideoActionsController.SetStopvideo();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        #endregion

    }
}