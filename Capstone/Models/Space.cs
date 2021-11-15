using System;
using System.Collections.Generic;
using System.Text;

namespace Capstone.Models
{
    public class Space
    {
        public Space(int spaceId, int venueId, string name, bool isAccessible, int openFromMonth, int openToMonth, decimal dailyRate, int maxOccupancy)
        {
            this.Id = spaceId;
            this.VenueId = venueId;
            this.Name = name;
            this.IsAccessible = isAccessible;
            this.OpenFromMonth = openFromMonth;
            this.OpenToMonth = openToMonth;
            this.DailyRate = dailyRate;
            this.MaxOccupancy = maxOccupancy;
        }
        public int Id { get; }

        public int VenueId { get; }

        public string Name { get; }

        public bool IsAccessible { get; }

        public int OpenFromMonth { get; }

        public int OpenToMonth { get; }

        public decimal DailyRate { get; }

        public int MaxOccupancy { get; }


    }
}
