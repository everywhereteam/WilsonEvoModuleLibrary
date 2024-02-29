using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Utility;

public static class StringTemplate
{
    public static string Interpolate(string template, SessionData data)
    {
        var jObject = JObject.FromObject(new Dictionary<string, object>(data.VarData));

        return Regex.Replace(template, @"\{[\w\.]+\}", match =>
        {
            var path = match.Value.Trim('{', '}');
            var token = jObject.SelectToken(path);
            return token?.ToString() ?? string.Empty;
        });
    }
}