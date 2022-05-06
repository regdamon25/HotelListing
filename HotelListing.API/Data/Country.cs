namespace HotelListing.API.Data
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        //One to many Relationship
        public virtual IList<Hotel> Hotels { get; set; }
    }
}
