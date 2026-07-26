namespace RacingDSX
{
    public struct DualSenseReportStruct
    {
        public enum StatusType : ushort
        {
            NONE = 0,
            CONNECT = 1,
            DISCONNECT = 2,
            SELECT = 3,
            UNSELECT = 4,
        }

        public DualSenseReportStruct(StatusType statusType, string message = "", bool value = false, string? macAddress = null) {
            status = statusType;
            this.message = message;
            this.value = value;
            this.macAddress = macAddress;
        }

        public StatusType status;
        public string message;
        public bool value;
        public string? macAddress;
    }
}
