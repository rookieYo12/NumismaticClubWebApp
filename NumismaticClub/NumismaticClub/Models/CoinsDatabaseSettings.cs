namespace NumismaticClub.Models
{
    public class CoinsDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string CoinsCollectionName { get; set; } = null!;
    }
}
