﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace BricksTwitchBot.Irc
{
    public static class MessageHandler
    {
        public static void HandleMessage(string data)
        {
            Globals.OnUi(delegate
            {
                var match = Globals.MessageMatch.Match(data);
                var paragraph = new Paragraph();
                switch (match.Groups["usertype"].Value)
                {
                    case "mod":
                    {
                        var image = Globals.FromResource("Images.Moderator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                        break;
                    case "admin":
                    {
                        var image = Globals.FromResource("Images.Admin.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                        break;
                    case "global_mod":
                    {
                        var image = Globals.FromResource("Images.GlobalModerator.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                        break;
                    case "staff":
                    {
                        var image = Globals.FromResource("Images.Staff.png");
                        paragraph.Inlines.Add(image);
                        paragraph.Inlines.Add(new Run(" "));
                    }
                        break;
                }
                if (match.Groups["isturbo"].Value.Equals("1"))
                {
                    paragraph.Inlines.Add(Globals.FromResource("Images.Turbo.png"));
                    paragraph.Inlines.Add(new Run(" "));
                }
                if (match.Groups["issub"].Value.Equals("1"))
                {
                    paragraph.Inlines.Add(Globals.FromResource("Images.Subscriber.png"));
                    paragraph.Inlines.Add(new Run(" "));
                }
                paragraph.Inlines.Add(
                    new Run((match.Groups["name"].Success
                        ? match.Groups["name"].Value
                        : match.Groups["secondname"].Value)
                        .Replace(@"\s", " "))
                    {
                        Foreground = Globals.RgbToBrush(match.Groups["color"].Value),
                        FontWeight = FontWeights.Bold
                    });

                paragraph.Inlines.Add(new Run(": "));

                var emotes = match.Groups["emote"].Value;
                var list = new List<Emote>();
                if (emotes != "")
                {
                    string[] emoteList;
                    if (!emotes.Contains("/"))
                    {
                        emoteList = new[]
                        {
                            emotes
                        };
                    }
                    else
                    {
                        emoteList = emotes.Split('/');
                    }
                    foreach (var rawEmote in emoteList)
                    {
                        var emoteParts = rawEmote.Split(':');
                        var emoteIndexString = emoteParts[1];
                        string[] emoteIndexes;
                        if (!emoteIndexString.Contains(","))
                        {
                            emoteIndexes = new[]
                            {
                                emoteIndexString
                            };
                        }
                        else
                        {
                            emoteIndexes = emoteIndexString.Split(',');
                        }
                        foreach (var index in emoteIndexes)
                        {
                            var indexSplit = index.Split('-');
                            var emote = new Emote
                            {
                                EmoteName = emoteParts[0],
                                Indexes = new List<int>()
                            };
                            for (var i = int.Parse(indexSplit[0]); i <= int.Parse(indexSplit[1]); ++i)
                            {
                                emote.Indexes.Add(i);
                            }
                            list.Add(emote);
                        }
                    }
                }
                var message = match.Groups["message"].Value;
                var finalMessage = "";
                for (int i = 0; i < message.Length; ++i)
                {
                    if (!list.Exists(s => s.Indexes.Contains(i)))
                    {
                        finalMessage += message[i].ToString();
                    }
                    else if (list.Exists(s => s.Indexes[0] == i))
                    {
                        paragraph.Inlines.Add(finalMessage);
                        finalMessage = "";
                        var image =
                            Globals.ImageFromUrl(
                                $"https://static-cdn.jtvnw.net/emoticons/v1/{ list.First(s => s.Indexes[0] == i).EmoteName}/1.0");
                        paragraph.Inlines.Add(image);
                    }
                }

                paragraph.Inlines.Add(finalMessage);

                Globals.ChatTextBoxQueue.Enqueue(paragraph);
            });
        }

        private struct Emote
        {
            public string EmoteName;
            public List<int> Indexes;
        }
    }
}