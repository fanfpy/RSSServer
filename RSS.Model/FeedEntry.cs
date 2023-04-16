using System;
using System.Collections.Generic;
using System.Text;

namespace RSS.Model
{
    public class FeedEntry
    {
        public long id { get; set; }
        public string feed_name { get; set; }
        public string   title { get; set; }
        public string   sub_title { get; set; }
        public string   image_url { get; set; }
        public int   is_read { get; set; }
        public DateTime? update_time { get; set; }
        public DateTime   publishingDate { get; set; }
        public string   todo { get; set; }
        public string icon_url { get; set; }
    }
}
