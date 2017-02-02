namespace TankIconMaker.SettingsMigrations
{
    class SettingsMigration_v051 : ISettingsMigration
    {
        public int ApplyForVersion
        {
            get { return 51; }
        }

        public void MigrateSettings(Settings settings)
        {
            foreach (var style in settings.Styles)
            {
                this.MigrateStyle(style);
            }
        }

        public void MigrateStyle(Style style)
        {
            if (!string.IsNullOrEmpty(style.PathTemplate))
            {
                style.PathTemplate = Ut.AppendExpandableFilename(style.PathTemplate, SaveType.Icons);
            }

            if (!string.IsNullOrEmpty(style.BattleAtlasPathTemplate))
            {
                style.BattleAtlasPathTemplate = Ut.AppendExpandableFilename(style.BattleAtlasPathTemplate, SaveType.BattleAtlas);
            }

            if (!string.IsNullOrEmpty(style.VehicleMarkersAtlasPathTemplate))
            {
                style.VehicleMarkersAtlasPathTemplate =
                    Ut.AppendExpandableFilename(style.VehicleMarkersAtlasPathTemplate, SaveType.VehicleMarkerAtlas);
            }
        }
    }
}
