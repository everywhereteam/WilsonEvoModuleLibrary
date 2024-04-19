using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using WilsonEvoModuleLibrary.Entities;

namespace WilsonEvoModuleLibrary.Utility;

public static class StringTemplate
{
    public static string Interpolate(string template,SessionData data)
    {
        var jObject = JObject.FromObject(new Dictionary<string, object>(data.Data));
        AddToken(jObject, "Id", data.Id);
        AddToken(jObject, "ChannelCustomerId", data.ChannelCustomerId);
        AddToken(jObject, "ChannelSender", data.ChannelSender);
        AddToken(jObject, "ChannelRecipient", data.ChannelRecipient);
        AddToken(jObject, "UserLanguage", data.UserLanguage);
        AddToken(jObject, "CommunicationChannel", data.CommunicationChannel);
        AddToken(jObject, "IsTestSession", data.IsTestSession);
        return Regex.Replace(template, @"\{[\w\.]+\}", match =>
        {
            var path = match.Value.Trim('{', '}');
            var token = jObject.SelectToken(path);
            return token?.ToString() ?? string.Empty;
        });
    }

    private static void AddToken(JObject jobject, string name, object value){
        if (value != null && !string.IsNullOrEmpty(value.ToString()))
            jobject.Add(name, JToken.FromObject(value));
    }

    private static Dictionary<string, Func<TaggedValue, SessionData, string, string>> Operations = new()
    {
        {"date-format",ParseDate},
        {"regex", RegexText},
        {"dotted", DottedSpacedText},
        
    };

    private static string RegexText(TaggedValue tag, SessionData session, string text)
    {
        var match = Regex.Match(text, tag.Value);
        return match.Success ? match.Value : "";
    }
    private static string DottedSpacedText(TaggedValue tag, SessionData session, string text)
    {
        // Convert the input string to a character array
        char[] charactersArray = text.ToCharArray();

        // Join the characters with a dot and space between them
        string stringWithSpaces = string.Join(". ", charactersArray);

        return stringWithSpaces;
    }
    private static string ParseDate(TaggedValue tag, SessionData session, string text)
    {
        var culture = CultureInfo.GetCultureInfo(session.UserLanguage);
        var info = DateTimeFormatInfo.GetInstance(culture);
        var dateTimePattern = @"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z\b";
        var matches = Regex.Matches(text, dateTimePattern);
        foreach (Match match in matches)
        {
            if (DateTime.TryParse(match.Value, culture, DateTimeStyles.None, out var parsedDateTime))
            {
                var newFormattedDateTime = parsedDateTime.ToString(info.LongDatePattern, culture);
                text = Regex.Replace(text, Regex.Escape(match.Value), newFormattedDateTime);
            }
        }
        return text;
    }

    public delegate bool ChannelAttributeValue(TaggedValue tag);
    public static string ProcessString(string text, SessionData session, Dictionary<string, ChannelAttributeValue>? scoreAction)
    {
        //1° Step add values in {}
        var textInterpolated = Interpolate(text, session);
        //parse the the text for special rules
        var textAttributes = ParseStringAttributes(textInterpolated, "text", scoreAction);
        //if the result is right pick that one
        if (textAttributes is { Success: true, ParsedAttributes: not null })
        {
            var builder = new StringBuilder();
            foreach (var attribute in textAttributes.ParsedAttributes)
            {
                var outputText = attribute.Text;
                foreach (var tag in attribute.Attributes)
                {
                    if (Operations.ContainsKey(tag.Tag))
                    {
                        outputText = Operations[tag.Tag].Invoke(tag, session, outputText);
                    }
                }

                builder.Append(outputText);
            }
            return builder.ToString();
        }
        else
        {
            return textInterpolated;
        }
    }

    public record TaggedValue(string Tag, string Value);

    public record TextWithAttributes(string Text, float Score, List<TaggedValue> Attributes);

    public record TextParsingResult(string InitialText, bool Success, List<TextWithAttributes>? ParsedAttributes);


    public static TextParsingResult ParseStringAttributes(string input, string tag, Dictionary<string, ChannelAttributeValue>? scoreAction)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new TextParsingResult(input, false, null);
        }
        if (Regex.IsMatch(input, "<.*?>"))
        {
            if (!input.TrimStart().StartsWith("<root>"))
            {
                input = $"<root>{input}</root>";
            }
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(input);

                var textNodes = xmlDoc.GetElementsByTagName(tag);
                var result = new List<TextWithAttributes>();
                scoreAction ??= new();
                foreach (XmlNode node in textNodes)
                {
                    var score = 0f;
                    var textAttributes = new List<TaggedValue>();
                    var shouldAdd = true;
                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        var tagValue = new TaggedValue(attribute.LocalName, attribute.Value);
                        textAttributes.Add(tagValue);
                        if (scoreAction.TryGetValue(tagValue.Tag, out var func))
                        {
                            if (!func.Invoke(tagValue))
                            {
                                shouldAdd = false;
                            }
                        }
                    }

                    if (shouldAdd)
                    {
                        result.Add(new TextWithAttributes(node.InnerText, score, textAttributes));
                    }
                }

                return new TextParsingResult(input, true, result);
            }
            catch (Exception e)
            {
                return new TextParsingResult(input, false, null);
            }
        }
        else
        {
            return new TextParsingResult(input, false, null);
        }
    }
}