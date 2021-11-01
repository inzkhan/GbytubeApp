using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using PlayTube.Helpers.Fonts;
using PlayTube.Helpers.Models;
using PlayTube.Helpers.Utils;
using PlayTubeClient.Classes.Global;

namespace PlayTube.Helpers.Controller
{
    public static class CategoriesController
    {
        //Categories Communities Local Custom
        public static ObservableCollection<Classes.Category> ListCategories =new ObservableCollection<Classes.Category>();
        public static ObservableCollection<Classes.Category> ListCategoriesMovies = new ObservableCollection<Classes.Category>();

        public static string GetCategoryName(VideoObject videoObject)
        {
            try
            {
                if (videoObject == null)
                    return Application.Context.GetString(Resource.String.Lbl_Unknown);
                 
                if (videoObject.IsMovie == "1")
                {
                    string name = Methods.FunString.DecodeString(ListCategoriesMovies?.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }
                else if (!string.IsNullOrEmpty(videoObject.CategoryId) && !string.IsNullOrEmpty(videoObject.SubCategory) && videoObject.SubCategory != "0")
                {
                    var category = ListCategories?.FirstOrDefault(a => a.Id == videoObject.CategoryId);
                    if (category != null)
                    {
                        string name = Methods.FunString.DecodeString(category.SubList.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
                        if (!string.IsNullOrEmpty(name))
                            return name; 
                    } 
                }
                else
                {
                    string name = Methods.FunString.DecodeString(ListCategories?.FirstOrDefault(a => a.Id == videoObject.CategoryId)?.Name);
                    if (!string.IsNullOrEmpty(name))
                        return name;
                }

                return Application.Context.GetString(Resource.String.Lbl_Unknown);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Application.Context.GetString(Resource.String.Lbl_Unknown);
            }
        }
         
        public static string GetImageCategory(string name)
        {
            if (name.Contains("Film & Animation"))
            {
                return "Cat_Film";
            }

            if (name.Contains("Cars & Vehicles"))
            {
                return "Cat_Cars";
            }

            if (name.Contains("Music"))
            {
                return "Cat_Music";
            }

            if (name.Contains("Pets & Animals"))
            {
                return "Cat_Animals";
            }

            if (name.Contains("Sports"))
            {
                return "Cat_Sport";
            }

            if (name.Contains("Travel & Events"))
            {
                return "Cat_Travel";
            }

            if (name.Contains("Gaming"))
            {
                return "Cat_Gaming";
            }

            if (name.Contains("People & Blogs"))
            {
                return "Cat_People";
            }

            if (name.Contains("Comedy"))
            {
                return "Cat_Comedy";
            }

            if (name.Contains("Entertainment"))
            {
                return "Cat_Entertainment";
            }

            if (name.Contains("News & Politics"))
            {
                return "Cat_News";
            }

            if (name.Contains("How-to & Style"))
            {
                return "Cat_Help";
            }

            if (name.Contains("Non-profits & Activism"))
            {
                return "Cat_Activism";
            }

            return "Cat_Other";
        }

        /// <summary>
        /// When the category name is changed to a language or not a name, it gives one symbol for it, and it must be changed by:
        /// https://fontawesome.com/icons
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetIconCategory(string name)
        {
            if (name.Contains("Film & Animation"))
            {
                return FontAwesomeIcon.Film;
            }

            if (name.Contains("Cars & Vehicles"))
            {
                return FontAwesomeIcon.Car;
            }

            if (name.Contains("Music"))
            {
                return FontAwesomeIcon.Music;
            }

            if (name.Contains("Pets & Animals"))
            {
                return FontAwesomeIcon.Crow;
            }

            if (name.Contains("Sports"))
            {
                return FontAwesomeIcon.VolleyballBall;
            }

            if (name.Contains("Travel & Events"))
            {
                return FontAwesomeIcon.Route;
            }

            if (name.Contains("Gaming"))
            {
                return FontAwesomeIcon.Gamepad;
            }

            if (name.Contains("People & Blogs"))
            {
                return FontAwesomeIcon.Blog;
            }

            if (name.Contains("Comedy"))
            {
                return FontAwesomeIcon.TheaterMasks;
            }

            if (name.Contains("Entertainment"))
            {
                return FontAwesomeIcon.SmileWink;
            }

            if (name.Contains("News & Politics"))
            {
                return FontAwesomeIcon.Newspaper;
            }

            if (name.Contains("How-to & Style"))
            {
                return FontAwesomeIcon.Tshirt;
            }

            if (name.Contains("Non-profits & Activism"))
            {
                return FontAwesomeIcon.Running;
            }

            return FontAwesomeIcon.FeatherAlt;
        }


    }
}