using System;

namespace CultureDocs.Models
{
    public class Event
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Place { get; set; }
        public string EventType { get; set; }
    }
}