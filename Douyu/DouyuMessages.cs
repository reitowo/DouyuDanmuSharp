using System;
using System.Collections.Generic;
using System.Text;

namespace LivestreamDanmuku.Douyu
{
    public enum ChatMessageColor
    {
        White,
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Pink
    }

    public class ChatMessage
    {
        public DateTime Time;
        public string Username;
        public string Content;
        public int Level;
        public ChatMessageColor Color;
    }
}
