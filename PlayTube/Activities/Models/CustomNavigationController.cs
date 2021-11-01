using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using PlayTube.Activities.Tabbes;
using PlayTube.Helpers.Ads;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace PlayTube.Activities.Models
{
    public class CustomNavigationController : Java.Lang.Object, View.IOnClickListener
    {
        private readonly Activity MainContext;

        private int PageNumber;
         
        public readonly List<Fragment> FragmentListTab0 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab1 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab2 = new List<Fragment>();
        public readonly List<Fragment> FragmentListTab3 = new List<Fragment>(); 

        private readonly TabbedMainActivity Context;

        private LinearLayout MainLayout;
        private LinearLayout HomeButton, TrendButton, LibraryButton, MoviesButton;
        private ImageView HomeImage, LibraryImage, TrendImage, MoviesImage;

        public CustomNavigationController(Activity activity)
        {
            try
            {
                MainContext = activity;

                if (activity is TabbedMainActivity cont)
                    Context = cont;

                Initialize();

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void Initialize()
        {
            try
            {
                MainLayout = MainContext.FindViewById<LinearLayout>(Resource.Id.llMain);
                HomeButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llHome);
                TrendButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llTrend);
                MoviesButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llMovies);
                LibraryButton = MainContext.FindViewById<LinearLayout>(Resource.Id.llLibrary);

                if (!UserDetails.IsLogin)
                {
                    MoviesButton.Visibility = ViewStates.Gone;
                    LibraryButton.Visibility = ViewStates.Gone;

                    MainLayout.WeightSum = 2;
                }
                else
                {
                    if (!AppSettings.ShowArticle)
                    {
                        MoviesButton.Visibility = ViewStates.Gone;
                        MainLayout.WeightSum = 4;
                    }
                }

                HomeImage = MainContext.FindViewById<ImageView>(Resource.Id.ivHome);
                LibraryImage = MainContext.FindViewById<ImageView>(Resource.Id.ivLibrary);
                TrendImage = MainContext.FindViewById<ImageView>(Resource.Id.ivTrend);
                MoviesImage = MainContext.FindViewById<ImageView>(Resource.Id.ivMovies);

                HomeButton.SetOnClickListener(this);
                TrendButton.SetOnClickListener(this);

                if (UserDetails.IsLogin)
                {
                    if (AppSettings.ShowArticle)
                        MoviesButton.SetOnClickListener(this);

                    LibraryButton.SetOnClickListener(this);
                }
            } 
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnClick(View v)
        {
            try
            {
                switch (v.Id)
                {
                    case Resource.Id.llHome:
                        EnableNavigationButton(HomeImage);
                        PageNumber = 0;
                        ShowFragment0();
                        AdsGoogle.Ad_AppOpenManager(MainContext);
                        break;

                    case Resource.Id.llTrend:
                        EnableNavigationButton(TrendImage);
                        PageNumber = 1;
                        ShowFragment1();
                        AdsGoogle.Ad_Interstitial(MainContext);
                        break;
                    case Resource.Id.llMovies:
                        EnableNavigationButton(MoviesImage);
                        PageNumber = 2;
                        ShowFragment2();
                        AdsGoogle.Ad_RewardedVideo(MainContext);
                        break;
                    case Resource.Id.llLibrary:
                        EnableNavigationButton(LibraryImage);
                        PageNumber = 3;
                        ShowFragment3();
                        AdsGoogle.Ad_RewardedInterstitial(MainContext);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void EnableNavigationButton(ImageView image)
        {
            try
            {
                DisableAllNavigationButton();
                image.Background = MainContext.GetDrawable(Resource.Drawable.shape_bg_bottom_navigation);
             
                image.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisableAllNavigationButton()
        {
            try
            {
                HomeImage.Background = null;
                HomeImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                LibraryImage.Background = null;
                LibraryImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
             
                TrendImage.Background = null;
                TrendImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MoviesImage.Background = null;
                MoviesImage.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public Fragment GetSelectedTabBackStackFragment()
        {
            switch (PageNumber)
            {
                case 0:
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 1:
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 2:
                    {
                        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 3:
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    } 
                default:
                    return null!;

            }

            return null!;
        }

        public int GetCountFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                        return FragmentListTab0.Count > 1 ? FragmentListTab0.Count : 0;
                    case 1:
                        return FragmentListTab1.Count > 1 ? FragmentListTab1.Count : 0;
                    case 2:
                        return FragmentListTab2.Count > 1 ? FragmentListTab2.Count : 0;
                    case 3:
                        return FragmentListTab3.Count > 1 ? FragmentListTab3.Count : 0; 
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }

        public static void HideFragmentFromList(List<Fragment> fragmentList, FragmentTransaction ft)
        {
            try
            {
                if (fragmentList.Count < 0) 
                    return;

                foreach (var fra in fragmentList)
                {
                    if (fra.IsVisible)
                        ft.Hide(fra);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DisplayFragment(Fragment newFragment)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft); 

                switch (PageNumber)
                {
                    case 0:
                    {
                        if (!FragmentListTab0.Contains(newFragment))
                            FragmentListTab0.Add(newFragment);
                        break;
                    }
                    case 1:
                    {
                        if (!FragmentListTab1.Contains(newFragment))
                            FragmentListTab1.Add(newFragment);
                        break;
                    }
                    case 2:
                    {
                        if (!FragmentListTab2.Contains(newFragment))
                            FragmentListTab2.Add(newFragment);
                        break;
                    }
                    case 3:
                    {
                        if (!FragmentListTab3.Contains(newFragment))
                            FragmentListTab3.Add(newFragment);
                        break;
                    } 
                }

                if (!newFragment.IsAdded)
                    ft.Add(Resource.Id.mainFragmentHolder, newFragment, newFragment.Id.ToString());

                ft.Show(newFragment)?.Commit();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void RemoveFragment(Fragment oldFragment)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                switch (PageNumber)
                {
                    case 0:
                    {
                        if (FragmentListTab0.Contains(oldFragment))
                            FragmentListTab0.Remove(oldFragment);
                        break;
                    }
                    case 1:
                    {
                        if (FragmentListTab1.Contains(oldFragment))
                            FragmentListTab1.Remove(oldFragment);
                        break;
                    }
                    case 2:
                    {
                        if (FragmentListTab2.Contains(oldFragment))
                            FragmentListTab2.Remove(oldFragment);
                        break;
                    }
                    case 3:
                    {
                        if (FragmentListTab3.Contains(oldFragment))
                            FragmentListTab3.Remove(oldFragment);
                        break;
                    } 
                }


                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab2, ft);
                HideFragmentFromList(FragmentListTab3, ft);  

                if (oldFragment.IsAdded)
                    ft.Remove(oldFragment);

                switch (PageNumber)
                {
                    case 0:
                        {
                            var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 1:
                        {
                            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 2:
                        {
                            var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        }
                    case 3:
                        {
                            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                            ft.Show(currentFragment)?.Commit();
                            break;
                        } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnBackStackClickFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0 when FragmentListTab0.Count > 1:
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                        break;
                    }
                    case 0:
                        Context.Finish();
                        break;
                    case 1 when FragmentListTab1.Count > 1:
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                        break;
                    }
                    case 1:
                        Context.Finish();
                        break;
                    case 2 when FragmentListTab2.Count > 1:
                    {
                        var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                        break;
                    }
                    case 2:
                        Context.Finish();
                        break;
                    case 3 when FragmentListTab3.Count > 1:
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                        if (currentFragment != null)
                            RemoveFragment(currentFragment);
                        break;
                    }
                    case 3:
                        Context.Finish();
                        break; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment0()
        {
            try
            {
                if (FragmentListTab0.Count <= 0) 
                    return;
                var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment1()
        {
            try
            {
                if (FragmentListTab1.Count <= 0) return;
                var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment); 

                Context.InAppReview();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment2()
        {
            try
            {
                if (FragmentListTab2.Count <= 0) return;
                var currentFragment = FragmentListTab2[FragmentListTab2.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ShowFragment3()
        {
            try
            {
                if (FragmentListTab3.Count <= 0) return;
                var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                if (currentFragment != null)
                    DisplayFragment(currentFragment); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
           
        public static bool BringFragmentToTop(Fragment tobeshown, FragmentManager fragmentManager, List<Fragment> videoFrameLayoutFragments)
        {
            try
            {
                if (tobeshown != null)
                {
                    FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();


                    foreach (var f in fragmentManager.Fragments)
                    {
                        if (videoFrameLayoutFragments.Contains(f))
                        {
                            if (f == tobeshown)
                                fragmentTransaction.Show(f);
                            else
                                fragmentTransaction.Hide(f);
                        }
                    
                    }

                    fragmentTransaction?.Commit();

                    return true;
                }
                else
                {
                    FragmentTransaction fragmentTransaction = fragmentManager.BeginTransaction();

                    foreach (var f in videoFrameLayoutFragments)
                    {
                        fragmentTransaction.Hide(f);
                    }

                    fragmentTransaction?.Commit();
                }
                return false;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            } 
        }

    }
}