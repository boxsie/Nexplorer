using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nexplorer.Config;
using Nexplorer.Core;
using Nexplorer.Data.Cache.Services;
using Nexplorer.Web.Extensions;

namespace Nexplorer.Web.Controllers
{
    public class WebControllerBase : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var redisCommand = (RedisCommand)context.HttpContext.RequestServices.GetService(typeof(RedisCommand));

            ViewBag.NodeVersion = redisCommand.Get<string>(Settings.Redis.NodeVersion);

            var actionDesc = (ControllerActionDescriptor)context.ActionDescriptor;
            var pageKey = $"{actionDesc.ControllerName.ToLower()}.{actionDesc.ActionName.ToLower()}";

            ViewBag.VendorJs = WebExtensions.JsUrls["vendor"];
            ViewBag.ValidateJs = WebExtensions.JsUrls["validate"];

            ViewBag.LayoutJs = WebExtensions.JsUrls["layout"];
            ViewBag.LayoutCss = WebExtensions.CssUrls["layout"];

            if (WebExtensions.JsUrls.ContainsKey(pageKey))
                ViewBag.ControllerJs = WebExtensions.JsUrls[pageKey];

            if (WebExtensions.CssUrls.ContainsKey(pageKey))
                ViewBag.ControllerCss = WebExtensions.CssUrls[pageKey];

            base.OnActionExecuting(context);
        }
    }
}
