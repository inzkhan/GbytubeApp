﻿using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Renderscripts;
using Android.Widget;
using AndroidX.Core.Content;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Engine.BitmapRecycle;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Load.Resource.Drawable;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Security;
using PlayTube.Helpers.Utils;
using Exception = System.Exception;

namespace PlayTube.Helpers.CacheLoaders
{
    public enum ImageStyle
    {
        CenterCrop, CircleCrop, RoundedCrop, FitCenter, Blur
    }

    public enum ImagePlaceholders
    {
        Color, Drawable
    }

    public static class GlideImageLoader
    {
        public static void LoadImage(Activity activity, string imageUri, ImageView image, ImageStyle style, ImagePlaceholders imagePlaceholders, RequestOptions options = null)
        {
            try
            {
                if (image == null || activity?.IsDestroyed != false)
                    return;

                if (string.IsNullOrEmpty(imageUri) || string.IsNullOrWhiteSpace(imageUri))
                    imageUri = "blackdefault";

                imageUri = imageUri.Replace(" ", "");

                var newImage = Glide.With(activity);

                options ??= GetOptions(style, imagePlaceholders);

                if (AppSettings.CompressImage && style != ImageStyle.RoundedCrop)
                {
                    if (imageUri.Contains("avatar") || imageUri.Contains("Avatar"))
                        options.Override(AppSettings.AvatarSize);
                    else if (imageUri.Contains("gif"))
                        options.Override(AppSettings.ImageSize);
                    else
                        options.Override(AppSettings.ImageSize);
                }

                if (AppSettings.CompressImage)
                    options.Override(AppSettings.ImageSize);

                if (style == ImageStyle.Blur)
                {
                    //options.Transform(new BlurTransformation(activity));
                    newImage.Load(imageUri).Apply(options)
                        .Transition(DrawableTransitionOptions.WithCrossFade()) 
                        .Transform(new BlurTransformation(activity), new CenterCrop())
                        .Into(image); 
                    return;
                }

                if (imageUri.Contains("no_profile_image") || imageUri.Contains("blackdefault") || imageUri.Contains("no_profile_image_circle")
                    || imageUri.Contains("ImagePlacholder") || imageUri.Contains("ImagePlacholder_circle") || imageUri.Contains("Grey_Offline")
                    || imageUri.Contains("d-avatar"))
                {
                    if (imageUri.Contains("no_profile_image_circle"))
                        newImage.Load(Resource.Drawable.no_profile_image_circle).Apply(options).Into(image);
                    else if (imageUri.Contains("no_profile_image") || imageUri.Contains("d-avatar"))
                        newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
                    else if (imageUri.Contains("ImagePlacholder"))
                        newImage.Load(Resource.Drawable.ImagePlacholder).Apply(options).Into(image);
                    else if (imageUri.Contains("ImagePlacholder_circle"))
                        newImage.Load(Resource.Drawable.ImagePlacholder_circle).Apply(options).Into(image);
                    else if (imageUri.Contains("blackdefault"))
                        newImage.Load(Resource.Drawable.blackdefault).Apply(options).Into(image);
                    else
                        newImage.Load(Resource.Drawable.ImagePlacholder).Apply(options).Into(image);
                }
                else switch (string.IsNullOrEmpty(imageUri))
                {
                    case false when imageUri.Contains("http"):
                        newImage.Load(imageUri).Apply(options).Into(image);
                        break;
                    case false when (imageUri.Contains("file://") || imageUri.Contains("content://") || imageUri.Contains("storage") || imageUri.Contains("/data/user/0/")):
                    {
                        File file2 = new File(imageUri);
                        var photoUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", file2);
                        RequestOptions option = style == ImageStyle.CircleCrop ? new RequestOptions().CircleCrop() : new RequestOptions();
                        newImage.Load(photoUri).Apply(option).Into(image);
                        break;
                    }
                    default:
                        newImage.Load(Resource.Drawable.no_profile_image).Apply(options).Into(image);
                        break;
                    } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static RequestOptions GetOptions(ImageStyle style, ImagePlaceholders imagePlaceholders)
        {
            try
            {
                RequestOptions options = new RequestOptions();

                switch (style)
                {
                    case ImageStyle.Blur:
                    case ImageStyle.CenterCrop:
                        options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                            .CenterCrop()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder)
                            .Placeholder(Resource.Drawable.ImagePlacholder));
                        break;
                    case ImageStyle.FitCenter:
                        options = new RequestOptions().Apply(RequestOptions.CenterCropTransform()
                            .CenterCrop().FitCenter()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder)
                            .Placeholder(Resource.Drawable.ImagePlacholder));
                        break;
                    case ImageStyle.CircleCrop:
                        options = new RequestOptions().Apply(RequestOptions.CircleCropTransform()
                            .CenterCrop().CircleCrop()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder_circle)
                            .Placeholder(Resource.Drawable.ImagePlacholder_circle));
                        break;
                    case ImageStyle.RoundedCrop:
                        options = new RequestOptions().Apply(RequestOptions.CircleCropTransform()
                            .CenterCrop()
                            .Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(20)))
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder_circle)
                            .Placeholder(Resource.Drawable.ImagePlacholder_circle));
                        break; 
                    default:
                        options.CenterCrop()
                            .SetPriority(Priority.High)
                            .SetUseAnimationPool(false).SetDiskCacheStrategy(DiskCacheStrategy.All)
                            .Error(Resource.Drawable.ImagePlacholder)
                            .Placeholder(Resource.Drawable.ImagePlacholder);  
                        break;
                }

                switch (imagePlaceholders)
                {
                    case ImagePlaceholders.Color:
                        var color = Methods.FunString.RandomColor();
                        options.Placeholder(new ColorDrawable(Color.ParseColor(color))).Fallback(new ColorDrawable(Color.ParseColor(color)));
                        break;
                    case ImagePlaceholders.Drawable:
                        switch (style)
                        {
                            case ImageStyle.CircleCrop:
                                options.Placeholder(Resource.Drawable.ImagePlacholder_circle).Fallback(Resource.Drawable.ImagePlacholder_circle);
                                break;
                            default:
                                options.Placeholder(Resource.Drawable.ImagePlacholder).Fallback(Resource.Drawable.ImagePlacholder);
                                break;
                        }
                        break;
                }
                 
                return options;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new RequestOptions().CenterCrop();
            }
        }

        public static RequestBuilder GetPreLoadRequestBuilder(Activity activityContext, string url, ImageStyle style)
        {
            try
            {
                if (url == null || string.IsNullOrEmpty(url))
                    return null!;

                var options = GetOptions(style, ImagePlaceholders.Drawable);

                if (url.Contains("avatar"))
                    options.CircleCrop();

                if (url.Contains("avatar"))
                {
                    options.Override(AppSettings.AvatarSize);
                }
                else if (url.Contains("gif"))
                {
                    options.Override(AppSettings.ImageSize);
                }
                else
                {
                    options.Override(AppSettings.ImageSize);
                }

                options.SetDiskCacheStrategy(DiskCacheStrategy.All);
                 
                return Glide.With(activityContext)
                    .Load(url)
                    .Apply(options);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }

        }

        public static RequestOptions GetRequestOptions(ImageStyle style, ImagePlaceholders imagePlaceholders)
        {
            try
            {
                var options = new RequestOptions();
                 
                switch (style)
                {
                    case ImageStyle.CenterCrop:
                        options.CenterCrop();
                        break;
                    case ImageStyle.FitCenter:
                        options.FitCenter();
                        break;
                    case ImageStyle.CircleCrop:
                        options.CircleCrop();
                        break;
                    case ImageStyle.RoundedCrop:
                        options.Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(20)));
                        break;

                    default:
                        options.CenterCrop();
                        break;
                }
                 
                switch (imagePlaceholders)
                {
                    case ImagePlaceholders.Color:
                        var color = Methods.FunString.RandomColor();
                        options.Placeholder(new ColorDrawable(Color.ParseColor(color))).Fallback(new ColorDrawable(Color.ParseColor(color)));
                        break;
                    case ImagePlaceholders.Drawable:
                        options.Placeholder(Resource.Drawable.ImagePlacholder).Fallback(Resource.Drawable.ImagePlacholder);
                        break;
                }

                return options;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new RequestOptions();

            } 
        }
         
        public class BlurTransformation : BitmapTransformation
        {
            private readonly RenderScript RenderScript;

            public BlurTransformation(Context context)
            {
                RenderScript = RenderScript.Create(context); 
            }
           
            protected override Bitmap Transform(IBitmapPool pool, Bitmap toTransform, int outWidth, int outHeight)
            { 
                Bitmap blurredBitmap = toTransform.Copy(Bitmap.Config.Argb8888, true);

                Bitmap outputBitmap = Bitmap.CreateBitmap(blurredBitmap);
                //RenderScript renderScript = RenderScript.Create(ActivityContext);
                Allocation tmpIn = Allocation.CreateFromBitmap(RenderScript, blurredBitmap);
                Allocation tmpOut = Allocation.CreateFromBitmap(RenderScript, outputBitmap);
                //Intrinsic Gausian blur filter
                ScriptIntrinsicBlur theIntrinsic = ScriptIntrinsicBlur.Create(RenderScript, Element.U8_4(RenderScript));
                theIntrinsic.SetRadius(25);
                theIntrinsic.SetInput(tmpIn);
                theIntrinsic.ForEach(tmpOut);
                tmpOut.CopyTo(outputBitmap);
                return outputBitmap;
            }

            public override void UpdateDiskCacheKey(MessageDigest p0)
            {
              
            } 
        } 
    }
}