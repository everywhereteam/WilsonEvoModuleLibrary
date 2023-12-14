using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace WilsonEvoModuleLibrary.Utility;

public static class StringTemplate
{
    public static string Interpolate(string template, Dictionary<string, object> variables)
    {
        var jObject = JObject.FromObject(new Dictionary<string, object>(variables));

        return Regex.Replace(template, @"\{[\w\.]+\}", match =>
        {
            var path = match.Value.Trim('{', '}');
            var token = jObject.SelectToken(path);
            return token?.ToString() ?? string.Empty;
        });
    }
}