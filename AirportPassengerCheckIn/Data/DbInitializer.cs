using Microsoft.EntityFrameworkCore;
using AirportPassengerCheckIn.Models;
using AirportPassengerCheckIn.Models.Enums;


namespace AirportPassengerCheckIn.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            //Block1 Entities without relationships
            if (!await context.Airports.AnyAsync())
            {
                var airports = new List<Airport>
                {
                    new Airport { Name = "Международный аэропорт Шереметьево", CodeIATA = "SVO", Country = "Россия", City = "Москва" },
                    new Airport { Name = "Международный аэропорт Внуково", CodeIATA = "VKO", Country = "Россия", City = "Москва" },
                    new Airport { Name = "Международный аэропорт Сочи", CodeIATA = "AER", Country = "Россия", City = "Сочи" },
                    new Airport { Name= "Аэропорт Пулково", CodeIATA = "LED", Country = "Россия", City = "Санкт-Петербург" },
                };
                await context.Airports.AddRangeAsync(airports);
                await context.SaveChangesAsync();
            }
            

            if (!await context.AircraftTypes.AnyAsync())
            {
                var aircraftTypes = new List<AircraftType>
                {
                    new AircraftType { Company = "Boeing", Model = "737-800", NumberOfSeats = 189 },
                    new AircraftType { Company = "Boeing", Model = "737 MAX", NumberOfSeats = 220 },
                    new AircraftType { Company = "Boeing", Model = "777-300ER", NumberOfSeats = 316},
                    new AircraftType { Company = "Airbus", Model = "A320-100", NumberOfSeats = 130 },
                    new AircraftType {Company = "Airbus", Model = "A320-200", NumberOfSeats = 150 },
                };
                await context.AircraftTypes.AddRangeAsync(aircraftTypes);
                await context.SaveChangesAsync();
            }

            if (!await context.Passengers.AnyAsync())
            {
                var passengers = new List<Passenger>
                {
                    new Passenger { FirstName = "Павел", LastName = "Брызгалов", MiddleName="Алексеевич",
                        BirthDate = new DateOnly(2006, 8, 17), PassportNumber = "1726000000" },
                    new Passenger { FirstName = "Михаил", LastName = "Зякин", MiddleName="Юрьевич",
                        BirthDate = new DateOnly(2005, 8, 31), PassportNumber = "1725000000" },
                    new Passenger { FirstName = "Илья", LastName = "Горин", MiddleName="Владимирович",
                        BirthDate = new DateOnly(2004, 7, 28), PassportNumber = "1724000000" },
                    new Passenger { FirstName = "Никита", LastName = "Рогов", MiddleName="Николаевич",
                        BirthDate = new DateOnly(2004, 1, 26), PassportNumber = "1723000000" },
                    new Passenger { FirstName = "Иван", LastName = "Смирнов", MiddleName="Иванович",
                        BirthDate = new DateOnly(1988, 11, 5), PassportNumber = "1715000000" },
                };
                await context.Passengers.AddRangeAsync(passengers);
                await context.SaveChangesAsync();
            }


            if (!await context.AirportEmployees.AnyAsync())
            {
                var airportEmployees = new List<AirportEmployee>
                {
                    new AirportEmployee {FirstName="Маргарита", LastName="Татаренко", MiddleName="Евгеньевна",
                        Position="Агент по регистрации"},
                    new AirportEmployee {FirstName="Егор", LastName="Швецов", MiddleName="Михайлович",
                        Position="Агент по регистрации"},
                    new AirportEmployee {FirstName="Дмитрий", LastName="Федин", MiddleName="Алексеевич",
                        Position="Агент по регистрации"}
                };
                await context.AirportEmployees.AddRangeAsync(airportEmployees);
                await context.SaveChangesAsync();
            }

            // Block2 Entities with relationships on Block1 Entities

            var boeing737800 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "737-800");
            var boeing737MAX = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "737 MAX");
            var boeing777300ER = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "777-300ER");
            var airbusA320100 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "A320-100");
            var airbusA320200 = await context.AircraftTypes.FirstOrDefaultAsync(a => a.Model == "A320-200");

            var SVO = await context.Airports.FirstOrDefaultAsync(a => a.CodeIATA == "SVO");
            var VKO = await context.Airports.FirstOrDefaultAsync(a => a.CodeIATA == "VKO");
            var AER = await context.Airports.FirstOrDefaultAsync(a => a.CodeIATA == "AER");
            var LED = await context.Airports.FirstOrDefaultAsync(a => a.CodeIATA == "LED");

            if (!await context.Aircrafts.AnyAsync())
            {
                var aircrafts = new List<Aircraft>
                {
                    new Aircraft { AircraftNumber = "RA-00001", AircraftType = boeing737800 },
                    new Aircraft { AircraftNumber = "RA-00002", AircraftType = boeing737MAX },
                    new Aircraft { AircraftNumber = "RA-00003", AircraftType = boeing777300ER },
                    new Aircraft { AircraftNumber = "RA-00004", AircraftType = airbusA320100 },
                    new Aircraft { AircraftNumber = "RA-00005", AircraftType = airbusA320200 },
                };
                await context.Aircrafts.AddRangeAsync(aircrafts);
            }

            //Block3 Entities with relationships on Block2 Entities

            var aircraftRA00001 = await context.Aircrafts.FirstOrDefaultAsync(a => a.AircraftNumber == "RA-00001");
            var aircraftRA00002 = await context.Aircrafts.FirstOrDefaultAsync(a => a.AircraftNumber == "RA-00002");
            var aircraftRA00003 = await context.Aircrafts.FirstOrDefaultAsync(a => a.AircraftNumber == "RA-00003");
            var aircraftRA00004 = await context.Aircrafts.FirstOrDefaultAsync(a => a.AircraftNumber == "RA-00004");
            var aircraftRA00005 = await context.Aircrafts.FirstOrDefaultAsync(a => a.AircraftNumber == "RA-00005");

            if (!await context.Flights.AnyAsync())
            {
                var flights = new List<Flight>
                {
                    new Flight { FlightNumber = "SU0001", DepartureAirport = SVO, ArrivalAirport = AER, 
                        DepartureTime = new DateTime(2026, 11, 25, 10, 0, 0), 
                        ArrivalTime = new DateTime(2024,11, 25, 13, 55, 0),
                        Aircraft = aircraftRA00001
                    },
                    new Flight { FlightNumber = "SU0011", DepartureAirport = SVO, ArrivalAirport = LED,
                        DepartureTime = new DateTime(2026, 11, 25, 8, 0, 0),
                        ArrivalTime = new DateTime(2024, 11, 25, 9, 50, 0),
                        Aircraft = aircraftRA00002
                    },
                    new Flight { FlightNumber = "SU0021", DepartureAirport = VKO, ArrivalAirport = LED,
                        DepartureTime = new DateTime(2026, 11, 25, 12, 30, 0),
                        ArrivalTime = new DateTime(2024, 11, 25, 16, 20, 0),
                        Aircraft = aircraftRA00003
                    },
                    new Flight { FlightNumber = "SU0031", DepartureAirport = LED, ArrivalAirport = AER,
                        DepartureTime = new DateTime(2026, 11, 25, 11, 30, 0),
                        ArrivalTime = new DateTime(2026, 11, 25, 16, 55, 0),
                        Aircraft = aircraftRA00004
                    },
                    new Flight { FlightNumber = "SU0002", DepartureAirport = AER, ArrivalAirport = SVO,
                        DepartureTime = new DateTime(2026, 11, 25, 22, 00, 0),
                        ArrivalTime = new DateTime(2026,11, 26, 1, 40, 0),
                        Aircraft = aircraftRA00001
                    },
                    new Flight { FlightNumber = "SU0012", DepartureAirport = LED, ArrivalAirport = SVO,
                        DepartureTime = new DateTime(2026, 11, 25, 11, 20, 0),
                        ArrivalTime = new DateTime(2026, 11, 25, 13, 10, 0),
                        Aircraft = aircraftRA00002
                    },
                    new Flight { FlightNumber = "SU0022", DepartureAirport = LED, ArrivalAirport = VKO,
                        DepartureTime = new DateTime(2026, 11, 25, 17, 50, 0),
                        ArrivalTime = new DateTime(2026, 11, 25, 21, 30, 0),
                        Aircraft = aircraftRA00003
                    },
                    new Flight { FlightNumber = "SU0032", DepartureAirport = AER, ArrivalAirport = LED,
                        DepartureTime = new DateTime(2026, 6, 1, 13, 50, 0),
                        ArrivalTime = new DateTime(2026, 6, 1, 12, 0, 0),
                        Aircraft = aircraftRA00004
                    },
                };

                await context.Flights.AddRangeAsync(flights);
                await context.SaveChangesAsync();
            }

            if (!await context.Seats.AnyAsync())
            {
                var seats = new List<Seat>
                {
                    new Seat { SeatNumber = "1A", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "1B", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "1C", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "1D", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "2A", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "2B", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "2C", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "2D", Aircraft = aircraftRA00001, Class=SeatClass.Business },
                    new Seat { SeatNumber = "3A", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "3B", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "3C", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "3D", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "3E", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "3F", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4A", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4B", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4C", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4D", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4E", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                    new Seat { SeatNumber = "4F", Aircraft = aircraftRA00001, Class=SeatClass.Economy },
                };
                await context.Seats.AddRangeAsync(seats);
                await context.SaveChangesAsync();
            }


            //Block4 Entities with relationships on Block3 Entities

            if (!await context.Bookings.AnyAsync())
            {
                var flightSU0001 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0001");
                var flightSU0011 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0011");
                var flightSU0021 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0021");
                var flightSU0031 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0031");
                var flightSU0002 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0002");
                var flightSU0012 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0012");
                var flightSU0022 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0022");
                var flightSU0032 = await context.Flights.FirstOrDefaultAsync(f => f.FlightNumber == "SU0032");

                var bookings = new List<Booking>
                {
                    new Booking { Flight = flightSU0001, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0011, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0021, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0031, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0002, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0012, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0022, CreatedAt = DateTime.Now, Status = BookingStatus.Pending },
                    new Booking { Flight = flightSU0032, CreatedAt = DateTime.Now, Status = BookingStatus.Pending }
                };
                await context.Bookings.AddRangeAsync(bookings);
                await context.SaveChangesAsync();
            }

            if (!await context.BookingPassengers.AnyAsync())
            {
                var booking1 = await context.Bookings.FirstOrDefaultAsync(b => b.Flight.FlightNumber == "SU0001");
                var booking2 = await context.Bookings.FirstOrDefaultAsync(b => b.Flight.FlightNumber == "SU0011");
                var booking3 = await context.Bookings.FirstOrDefaultAsync(b => b.Flight.FlightNumber == "SU0021");
                var booking4 = await context.Bookings.FirstOrDefaultAsync(b => b.Flight.FlightNumber == "SU0031");
                var passenger1 = await context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == "1726000000");
                var passenger2 = await context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == "1725000000");
                var passenger3 = await context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == "1724000000");
                var passenger4 = await context.Passengers.FirstOrDefaultAsync(p => p.PassportNumber == "1723000000");
                var bookingPassengers = new List<BookingPassenger>
                {
                    new BookingPassenger { Booking = booking1, Passenger = passenger1 },
                    new BookingPassenger { Booking = booking2, Passenger = passenger2 },
                    new BookingPassenger { Booking = booking3, Passenger = passenger3 },
                    new BookingPassenger { Booking = booking4, Passenger = passenger4 }
                };
                await context.BookingPassengers.AddRangeAsync(bookingPassengers);
                await context.SaveChangesAsync();
            }
        }
    }
}
