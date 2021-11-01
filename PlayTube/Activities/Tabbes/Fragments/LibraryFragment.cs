using System;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Com.Sothree.Slidinguppanel;
using PlayTube.Activities.Library;
using PlayTube.Adapters;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace PlayTube.Activities.Tabbes.Fragments
{
    public class LibraryFragment : Fragment
    {
        #region Variables Basic

        public LibraryAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private TabbedMainActivity GlobalContext;
        public TextView NotificationButton;
        public SubscriptionsFragment SubscriptionsFragment;
        private WatchLaterVideosFragment WatchLaterVideosFragment;
        private RecentlyWatchedVideosFragment RecentlyWatchedVideosFragment;
        private WatchOfflineVideosFragment WatchOfflineVideosFragment;
        public PlayListsVideosFragment PlayListsVideosFragment;
        private LikedVideosFragment LikedVideosFragment;
        private SharedVideosFragment SharedVideosFragment;
        private PaidVideosFragment PaidVideosFragment;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (TabbedMainActivity)Activity;
                HasOptionsMenu = true;

                MAdapter ??= new LibraryAdapter(Activity);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            } 
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater?.Inflate(Resource.Layout.Library_Layout, container, false); 
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
                SetRecyclerViewAdapters();

                MAdapter.ItemClick += MAdapterOnItemClick;
                NotificationButton.Click += NotificationButton_Click;
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

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    try
                    {
                        GlobalContext.FragmentNavigatorBack();
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }

                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);

                NotificationButton = (TextView)view.FindViewById(Resource.Id.toolbar_title);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, NotificationButton, IonIconsFonts.Notifications);
                NotificationButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                NotificationButton.SetTextSize(ComplexUnitType.Sp, 20f);
                NotificationButton.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager); 
                MRecycler.SetAdapter(MAdapter);
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
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    string title = Context.GetString(Resource.String.Lbl_Library);
                    GlobalContext.SetToolBar(toolbar, title,false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

          
        private void MAdapterOnItemClick(object sender, LibraryAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position <= -1) return;

                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    switch (item.SectionId)
                    {
                        // Subscriptions
                        case "1":
                            SubscriptionsFragment = new SubscriptionsFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(SubscriptionsFragment);
                            break;
                        // Watch Later
                        case "2":
                            WatchLaterVideosFragment = new WatchLaterVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(WatchLaterVideosFragment);
                            break;
                        // Recently Watched 
                        case "3":
                            RecentlyWatchedVideosFragment = new RecentlyWatchedVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(RecentlyWatchedVideosFragment);
                            break;
                        // Watch Offline 
                        case "4":
                            WatchOfflineVideosFragment = new WatchOfflineVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(WatchOfflineVideosFragment);
                            break;
                        // PlayLists
                        case "5":
                            PlayListsVideosFragment = new PlayListsVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(PlayListsVideosFragment);
                            break;
                        // Liked
                        case "6":
                            LikedVideosFragment = new LikedVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(LikedVideosFragment);
                            break;
                        // Shared
                        case "7":
                            SharedVideosFragment = new SharedVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(SharedVideosFragment);
                            break;
                        // Paid
                        case "8":
                            PaidVideosFragment = new PaidVideosFragment();
                            GlobalContext.FragmentBottomNavigator.DisplayFragment(PaidVideosFragment);
                            break;
                    }

                    if (GlobalContext.SlidingUpPanel.GetPanelState() == SlidingUpPanelLayout.PanelState.Expanded)
                        GlobalContext.SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void NotificationButton_Click(object sender, EventArgs e)
        {
            try
            {
                NotificationFragment notificationFragment = new NotificationFragment();
                GlobalContext.FragmentBottomNavigator.DisplayFragment(notificationFragment);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

    }
}    