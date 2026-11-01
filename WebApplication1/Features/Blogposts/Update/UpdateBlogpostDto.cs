namespace WebApplication1.Features.Blogposts.Update
{
    public class UpdateBlogpostDto
    {
        public string Titel { get; set; } = "";
        public string Inhoud { get; set; } = "";
        public DateTime Publicatiedatum { get; set; }
    }
}