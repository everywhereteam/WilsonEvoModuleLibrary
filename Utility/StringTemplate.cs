using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WilsonEvoModuleLibrary.Utility
{
    public static class StringTemplate
    {
        public static string Interpolate(string template, Dictionary<string, object> variables)
        {
            JObject jObject = JObject.FromObject(new Dictionary<string, object>(variables));

            return Regex.Replace(template, @"\{[\w\.]+\}", match =>
            {
                string path = match.Value.Trim('{', '}');
                JToken token = jObject.SelectToken(path);
                return token?.ToString() ?? string.Empty;
            });
        }
    }
}
