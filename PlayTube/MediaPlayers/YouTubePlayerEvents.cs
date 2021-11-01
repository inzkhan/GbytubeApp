using System;
using System.Linq;
using Android.Views;
using AndroidX.Lifecycle;
using PlayTube.Activities.PlayersView;
using PlayTube.Activities.Tabbes;
using PlayTube.Activities.Videos;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using YouTubePlayerAndroidX.Player;

namespace PlayTube.MediaPlayers
{
    public class YouTubePlayerEvents : IYouTubePlayerListener
    {
        private readonly IYouTubePlayer PlayerView;
        private readonly string VideoIdYoutube;
        public bool IsPlaying;
        public float CurrentSecond;
        public string Type;

        public YouTubePlayerEvents(IYouTubePlayer playerView , string videoId , string type = "Normal" , float currentSecond = 0)
        {
            PlayerView = playerView;
            VideoIdYoutube = videoId;
            Type = type;
            CurrentSecond = currentSecond;
        }
        
        public void OnReady()
        {
            try
            {
                if (Type == "FullScreen")
                {
                    if (YouTubePlayerFullScreenActivity.Instance.Lifecycle.CurrentState == Lifecycle.State.Resumed)
                        PlayerView.LoadVideo(VideoIdYoutube, CurrentSecond);
                    else
                        PlayerView.CueVideo(VideoIdYoutube, CurrentSecond);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            }
        }
         
        public void OnStateChange(int state)
        {
            try
            {
                if (state == PlayerConstants.PlayerState.Ended)
                {
                    if (Type != "FullScreen")
                        OnVideoEnded();

                    IsPlaying = false;
                }
                else if (state == PlayerConstants.PlayerState.Paused)
                {
                    IsPlaying = false;
                }
                else if (state == PlayerConstants.PlayerState.Playing)
                {
                    IsPlaying = true; 
                }

                var mainActivity = TabbedMainActivity.GetInstance();
                if (mainActivity?.VideoActionsController != null)
                {
                    mainActivity.VideoActionsController.LoadingProgressBar.Visibility = ViewStates.Invisible;
                }

                var globalPlayerActivity = GlobalPlayerActivity.GetInstance();
                if (globalPlayerActivity?.VideoActionsController != null)
                {
                    globalPlayerActivity.VideoActionsController.LoadingProgressBar.Visibility = ViewStates.Invisible;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnPlaybackQualityChange(string playbackQuality)
        {
             
        }

        public void OnPlaybackRateChange(string playbackRate)
        {
             
        }

        public void OnError(int error)
        {
            IsPlaying = false;
        }

        public void OnApiChange()
        {
             
        }

        public void OnCurrentSecond(float second)
        {
            CurrentSecond = second;
        }

        public void OnVideoDuration(float duration)
        {
            
        }

        public void OnVideoLoadedFraction(float loadedFraction)
        {
            
        }

        public void OnVideoId(string videoId)
        {
            
        }

        public void OnVideoEnded()
        {
            try
            {
                if (ListUtils.ArrayListPlay.Count > 0 && UserDetails.AutoNext)
                {
                    var data = ListUtils.ArrayListPlay.FirstOrDefault();
                    if (data != null)
                    {
                        TabbedMainActivity.GetInstance()?.StartPlayVideo(data);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}