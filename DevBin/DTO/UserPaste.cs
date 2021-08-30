﻿namespace DevBin.Models
{


    public class UserPaste
    {
        public string Title { get; set; }
        public string Syntax { get; set; }
        public int Exposure { get; set; }
        public string Content { get; set; }
        public bool AsGuest { get; set; } = false;
    }
}