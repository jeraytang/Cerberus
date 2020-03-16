namespace Cerberus.API.DTO
{
    public class ListUserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Title { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string Company { get; set; }
        public bool Enabled { get; set; }
        public string Source { get; set; }
    }
}