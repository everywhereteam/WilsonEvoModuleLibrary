using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Newtonsoft.Json.Linq;
using WilsonEvoCoreLibrary.Core.Models;

namespace WilsonEvoModuleLibrary.Utility;

public static class StringTemplate
{
    public delegate bool ChannelAttributeValue(TaggedValue tag);

    private static readonly Dictionary<string, Func<TaggedValue, ProcessSession, string, string>> Operations = new() { { "date-format", ParseDate }, { "regex", RegexText }, { "dotted", DottedSpacedText } };

    public static string Interpolate(string template, ProcessSession session)
    {
        var jObject = JObject.FromObject(session.Data.GetProperties());
        AddToken(jObject, "Id", session.Id);
        AddToken(jObject, "ChannelCustomerId", session.ChannelSessionId);
        AddToken(jObject, "ChannelSender", session.ChannelSender);
        AddToken(jObject, "ChannelRecipient", session.ChannelRecipient);
        AddToken(jObject, "UserLanguage", session.UserLanguage);
        AddToken(jObject, "CommunicationChannel", session.ChannelName);
        AddToken(jObject, "IsTestSession", session.IsTest);
        return Regex.Replace(template, @"\{[\w\.]+\}", match =>
        {
            var path = match.Value.Trim('{', '}');
            var token = jObject.SelectToken(path);
            return token?.ToString() ?? string.Empty;
        });
    }

    private static void AddToken(JObject jobject, string name, object value)
    {
        if (value != null && !string.IsNullOrEmpty(value.ToString()))
        {
            jobject.Add(name, JToken.FromObject(value));
        }
    }

    private static string RegexText(TaggedValue tag, ProcessSession session, string text)
    {
        var match = Regex.Match(text, tag.Value);
        return match.Success ? match.Value : "";
    }

    private static string DottedSpacedText(TaggedValue tag, ProcessSession session, string text)
    {
        // Convert the input string to a character array
        var charactersArray = text.ToCharArray();

        // Join the characters with a dot and space between them
        var stringWithSpaces = string.Join(". ", charactersArray);

        return stringWithSpaces;
    }

    private static string ParseDate(TaggedValue tag, ProcessSession session, string text)
    {
        var culture = CultureInfo.GetCultureInfo(session.UserLanguage);
        var info = DateTimeFormatInfo.GetInstance(culture);
        var dateTimePattern = @"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z\b";
        var matches = Regex.Matches(text, dateTimePattern);
        foreach (Match match in matches)
            if (DateTime.TryParse(match.Value, culture, DateTimeStyles.None, out var parsedDateTime))
            {
                var newFormattedDateTime = parsedDateTime.ToString(info.LongDatePattern, culture);
                text = Regex.Replace(text, Regex.Escape(match.Value), newFormattedDateTime);
            }

        return text;
    }

    public static string ProcessString(string text, ProcessSession session, Dictionary<string, ChannelAttributeValue>? scoreAction)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
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
                    if (Operations.ContainsKey(tag.Tag))
                    {
                        outputText = Operations[tag.Tag].Invoke(tag, session, outputText);
                    }

                builder.Append(outputText);
            }

            return builder.ToString();
        }

        return textInterpolated;
    }


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
                scoreAction ??= new Dictionary<string, ChannelAttributeValue>();
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

        return new TextParsingResult(input, false, null);
    }

    public record TaggedValue(string Tag, string Value);

    public record TextWithAttributes(string Text, float Score, List<TaggedValue> Attributes);

    public record TextParsingResult(string InitialText, bool Success, List<TextWithAttributes>? ParsedAttributes);
}