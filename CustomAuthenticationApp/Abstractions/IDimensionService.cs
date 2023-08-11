namespace CustomAuthenticationApp.Abstractions
{
    public interface IDimensionService
    {
        Task<RectangleSize> StartAsync(Action<RectangleSize> action);
    }

    public record struct RectangleSize(int Width, int Height);

    public class DimensionChangeOptions
    {
        public int ReportRate { get; set; } = 300;
    }
}