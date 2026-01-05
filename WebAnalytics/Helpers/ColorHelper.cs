namespace WebAnalytics.Helpers
{
    public static class ColorHelper
    {
        private static readonly string[] Palette = new[]
        {
        "#73a3f9", "#fbbb4b", "#c3cad4", "#48d0ad", "#a88bfa",
        "#4ee17e", "#fee47d", "#f76e6e", "#8f88f9", "#3edfd0",
        "#f9cb48", "#d7aaff", "#4fd4ff", "#faa4a4", "#a6eb48",
        "#faa6d6", "#c6b4ff", "#40dff4", "#fa4d78", "#a285ff",
        "#ffb3ba", "#ffdfba", "#ffffba", "#baffc9", "#bae1ff",
        "#ff9cee", "#ffb347", "#c3f0ca", "#f6d6ad", "#d4a5a5",
        "#a3c4f3", "#ffadad", "#ffd6a5", "#fdffb6", "#caffbf",
        "#9bf6ff", "#a0c4ff", "#bdb2ff", "#ffc6ff", "#fffffc",
        "#ffcad4", "#b5ead7", "#ffc3a0", "#ffaaa6", "#d5b8ff",
        "#d0f4de", "#fcd5ce", "#f8edeb", "#f0efeb", "#d8e2dc"
    };

        private static readonly Random rand = new Random();

        public static string GetRandomHexColor()
        {
            int index = rand.Next(Palette.Length);
            return Palette[index];
        }
    }
}
