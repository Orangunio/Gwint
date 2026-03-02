namespace Backend.Models.Body
{
    public class UpdateDeckBody
    {
        public int PlayerId { get; set; }
        public List<int> CardIdsToAdd { get; set; }
        public List<int> CardIdsToRemove { get; set; }
    }
}