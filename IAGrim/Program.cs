﻿using EvilsoftCommons.Exceptions;
using EvilsoftCommons.Exceptions.UUIDGenerator;
using EvilsoftCommons.SingleInstance;
using IAGrim.Backup.Azure.Constants;
using IAGrim.Database;
using IAGrim.Database.Interfaces;
using IAGrim.Database.Migrations;
using IAGrim.Database.Synchronizer;
using IAGrim.Parsers.Arz;
using IAGrim.Parsers.GameDataParsing.Service;
using IAGrim.Properties;
using IAGrim.UI;
using IAGrim.UI.Misc.CEF;
using IAGrim.Utilities;
using log4net;
using StatTranslator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using DllInjector;


namespace IAGrim
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        private static MainWindow _mw;

#if DEBUG

        private static void Test() {
            return;
            using (ThreadExecuter threadExecuter = new ThreadExecuter())
            {
                var factory = new SessionFactory();
                string _grimdawnLocation = new DatabaseSettingRepo(threadExecuter, factory).GetCurrentDatabasePath();
                IItemTagDao _itemTagDao = new ItemTagRepo(threadExecuter, factory);
                IDatabaseItemDao _databaseItemDao = new DatabaseItemRepo(threadExecuter, factory);
                IDatabaseItemStatDao _databaseItemStatDao = new DatabaseItemStatRepo(threadExecuter, factory);
                IItemSkillDao _itemSkillDao = new ItemSkillRepo(threadExecuter, factory);

                ParsingService parsingService = new ParsingService(
                    _itemTagDao,
                    _grimdawnLocation,
                    _databaseItemDao,
                    _databaseItemStatDao,
                    _itemSkillDao,
                    Properties.Settings.Default.LocalizationFile
                );

                parsingService.Execute();

                int x = 9;
            }

        }
#endif

        private static void LoadUuid(IDatabaseSettingDao dao)
        {
            string uuid = dao.GetUuid();
            if (string.IsNullOrEmpty(uuid))
            {
                UuidGenerator g = Guid.NewGuid();
                uuid = g.ToString().Replace("-", "");
                dao.SetUuid(uuid);
            }

            GlobalSettings.Uuid = uuid;
            ExceptionReporter.Uuid = uuid;
            Logger.InfoFormat("Your user id is {0}, use this for any bug reports", GlobalSettings.Uuid);
        }

        public static MainWindow MainWindow => _mw;

        [STAThread]
        private static void Main(string[] args)
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Main";

            Logger.Info("Starting IA:GD..");
            ExceptionReporter.UrlCrashreport = "http://ribbs.dreamcrash.org/iagd/crashreport.php";
            ExceptionReporter.UrlStats = "http://ribbs.dreamcrash.org/iagd/stats.php";
#if !DEBUG
            ExceptionReporter.LogExceptions = true;
#endif

            Logger.Info("Starting exception monitor for bug reports.."); // Phrased this way since people took it as a 'bad' thing.
            Logger.Debug("Crash reports can be seen at http://ribbs.dreamcrash.org/iagd/logs.html");
            ExceptionReporter.EnableLogUnhandledOnThread();

#if DEBUG
            AzureUris.Initialize(AzureUris.EnvAzure);
            //AzureUris.Initialize(AzureUris.EnvDev);
#else
            AzureUris.Initialize(AzureUris.EnvAzure);
