using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class WebPreviewMetadata
    {
        public Dictionary<string, string> Properties { get; private set; }
        public string Redirect { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AppIdITunes { get; set; }
        public string AppIdPlay { get; set; }
        public string ImageUrl { get; set; }

        public WebPreviewMetadata()
        {
            Properties = new Dictionary<string, string>();
        }

        public WebPreviewMetadata WithProperty(string property, string value)
        {
            Properties.Add(property, value);
            return this;
        }
        public WebPreviewMetadata WithTitle(string value)
        {
            Title = value;
            Properties.Add("og:title", value);
            return this;
        }

        public WebPreviewMetadata WithDescription(string value)
        {
            Description = value;
            Properties.Add("og:description", value);
            return this;
        }

        public WebPreviewMetadata WithRedirect(string targetUrl)
        {
            Redirect = targetUrl;
            return this;
        }

        public WebPreviewMetadata WithAppIdITunes(string appIdITunes)
        {
            AppIdITunes = appIdITunes;
            return this;
        }

        public WebPreviewMetadata WithAppIdPlay(string appIdPlay)
        {
            AppIdPlay = appIdPlay;
            return this;
        }

        public WebPreviewMetadata WithImage(string imageUrl)
        {
            ImageUrl = imageUrl;
            Properties.Add("og:image", imageUrl);
            return this;
        }

        //public ActionResult AsViewResult()
        //{
        //    if (!Properties.ContainsKey("og:type")) Properties.Add("og:type", "website");

        //    return new ViewResult() {
        //        ViewName = "WebPreview",
        //        ViewData = new ViewDataDictionary(
        //            new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
        //            new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary()) { Model = this }
        //    };
        //}
    }
}
