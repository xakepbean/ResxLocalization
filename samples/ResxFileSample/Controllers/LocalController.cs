using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using System.IO;
using ResxFileSample.Models;
using System.Xml;
using System.Xml.Linq;
using System.Text.Encodings.Web;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Resources;
using System.Collections;
using Xakep.Extensions.Localization;
using Xakep.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace ResxFileSample.Controllers
{
    public class LocalController:Controller
    {
        private IHostingEnvironment _env = null;
        LocalRequestLocalizationOptions _options;
        public LocalController(IHostingEnvironment env, IOptions<LocalRequestLocalizationOptions> options)
        {
            _env = env;
            _options = options.Value.GetReload();
        }

        public ActionResult Add()
        {
            //System.Globalization.
            return View();
        }

        [HttpPost]
        public ActionResult Add(string Culture, string AliaName)
        {
            _options.SupportedAlias.Add(new AliasCulture() { Alia=AliaName, Enabled=true, Name=Culture });
            string SaveJson= _options.ToJson();
            System.IO.File.WriteAllText(Path.Combine(_env.ContentRootPath, "resxlocalization.json"), SaveJson);
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            //var XOptions = HttpContext.RequestServices.GetService<IOptions<LocalRequestLocalizationOptions>>();
            return View(_options.SupportedAlias.Select(w=> new AliasCultureModel() { Name=w.Name, Alia=w.Alia,Enabled=w.Enabled }).ToList());
        }

        public List<TreeNode> ResxFile(string ID)
        {
            string CultureName = ID;
            var locOptions = HttpContext.RequestServices.GetService<IOptions<FileLocalizationOptions>>();
            string ResourcesPath = locOptions.Value.ResourcesPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)[0];
            
            List<TreeNode> ReturnTree = new List<TreeNode>();
            if (string.IsNullOrWhiteSpace(CultureName))
            {
                ReturnTree.Add(new TreeNode() { id = ResourcesPath, name = ResourcesPath, isParent = true });
            }
            else
            {
                CultureName += ".";
                var vResource = Assembly.GetEntryAssembly().GetManifestResourceNames().Select(w => w.Substring(w.IndexOf('.') + 1)).Where(w => w.StartsWith(CultureName));
                ReturnTree.AddRange(vResource.Select(w =>
                {
                    var vID = w.Substring(0, w.IndexOf('.', CultureName.Length));
                    var vName = vID.Substring(CultureName.Length);
                    var vLast = w.Substring(vID.Length);
                    vName += vLast.Length > 10 ? "" : ".resx";
                    return new TreeNode()
                    {
                        id = vID,
                        name = vName,
                        isParent = vLast.Length > 10
                    };
                }).Distinct());

            }
            return ReturnTree;
        }

        public IActionResult EditLang(string ID)
        {
            return View(_options.SupportedAlias.Where(w=>w.Name.Equals(ID,StringComparison.OrdinalIgnoreCase))
                .Select(w=> new AliasCultureModel() { Name = w.Name, Alia = w.Alia, Enabled = w.Enabled }).First());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditLang(string ID, AliasCultureModel Model)
        {
            if (ModelState.IsValid)
            {
            }
            return View(Model);
        }

        public IActionResult Edit(string ID)
        {
            return View(ResxFile(null));
        }

        public IActionResult EditResx(string ID, string ReName)
        {
            string CultureName = ID;
            if (string.IsNullOrWhiteSpace(CultureName) || string.IsNullOrWhiteSpace(ReName))
            {
                return View(new List<ResouresEditModel>());
            }
            ViewData["ReName"] = ReName;
            return View(LoadResource(CultureName, ReName));
        }

        [HttpPost]
        public IActionResult EditResx(string ID, string ReName, List<ResouresEditModel> RMode)
        {
            string CultureName = ID;
            if (string.IsNullOrWhiteSpace(CultureName) || string.IsNullOrWhiteSpace(ReName))
            {
                return View(RMode);
            }
            ViewData["ReName"] = ReName;
            SaveResource(CultureName, ReName, RMode);
            return View(RMode);
        }

        private List<ResouresEditModel> LoadResource(string CultureName, string ResourceName)
        {
            List<ResouresEditModel> ListRes = new List<ResouresEditModel>();
            #region Source Resource
            using (var resourceStream = Assembly.GetEntryAssembly().GetManifestResourceStream(_env.ApplicationName + "." + ResourceName + ".resources"))
            {
                if (resourceStream != null)
                {
                    using (var resources = new ResourceReader(resourceStream))
                    {
                        foreach (DictionaryEntry entry in resources)
                        {
                            ListRes.Add(new ResouresEditModel
                            {
                                ResourceName = (string)entry.Key,
                                DefaultValue = (string)entry.Value
                            });
                        }
                    }
                }
            }
            #endregion
            var vResxFile = _env.ContentRootFileProvider.GetFileInfo(ResourceName.Replace('.',Path.DirectorySeparatorChar) + "." + CultureName + ".resx");
            if (vResxFile.Exists)
            {
                Dictionary<string, string> DicNew = new Dictionary<string, string>();
                using (var FStream = vResxFile.CreateReadStream())
                {
                    var d = XElement.Load(FStream);
                    foreach (var node in d.Elements("data"))
                    {
                        if (node.Attribute("name") != null && node.Element("value") != null)
                        {
                            string name = node.Attribute("name").Value;
                            string val = node.Element("value").Value;
                            if (!string.IsNullOrWhiteSpace(val) && !DicNew.ContainsKey(name))
                                DicNew.Add(name, val);
                        }
                    }
                    FStream.Dispose();
                }
                for (int i = 0; i < ListRes.Count; i++)
                {
                    if (DicNew.ContainsKey(ListRes[i].ResourceName))
                        ListRes[i].LocalizedValue = DicNew[ListRes[i].ResourceName];
                }
            }
            return ListRes;
        }

        private List<ResouresEditModel> SaveResource(string CultureName, string ResourceName, List<ResouresEditModel> RMode)
        {
            string NewMapPath = Path.Combine(_env.ContentRootPath, ResourceName.Replace('.', Path.DirectorySeparatorChar) + "." + CultureName + ".resx");
            var resDoc = new XElement("Init");
            if (!System.IO.File.Exists(NewMapPath))
            {
                string[] strDic = ResourceName.Split('.');
                for (int i = 0; i < strDic.Length; i++)
                {
                    var DicName = Path.Combine(_env.ContentRootPath, string.Join(Path.DirectorySeparatorChar.ToString(), strDic, 0, i + 1));
                    if (!System.IO.Directory.Exists(DicName))
                        System.IO.Directory.CreateDirectory(DicName);
                }
                resDoc = XElement.Parse("<?xml version=\"1.0\" encoding=\"utf-8\"?><root><resheader name=\"resmimetype\"><value>text/microsoft-resx</value></resheader><resheader name=\"version\"><value>2.0</value></resheader><resheader name=\"reader\"><value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader><resheader name=\"writer\"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value></resheader></root>");
            }
            else
            {
                resDoc = XElement.Load(NewMapPath);
            }

            var changedResources = new Dictionary<string, string>();
            foreach (var di in RMode)
            {
                if (di.DefaultValue != di.LocalizedValue)
                {
                    var node = resDoc.Elements().Where(w => w.Name == "data" && w.Attribute("name") != null && w.Attribute("name").Value == di.ResourceName);
                    if (node.Count() == 0)
                    {
                        resDoc.Add(new XElement("data", new XAttribute("name", di.ResourceName), new XElement("value", di.LocalizedValue)));
                    }
                    else
                    {
                        node.First().Element("value").Value = di.LocalizedValue;
                    }
                }
            }
            if (resDoc.Elements("data").Count() > 0)
            {
                System.IO.MemoryStream NewFileStream = new MemoryStream();
                resDoc.Save(NewFileStream);
                System.IO.File.WriteAllBytes(NewMapPath, NewFileStream.ToArray());
                NewFileStream.Dispose();
            }
            return RMode;
        }

        //public IActionResult Create() => View();

        //public IActionResult CreateLang(string ID)
        //{
        //    if (!string.IsNullOrWhiteSpace(ID))
        //    {
        //        var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>();
        //        RequestCultureProvider AcceptUI = requestCulture.Provider as RequestCultureProvider;
        //        CultureInfo CInfo = new CultureInfo(ID);
        //        if (AcceptUI.Options.SupportedCultures.Where(w => w.Name == ID).Count() == 0)
        //        {
        //            AcceptUI.Options.SupportedUICultures.Add(CInfo);
        //            AcceptUI.Options.SupportedCultures.Add(CInfo);
        //        }
        //    }

        //    return RedirectToAction("Index");
        //}

        public void Redirect()
        {
            string culture = null;
            string returnUrl = null;
            if (HttpContext.Request.Form["culture"] != StringValues.Empty)
                culture = HttpContext.Request.Form["culture"].ToString();

            if (HttpContext.Request.Form["returnUrl"] != StringValues.Empty)
                returnUrl = HttpContext.Request.Form["returnUrl"].ToString();

            if (culture == null && HttpContext.Request.Query["culture"] != StringValues.Empty)
                culture = HttpContext.Request.Query["culture"].ToString();

            if (returnUrl == null && HttpContext.Request.Query["returnUrl"] != StringValues.Empty)
                returnUrl = HttpContext.Request.Query["returnUrl"].ToString();

            if (culture != null && returnUrl != null)
            {
                if (!_options.SupportedDefaultAlias && culture.Equals(_options.DefaultCulture, StringComparison.OrdinalIgnoreCase))
                {
                    HttpContext.Response.Redirect(returnUrl);
                    return;
                }
                var vSupport = _options.SupportedAlias.Where(w => w.Enabled && w.Name.Equals(culture, StringComparison.OrdinalIgnoreCase));
                if (vSupport.Count() == 0)
                {
                    HttpContext.Response.Redirect(returnUrl);
                    return;
                }
                HttpContext.Response.Redirect("/" + vSupport.First().Alia + returnUrl);
            }
        }
    }
}
