using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;

namespace Xakep.AspNetCore.Localization.TagHelpers
{
    [HtmlTargetElement("LangSelect", Attributes = "show-model")]
    public class LangSelectTagHelper : TagHelper
    {
        public EShowModel ShowModel { get; set; }

        private IOptions<LocalRequestLocalizationOptions> LocOptions { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public IUrlHelperFactory _urlHelper { get; set; }

        public LangSelectTagHelper(IOptions<LocalRequestLocalizationOptions> options, IUrlHelperFactory urlHelper)
        {
            LocOptions = options;
            _urlHelper = urlHelper;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ShowModel == EShowModel.DropDown)
            {
                output.TagName = "select";
                output.Attributes.SetAttribute("onchange", "location.href= this.value;");
            }
            else
                output.TagName = "div";
            var routeData = ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];

            var urlHelper = _urlHelper.GetUrlHelper(ViewContext);
            string menuUrl = urlHelper.Action(new UrlActionContext()
            {
                Controller = currentController.ToString(),
                Action = currentAction.ToString(),
                Values = ViewContext.RouteData.Values
            });


            var XOptions = LocOptions.Value.GetReload();
            var vAliasDefault = XOptions.SupportedAlias.Where(w => w.Enabled && w.Name.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase));
            if (vAliasDefault.Count() > 0 && menuUrl.StartsWith("/" + vAliasDefault.First().Alia))
            {
                var vLangth = "/" + vAliasDefault.First().Alia;
                if (menuUrl == vLangth)
                {
                    menuUrl = "/";
                }
                else
                {
                    vLangth += "/";
                    if (menuUrl.StartsWith(vLangth))
                    {
                        menuUrl = menuUrl.Substring(vLangth.Length - 1);
                    }
                }
            }
            if (menuUrl == "/")
                menuUrl = "";
            foreach (var item in XOptions.SupportedCultures)
            {
                var vAlias = XOptions.SupportedAlias.Where(w => w.Enabled && w.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));
                if (vAlias.Count() > 0)
                {
                    string strUrl = "/" + vAlias.First().Alia + menuUrl;
                    bool IsCurrent = vAlias.First().Name.Equals(CultureInfo.CurrentUICulture.Name, StringComparison.OrdinalIgnoreCase);
                    if (IsCurrent)
                        strUrl = menuUrl == "" ? "/" : menuUrl;

                    if (ShowModel == EShowModel.DropDown)
                    {
                        var a = new TagBuilder("option");
                        a.MergeAttribute("value", strUrl);
                        a.InnerHtml.AppendHtml(item.NativeName);
                        if (IsCurrent)
                            a.MergeAttribute("selected", "selected");
                        output.Content.AppendHtml(a);
                    }
                    else
                    {
                        var a = new TagBuilder("a");
                        a.MergeAttribute("href", strUrl);
                        a.MergeAttribute("title", item.Name);
                        a.InnerHtml.AppendHtml(item.Name + " ");
                        output.Content.AppendHtml(a);
                    }
                }
            }

           
        }
    }

    public enum EShowModel
    {
        Default,
        DropDown
    }
}
