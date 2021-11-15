﻿using Capstone.DAL;
using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Capstone
{
    /// <summary>
    /// This class is responsible for representing the main user interface to the user.
    /// </summary>
    /// <remarks>
    /// ALL Console.ReadLine and WriteLine in this class
    /// NONE in any other class. 
    ///  
    /// The only exceptions to this are:
    /// 1. Error handling in catch blocks
    /// 2. Input helper methods in the CLIHelper.cs file
    /// 3. Things your instructor explicitly says are fine
    /// 
    /// No database calls should exist in classes outside of DAO objects
    /// </remarks>
    public class UserInterface
    {
        private readonly string connectionString;

        private IVenueDAO venueDAO;
        private ISpaceDAO spaceDAO;
        private IReservationDAO reservationDAO;

        public UserInterface(string connectionString)
        {
            this.connectionString = connectionString;
            this.venueDAO = new VenueSqlDAO(connectionString);
            this.spaceDAO = new SpaceSqlDAO(connectionString);
            this.reservationDAO = new ReservationSqlDAO(connectionString);
        }

        public void RunMainMenu()
        {
            bool shouldContinue = true;
            PrintHeader(" Main Menu");
            while (shouldContinue)
            {
                string choiceString = CLIHelper.GetString(" 1) List All Venues\n 2) Filter Venues By Category\n Q) Quit\n Your Selection: ");
                switch (choiceString.ToUpper())
                {
                    case "1":
                        ListVenues(0); // 0 = all venue categories
                        break;
                    case "2":
                        int filter = ChooseACategory();
                        ListVenues(filter);
                        break;
                    case "Q":
                        Console.WriteLine("Thank you for using Excelsior Venues");
                        shouldContinue = false;
                        break;
                    default:
                        Console.WriteLine($"{choiceString} is not a valid choice");
                        break;
                }
            }
        }
        private void PrintHeader(string menu)
        {
            Console.WriteLine();
            Console.WriteLine("Excelsior Venues ");
            Console.WriteLine();
            Console.WriteLine(menu);
        }
        /// <summary>
        /// Get a list of venues from DAO and list to console.
        /// Filter on venue category (0 = all categories)
        /// </summary>
        private void ListVenues(int categoryId)
        {
            bool shouldContinue = true;
            while (shouldContinue)
            {
                //Instantiate a new collection
                IList<Venue> venues = venueDAO.GetVenues(categoryId);
                if (venues.Count > 0)
                {
                    PrintHeader("Which venue would you like to view? ");
                    int count = 1;
                    foreach (Venue venue in venues)
                    {
                        Console.WriteLine($"{count++.ToString().PadLeft(2)}) {venue.Name}");
                    }
                    Console.WriteLine(" R) Return to previous menue ");
                    Console.WriteLine();
                    string choiceString = CLIHelper.GetString("Enter your choice of venue: ");
                    bool intOrString = int.TryParse(choiceString, out int choice);
                    if (intOrString)
                    {
                        ShowVenueDetails(venues[choice - 1]);
                    }
                    else
                    {
                        if (choiceString.ToUpper() == "R")
                        {
                            shouldContinue = false;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("There are no venues to list");
                }
            }
        }
        /// <summary>
        /// Given a venue, show details on that venue.  Offer to work with spaces/reservations.
        /// </summary>
        public void ShowVenueDetails(Venue venue)
        {
            PrintHeader("Venue Details");
            Console.WriteLine(venue.Name);
            Console.WriteLine($"Location: {venue.CityName}, {venue.StateAbv}");
            ICollection<Category> categories = venueDAO.GetCategoriesByVenue(venue.Id);
            Console.Write("Categories: ");
            bool isFirstTime = true;
            foreach (Category category in categories)
            {
                // Screen formatting needs 12 spaces of padding all but 1st console.writeline.
                string padding = "            ";
                if (isFirstTime)
                {
                    isFirstTime = false;
                    padding = "";
                }
                Console.WriteLine(padding + category.Name);
            }
            Console.WriteLine();
            Console.WriteLine(venue.Description);
            Console.WriteLine();
            bool shouldContinue = true;
            while (shouldContinue)
            {
                Console.WriteLine();
                Console.WriteLine("What would you like to do next: ");
                string choiceString = CLIHelper.GetString(" 1) View Spaces \n 2) Show Upcomming Reservations \n R) Return to Previous Screen \n\nYour Selection?  ");
                switch (choiceString.ToUpper())
                {
                    case "1":
                        ListVenueSpaces(venue);
                        break;
                    case "2":
                        ShowVenueReservations(venue);
                        break;
                    case "R":
                        shouldContinue = false;
                        break;
                    default:
                        Console.WriteLine($"{choiceString} is not a valid choice.");
                        break;
                }
            }
        }
        /// <summary>
        /// Given a venue, retrieve a list of spaces in that venue from DAO and list to Console.
        /// Offer to work with reservations.
        /// </summary>
        public void ListVenueSpaces(Venue venue)
        {
            //Instantiate a new collection
            ICollection<Space> spaces = spaceDAO.GetVenueSpaces(venue.Id);
            //Call to print header
            PrintHeader("List Venue Spaces");
            Console.WriteLine();
            Console.WriteLine($"{venue.Name} Spaces");
            Console.WriteLine();
            Console.WriteLine("{0, -5} {1, -30} {2, -8} {3, -8} {4, -13} {5, -13}",
                 "#", "Name", "Open", "Close", "Daily Rate", "Max Occupancy");
            int index = 1;
            foreach (Space space in spaces)
            {
                Console.WriteLine("{0, -5} {1, -30} {2, -8} {3, -8} {4, -13} {5, -13}",
                    index++, space.Name, CLIHelper.GetStringMonth(space.OpenFromMonth),
                    CLIHelper.GetStringMonth(space.OpenToMonth), space.DailyRate.ToString("C"), space.MaxOccupancy.ToString());
            }

            bool shouldContinue = true;
            while (shouldContinue)
            {
                Console.WriteLine();
                string choiceString = CLIHelper.GetString
                    ("What would you like to do next?\n 1) Reserve a Space\n 2)Reserve a space using advanced options\n R) Return to Previous Screen ");
                switch (choiceString.ToUpper())
                {
                    case "1":
                        ReserveASpace(venue, false);
                        break;
                    case "2":
                        ReserveASpace(venue, true);
                        break;
                    case "R":
                        shouldContinue = false;
                        break;
                    default:
                        Console.WriteLine($"{ choiceString} is not a valid choice.");
                        break;
                }
            }
        }

        /// <summary>
        /// Given a venue, get user requirements to make a reservation.
        /// Pass requirements to ListAvailableVenueSpaces 
        /// </summary>
        public void ReserveASpace(Venue venue, bool useAdvanced)
        {
            bool shouldContinue = true;
            while (shouldContinue)
            {
                PrintHeader("Reserve A Space");
                Console.WriteLine();
                DateTime reserveDate = CLIHelper.GetDate("When do you need the space? (yyyy-mm-dd) ");
                int reserveDays = CLIHelper.GetInteger("How many days will you need the space? ");
                int reserveOccupants = CLIHelper.GetInteger("How many people will be in attendance?  (Enter 0 if unknown.)");
                bool requiresAccessibility = false;
                int dailyBudget = 0;
                if (useAdvanced)
                {
                    requiresAccessibility = CLIHelper.GetBool("Show only spaces with accessibility?  (Y/N) ");
                    bool hasDailyBudget = CLIHelper.GetBool("Restrict spaces based on daily budget?  (Y/N)");
                    if (hasDailyBudget)
                    {
                        dailyBudget = CLIHelper.GetInteger("What is your daily budget?  (Enter whole dollars.)  ");
                    }
                    else
                    {
                        dailyBudget = int.MaxValue;
                    }

                }
                Console.WriteLine();
                Console.WriteLine();
                int availableSpaces = ListAvailableVenueSpaces(venue, reserveDate, reserveDays, reserveOccupants, requiresAccessibility, dailyBudget);
                if (availableSpaces == 0)
                {
                    shouldContinue = CLIHelper.GetBool("Would you like to try another search? (Y/N)");
                }
                else
                {
                    shouldContinue = false;
                }
            }
        }
        /// <summary>
        /// Given a set of requirements, retrieve available spaces from DAO and list to console.
        /// </summary>
        public int ListAvailableVenueSpaces(Venue venue, DateTime reserveDate, int reserveDays, int reserveOccupants, bool requiresAccessibility, int dailyBudget)
        {
            ICollection<Space> spaces = spaceDAO.GetAvailableVenueSpaces(venue.Id, reserveDate, reserveDays, reserveOccupants, requiresAccessibility, dailyBudget);
            int availableSpaces = spaces.Count;
            if (availableSpaces > 0)
            {
                // List available spaces
                Console.WriteLine("The following spaces are available based on your needs:");
                Console.WriteLine();
                Console.WriteLine("{0, -10} {1, -30} {2, -13} {3, -15} {4, -15} {5, -13}",
                                  "Space #", "Name", "Daily Rate", "Max Occup.", "Accessible?", "Total Cost");
                foreach (Space space in spaces)
                {
                    Console.WriteLine("{0, -10} {1, -30} {2, -13} {3, -15} {4, -15} {5, -13}", space.Id, space.Name, space.DailyRate.ToString("C"),
                        space.MaxOccupancy, space.IsAccessible, (space.DailyRate * reserveDays).ToString("C"));
                }
                bool shouldContinue = true;

                // Get chosen space from user
                while (shouldContinue)
                {
                    Console.WriteLine();
                    int choiceSpace = CLIHelper.GetInteger
                        ("Which space would you like to reserve (enter 0 to cancel)? ");
                    if (choiceSpace == 0)
                    {
                        shouldContinue = false;
                        continue;
                    }
                    string inputName = CLIHelper.GetString("Who is this reservation for? ");
                    Console.WriteLine();
                    // if successful reservation, set shouldContinue to false.
                    shouldContinue = !(ReserveASpace(venue, choiceSpace, reserveDate, reserveDays, reserveOccupants, inputName));
                }
            }
            else
            {
                Console.WriteLine("There are no spaces available that meet your requirements.");
            }
            return availableSpaces;
        }

        /// <summary>
        /// Gather all data needed for a reservation and pass to DAO.
        /// Display confirmation to console.
        /// If successful return true so that calling method can exit.
        /// </summary>
        public bool ReserveASpace(Venue venue, int space_Id, DateTime reserveDate, int reserveDays, int reserveOccupants, string inputName)
        {
            Space space = spaceDAO.GetSpace(space_Id);

            // Constructor sets all properties except reservation Id.
            Reservation reservation = new Reservation(space_Id, reserveOccupants, reserveDate, reserveDays, inputName, space.Name, venue.Name, space.DailyRate);

            // MakeAReservation inserts reservation into the DB table & returns the new reservation id
            // which is used by ConfirmReservation to set the property in reservation (used for display).
            reservation.ConfirmReservation(reservationDAO.MakeAReservation(reservation));
            if (reservation.ReservationId > 0)
            {

                Console.WriteLine("Thanks for submitting your reservation! The details for you event are listed below: ");
                Console.WriteLine();
                Console.WriteLine(reservation);
                Console.WriteLine();
                return true;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Unable to make reservation.");
                Console.WriteLine();
                return false;
            }
        }
        public void ShowVenueReservations(Venue venue)
        {
            ICollection<Reservation> reservations = reservationDAO.GetVenueReservations(venue);
            PrintHeader($"For the venue {venue.Name}\nThe following reservations are comming up in the next 30 days");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("{0, -30} {1, -30} {2, -13} {3, -13}",
        "Space", "Reserved For", "From", "To");
            foreach (Reservation reservation in reservations)
            {
                Console.WriteLine("{0, -30} {1, -30} {2, -13} {3, -13}", reservation.SpaceName, reservation.ReservedFor, reservation.StartDate.ToShortDateString(), reservation.EndDate.ToShortDateString());
            }
            Console.WriteLine();
            Console.WriteLine("Hit Enter to continue.");
            Console.ReadLine();
        }

        public int ChooseACategory()
        {
            //Instantiate a new collection
            ICollection<Category> categories = venueDAO.GetCategories();
            if (categories.Count > 0)
            {
                PrintHeader("Select venues for which category? ");
                foreach (Category category in categories)
                {
                    Console.WriteLine(category);
                }
                Console.WriteLine(" R) Return to previous menue ");
                Console.WriteLine();
                return CLIHelper.GetInteger("Select category by id: (0 for all categories): ");
            }
            else
            {
                Console.WriteLine("There are no categories to list");
                return -1;
            }
        }
    }
}