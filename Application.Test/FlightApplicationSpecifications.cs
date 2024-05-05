using FluentAssertions;
using Data;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Test
{
    public class FlightApplicationSpecifications
    {
        readonly Entities entities = new Entities(
                   new DbContextOptionsBuilder<Entities>()
                       .UseInMemoryDatabase("Flights")
                       .Options
                   );
        readonly BookingService bookingServices;

        public FlightApplicationSpecifications()
        {
            bookingServices = new BookingService( entities );
        }

        [Theory]
        [InlineData("jeal@mail.com", 3)]
        [InlineData("some@mail.com", 2)]
        public void Remembers_bookings(string passengerEmail, int numberOfSeats)
        {
            var flight = new Flight(3);
            entities.Flights.Add(flight);

            bookingServices.Book(new BookDto(
                flightId: flight.Id,
                passengerEmail,
                numberOfSeats
                ));

            bookingServices.FindBookings(flight.Id).Should().ContainEquivalentOf(
                new BookingRm(passengerEmail, numberOfSeats)
                );
        }

        [Theory]
        [InlineData(3)]
        [InlineData(10)]
        public void Frees_up_seats_after_booking(int initalCapacity)
        {
            var flight = new Flight(initalCapacity);
            entities.Flights.Add(flight);

            bookingServices.Book(new BookDto(
                flightId: flight.Id, 
                passengerEmail: "jeal@mail.com", 
                numberOfSeats: 2));

            bookingServices.CancelBooking(
                new CancelBookingDto(
                    flightId: flight.Id,
                    passengerEmail: "jeal@mail.com",
                    numberOfSeats: 2
                    )
                );

            bookingServices.GetRemainingNumberOfSeatsFor(flight.Id)
                .Should().Be(initalCapacity);
        }
    }
}

namespace Application
{
    public class BookingService
    {
        public Entities Entities { get; set; }
        public BookingService(Entities entities)
        {
            Entities = entities;
        }

        public void Book(BookDto bookDto)
        {
            var flight = Entities.Flights.Find(bookDto.FlightId);
            flight?.Book(bookDto.PassengerEmail, bookDto.NumberOfSeats);
            Entities.SaveChanges();
        }

        public IEnumerable<BookingRm> FindBookings(Guid flightId)
        {
            return Entities.Flights
                .Find(flightId)
                .BookingList
                .Select(booking => new BookingRm(
                    booking.Email,
                    booking.NumberOfSeats
                    ));
        }

        public void CancelBooking(CancelBookingDto cancelBookingDto)
        {
            var flight = Entities.Flights.Find(cancelBookingDto.FlightId);
            flight.CancelBooking(cancelBookingDto.PassengerEmail, cancelBookingDto.NumberOfSeats);
            Entities.SaveChanges();
        }

        internal object GetRemainingNumberOfSeatsFor(Guid flightId)
        {
            return Entities.Flights.Find(flightId).RemainingNumberOfSeats;
        }
    }

    public class BookDto
    {
        public Guid FlightId { get; set; }
        public string PassengerEmail { get; set; }
        public int NumberOfSeats { get; set; }
        public BookDto(Guid flightId, string passengerEmail, int numberOfSeats)
        {
            FlightId = flightId;
            PassengerEmail = passengerEmail;
            NumberOfSeats = numberOfSeats;
        }
    }

    public class BookingRm
    {
        public string PassengerEmail { get; set; }
        public int NumberOfSeats { get; set; }

        public BookingRm(string passengerEmail, int numberOfSeats)
        {
            PassengerEmail = passengerEmail;
            NumberOfSeats = numberOfSeats;
        }
    }

    public class CancelBookingDto
    {
        public Guid FlightId { get; set; }
        public string PassengerEmail { get; set; }
        public int NumberOfSeats { get; set; }
        public CancelBookingDto(Guid flightId, string passengerEmail, int numberOfSeats)
        {
            FlightId = flightId; 
            PassengerEmail = passengerEmail; 
            NumberOfSeats = numberOfSeats;
        }
    }
}