#endif

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build)
                .AddSeconds(version.Revision * 2);

            Logger.InfoFormat("Running version {0}.{1}.{2}.{3} from {4}", version.Major, version.Minor, version.Build, version.Revision, buildDate.ToString("dd/MM/yyyy"));

            if (!DependencyChecker.CheckNet452Installed()) {
                MessageBox.Show("It appears .Net Framework 4.5.2 is not installed.\nIA May not function correctly", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (!DependencyChecker.CheckVs2013Installed()) {
                MessageBox.Show("It appears VS 2013 (x86) redistributable is not installed.\nPlease install it to continue using IA", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (!DependencyChecker.CheckVs2010Installed()) {
                MessageBox.Show("It appears VS 2010 (x86) redistributable is not installed.\nPlease install it to continue using IA", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

#if DEBUG
            Test();
#endif

            // Prevent running in RELEASE mode by accident
            // And thus risking the live database
#if !DEBUG
            if (Debugger.IsAttached) {
                Logger.Fatal("Debugger attached, please run in DEBUG mode");
                return;
            }
#endif
            //ParsingUIBackgroundWorker tmp = new ParsingUIBackgroundWorker();

            ItemHtmlWriter.CopyMissingFiles();

            Guid guid = new Guid("{F3693953-C090-4F93-86A2-B98AB96A9368}");
            using (SingleInstance singleInstance = new SingleInstance(guid))
            {
                if (singleInstance.IsFirstInstance)
                {
                    Logger.Info("Calling run..");
                    singleInstance.ArgumentsReceived += singleInstance_ArgumentsReceived;
                    singleInstance.ListenForArgumentsFromSuccessiveInstances();
                    using (ThreadExecuter threadExecuter = new ThreadExecuter())
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Logger.Info("Visual styles enabled..");
                        Run(args, threadExecuter);
                    }
                }
                else
                {
                    if (args != null && args.Length > 0)
                    {
                        singleInstance.PassArgumentsToFirstInstance(args);
                    }
                    else
                    {
                        singleInstance.PassArgumentsToFirstInstance(new string[] { "--ignore" });
                    }
                }
            }

            Logger.Info("IA Exited");
        }


        /// <summary>
        /// Upgrade any settings if required
        /// This happens for just about every compile
        /// </summary>
        private static bool UpgradeSettings()
        {
            try
            {
                if (Properties.Settings.Default.CallUpgrade)
                {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.CallUpgrade = false;
                    Logger.Info("Settings upgraded..");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.Message);
                Logger.Warn(ex.StackTrace);
                ExceptionReporter.ReportException(ex);
            }

            return false;
        }

        /// <summary>
        /// Attempting to run a second copy of the program
        /// </summary>
        private static void singleInstance_ArgumentsReceived(object _, ArgumentsReceivedEventArgs e)
        {
            try
            {
                if (_mw != null)
                {
                    _mw.Invoke((MethodInvoker)delegate { _mw.notifyIcon1_MouseDoubleClick(null, null); });
                    _mw.Invoke((MethodInvoker)delegate { _mw.Activate(); });
                }
            }
            catch (Exception ex)
            {
                ExceptionReporter.ReportException(ex, "singleInstance_ArgumentsReceived");
            }
        }

        // TODO: This creates another session instance, should be executed inside the ThreadExecuter
        private static void PrintStartupInfo(SessionFactory factory)
        {
            if (Properties.Settings.Default.StashToLootFrom == 0)
            {
                Logger.Info("IA is configured to loot from the last stash page");
            }
            else
            {
                Logger.Info($"IA is configured to loot from stash page #{Properties.Settings.Default.StashToLootFrom}");
            }
            if (Properties.Settings.Default.StashToDepositTo == 0)
            {
                Logger.Info("IA is configured to deposit to the second-to-last stash page");
            }
            else
            {
                Logger.Info($"IA is configured to deposit to stash page #{Properties.Settings.Default.StashToDepositTo}");
            }

            using (var session = factory.OpenSession())
            {
                var numItemsStored = session.CreateCriteria<PlayerItem>()
                    .SetProjection(NHibernate.Criterion.Projections.RowCountInt64())
                    .UniqueResult<long>();

                Logger.Info($"There are {numItemsStored} items stored in the database.");
            }

            if (Properties.Settings.Default.BuddySyncEnabled)
                Logger.Info($"Buddy items is enabled with user id {Properties.Settings.Default.BuddySyncUserIdV2}");
            else
                Logger.Info("Buddy items is disabled");

            if (Properties.Settings.Default.ShowRecipesAsItems)
                Logger.Info("Show recipes as items is enabled");
            else
                Logger.Info("Show recipes as items is disabled");

            Logger.Info("Transfer to any mod is " + (Properties.Settings.Default.TransferAnyMod ? "enabled" : "disabled"));
            Logger.Info("Experimental updates is " + (Properties.Settings.Default.SubscribeExperimentalUpdates ? "enabled" : "disabled"));


            var mods = GlobalPaths.TransferFiles;
            if (mods.Count == 0)
            {
                Logger.Warn("No transfer files has been found");
            }
            else
            {
                Logger.Info("The following transfer files has been found:");
                foreach (var mod in mods)
                {
                    Logger.Info($"\"{mod.Filename}\": Mod: \"{mod.Mod}\", HC: {mod.IsHardcore}");
                }
            }

            Logger.Info("There are items stored for the following mods:");
            foreach (var entry in new PlayerItemDaoImpl(factory, new DatabaseItemStatDaoImpl(factory)).GetModSelection())
            {
                Logger.Info($"Mod: \"{entry.Mod}\", HC: {entry.IsHardcore}");
            }

            var gdPath = new DatabaseSettingDaoImpl(factory).GetCurrentDatabasePath();
            if (string.IsNullOrEmpty(gdPath))
            {
                Logger.Info("The path to Grim Dawn is unknown (not great)");
            }
            else
            {
                Logger.Info($"The path to Grim Dawn is \"{gdPath}\"");
            }

            Logger.Info("Startup data dump complete");
        }

        private static void DumpTranslationTemplate() {
            try {
                File.WriteAllText(Path.Combine(GlobalPaths.CoreFolder, "tags_ia.template.txt"), new EnglishLanguage(new Dictionary<string, string>()).Export());
            }
            catch (Exception ex) {
                Logger.Debug("Error dumping translation template", ex);
            }
        }

        private static void Run(string[] args, ThreadExecuter threadExecuter)
        {
            var factory = new SessionFactory();

            // Settings should be upgraded early, it contains the language pack etc and some services depends on settings.
            IPlayerItemDao playerItemDao = new PlayerItemRepo(threadExecuter, factory);
            var statUpgradeNeeded = UpgradeSettings();

            // X
            IDatabaseItemDao databaseItemDao = new DatabaseItemRepo(threadExecuter, factory);
            GlobalSettings.InitializeLanguage(Properties.Settings.Default.LocalizationFile, databaseItemDao.GetTagDictionary());
            DumpTranslationTemplate();

            // Prohibited for now
            threadExecuter.Execute(() => new MigrationHandler(factory).Migrate());

            IDatabaseSettingDao databaseSettingDao = new DatabaseSettingRepo(threadExecuter, factory);
            LoadUuid(databaseSettingDao);
            var azurePartitionDao = new AzurePartitionRepo(threadExecuter, factory);
            IDatabaseItemStatDao databaseItemStatDao = new DatabaseItemStatRepo(threadExecuter, factory);
            IItemTagDao itemTagDao = new ItemTagRepo(threadExecuter, factory);


#if !DEBUG
            if (statUpgradeNeeded) {

                // If we don't also update item stats, a lot of whining will ensue.
                // This accounts for most database updates (new fields added that needs to get populated etc)
                UpdatingPlayerItemsScreen x = new UpdatingPlayerItemsScreen(playerItemDao);
                x.ShowDialog();
            }
#endif


            IBuddyItemDao buddyItemDao = new BuddyItemRepo(threadExecuter, factory);
            IBuddySubscriptionDao buddySubscriptionDao = new BuddySubscriptionRepo(threadExecuter, factory);
            IRecipeItemDao recipeItemDao = new RecipeItemRepo(threadExecuter, factory);
            IItemSkillDao itemSkillDao = new ItemSkillRepo(threadExecuter, factory);
            ArzParser arzParser = new ArzParser(databaseSettingDao);
            AugmentationItemRepo augmentationItemRepo = new AugmentationItemRepo(threadExecuter, factory, new DatabaseItemStatDaoImpl(factory));

            Logger.Debug("Updating augment state..");
            augmentationItemRepo.UpdateState();

            // TODO: GD Path has to be an input param, as does potentially mods.
            ParsingService parsingService = new ParsingService(itemTagDao, null, databaseItemDao, databaseItemStatDao, itemSkillDao, Properties.Settings.Default.LocalizationFile);

            PrintStartupInfo(factory);



            if (GlobalSettings.Language is EnglishLanguage language)
            {
                foreach (var tag in itemTagDao.GetClassItemTags())
                {
                    language.SetTagIfMissing(tag.Tag, tag.Name);
                }
            }

            if (args != null && args.Any(m => m.Contains("-logout"))) {
                Logger.Info("Started with -logout specified, logging out of online backups.");
                Settings.Default.AzureAuthToken = null;
                Settings.Default.Save();
            }

            // TODO: Urgent, introduce DI and have MainWindow receive premade objects, not create them itself.
            using (CefBrowserHandler browser = new CefBrowserHandler())
            {
                _mw = new MainWindow(browser,
                    databaseItemDao,
                    databaseItemStatDao,
                    playerItemDao,
                    azurePartitionDao,
                    databaseSettingDao,
                    buddyItemDao,
                    buddySubscriptionDao,
                    recipeItemDao,
                    itemSkillDao,
                    itemTagDao,
                    parsingService,
                    augmentationItemRepo
                );

                Logger.Info("Checking for database updates..");


                // Load the GD database (or mod, if any)
                string GDPath = databaseSettingDao.GetCurrentDatabasePath();
                if (string.IsNullOrEmpty(GDPath) || !Directory.Exists(GDPath))
                {
                    GDPath = GrimDawnDetector.GetGrimLocation();
                }

                if (!string.IsNullOrEmpty(GDPath) && Directory.Exists(GDPath)) {

                    var numFiles = Directory.GetFiles(GlobalPaths.StorageFolder).Length;
                    int numFilesExpected = 2100;
                    if (Directory.Exists(Path.Combine(GDPath, "gdx2"))) {
                        numFilesExpected += 580;
                    }
                    if (Directory.Exists(Path.Combine(GDPath, "gdx1"))) {
                        numFilesExpected += 890;
                    }

                    if (numFiles < numFilesExpected) {
                        Logger.Debug($"Only found {numFiles} in storage, expected ~{numFilesExpected}+, parsing item icons.");
                        ThreadPool.QueueUserWorkItem((m) => ArzParser.LoadIconsOnly(GDPath));
                    }

                }
                else
                {
                    Logger.Warn("Could not find the Grim Dawn install location");
                }

                playerItemDao.UpdateHardcoreSettings();

                _mw.Visible = false;
                if (DonateNagScreen.CanNag)
                    Application.Run(new DonateNagScreen());

                Logger.Info("Running the main application..");


                Application.Run(_mw);
            }

            Logger.Info("Application ended.");
        }
    }
}