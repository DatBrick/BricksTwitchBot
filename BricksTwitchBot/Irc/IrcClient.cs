﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace BricksTwitchBot.Irc
{
    public class IrcClient
    {
        private string _channel;

        public StreamReader StreamReader;
        public StreamWriter StreamWriter;
        public TcpClient TcpClient;

        public bool Connected => TcpClient.Connected;

        public IrcClient(string username, string oauth, string channel)
        {
            TcpClient = new TcpClient("irc.twitch.tv", 6667);
            StreamReader = new StreamReader(TcpClient.GetStream());
            StreamWriter = new StreamWriter(TcpClient.GetStream())
            {
                AutoFlush = true
            };

            WriteOther("CAP REQ :twitch.tv/membership");
            WriteOther("CAP REQ :twitch.tv/commands");
            WriteOther("CAP REQ :twitch.tv/tags");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(oauth))
            {
                WriteOther($"NICK justinfan{ new Random().Next(0, int.MaxValue) }");
            }
            else
            {
                WriteOther($"PASS {oauth}");
                WriteOther($"NICK {username.ToLower()}");
            }

            _channel = channel.ToLower();

            WriteOther($"JOIN #{_channel}");
        }

        public string ReadData()
        {
            try
            {
                string data = StreamReader.ReadLine();

                if (data != null)
                {
                    Match match = Globals.PingMatch.Match(data);
                    if (match.Success)
                    {
                        WriteOther($"PONG {match.Groups["ip"].Value}");
                        return null;
                    }
                }

                return data;
            }
            catch (IOException e) // Just ignore any bad data
            {
                Globals.Log($"[EXCEPTION] | { e.Message }");
                return null;
            }
        }

        public void WriteOther(string other)
        {
            StreamWriter.WriteLine(other);
        }

        public void WriteMessage(string message)
        {
            StreamWriter.WriteLine("PRIVMSG #{0} :{1}", _channel, message);
        }

        public void Disconnect()
        {
            TcpClient.Close();
        }
    }
}
