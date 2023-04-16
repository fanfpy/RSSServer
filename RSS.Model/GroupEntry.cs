using System;
using System.Collections.Generic;
using System.Text;

namespace RSS.Model
{
    public class GroupEntry
    {
        public string GroupName { get; set; }

        public List<item> items { get; set; }
    }

    public class item 
    {
        public int FeedId { get; set; }
        public string FeedIcon { get; set; }
        public string FeedName { get; set; }
        public int unread { get; set; }
    }
}
