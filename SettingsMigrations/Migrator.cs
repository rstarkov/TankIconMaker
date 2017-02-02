using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TankIconMaker.SettingsMigrations
{
    class Migrator
    {
        private static List<ISettingsMigration> migrations;

        static Migrator()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var inter = typeof(ISettingsMigration);
            var types = assembly.GetTypes().Where(x => inter.IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
            migrations = new List<ISettingsMigration>();
            foreach (var type in types)
            {
                var migration = (ISettingsMigration)Activator.CreateInstance(type);
                migrations.Add(migration);
            }
        }

        public void MigrateToVersion(Settings settings, int oldVersion, int newVersion)
        {
            var migrationQueue = this.GetSuitableMigrations(oldVersion, newVersion);
            foreach (var migration in migrationQueue)
            {
                migration.MigrateSettings(settings);
            }

            foreach (var style in settings.Styles)
            {
                style.SavedByVersion = newVersion;
            }

            settings.SavedByVersion = newVersion;
        }

        public void MigrateToVersion(Style style, int oldVersion, int newVersion)
        {
            var migrationQueue = this.GetSuitableMigrations(oldVersion, newVersion);
            foreach (var migration in migrationQueue)
            {
                migration.MigrateStyle(style);
            }

            style.SavedByVersion = newVersion;
        }

        private List<ISettingsMigration> GetSuitableMigrations(int versionCur, int versionTo)
        {
            var migrationQueue = new List<ISettingsMigration>();
            foreach (var migration in migrations)
            {
                if (migration.ApplyForVersion > versionCur && migration.ApplyForVersion <= versionTo)
                {
                    migrationQueue.Add(migration);
                }
            }

            migrationQueue.Sort((x, y) => x.ApplyForVersion - y.ApplyForVersion);
            return migrationQueue;
        }
    }
}
