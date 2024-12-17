using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace TeslaRentalPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeDatabase();

            while (true)
            {
                Console.WriteLine("Welcome to Tesla Rental Platform!");
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        RegisterClient();
                        break;
                    case "2":
                        if (LoginClient())
                        {
                            ShowMainMenu();
                        }
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }

        static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection("Data Source=tesla_rental.db"))
            {
                connection.Open();
                string createCarsTable = @"CREATE TABLE IF NOT EXISTS Cars (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Model TEXT NOT NULL,
                    HourlyRate REAL NOT NULL,
                    PerKmRate REAL NOT NULL
                );";

                string createClientsTable = @"CREATE TABLE IF NOT EXISTS Clients (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL
                );";

                string createRentalsTable = @"CREATE TABLE IF NOT EXISTS Rentals (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ClientID INTEGER NOT NULL,
                    CarID INTEGER NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT,
                    Kilometers REAL,
                    TotalPayment REAL,
                    FOREIGN KEY(ClientID) REFERENCES Clients(ID),
                    FOREIGN KEY(CarID) REFERENCES Cars(ID)
                );";

                using (var command = new SQLiteCommand(createCarsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createClientsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createRentalsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        static void RegisterClient()
        {
            Console.Write("Enter your full name: ");
            string fullName = Console.ReadLine();
            Console.Write("Enter your email: ");
            string email = Console.ReadLine();
            Console.Write("Enter your password: ");
            string password = Console.ReadLine();

            string passwordHash = ComputeSha256Hash(password);

            using (var connection = new SQLiteConnection("Data Source=tesla_rental.db"))
            {
                connection.Open();
                string insertClient = "INSERT INTO Clients (FullName, Email, PasswordHash) VALUES (@FullName, @Email, @PasswordHash);";

                using (var command = new SQLiteCommand(insertClient, connection))
                {
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Registration successful!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        static bool LoginClient()
        {
            Console.Write("Enter your email: ");
            string email = Console.ReadLine();
            Console.Write("Enter your password: ");
            string password = Console.ReadLine();

            string passwordHash = ComputeSha256Hash(password);

            using (var connection = new SQLiteConnection("Data Source=tesla_rental.db"))
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM Clients WHERE Email = @Email AND PasswordHash = @PasswordHash;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 1)
                    {
                        Console.WriteLine("Login successful!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid email or password.");
                        return false;
                    }
                }
            }
        }

        static void ShowMainMenu()
        {
            // TODO
        }

        static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
