using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Mono.Reflection;

namespace i18n
{
    /// <summary>
    /// The entrypoint for internationalization features
    /// </summary>
    public class I18N
    {
        /// <summary>
        /// The default language for all localized keys; when a PO database
        /// is built, the default key file is stored at this locale location
        /// </summary>
        public static string DefaultTwoLetterISOLanguageName { get; set; }

        static I18N()
        {
            DefaultTwoLetterISOLanguageName = "en";
        }

        /// <summary>
        /// Scans source code to produce a master PO file in at the default locale location
        /// </summary>
        /// <param name="compileViews">Whether dynamic views are compiled for inspection at runtime</param>
        public static void BuildDatabase(bool compileViews = true)
        {
            var database = RebuildLocalizationDatabase(compileViews);
            SaveToDisk(database);
        }
        
        /// <summary>
        /// Registers the calling web application for automatic language
        /// URL routing based on the existing PO database
        /// </summary>
        public static void Register()
        {
            GlobalFilters.Filters.Add(new LanguageFilter());
            ApplyDecoratorToRoutes();
        }

        private static void ApplyDecoratorToRoutes()
        {
            var routes = RouteTable.Routes;
            using(routes.GetReadLock())
            {
                for (var i = 0; i < routes.Count; i++)
                {
                    routes[i] = new LanguageRouteDecorator(routes[i]);
                }
            }
        }

        private static void SaveToDisk(IEnumerable<I18NMessage> messages)
        {
            var physicalPath = HostingEnvironment.ApplicationPhysicalPath.ToLowerInvariant();
            var localePath = Path.Combine(physicalPath, "locale");
            if (!Directory.Exists(localePath))
            {
                Directory.CreateDirectory(localePath);
            }

            var defaultPath = Path.Combine(localePath, DefaultTwoLetterISOLanguageName.ToLowerInvariant());
            if (!Directory.Exists(defaultPath))
            {
                Directory.CreateDirectory(defaultPath);
            }

            var filename = Path.Combine(defaultPath, "messages.po");
            using (var fs = File.Open(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    foreach(var message in messages)
                    {
                        sw.WriteLine(message.ToString());
                    }
                }
            }
        }

        private static IEnumerable<I18NMessage> RebuildLocalizationDatabase(bool compileViews)
        {
            var rollingCount = new Dictionary<string, int>(0);
            var rollingCache = new List<string>(0);

            var localizingMethods = GetPotentialLocalizingMethods(compileViews);

            foreach (var method in localizingMethods)
            {
                if (method.GetMethodBody() == null)
                {
                    continue;
                }

                var instructions = method.GetInstructions();
                foreach (var instruction in instructions)
                {
                    var methodInfo = instruction.Operand as MethodInfo;
                    if(methodInfo == null)
                    {
                        continue;
                    }

                    if (!methodInfo.Name.Equals("_"))
                    {
                        continue;
                    }

                    var source = method.DeclaringType.Name;

                    if (rollingCount.ContainsKey(source))
                    {
                        rollingCount[source]++;
                    }
                    else
                    {
                        rollingCount.Add(source, 1);
                    }

                    var text = instruction.Previous.Operand.ToString();

                    if (rollingCache.Contains(text))
                    {
                        continue;
                    }

                    var message = new I18NMessage
                                      {
                                          Comment = string.Format("{0}:{1}", source, rollingCount[source]),
                                          MsgId = text,
                                          MsgStr = ""
                                      };
                    rollingCache.Add(text);
                    yield return message;
                }
            }
        }

        private static IEnumerable<MethodInfo> GetPotentialLocalizingMethods(bool compileViews)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GlobalAssemblyCache && !a.ReflectionOnly).ToList();
            
            var localizingTypes = assemblies.SelectMany(assembly => assembly.GetTypes().Where(t => typeof(ILocalizing).IsAssignableFrom(t))).ToList();
            
            if(compileViews)
            {
                localizingTypes.AddRange(CompileRazorViews());
            }

            return localizingTypes.SelectMany(localizingType => localizingType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsAbstract)).ToList();
        }

        private static IEnumerable<Type> CompileRazorViews()
        {
            var templates = LoadRazorTemplatePaths();
            var types = new List<Type>();
            foreach(var file in templates.Select(t=>t.ToLowerInvariant()).Where(t => !t.Contains("app_code")))
            {
                try
                {
                    var path = file;
                    var compiled = BuildManager.GetCompiledType(path);
                    Console.WriteLine(compiled);
                    types.Add(compiled);
                }
                catch (HttpCompileException)
                {
                    // Skip views that don't compile
                }
            }
            return types;
        }

        private static IEnumerable<string> LoadRazorTemplatePaths()
        {
            var physicalPath = HostingEnvironment.ApplicationPhysicalPath.ToLowerInvariant();
            var templates = Directory.GetFiles(physicalPath, "*.cshtml", SearchOption.AllDirectories).ToList();
            return templates.Select(t => t.Replace(physicalPath, "~/"));
        }
    }
}