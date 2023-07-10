namespace Tax
{
    public class Address
    {
        public string Name { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
        public required string CountryCode { get; set; }
        public object toJSON()
        {
            return new
            {
                type = this.Name,
                name = this.Street,
                description = this.City
            };
        }
    }
}