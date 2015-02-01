﻿namespace CoC.Bot.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using CoC.Bot.Data;
    using CoC.Bot.Tools;
    using CoC.Bot.Tools.FastFind;
    using CoC.Bot.UI.Commands;

    /// <summary>
    /// Provides functionality for the MainWindow
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private static StringBuilder _output = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            /*
             * -------------------------------------------------------------------------------------------------------------
             * UI Usage Notes
             * -------------------------------------------------------------------------------------------------------------
             * 
             * 
             * HowTo: Start/Stop/Hide etc
             * ------------------------------------------------------------------------------------------------------------
             * Under 'Main Methods' you will find the Start(), Stop(), Hide(), etc methods.
             * All those methods are already vinculed to the UI by using Commands
             * You can make them async if you wish.
             * 
             * 
             * HowTo: Access a specific value or setting in the UI 
             * ------------------------------------------------------------------------------------------------------------
             * All UI properties (User settings) are defined in Properties, no need to access the controls directly.
             * For example:
             *          (bool)  MeetGold
             *          (int)   MinimumGold
             *          (Model) SelectedDeployStrategy
             *          (int)   SelectedTroopComposition.Id     <--- The Id is defined in Data.TroopComposition enum
             *                  DataCollection.TroopTiers       <--- Contains the Troop Tier (Tier 1, Tier 2, Tier 3, etc)
             *                  DataCollection.TroopTiers.Troop <--- ontains Troops per Tier (Barbs, Archs, ... in Tier 1)
             *          
             * 
             * HowTo: Pass a value or values (Properties) into another method/class for accessing it
             * ------------------------------------------------------------------------------------------------------------
             * Just use as parameter the MainViewModel. See Samples.GettingAroundTheUI for a code example.
             * We can access all values by passing the MainViewModel as parameter.
             * We can retrieve or change a property's value, those will get reflected automatically in the UI.
             * Ex.:
             *          Samples.GettingAroundTheUI.UseValuesInUI(this);
             * 
             * 
             * HowTo: Access the Troops data in the Donate Settings
             * ------------------------------------------------------------------------------------------------------------
             * Each Troop is stored in TroopTier which is exposed by the TroopTiers property.
             * You can access a TroopTier by using (either one) as example:
             *          var t1 = DataCollection.TroopTiers[(int)TroopType.Tier1];
             *          var t1 = DataCollection.TroopTiers.Get(TroopType.Tier1);
             *          var t1 = DataCollection.TroopTiers.Where(tt => tt.Id == (int)TroopType.Tier1).FirstOrDefault();
             *          
             * You can access a Troop by using (either one) as example (same as TroopTier):
             *          var troop = DataCollection.TroopTiers[(int)TroopType.Tier1].Troops[Troop.Barbarian];
             *          var troop = DataCollection.TroopTiers.Get(TroopType.Tier1).Troops.Get(Troop.Barbarian);
             *          var troop = DataCollection.TroopTiers.All().Troops.Get(Troop.Barbarian);
             *          
             * You can acces a specific Troop setting, for example:
             *          var troop = DataCollection.TroopTiers.All().Troops.Get(Troop.Barbarian).IsDonateAll;
             *          var troop = DataCollection.TroopTiers.All().Troops.Get(Troop.Barbarian).DonateKeywords;
             * 
             * 
             * HowTo: Write to the Output (Window Log)
             * ------------------------------------------------------------------------------------------------------------
             * Just set a string value into the Output property.
             * For example:
             *          Output = "Hello there!";
             * 
             * 
             * ------------------------------------------------------------------------------------------------------------
             */

            Init();
            GetUserSettings();

            Message = Properties.Resources.StartMessage;
        }

        #region Properties

        /// <summary>
        /// Gets the application title.
        /// </summary>
        /// <value>The application title.</value>
        public static string AppTitle { get { return string.Format("{0} v{1}", Properties.Resources.AppName, typeof(App).Assembly.GetName().Version.ToString(3)); } }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <value>The application settings.</value>
        internal static Properties.Settings AppSettings { get { return Properties.Settings.Default; } }

        #region Behaviour Properties

        /// <summary>
        /// Gets or sets a value indicating whether this bot has started.
        /// </summary>
        /// <value><c>true</c> if this bot has started; otherwise, <c>false</c>.</value>
        private bool IsStarted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this bot is hidden.
        /// </summary>
        /// <value><c>true</c> if this bot is hidden; otherwise, <c>false</c>.</value>
        private bool IsHidden { get; set; }

        #endregion

        #region General Properties

        /// <summary>
        /// Gets or sets the Output (Window Log).
        /// </summary>
        /// <value>The Output (Window Log).</value>
        public string Output
        {
            get { return _output.ToString(); }
            set
            {
                if (_output == null)
                    _output = new StringBuilder(value);
                else
                    _output.AppendFormat("[{0:HH:mm:ss}] {1}", DateTime.Now, value + Environment.NewLine);

                Message = value;

                OnPropertyChanged();
            }
        }

        private string _message;
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>The status message.</value>
        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxTrophies;
        /// <summary>
        /// Gets or sets the maximum Trophies.
        /// </summary>
        /// <value>The maximum Trophies.</value>
        public int MaxTrophies
        {
            get { return _maxTrophies; }
            set
            {
                if (_maxTrophies != value)
                {
                    _maxTrophies = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Search Settings Properties

        private bool _meetGold;
        /// <summary>
        /// Gets or sets a value indicating whether should meet Gold conditions.
        /// </summary>
        /// <value><c>true</c> if should meet Gold conditions; otherwise, <c>false</c>.</value>
        public bool MeetGold
        {
            get { return _meetGold; }
            set
            {
                if (_meetGold != value)
                {
                    _meetGold = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _meetElixir;
        /// <summary>
        /// Gets or sets a value indicating whether should meet Elixir conditions.
        /// </summary>
        /// <value><c>true</c> if should meet Elixir conditions; otherwise, <c>false</c>.</value>
        public bool MeetElixir
        {
            get { return _meetElixir; }
            set
            {
                if (_meetElixir != value)
                {
                    _meetElixir = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _meetDarkElixir;
        /// <summary>
        /// Gets or sets a value indicating whether should meet Dark Elixir conditions.
        /// </summary>
        /// <value><c>true</c> if should meet Dark Elixir conditions; otherwise, <c>false</c>.</value>
        public bool MeetDarkElixir
        {
            get { return _meetDarkElixir; }
            set
            {
                if (_meetDarkElixir != value)
                {
                    _meetDarkElixir = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _meetTrophyCount;
        /// <summary>
        /// Gets or sets a value indicating whether should meet Trophy count conditions.
        /// </summary>
        /// <value><c>true</c> if should meet Trophy count conditions; otherwise, <c>false</c>.</value>
        public bool MeetTrophyCount
        {
            get { return _meetTrophyCount; }
            set
            {
                if (_meetTrophyCount != value)
                {
                    _meetTrophyCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _meetTownhallLevel;
        /// <summary>
        /// Gets or sets a value indicating whether should meet Townhall level conditions.
        /// </summary>
        /// <value><c>true</c> if should meet Townhall level conditions; otherwise, <c>false</c>.</value>
        public bool MeetTownhallLevel
        {
            get { return _meetTownhallLevel; }
            set
            {
                if (_meetTownhallLevel != value)
                {
                    _meetTownhallLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumGold;
        /// <summary>
        /// Gets or sets the minimum Gold.
        /// </summary>
        /// <value>The minimum Gold.</value>
        public int MinimumGold
        {
            get { return _minimumGold; }
            set
            {
                if (_minimumGold != value)
                {
                    _minimumGold = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumElixir;
        /// <summary>
        /// Gets or sets the minimum Elixir.
        /// </summary>
        /// <value>The minimum Elixir.</value>
        public int MinimumElixir
        {
            get { return _minimumElixir; }
            set
            {
                if (_minimumElixir != value)
                {
                    _minimumElixir = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumDarkElixir;
        /// <summary>
        /// Gets or sets the minimum Dark Elixir.
        /// </summary>
        /// <value>The minimum Dark Elixir.</value>
        public int MinimumDarkElixir
        {
            get { return _minimumDarkElixir; }
            set
            {
                if (_minimumDarkElixir != value)
                {
                    _minimumDarkElixir = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumTrophyCount;
        /// <summary>
        /// Gets or sets the minimum Trophy count.
        /// </summary>
        /// <value>The minimum Trophy count.</value>
        public int MinimumTrophyCount
        {
            get { return _minimumTrophyCount; }
            set
            {
                if (_minimumTrophyCount != value)
                {
                    _minimumTrophyCount = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _minimumTownhallLevel;
        /// <summary>
        /// Gets or sets the minimum Townhall level.
        /// </summary>
        /// <value>The minimum Townhall level.</value>
        public int MinimumTownhallLevel
        {
            get { return _minimumTownhallLevel; }
            set
            {
                if (_minimumTownhallLevel != value)
                {
                    _minimumTownhallLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _alertWhenBaseFound;
        /// <summary>
        /// Gets or sets a value indicating whether should alert when base found.
        /// </summary>
        /// <value><c>true</c> if alert when base found; otherwise, <c>false</c>.</value>
        public bool AlertWhenBaseFound
        {
            get { return _alertWhenBaseFound; }
            set
            {
                if (_alertWhenBaseFound != value)
                {
                    _alertWhenBaseFound = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Attack Settings Properties

        public static IEnumerable<int> CannonLevels { get { return BuildingLevels.Cannon; } }
        public static IEnumerable<int> ArcherTowerLevels { get { return BuildingLevels.ArcherTower; } }
        public static IEnumerable<int> MortarLevels { get { return BuildingLevels.Mortar; } }
        public static IEnumerable<int> WizardTowerLevels { get { return BuildingLevels.WizardTower; } }
        public static IEnumerable<int> XbowLevels { get { return BuildingLevels.Xbow; } }

        private int _selectedMaxCannonLevel;
        /// <summary>
        /// Gets or sets the maximum Cannon level.
        /// </summary>
        /// <value>The maximum Cannon level.</value>
        public int SelectedMaxCannonLevel
        {
            get { return _selectedMaxCannonLevel; }
            set
            {
                if (_selectedMaxCannonLevel != value)
                {
                    _selectedMaxCannonLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedMaxArcherTowerLevel;
        /// <summary>
        /// Gets or sets the maximum Archer Tower level.
        /// </summary>
        /// <value>The maximum Archer Tower level.</value>
        public int SelectedMaxArcherTowerLevel
        {
            get { return _selectedMaxArcherTowerLevel; }
            set
            {
                if (_selectedMaxArcherTowerLevel != value)
                {
                    _selectedMaxArcherTowerLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedMaxMortarLevel;
        /// <summary>
        /// Gets or sets the maximum Mortar level.
        /// </summary>
        /// <value>The maximum Mortar level.</value>
        public int SelectedMaxMortarLevel
        {
            get { return _selectedMaxMortarLevel; }
            set
            {
                if (_selectedMaxMortarLevel != value)
                {
                    _selectedMaxMortarLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedWizardTowerLevel;
        /// <summary>
        /// Gets or sets the maximum Wizard Tower level.
        /// </summary>
        /// <value>The maximum Wizard Tower level.</value>
        public int SelectedWizardTowerLevel
        {
            get { return _selectedWizardTowerLevel; }
            set
            {
                if (_selectedWizardTowerLevel != value)
                {
                    _selectedWizardTowerLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedXbowLevel;
        /// <summary>
        /// Gets or sets the maximum Xbow level.
        /// </summary>
        /// <value>The maximum Xbow level.</value>
        public int SelectedXbowLevel
        {
            get { return _selectedXbowLevel; }
            set
            {
                if (_selectedXbowLevel != value)
                {
                    _selectedXbowLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _attackTheirKing;
        /// <summary>
        /// Gets or sets a value indicating whether to attack their King.
        /// </summary>
        /// <value><c>true</c> if attack their King; otherwise, <c>false</c>.</value>
        public bool AttackTheirKing
        {
            get { return _attackTheirKing; }
            set
            {
                if (_attackTheirKing != value)
                {
                    _attackTheirKing = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _attackTheirQueen;
        /// <summary>
        /// Gets or sets a value indicating whether to attack their Queen.
        /// </summary>
        /// <value><c>true</c> if attack their Queen; otherwise, <c>false</c>.</value>
        public bool AttackTheirQueen
        {
            get { return _attackTheirQueen; }
            set
            {
                if (_attackTheirQueen != value)
                {
                    _attackTheirQueen = value;
                    OnPropertyChanged();
                }
            }
        }

        private AttackMode _selectedAttackMode;
        /// <summary>
        /// Gets or sets the selected attack mode.
        /// </summary>
        /// <value>The selected attack mode.</value>
        public AttackMode SelectedAttackMode
        {
            get { return _selectedAttackMode; }
            set
            {
                if (_selectedAttackMode != value)
                {
                    _selectedAttackMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private HeroAttackMode _selectedKingAttackMode;
        /// <summary>
        /// Gets or sets the selected King attack mode.
        /// </summary>
        /// <value>The selected King attack mode.</value>
        public HeroAttackMode SelectedKingAttackMode
        {
            get { return _selectedKingAttackMode; }
            set
            {
                if (_selectedKingAttackMode != value)
                {
                    _selectedKingAttackMode = value;
                    OnPropertyChanged();
                }
            }
        }

        private HeroAttackMode _selectedQueenAttackMode;
        /// <summary>
        /// Gets or sets the selected Queen attack mode.
        /// </summary>
        /// <value>The selected Queen attack mode.</value>
        public HeroAttackMode SelectedQueenAttackMode
        {
            get { return _selectedQueenAttackMode; }
            set
            {
                if (_selectedQueenAttackMode != value)
                {
                    _selectedQueenAttackMode = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the Deploy Strategies.
        /// </summary>
        /// <value>The Deploy Strategies.</value>
        public static BindingList<Model> DeployStrategies { get { return DataCollection.DeployStrategies; } }

        private Model _selectedDeployStrategy;
        public Model SelectedDeployStrategy
        {
            get { return _selectedDeployStrategy; }
            set
            {
                if (_selectedDeployStrategy != value)
                {
                    _selectedDeployStrategy = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the Deploy Troops.
        /// </summary>
        /// <value>The Deploy Troops.</value>
        public static BindingList<Model> DeployTroops { get { return DataCollection.DeployTroops; } }

        private Model _selectedDeployTroop;
        public Model SelectedDeployTroop
        {
            get { return _selectedDeployTroop; }
            set
            {
                if (_selectedDeployTroop != value)
                {
                    _selectedDeployTroop = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _attackTownhall;
        /// <summary>
        /// Gets or sets a value indicating whether to attack the Townhall.
        /// </summary>
        /// <value><c>true</c> if attack to Townhall; otherwise, <c>false</c>.</value>
        public bool AttackTownhall
        {
            get { return _attackTownhall; }
            set
            {
                if (_attackTownhall != value)
                {
                    _attackTownhall = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _attackUsingClanCastle;
        /// <summary>
        /// Gets or sets a value indicating whether to attack using clan castle troops.
        /// </summary>
        /// <value><c>true</c> if attack using clan castle troops; otherwise, <c>false</c>.</value>
        public bool AttackUsingClanCastle
        {
            get { return _attackUsingClanCastle; }
            set
            {
                if (_attackUsingClanCastle != value)
                {
                    _attackUsingClanCastle = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Donate Settings Properties

        /// <summary>
        /// Gets the Troop Tier.
        /// </summary>
        /// <value>The Troop Compositions.</value>
        public static BindingList<TroopTierModel> TroopTiers { get { return DataCollection.TroopTiers; } }

        private bool _requestTroops;
        /// <summary>
        /// Gets or sets a value indicating whether it should request for troops.
        /// </summary>
        /// <value><c>true</c> if request for troops; otherwise, <c>false</c>.</value>
        public bool RequestTroops
        {
            get { return _requestTroops; }
            set
            {
                if (_requestTroops != value)
                {
                    _requestTroops = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _requestTroopsMessage;
        /// <summary>
        /// Gets or sets the request troops message.
        /// </summary>
        /// <value>The request troops message.</value>
        public string RequestTroopsMessage
        {
            get { return _requestTroopsMessage; }
            set
            {
                if (_requestTroopsMessage != value)
                {
                    _requestTroopsMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private object _selectedTroopForDonate;
        /// <summary>
        /// [For use in UI only] Gets or sets the selected troop for donate.
        /// </summary>
        /// <value>The selected troop for donate.</value>
        public object SelectedTroopForDonate
        {
            get { return _selectedTroopForDonate; }
            set
            {
                if (_selectedTroopForDonate != value)
                {
                    _selectedTroopForDonate = value;
                    if (_selectedTroopForDonate is TroopModel)
                    {
                        ShouldHideDonateControls = Visibility.Visible;
                        ShouldHideTierInfoMessage = Visibility.Collapsed;

                        var troop = (TroopModel)_selectedTroopForDonate;
                        IsCurrentDonateAll = troop.IsDonateAll;
                        CurrentDonateKeywords = troop.DonateKeywords.Replace(@"|", Environment.NewLine);
                    }
                    else
                    {
                        var tt = (TroopTierModel)_selectedTroopForDonate;

                        switch ((TroopType)tt.Id)
                        {
                            case TroopType.Tier1:
                                TroopTierSelectedInfoMessage = Properties.Resources.Tier1;
                                break;
                            case TroopType.Tier2:
                                TroopTierSelectedInfoMessage = Properties.Resources.Tier2;
                                break;
                            case TroopType.Tier3:
                                TroopTierSelectedInfoMessage = Properties.Resources.Tier3;
                                break;
                            case TroopType.DarkTroops:
                                TroopTierSelectedInfoMessage = Properties.Resources.DarkTroops;
                                break;
                            default:
                                TroopTierSelectedInfoMessage = string.Empty;
                                break;
                        }

                        ShouldHideDonateControls = Visibility.Collapsed;
                        ShouldHideTierInfoMessage = Visibility.Visible;
                    }

                    OnPropertyChanged();
                }
            }
        }

        private bool _isCurrentDonateAll;
        /// <summary>
        /// [For use in UI only] Gets or sets a value indicating whether the selected troop is for donate all.
        /// </summary>
        /// <value><c>true</c> if selected troop is for donate all; otherwise, <c>false</c>.</value>
        public bool IsCurrentDonateAll
        {
            get { return _isCurrentDonateAll; }
            set
            {
                if (_isCurrentDonateAll != value)
                {
                    if (SelectedTroopForDonate is TroopModel)
                    {
                        var troop = (TroopModel)SelectedTroopForDonate;
                        troop.IsDonateAll = value;
                    }

                    _isCurrentDonateAll = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _currentDonateKeywords;
        /// <summary>
        /// [For use in UI only] Gets or sets the selected troop donate keywords.
        /// </summary>
        /// <value>The selected troop donate keywords.</value>
        public string CurrentDonateKeywords
        {
            get { return _currentDonateKeywords; }
            set
            {
                if (_currentDonateKeywords != value)
                {
                    if (SelectedTroopForDonate is TroopModel)
                    {
                        var troop = (TroopModel)SelectedTroopForDonate;
                        troop.DonateKeywords = value.Replace(Environment.NewLine, @"|");
                    }

                    _currentDonateKeywords = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _shouldHideDonateControls = Visibility.Collapsed;
        /// <summary>
        /// [For use in UI only] Gets or sets if should hide donate controls.
        /// </summary>
        /// <value>If should hide donate controls.</value>
        public Visibility ShouldHideDonateControls
        {
            get { return _shouldHideDonateControls; }
            set
            {
                if (_shouldHideDonateControls != value)
                {
                    _shouldHideDonateControls = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _shouldHideTierInfoMessage = Visibility.Visible;
        /// <summary>
        /// [For use in UI only] Gets or sets if should hide tier information message.
        /// </summary>
        /// <value>If should hide tier information message.</value>
        public Visibility ShouldHideTierInfoMessage
        {
            get { return _shouldHideTierInfoMessage; }
            set
            {
                if (_shouldHideTierInfoMessage != value)
                {
                    _shouldHideTierInfoMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _troopTierSelectedInfoMessage;
        /// <summary>
        /// [For use in UI only] Gets or sets the troop tier selected information message.
        /// </summary>
        /// <value>The troop tier selected information message.</value>
        public string TroopTierSelectedInfoMessage
        {
            get { return _troopTierSelectedInfoMessage; }
            set
            {
                if (_troopTierSelectedInfoMessage != value)
                {
                    _troopTierSelectedInfoMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Troop Settings Properties

        private int _barbariansPercent;
        /// <summary>
        /// Gets or sets the Barbarians percent in the Troop Settings Tab.
        /// </summary>
        /// <value>The Barbarians percent.</value>
        public int BarbariansPercent
        {
            get { return _barbariansPercent; }
            set
            {
                if (_barbariansPercent != value)
                {
                    _barbariansPercent = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalPercent");
                }
            }
        }

        private int _archersPercent;
        /// <summary>
        /// Gets or sets the Archers percent in the Troop Settings Tab.
        /// </summary>
        /// <value>The Archers percent.</value>
        public int ArchersPercent
        {
            get { return _archersPercent; }
            set
            {
                if (_archersPercent != value)
                {
                    _archersPercent = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalPercent");
                }
            }
        }

        private int _goblinsPercent;
        /// <summary>
        /// Gets or sets the Goblins percent in the Troop Settings Tab.
        /// </summary>
        /// <value>The Goblins percent.</value>
        public int GoblinsPercent
        {
            get { return _goblinsPercent; }
            set
            {
                if (_goblinsPercent != value)
                {
                    _goblinsPercent = value;
                    OnPropertyChanged();
                    OnPropertyChanged("TotalPercent");
                }
            }
        }

        /// <summary>
        /// [For use in UI only] Gets the total percent.
        /// </summary>
        /// <value>The total percent.</value>
        public int TotalPercent
        {
            get { return BarbariansPercent + ArchersPercent + GoblinsPercent; }
        }

        /// <summary>
        /// Gets the Troop Compositions.
        /// </summary>
        /// <value>The Troop Compositions.</value>
        public static BindingList<Model> TroopCompositions { get { return DataCollection.TroopCompositions; } }

        private Model _selectedTroopComposition;
        /// <summary>
        /// Gets or sets the selected Troop Composition.
        /// </summary>
        /// <value>The selected Troop Composition.</value>
        public Model SelectedTroopComposition
        {
            get { return _selectedTroopComposition; }
            set
            {
                if (_selectedTroopComposition != value)
                {
                    _selectedTroopComposition = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsUseBarracksEnabled");
                    OnPropertyChanged("IsCustomTroopsEnabled");
                }
            }
        }

        private int _numberOfGiants;
        /// <summary>
        /// Gets or sets the number of Giants.
        /// </summary>
        /// <value>The number of Giants.</value>
        public int NumberOfGiants
        {
            get { return _numberOfGiants; }
            set
            {
                if (_numberOfGiants != value)
                {
                    _numberOfGiants = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _numberOfWallBreakers;
        /// <summary>
        /// Gets or sets the number of Wall Breakers.
        /// </summary>
        /// <value>The number of Wall Breakers.</value>
        public int NumberOfWallBreakers
        {
            get { return _numberOfWallBreakers; }
            set
            {
                if (_numberOfWallBreakers != value)
                {
                    _numberOfWallBreakers = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets the Troops.
        /// </summary>
        /// <value>The Troops.</value>
        public static BindingList<Model> Troops { get { return DataCollection.BarracksTroops; } }

        private Model _selectedBarrack1;
        public Model SelectedBarrack1
        {
            get { return _selectedBarrack1; }
            set
            {
                if (_selectedBarrack1 != value)
                {
                    _selectedBarrack1 = value;
                    OnPropertyChanged();
                }
            }
        }

        private Model _selectedBarrack2;
        public Model SelectedBarrack2
        {
            get { return _selectedBarrack2; }
            set
            {
                if (_selectedBarrack2 != value)
                {
                    _selectedBarrack2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private Model _selectedBarrack3;
        public Model SelectedBarrack3
        {
            get { return _selectedBarrack3; }
            set
            {
                if (_selectedBarrack3 != value)
                {
                    _selectedBarrack3 = value;
                    OnPropertyChanged();
                }
            }
        }

        private Model _selectedBarrack4;
        public Model SelectedBarrack4
        {
            get { return _selectedBarrack4; }
            set
            {
                if (_selectedBarrack4 != value)
                {
                    _selectedBarrack4 = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// [For use in UI only] Gets a value indicating whether the use of barracks is enabled.
        /// </summary>
        /// <value><c>true</c> if the use of barracks is enabled; otherwise, <c>false</c>.</value>
        public bool IsUseBarracksEnabled
        {
            get
            {
                if (SelectedTroopComposition.Id == (int)TroopComposition.UseBarracks)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// [For use in UI only] Gets a value indicating whether the use of custom troops is enabled.
        /// </summary>
        /// <value><c>true</c> if the use of custom troops enabled; otherwise, <c>false</c>.</value>
        public bool IsCustomTroopsEnabled
        {
            get
            {
                if (SelectedTroopComposition.Id == (int)TroopComposition.CustomTroops)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #endregion

        #region Commands

        private AboutCommand _aboutCommand = new AboutCommand();
        public ICommand AboutCommand
        {
            get { return _aboutCommand; }
        }

        private DelegateCommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new DelegateCommand(() => Exit());
                return _exitCommand;
            }
        }

        #region General Settings Commands

        private DelegateCommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                    _startCommand = new DelegateCommand(() => Start(), StartCanExecute);
                return _startCommand;
            }
        }

        private DelegateCommand _stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                    _stopCommand = new DelegateCommand(() => Stop(), StopCanExecute);
                return _stopCommand;
            }
        }

        private DelegateCommand _hideCommand;
        public ICommand HideCommand
        {
            get
            {
                if (_hideCommand == null)
                    _hideCommand = new DelegateCommand(() => Hide(), HideCanExecute);
                return _hideCommand;
            }
        }

        #endregion

        #region Search Settings Commands

        private DelegateCommand _searchModeCommand;
        public ICommand SearchModeCommand
        {
            get
            {
                if (_searchModeCommand == null)
                    _searchModeCommand = new DelegateCommand(() => SearchMode(), SearchModeCanExecute);
                return _searchModeCommand;
            }
        }

        #endregion

        #region Donate Settings Commands

        private DelegateCommand _locateClanCastleCommand;
        public ICommand LocateClanCastleCommand
        {
            get
            {
                if (_locateClanCastleCommand == null)
                    _locateClanCastleCommand = new DelegateCommand(() => LocateClanCastle(), LocateClanCastleCanExecute);
                return _locateClanCastleCommand;
            }
        }

        #endregion

        #region Troop Settings Commands

        private DelegateCommand _locateCollectorsCommand;
        public ICommand LocateCollectorsCommand
        {
            get
            {
                if (_locateCollectorsCommand == null)
                    _locateCollectorsCommand = new DelegateCommand(() => LocateCollectors(), LocateCollectorsCanExecute);
                return _locateCollectorsCommand;
            }
        }

        private DelegateCommand _locateBarracksCommand;
        public ICommand LocateBarracksCommand
        {
            get
            {
                if (_locateBarracksCommand == null)
                    _locateBarracksCommand = new DelegateCommand(() => LocateBarracks(), LocateBarracksCanExecute);
                return _locateBarracksCommand;
            }
        }

        #endregion

        #endregion

        #region Can Execute Methods

        /// <summary>
        /// Determines whether the Start command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool StartCanExecute()
        {
            return IsStarted == false ? true : false;
        }

        /// <summary>
        /// Determines whether the Stop command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool StopCanExecute()
        {
            return IsStarted;
        }

        /// <summary>
        /// Determines whether the HideCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool HideCanExecute()
        {
            return IsHidden;
        }

        /// <summary>
        /// Determines whether the SearchModeCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool SearchModeCanExecute()
        {
            return true; // TODO: We need to define this
        }

        /// <summary>
        /// Determines whether the LocateClanCastleCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool LocateClanCastleCanExecute()
        {
            return true; // TODO: We need to define this
        }

        /// <summary>
        /// Determines whether the LocateCollectorsCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool LocateCollectorsCanExecute()
        {
            return true; // TODO: We need to define this
        }

        /// <summary>
        /// Determines whether the LocateBarracksCommand command can be executed.
        /// </summary>
        /// <returns><c>true</c> if can execute, <c>false</c> otherwise</returns>
        private bool LocateBarracksCanExecute()
        {
            return true; // TODO: We need to define this
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes everything that is needed.
        /// </summary>
        private void Init()
        {
            // Fill the Troop Compositions
            if (DataCollection.TroopCompositions.Count == 0)
            {
                foreach (var tc in Enum.GetValues(typeof(TroopComposition)))
                {
                    DataCollection.TroopCompositions.Add(Model.CreateNew((int)tc, ((TroopComposition)tc).Name()));
                }
            }

            // Fill the Barracks Troops
            // TODO: Make this better!
            if (DataCollection.BarracksTroops.Count == 0)
            {
                DataCollection.BarracksTroops.Add(Model.CreateNew(1, Properties.Resources.Barbarians));
                DataCollection.BarracksTroops.Add(Model.CreateNew(2, Properties.Resources.Archers));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(3, Properties.Resources.Goblins));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(4, Properties.Resources.Giants));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(5, Properties.Resources.WallBreakers));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(6, Properties.Resources.Balloons));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(7, Properties.Resources.Wizards));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(8, Properties.Resources.Healers));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(9, Properties.Resources.Dragons));
                //DataCollection.BarracksTroops.Add(Model.CreateNew(10, Properties.Resources.Pekkas));
                // add more troop types
            }

            // Fill the Deploy Strategies
            if (DataCollection.DeployStrategies.Count == 0)
            {
                foreach (var ds in Enum.GetValues(typeof(DeployStrategy)))
                {
                    DataCollection.DeployStrategies.Add(Model.CreateNew((int)ds, ((DeployStrategy)ds).Name()));
                }
            }

            // Fill the Deploy Troops
            if (DataCollection.DeployTroops.Count == 0)
            {
                foreach (var dt in Enum.GetValues(typeof(DeployTroop)))
                {
                    DataCollection.DeployTroops.Add(Model.CreateNew((int)dt, ((DeployTroop)dt).Name()));
                }
            }

            // Fill the Troop Tiers
            if (DataCollection.TroopTiers.Count == 0)
            {
                foreach (var tier in Enum.GetValues(typeof(TroopType)))
                {
                    switch ((TroopType)tier)
                    {
                        case TroopType.Tier1:
                            DataCollection.TroopTiers.Add(TroopTierModel.CreateNew((int)tier, TroopType.Tier1.Name()));

                            var t1 = DataCollection.TroopTiers.First(tt => tt.Id == (int)tier);
                            t1.Troops.Add(TroopModel.CreateNew((int)Troop.Barbarian, Troop.Barbarian.Name(), AppSettings.DonateBarbarians, AppSettings.DonateAllBarbarians, AppSettings.DonateKeywordsBarbarians));
                            t1.Troops.Add(TroopModel.CreateNew((int)Troop.Archer, Troop.Archer.Name(), AppSettings.DonateArchers, AppSettings.DonateAllArchers, AppSettings.DonateKeywordsArchers));
                            t1.Troops.Add(TroopModel.CreateNew((int)Troop.Goblin, Troop.Goblin.Name(), AppSettings.DonateGoblins, AppSettings.DonateAllGoblins, AppSettings.DonateKeywordsGoblins));
                            break;
                        case TroopType.Tier2:
                            DataCollection.TroopTiers.Add(TroopTierModel.CreateNew((int)tier, TroopType.Tier2.Name()));

                            var t2 = DataCollection.TroopTiers.First(tt => tt.Id == (int)tier);
                            t2.Troops.Add(TroopModel.CreateNew((int)Troop.Giant, Troop.Giant.Name(), AppSettings.DonateGiants, AppSettings.DonateAllGiants, AppSettings.DonateKeywordsGiants));
                            t2.Troops.Add(TroopModel.CreateNew((int)Troop.WallBreaker, Troop.WallBreaker.Name(), AppSettings.DonateWallBreakers, AppSettings.DonateAllWallBreakers, AppSettings.DonateKeywordsWallBreakers));
                            t2.Troops.Add(TroopModel.CreateNew((int)Troop.Balloon, Troop.Balloon.Name(), AppSettings.DonateBalloons, AppSettings.DonateAllBalloons, AppSettings.DonateKeywordsBalloons));
                            t2.Troops.Add(TroopModel.CreateNew((int)Troop.Wizard, Troop.Wizard.Name(), AppSettings.DonateWizards, AppSettings.DonateAllWizards, AppSettings.DonateKeywordsWizards));
                            break;
                        case TroopType.Tier3:
                            DataCollection.TroopTiers.Add(TroopTierModel.CreateNew((int)tier, TroopType.Tier3.Name()));

                            var t3 = DataCollection.TroopTiers.First(tt => tt.Id == (int)tier);
                            t3.Troops.Add(TroopModel.CreateNew((int)Troop.Healer, Troop.Healer.Name(), AppSettings.DonateHealers, AppSettings.DonateAllHealers, AppSettings.DonateKeywordsHealers));
                            t3.Troops.Add(TroopModel.CreateNew((int)Troop.Dragon, Troop.Dragon.Name(), AppSettings.DonateDragons, AppSettings.DonateAllDragons, AppSettings.DonateKeywordsDragons));
                            t3.Troops.Add(TroopModel.CreateNew((int)Troop.Pekka, Troop.Pekka.Name(), AppSettings.DonatePekkas, AppSettings.DonateAllPekkas, AppSettings.DonateKeywordsPekkas));
                            break;
                        case TroopType.DarkTroops:
                            DataCollection.TroopTiers.Add(TroopTierModel.CreateNew((int)tier, TroopType.DarkTroops.Name()));

                            var dt = DataCollection.TroopTiers.First(tt => tt.Id == (int)tier);
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.Minion, Troop.Minion.Name(), AppSettings.DonateMinions, AppSettings.DonateAllMinions, AppSettings.DonateKeywordsMinions));
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.HogRider, Troop.HogRider.Name(), AppSettings.DonateHogRiders, AppSettings.DonateAllHogRiders, AppSettings.DonateKeywordsHogRiders));
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.Valkyrie, Troop.Valkyrie.Name(), AppSettings.DonateValkyries, AppSettings.DonateAllValkyries, AppSettings.DonateKeywordsValkyries));
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.Golem, Troop.Golem.Name(), AppSettings.DonateGolems, AppSettings.DonateAllGolems, AppSettings.DonateKeywordsGolems));
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.Witch, Troop.Witch.Name(), AppSettings.DonateWitches, AppSettings.DonateAllWitches, AppSettings.DonateKeywordsWitches));
                            dt.Troops.Add(TroopModel.CreateNew((int)Troop.LavaHound, Troop.LavaHound.Name(), AppSettings.DonateLavaHounds, AppSettings.DonateAllLavaHounds, AppSettings.DonateKeywordsLavaHounds));
                            break;
                        default:
                            // Troop Type Heroes, do nothing!
                            break;
                    }
                }
            }
        }

        #endregion

        #region Main Methods

        /// <summary>
        /// Starts the bot functionality
        /// </summary>
        private void Start()
        {
            ClearOutput(); // Clear everything before we start

            // Sample for getting familiar with the UI (used for accessing the properties/user settings values)
            Samples.GettingAroundTheUI.UseValuesInUI(this);

            Output = "Trying some simple captures within FastFind, and Keyboard injection";
            MessageBox.Show("Trying some simple captures within FastFind, and Keyboard injection", "Start", MessageBoxButton.OK, MessageBoxImage.Information);
            FastFindTesting.Test();
            KeyboardHelper.BSTest();
            KeyboardHelper.BSTest2();
            KeyboardHelper.NotePadTest();          
        }

        /// <summary>
        /// Stops the bot functionality
        /// </summary>
        private void Stop()
        {
            Output = "Stop bot execution...";
            MessageBox.Show("You clicked on the Stop button!", "Stop", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Hide the bot
        /// </summary>
        private void Hide()
        {
            Output = "Hidding...";
            MessageBox.Show("You clicked on the Hide button!", "Hide", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Starts the Search Mode.
        /// </summary>
        private void SearchMode()
        {
            Output = "Search Mode...";
            MessageBox.Show("You clicked on the Search Mode button!", "Search Mode", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Manually locates the Clan Castle.
        /// </summary>
        private void LocateClanCastle()
        {
            Output = "Locating Clan Castle...";
            MessageBox.Show("You clicked on the Locate Clan Castle Manually button!", "Locate Clan Castle Manually", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Manually locates the Collectors and Mines.
        /// </summary>
        private void LocateCollectors()
        {
            Output = "Locate Collectors and Gold Mines...";
            MessageBox.Show("You clicked on the Locate Collectors Manually button!", "Locate Collectors Manually", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Manually locates the Barracks.
        /// </summary>
        private void LocateBarracks()
        {
            Output = "Locate Barracks...";
            MessageBox.Show("You clicked on the Locate Barracks Manually button!", "Locate Barracks Manually", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private void Exit()
        {
            SaveUserSettings();
            Application.Current.MainWindow.Close();
        }

        #endregion

        #region Output Methods

        /// <summary>
        /// Clear all messages from the output.
        /// </summary>
        public static void ClearOutput()
        {
            _output.Clear();
        }

        #endregion

        #region Application User Settings Methods

        /// <summary>
        /// Gets the application user settings.
        /// </summary>
        private void GetUserSettings()
        {
            // General
            MaxTrophies = AppSettings.MaxTrophies;

            // Search Settings
            MeetGold = AppSettings.MeetGold;
            MeetElixir = AppSettings.MeetElixir;
            MeetDarkElixir = AppSettings.MeetDarkElixir;
            MeetTrophyCount = AppSettings.MeetTrophyCount;
            MeetTownhallLevel = AppSettings.MeetTownhallLevel;

            MinimumGold = AppSettings.MinGold;
            MinimumElixir = AppSettings.MinElixir;
            MinimumDarkElixir = AppSettings.MinDarkElixir;
            MinimumTrophyCount = AppSettings.MinTrophyCount;
            MinimumTownhallLevel = AppSettings.MinTownhallLevel;

            AlertWhenBaseFound = AppSettings.AlertWhenBaseFound;

            // Attack Settings
            SelectedMaxCannonLevel = AppSettings.MaxCannonLevel;
            SelectedMaxArcherTowerLevel = AppSettings.MaxArcherTowerLevel;
            SelectedMaxMortarLevel = AppSettings.MaxMortarLevel;
            SelectedWizardTowerLevel = AppSettings.MaxWizardTowerLevel;
            SelectedXbowLevel = AppSettings.MaxXbowLevel;

            AttackTheirKing = AppSettings.AttackTheirKing;
            AttackTheirQueen = AppSettings.AttackTheirQueen;

            SelectedAttackMode = (AttackMode)AppSettings.SelectedAttackMode;

            SelectedKingAttackMode = (HeroAttackMode)AppSettings.SelectedKingAttackMode;
            SelectedQueenAttackMode = (HeroAttackMode)AppSettings.SelectedQueenAttackMode;
            AttackUsingClanCastle = AppSettings.AttackUsingClanCastle;

            SelectedDeployStrategy = DataCollection.DeployStrategies.Where(ds => ds.Id == AppSettings.SelectedDeployStrategy).DefaultIfEmpty(DataCollection.DeployStrategies.Last()).First();
            SelectedDeployTroop = DataCollection.DeployTroops.Where(dt => dt.Id == AppSettings.SelectedDeployTroop).DefaultIfEmpty(DataCollection.DeployTroops.First()).First();
            AttackTownhall = AppSettings.AttackTownhall;

            // Donate Settings
            RequestTroops = AppSettings.RequestTroops;
            RequestTroopsMessage = AppSettings.RequestTroopsMessage;

            // Troop Settings
            BarbariansPercent = AppSettings.BarbariansPercent;
            ArchersPercent = AppSettings.ArchersPercent;
            GoblinsPercent = AppSettings.GoblinsPercent;

            SelectedTroopComposition = DataCollection.TroopCompositions.Where(tc => tc.Id == AppSettings.SelectedTroopComposition).DefaultIfEmpty(DataCollection.TroopCompositions.First()).First();

            NumberOfGiants = AppSettings.NumberOfGiants;
            NumberOfWallBreakers = AppSettings.NumberOfWallBreakers;

            SelectedBarrack1 = DataCollection.BarracksTroops.Where(b1 => b1.Id == AppSettings.SelectedBarrack1).DefaultIfEmpty(DataCollection.BarracksTroops.First()).First();
            SelectedBarrack2 = DataCollection.BarracksTroops.Where(b2 => b2.Id == AppSettings.SelectedBarrack2).DefaultIfEmpty(DataCollection.BarracksTroops.First()).First();
            SelectedBarrack3 = DataCollection.BarracksTroops.Where(b3 => b3.Id == AppSettings.SelectedBarrack3).DefaultIfEmpty(DataCollection.BarracksTroops.First()).First();
            SelectedBarrack4 = DataCollection.BarracksTroops.Where(b4 => b4.Id == AppSettings.SelectedBarrack4).DefaultIfEmpty(DataCollection.BarracksTroops.First()).First();
        }

        /// <summary>
        /// Saves the application user settings.
        /// </summary>
        private void SaveUserSettings()
        {
            // General
            AppSettings.MaxTrophies = MaxTrophies;

            // Search Settings
            AppSettings.MeetGold = MeetGold;
            AppSettings.MeetElixir = MeetElixir;
            AppSettings.MeetDarkElixir = MeetDarkElixir;
            AppSettings.MeetTrophyCount = MeetTrophyCount;
            AppSettings.MeetTownhallLevel = MeetTownhallLevel;

            AppSettings.MinGold = MinimumGold;
            AppSettings.MinElixir = MinimumElixir;
            AppSettings.MinDarkElixir = MinimumDarkElixir;
            AppSettings.MinTrophyCount = MinimumTrophyCount;
            AppSettings.MinTownhallLevel = MinimumTownhallLevel;

            AppSettings.AlertWhenBaseFound = AlertWhenBaseFound;

            // Attack Settings
            AppSettings.MaxCannonLevel = SelectedMaxCannonLevel;
            AppSettings.MaxArcherTowerLevel = SelectedMaxArcherTowerLevel;
            AppSettings.MaxMortarLevel = SelectedMaxMortarLevel;
            AppSettings.MaxWizardTowerLevel = SelectedWizardTowerLevel;
            AppSettings.MaxXbowLevel = SelectedXbowLevel;

            AppSettings.AttackTheirKing = AttackTheirKing;
            AppSettings.AttackTheirQueen = AttackTheirQueen;

            AppSettings.SelectedAttackMode = (int)SelectedAttackMode;

            AppSettings.SelectedKingAttackMode = (int)SelectedKingAttackMode;
            AppSettings.SelectedQueenAttackMode = (int)SelectedQueenAttackMode;
            AppSettings.AttackUsingClanCastle = AttackUsingClanCastle;

            AppSettings.SelectedDeployStrategy = SelectedDeployStrategy.Id;
            AppSettings.SelectedDeployTroop = SelectedDeployTroop.Id;
            AppSettings.AttackTownhall = AttackTownhall;

            // Donate Settings
            AppSettings.RequestTroops = RequestTroops;
            AppSettings.RequestTroopsMessage = RequestTroopsMessage;

            foreach (var tier in Enum.GetValues(typeof(TroopType)))
            {
                switch ((TroopType)tier)
                {
                    case TroopType.Tier1:
                        AppSettings.DonateAllBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Barbarian].IsDonateAll;
                        AppSettings.DonateAllArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Archer].IsDonateAll;
                        AppSettings.DonateAllGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Goblin].IsDonateAll;

                        AppSettings.DonateKeywordsBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Barbarian].DonateKeywords;
                        AppSettings.DonateKeywordsArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Archer].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Goblin].DonateKeywords;
                        
                        break;
                    case TroopType.Tier2:
                        var t2 = DataCollection.TroopTiers[(int)tier];

                        AppSettings.DonateAllGiants = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Giant].IsDonateAll;
                        AppSettings.DonateAllWallBreakers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.WallBreaker].IsDonateAll;
                        AppSettings.DonateAllBalloons = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Balloon].IsDonateAll;
                        AppSettings.DonateAllWizards = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Wizard].IsDonateAll;

                        AppSettings.DonateKeywordsBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Giant].DonateKeywords;
                        AppSettings.DonateKeywordsArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.WallBreaker].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Balloon].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Wizard].DonateKeywords;
                        break;
                    case TroopType.Tier3:
                        AppSettings.DonateAllHealers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Healer].IsDonateAll;
                        AppSettings.DonateAllDragons = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Dragon].IsDonateAll;
                        AppSettings.DonateAllPekkas = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Pekka].IsDonateAll;

                        AppSettings.DonateKeywordsBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Healer].DonateKeywords;
                        AppSettings.DonateKeywordsArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Dragon].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Pekka].DonateKeywords;
                        break;
                    case TroopType.DarkTroops:
                        AppSettings.DonateAllMinions = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Minion].IsDonateAll;
                        AppSettings.DonateAllHogRiders = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.HogRider].IsDonateAll;
                        AppSettings.DonateAllValkyries = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Valkyrie].IsDonateAll;
                        AppSettings.DonateAllGolems = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Golem].IsDonateAll;
                        AppSettings.DonateAllWitches = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Witch].IsDonateAll;
                        AppSettings.DonateAllLavaHounds = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.LavaHound].IsDonateAll;

                        AppSettings.DonateKeywordsBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Minion].DonateKeywords;
                        AppSettings.DonateKeywordsArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.HogRider].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Valkyrie].DonateKeywords;
                        AppSettings.DonateKeywordsBarbarians = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Golem].DonateKeywords;
                        AppSettings.DonateKeywordsArchers = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.Witch].DonateKeywords;
                        AppSettings.DonateKeywordsGoblins = DataCollection.TroopTiers[(int)tier].Troops[(int)Troop.LavaHound].DonateKeywords;
                        break;
                    default:
                        // Troop Type Heroes, do nothing!
                        break;
                }
            }

            // Troop Settings
            AppSettings.BarbariansPercent = BarbariansPercent;
            AppSettings.ArchersPercent = ArchersPercent;
            AppSettings.GoblinsPercent = GoblinsPercent;

            AppSettings.SelectedTroopComposition = SelectedTroopComposition.Id;

            AppSettings.NumberOfGiants = NumberOfGiants;
            AppSettings.NumberOfWallBreakers = NumberOfWallBreakers;

            AppSettings.SelectedBarrack1 = SelectedBarrack1.Id;
            AppSettings.SelectedBarrack2 = SelectedBarrack2.Id;
            AppSettings.SelectedBarrack3 = SelectedBarrack3.Id;
            AppSettings.SelectedBarrack4 = SelectedBarrack4.Id;

            // Save it!
            AppSettings.Save();
        }

        #endregion
    }
}
