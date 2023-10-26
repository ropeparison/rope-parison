namespace RopeParison.Protocol
{
    public class CategoryDto
    {
        public int CategoryId { get { return (int)Category; } }
        public string? Name { get { return Category.ToString(); } }
        public Category Category { get; set; }

        public string? DisplayName { get; set; }
        public StandardDto? Standard { get; set; }
    }

}
