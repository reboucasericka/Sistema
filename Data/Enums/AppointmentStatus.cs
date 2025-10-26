using System.ComponentModel;

namespace Sistema.Data.Enums
{
    public enum AppointmentStatus
    {
        [Description("Pending")]
        Pending = 1,

        [Description("Confirmed")]
        Confirmed = 2,

        [Description("In Progress")]
        InProgress = 3,

        [Description("Completed")]
        Completed = 4,

        [Description("Cancelled")]
        Cancelled = 5
    }
}
