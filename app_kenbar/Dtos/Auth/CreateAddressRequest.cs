namespace Kenbar.Api.Dtos.Addresses
{
    public class CreateAddressRequest
    {
        public string ReceiverName { get; set; } = "";

        public string ReceiverPhone { get; set; } = "";

        public string Province { get; set; } = "";

        public string District { get; set; } = "";

        public string Ward { get; set; } = "";

        public string AddressLine { get; set; } = "";

        public bool IsDefault { get; set; } = false;
    }
}