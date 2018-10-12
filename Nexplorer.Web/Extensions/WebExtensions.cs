﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexplorer.Core;
using Nexplorer.Domain.Entity.User;
using Nexplorer.Domain.Enums;
using Nexplorer.Web.Controllers;

namespace Nexplorer.Web.Extensions
{
    public static class WebExtensions
    {
        public static Dictionary<string, string> JsUrls { get; }
        public static Dictionary<string, string> CssUrls { get; }

        static WebExtensions()
        {
            const string packageJsonFilePath = "App_Data/webpack.assets.json";

            JsUrls = new Dictionary<string, string>();
            CssUrls = new Dictionary<string, string>();

            var root = JObject.Parse(File.ReadAllText(packageJsonFilePath));

            foreach (var obj in root)
            {
                foreach (var child in obj.Value.Children().Select(x => (JProperty)x))
                {
                    switch (child.Name)
                    {
                        case "js":
                            JsUrls.Add(obj.Key, $"/{(string)child.Value}");
                            break;
                        case "css":
                            CssUrls.Add(obj.Key, $"/{(string)child.Value}");
                            break;
                    }
                }
            }
        }

        public static IHtmlContent GetActiveControllerClass(this IHtmlHelper<dynamic> helper, string controllerName)
        {
            var currentControllerName = (string)helper.ViewContext.RouteData.Values["controller"];

            return currentControllerName.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase) 
                ? helper.Raw("class=\"active\"") 
                : helper.Raw("") ;
        }

        public static string ToReadableAgeString(this TimeSpan span)
        {
            return string.Format("{0:0}", span.Days / 365.25);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            var formatted = $@"{(span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty)}
                               {(span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty)}
                               {(span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty)}
                               {(span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty)}";

            if (formatted.EndsWith(", "))
                formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted))
                formatted = "0 seconds";

            return formatted;
        }

        public static string ToCurrencyString(this double val, bool isNegative = false)
        {
            var minus = isNegative ? "-" : "";

            return val > 0 
                ? $"{minus}{val:##,###.########}" 
                : "0";
        }

        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(
                action: nameof(AccountController.ResetPassword),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TAttribute>();
        }
    }
}
