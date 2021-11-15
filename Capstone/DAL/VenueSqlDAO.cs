﻿using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Capstone.DAL
{
    /// <summary>
    /// This class handles working with Venues in the database.
    /// </summary>
    public class VenueSqlDAO : IVenueDAO
    {
        private readonly string connectionString;

        private const string SqlSelectAllVenues =
            "SELECT venue.name AS venue_name, venue.id AS venue_id, venue.description, city.name AS city_name, city.state_abbreviation " +
            "FROM venue INNER JOIN city ON venue.city_id = city.id " +
            "ORDER BY venue_name";

        private const string SqlSelectFilteredVenues =
            "SELECT venue.name AS venue_name, venue.id AS venue_id, venue.description, city.name AS city_name, city.state_abbreviation " +
            "FROM venue INNER JOIN city ON venue.city_id = city.id INNER JOIN category_venue ON venue.id = category_venue.venue_id " +
            "WHERE category_id = @filter ORDER BY venue_name;";

        private const string SqlSelectCategoriesByVenue =
            "SELECT category.id, category.name " + "" +
            "FROM category_venue INNER JOIN category ON category.id = category_venue.category_id " +
            "WHERE category_venue.venue_id = @venue_id";

        private const string SqlSelectAllCategories =
            "SELECT category.id, category.name " +
            "FROM category " +
            "ORDER BY category.name";

        public VenueSqlDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IList<Venue> GetVenues(int categoryId)
        {
            List<Venue> venues = new List<Venue>();

            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();


                    // How would you only set the command in the if statement and do the rest once?
                    // The actions are identicle except for the command text.
                    // My solution uses sets the value of a string to the correct sql statement
                    // and then passes that string to SqlCommand.  Does that create any risks?
                    string commandString = "";
                    if (categoryId == 0)
                    {
                        commandString = SqlSelectAllVenues;
                    }
                    else
                    {
                        commandString = SqlSelectFilteredVenues;
                    }
                    SqlCommand command = new SqlCommand(commandString, conn);
                    command.Parameters.AddWithValue("@filter", categoryId);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Venue venue = new Venue();

                        venue.Id = Convert.ToInt32(reader["venue_id"]);
                        venue.Name = Convert.ToString(reader["venue_name"]);
                        venue.Description = Convert.ToString(reader["description"]);
                        venue.CityName = Convert.ToString(reader["city_name"]);
                        venue.StateAbv = Convert.ToString(reader["state_abbreviation"]);
                        venues.Add(venue);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("An eror occured reading database. ");
                Console.WriteLine(ex.Message);
            }
            return venues;
        }

        public ICollection<Category> GetCategoriesByVenue(int venueId)
        {
            List<Category> categories = new List<Category>();
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();

                    SqlCommand command = new SqlCommand(SqlSelectCategoriesByVenue, conn);
                    command.Parameters.AddWithValue("@venue_id", venueId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = Convert.ToString(reader["name"]);

                        Category category = new Category(id, name);


                        categories.Add(category);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("An eror occured reading database. ");
                Console.WriteLine(ex.Message);
            }
            return categories;
        }
        public ICollection<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            try
            {
                using (SqlConnection conn = new SqlConnection(this.connectionString))
                {
                    conn.Open();

                    SqlCommand command = new SqlCommand(SqlSelectAllCategories, conn);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string name = Convert.ToString(reader["name"]);

                        Category category = new Category(id, name);


                        categories.Add(category);
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("An eror occured reading database. ");
                Console.WriteLine(ex.Message);
            }
            return categories;
        }
    }
}
