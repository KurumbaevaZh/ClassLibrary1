using Newtonsoft.Json;
using RestaurantBookingLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantBookingLibrary.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }  // Пароль должен быть захеширован в реальном приложении
    }
    public class Table
    {
        public int TableId { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
    }
    public class Reservation
    {
        public int ReservationId { get; set; }
        public User User { get; set; }
        public Table Table { get; set; }
        public DateTime ReservationTime { get; set; }
    }
    public class Restaurant
    {
        public List<Table> Tables { get; set; } = new List<Table>();
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }


    public interface IUserManager
    {
        event EventHandler<User> UserRegistered;
        void RegisterUser(User user);
        User Login(string username, string password);
    }
    public interface ITableManager
    {
        List<Table> GetAvailableTables();
        void UpdateTableAvailability(int tableId, bool isAvailable);
        List<Table> GetAllTables();
        List<Table> GetTablesByCapacity(int capacity);
        List<Table> GetAvailableTablesByCapacityAndTime(int capacity, DateTime dateTime);
    }
    public interface IReservationManager
    {
        event EventHandler<Reservation> ReservationConfirmed;
        void MakeReservation(Reservation reservation);
        void CancelReservation(int reservationId);
        List<Reservation> GetAllReservations();
    }


    public class UserManager : IUserManager
    {
        private List<User> users = new List<User>();
        public event EventHandler<User> UserRegistered;

        public void RegisterUser(User user)
        {
            users.Add(user);
            UserRegistered?.Invoke(this, user);
        }

        public User Login(string username, string password)
        {
            return users.FirstOrDefault(u => u.Username == username && u.Password == password);
        }
    }
    public class TableManager : ITableManager
    {
        private Restaurant restaurant;

        public TableManager(Restaurant restaurant)
        {
            this.restaurant = restaurant;
        }

        public List<Table> GetAvailableTables()
        {
            return restaurant.Tables.Where(t => t.IsAvailable).ToList();
        }

        public void UpdateTableAvailability(int tableId, bool isAvailable)
        {
            var table = restaurant.Tables.FirstOrDefault(t => t.TableId == tableId);
            if (table != null)
            {
                table.IsAvailable = isAvailable;
            }
        }

        public List<Table> GetAllTables()
        {
            return restaurant.Tables;
        }

        public List<Table> GetTablesByCapacity(int capacity)
        {
            return restaurant.Tables.Where(t => t.Capacity >= capacity && t.IsAvailable).ToList();
        }

        public List<Table> GetAvailableTablesByCapacityAndTime(int capacity, DateTime dateTime)
        {
            return restaurant.Tables.Where(t => t.Capacity >= capacity && t.IsAvailable &&
                                                !restaurant.Reservations.Any(r => r.Table.TableId == t.TableId &&
                                                                                  r.ReservationTime == dateTime))
          .ToList();
        }
    }
    public class ReservationManager : IReservationManager
    {
        private Restaurant restaurant;
        public event EventHandler<Reservation> ReservationConfirmed;

        public ReservationManager(Restaurant restaurant)
        {
            this.restaurant = restaurant;
        }

        public void MakeReservation(Reservation reservation)
        {
            restaurant.Reservations.Add(reservation);
            ReservationConfirmed?.Invoke(this, reservation);
            UpdateTableAvailability(reservation.Table.TableId, false);
        }

        public void CancelReservation(int reservationId)
        {
            var reservation = restaurant.Reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            if (reservation != null)
            {
                restaurant.Reservations.Remove(reservation);
                UpdateTableAvailability(reservation.Table.TableId, true);
            }
        }

        private void UpdateTableAvailability(int tableId, bool isAvailable)
        {
            var table = restaurant.Tables.FirstOrDefault(t => t.TableId == tableId);
            if (table != null)
            {
                table.IsAvailable = isAvailable;
            }
        }

        public List<Reservation> GetAllReservations()
        {
            return restaurant.Reservations;
        }
    }
    public static class DataManager
    {
        private static readonly string FilePath = "restaurant_data.json";

        public static Restaurant LoadData()
        {
            if (File.Exists(FilePath))
            {
                var jsonData = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<Restaurant>(jsonData);
            }
            return new Restaurant();
        }

        public static void SaveData(Restaurant restaurant)
        {
            var jsonData = JsonConvert.SerializeObject(restaurant, Formatting.Indented);
            File.WriteAllText(FilePath, jsonData);
        }
    }
}