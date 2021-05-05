using System;
using System.Collections.Generic;

namespace PipeHow.DungeonGenerator
{
    internal class DiscordInteractionModel
    {
        public int Type { get; set; }
        public string Token { get; set; }
        public DiscordMember Member { get; set; }
        public string Id { get; set; }
        public string Guild_Id { get; set; }
        public DiscordData Data { get; set; }
        public string Channel_Id { get; set; }
    }

    public class DiscordMember
    {
        public DiscordMemberUser User { get; set; }
        public List<string> Roles { get; set; }
        public string Permissions { get; set; }
        public bool Pending { get; set; }
        public string Nick { get; set; }
        public bool Mute { get; set; }
        public DateTime Joined_At { get; set; }
        public bool Is_Pending { get; set; }
        public bool Deaf { get; set; }
    }

    public class DiscordMemberUser
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Avatar { get; set; }
        public string Discriminator { get; set; }
        public long Public_Flags { get; set; }
    }

    public class DiscordData
    {
        public List<DiscordDataOption> Options { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class DiscordDataOption
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
