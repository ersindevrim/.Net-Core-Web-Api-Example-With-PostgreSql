namespace Todo2Api.V2.Models
{
    public class TodoItem
    {
        public long Id {get;set;}
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; }
    }
}