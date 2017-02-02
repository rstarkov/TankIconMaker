namespace TankIconMaker.SettingsMigrations
{
    interface ISettingsMigration
    {
        /// <summary>
        /// Target version (migration number 51 is apllied when switching from TIM 50 to 51)
        /// </summary>
        int ApplyForVersion { get; }

        /// <summary>
        /// Migrate settings. It called on updated TIM first start.
        /// </summary>
        /// <param name="settings"></param>
        void MigrateSettings(Settings settings);

        /// <summary>
        /// Migrate style. It called by TIM only when user import an old style.
        /// </summary>
        /// <param name="style"></param>
        void MigrateStyle(Style style);
    }
}
