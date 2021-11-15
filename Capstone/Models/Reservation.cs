using System;
using System.Collections.Generic;
using System.Text;

namespace Capstone.Models
{
    public class Reservation
    {


        public Reservation(int space_Id, int reserveOccupants, DateTime reserveDate, int reserveDays, string reservationName, string spaceName, string venueName, decimal dailyRate)
        {
            this.SpaceId = space_Id;
            this.NumberOfAttendees = reserveOccupants;
            this.StartDate = reserveDate;
            this.EndDate = reserveDate.Date.AddDays(reserveDays - 1);
            this.ReservedFor = reservationName;
            this.SpaceName = spaceName;
            this.VenueName = venueName;
            this.TotalCost = dailyRate * reserveDays;
        }

        public int ReservationId { get; private set; }

        public int SpaceId { get; }

        public int NumberOfAttendees { get; }

        public DateTime StartDate { get; }

        public DateTime EndDate { get; }

        public string ReservedFor { get; }

        public string VenueName { get; }

        public string SpaceName { get; }

        public decimal TotalCost { get; }
    public void ConfirmReservation(int reservation_Id)
        {
            this.ReservationId = reservation_Id;
        }
        public override string ToString()
        {
            return $"Confirmation #: {ReservationId}\n" +
            $"Venue: {VenueName}\n" +
            $"Space: {SpaceName}\n" +
            $"Reserved For: {ReservedFor}\n" +
            $"Attendees: {NumberOfAttendees}\n" +
            $"Arrival Date: {StartDate.ToShortDateString()}\n" +
            $"Depart Date: {EndDate.ToShortDateString()}\n" +
            $"Total Cost {TotalCost.ToString("C")}";
        }
    }
}
