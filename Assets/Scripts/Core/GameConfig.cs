namespace Tank90
{
    /// <summary>Global gameplay constants. World units: 1 cell = 1 unit (PPU 16); sub-tile = 0.5 unit (8px).</summary>
    public static class GameConfig
    {
        public const int FieldCells = 13;        // 13x13 playfield
        public const float CellSize = 1f;        // world units per cell
        public const float TileSize = 0.5f;      // sub-tile (8px) for bricks / alignment

        // Playfield bounds (cell centers span 0..12; outer walls at +/-0.5).
        public const float FieldMin = -0.5f;
        public const float FieldMax = FieldCells - 0.5f; // 12.5
        public const float FieldCenter = (FieldCells - 1) / 2f; // 6

        // Speeds (units per second)
        public const float PlayerMoveSpeed = 4f;
        public const float EnemyMoveSpeed = 2.5f;
        public const float BulletSpeed = 9f;

        // Sorting orders
        public const int OrderTerrainLow = 0;    // water, ice (under tanks)
        public const int OrderTank = 10;
        public const int OrderBullet = 15;
        public const int OrderTerrainHigh = 20;  // trees (over tanks)
        public const int OrderEffect = 30;

        // Tags & layers
        public const string TagPlayer = "Player";
        public const string TagEnemy = "Enemy";
        public const string TagBullet = "Bullet";
        public const string TagBase = "Base";
    }
}
