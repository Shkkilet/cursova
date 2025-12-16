using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cursova.Models
{
    public class Trip
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string StartTrip { get; set; } = string.Empty;
        public string StartTripName { get; set; } = string.Empty;

        public string EndTrip { get; set; } = string.Empty;
        public string EndTripName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public TripType Type { get; set; }

        public bool IsDone { get; set; } = false;
    }

    public enum TripType
    {
        Vehicle,
        Airplane,
        Bus,
        Pedestrian
    }
}
