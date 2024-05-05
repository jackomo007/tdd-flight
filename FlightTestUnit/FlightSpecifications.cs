using Domain;
using FluentAssertions;

namespace FlightTestUnit
{
    public class FlightSpecifications
    {
        [Theory]
        [InlineData(3,2,1)]
        public void Booking_reduce_the_number_of_seats(
            int seatCapacity, 
            int numberOfSeats, 
            int remainingNumberOfSeat
            )
        {
            var fligth = new Flight(seatCapacity: seatCapacity);

            fligth.Book("jeal@mail.com", numberOfSeats);

            fligth.RemainingNumberOfSeats.Should().Be(remainingNumberOfSeat);
        }

        [Fact]
        public void Avoids_overbooking()
        {
            var flight = new Flight(seatCapacity: 3);
            var error = flight.Book("jeal@mail.com", 4);
            error.Should().BeOfType<OverbookingError>();
        }

        [Fact]
        public void Books_flights_successfully()
        {
            var flight = new Flight(seatCapacity: 3);
            var error = flight.Book("jeal@mail.com", 1);
            error.Should().BeNull();
        }

        [Fact]
        public void Remember_bookings()
        {
            var fligth = new Flight(seatCapacity: 150);
            fligth.Book(passengerEmail: "jeal@mail.com", numberOfSeats: 4);
            fligth.BookingList.Should().ContainEquivalentOf(new Booking("jeal@mail.com", 4));
        }

        [Theory]
        [InlineData(3,1,1,3)]
        [InlineData(4,2,2,4)]
        [InlineData(7,5,4,6)]
        public void Canceling_bookings_frees_up_the_seats(
            int initialCapacity,
            int numberOfSeatsToBook,
            int numberOfSeatsToCancel,
            int remainingNumberOfSeats
            )
        {
            var flight = new Flight(initialCapacity);
            flight.Book(passengerEmail: "jeal@mail.com", numberOfSeats: numberOfSeatsToBook);
            flight.CancelBooking(passengerEmail: "jeal@mail.com", numberOfSeats: numberOfSeatsToCancel);
            flight.RemainingNumberOfSeats.Should().Be(remainingNumberOfSeats);
        }

        [Fact]
        public void Doesnt_cancel_booking_for_passangers_who_have_not_booked()
        {
            var flight = new Flight(3);
            var error = flight.CancelBooking(passengerEmail: "jeal@mail.com", numberOfSeats: 2);
            error.Should().BeOfType<BookingNotFoundError>();
        }

        [Fact]
        public void Returns_null_when_successfully_cancels_a_booking()
        {
            var flight = new Flight(3);
            flight.Book(passengerEmail: "jeal@mail.com", numberOfSeats: 1);
            var error = flight.CancelBooking(passengerEmail: "jeal@mail.com", numberOfSeats: 1);
            error.Should().BeNull();
        }
    }
}