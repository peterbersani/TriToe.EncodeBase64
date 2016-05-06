using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Web;
using Newtonsoft.Json;

namespace TriToe.EncodeBase64
{
    public class RegisterEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            MediaService.Saving += MediaService_Saving;
        }

        private void MediaService_Saving(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            foreach (var mediaItem in e.SavedEntities)
            {
                if (mediaItem.ContentType.Alias == "base64Image" && mediaItem.HasProperty("base64EncodedImage") && mediaItem.HasProperty("umbracoFile"))
                {
                    // Check that image is less than 75k. It's a bad idea to base64 encode big images.
                    var umbracoBytes = mediaItem.GetValue<int>("umbracoBytes");
                    if (umbracoBytes <= 76800)
                    {
                        var umbracoFile = mediaItem.GetValue("umbracoFile");
                        var base64 = string.Empty;
                        if (umbracoFile != null)
                        {
                            var uFile = umbracoFile.ToString().Trim();

                            // Checking for presence of json. This will be true if this is a crop rather than an upload file.
                            if (uFile.StartsWith("{") && uFile.EndsWith("}"))
                            {
                                dynamic d = JsonConvert.DeserializeObject(uFile);
                                uFile = d.src;
                            }

                            if (!string.IsNullOrEmpty(uFile))
                            {
                                var filePath = HttpContext.Current.Server.MapPath(uFile);
                                var encoder = new Encoder();
                                base64 = encoder.ConvertToBase64(filePath);
                            }
                        }

                        if (string.IsNullOrEmpty(base64) && mediaItem.GetValue("base64EncodedImage") != null)
                        {
                            mediaItem.SetValue("base64EncodedImage", null);
                        }
                        else
                        {
                            mediaItem.SetValue("base64EncodedImage", base64);
                        }
                    }
                }
            }
        }
    }
}
