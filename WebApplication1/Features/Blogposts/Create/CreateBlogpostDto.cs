namespace WebApplication1.Features.Blogposts.Create
{
    public class CreateBlogpostDto
    {
        public string Titel { get; set; } = "";
        public string Inhoud { get; set; } = "";
        public DateTime Publicatiedatum { get; set; }
    }
}