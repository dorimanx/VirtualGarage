using Torch;

namespace VirtualGarage
{
    public class Config : ViewModel
    {
        private float _MinAllowedGravityToLoad = 0.4f;

        private double _EnemyPlayerInRange = 3000.0;

        private bool _ChangeBuiltBy;
        private bool _ChangeOwner;
        private bool _ConvertToStatic;
        private bool _ConvertToDynamic;

        private float _MaxSpawnRadius = 500;
        private float _MaxRangeToGrid = 1000;

        private int _OldGridDays = 7;
        private int _MaxPCUForGridOnSave = 300000;
        private int _MaxBlocksForGridOnSave = 30000;

        private string _PathToVirtualGarage = "Garage";
        private string _OnlyOwnerCanSaveResponce = "Only grid owner can save to garage.";
        private string _SavingGridResponce = "Saving grid to garage now";
        private string _GridBlocksOverLimitResponce = "Cant save grid to garage, too many blocks!";
        private string _GridPCUOverLimitResponce = "Cant save grid to garage, too much PCU!";
        private string _GridSavedToVirtualGarageResponce = "Saved to garage!";
        private string _EnemyNearByChatRespond = "Cant use garage! Enemy player near by!";
        private string _VirtualGarageNotAllowedInGravityMoreThanResponce = "Cant use garage in gravity more than";
        private string _GridSpawnedToWorldRespond = "Spawned grid to World";
        private string _NoRoomToSpawnRespond = "No place to spawn grid, find better location.";
        private string _NoGridsInVirtualGarageRespond = "No grids found in garage!";
        private string _GridsInVirtualGarageRespond = "List of your grid in garage.";
        private string _NoGridInViewResponce = "There is no grid in view";
        private string _GridToFarResponce = "Grid is too far from you!";

        
        [DisplayTab(Name = "Min allowed gravity to load", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Min allowed gravity to load")]
        public float MinAllowedGravityToLoad
        {
            get => _MinAllowedGravityToLoad;
            set => SetValue(ref _MinAllowedGravityToLoad, value);
        }

        [DisplayTab(Name = "Change BuiltBy", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Change BuiltBy")]
        public bool ChangeBuiltBy
        {
            get => _ChangeBuiltBy;
            set => SetValue(ref _ChangeBuiltBy, value);
        }

        [DisplayTab(Name = "Change Owner", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Change Owner")]
        public bool ChangeOwner
        {
            get => _ChangeOwner;
            set => SetValue(ref _ChangeOwner, value);
        }

        [DisplayTab(Name = "Convert to static", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Convert to static")]
        public bool ConvertToStatic
        {
            get => _ConvertToStatic;
            set => SetValue(ref _ConvertToStatic, value);
        }
        
        [DisplayTab(Name = "Convert to dynamic", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Convert to dynamic")]
        public bool ConvertToDynamic
        {
            get => _ConvertToDynamic;
            set => SetValue(ref _ConvertToDynamic, value);
        }

        [DisplayTab(Name = "Max spawn radius", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Max spawn radius")]
        public float MaxSpawnRadius
        {
            get => _MaxSpawnRadius;
            set => SetValue(ref _MaxSpawnRadius, value);
        }

        [DisplayTab(Name = "Max range to grid", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Max range to grid")]
        public float MaxRangeToGrid
        {
            get => _MaxRangeToGrid;
            set => SetValue(ref _MaxRangeToGrid, value);
        }

        [DisplayTab(Name = "Move grids to garage after player offline days", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Move grids to garage after player offline days")]
        public int OldGridDays
        {
            get => _OldGridDays;
            set => SetValue(ref _OldGridDays, value);
        }

        [DisplayTab(Name = "Max pcu for grid on save", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Max pcu for grid on save")]
        public int MaxPCUForGridOnSave
        {
            get => _MaxPCUForGridOnSave;
            set => SetValue(ref _MaxPCUForGridOnSave, value);
        }

        [DisplayTab(Name = "Max blocks for grid on save", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Max blocks for grid on save")]
        public int MaxBlocksForGridOnSave
        {
            get => _MaxBlocksForGridOnSave;
            set => SetValue(ref _MaxBlocksForGridOnSave, value);
        }

        [DisplayTab(Name = "Check enemy radius before save/load", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Check enemy radius before save/load")]
        public double EnemyPlayerInRange
        {
            get => _EnemyPlayerInRange;
            set => SetValue(ref _EnemyPlayerInRange, value);
        }

        [DisplayTab(Name = "Path to virtual garage", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Path to virtual garage")]
        public string PathToVirtualGarage
        {
            get => _PathToVirtualGarage;
            set => SetValue(ref _PathToVirtualGarage, value);
        }

        [DisplayTab(Name = "Only owner can save response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Only owner can save response")]
        public string OnlyOwnerCanSaveResponce
        {
            get => _OnlyOwnerCanSaveResponce;
            set => SetValue(ref _OnlyOwnerCanSaveResponce, value);
        }

        [DisplayTab(Name = "Saving grid response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Saving grid response")]
        public string SavingGridResponce
        {
            get => _SavingGridResponce;
            set => SetValue(ref _SavingGridResponce, value);
        }

        [DisplayTab(Name = "Grid blocks over limit response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Grid blocks over limit response")]
        public string GridBlocksOverLimitResponce
        {
            get => _GridBlocksOverLimitResponce;
            set => SetValue(ref _GridBlocksOverLimitResponce, value);
        }

        [DisplayTab(Name = "Grid PCU over limit response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Grid PCU over limit response")]
        public string GridPCUOverLimitResponce
        {
            get => _GridPCUOverLimitResponce;
            set => SetValue(ref _GridPCUOverLimitResponce, value);
        }

        [DisplayTab(Name = "Grid saved response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Grid saved response")]
        public string GridSavedToVirtualGarageResponce
        {
            get => _GridSavedToVirtualGarageResponce;
            set => SetValue(ref _GridSavedToVirtualGarageResponce, value);
        }

        [DisplayTab(Name = "Enemy near by chat respond", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Enemy near by chat respond")]
        public string EnemyNearByChatRespond
        {
            get => _EnemyNearByChatRespond;
            set => SetValue(ref _EnemyNearByChatRespond, value);
        }

        [DisplayTab(Name = "Not allowed in gravity respond", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Not allowed in gravity respond")]
        public string VirtualGarageNotAllowedInGravityMoreThanResponce
        {
            get => _VirtualGarageNotAllowedInGravityMoreThanResponce;
            set => SetValue(ref _VirtualGarageNotAllowedInGravityMoreThanResponce, value);
        }

        [DisplayTab(Name = "Grid spawned respond", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "Grid spawned respond")]
        public string GridSpawnedToWorldRespond
        {
            get => _GridSpawnedToWorldRespond;
            set => SetValue(ref _GridSpawnedToWorldRespond, value);
        }

        [DisplayTab(Name = "No free space to spawn respond", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "No free space to spawn respond")]
        public string NoRoomToSpawnRespond
        {
            get => _NoRoomToSpawnRespond;
            set => SetValue(ref _NoRoomToSpawnRespond, value);
        }

        [DisplayTab(Name = "No grid in garage respond", GroupName = "Load settings", Tab = "Load settings", Order = 0, Description = "No grid in garage respond")]
        public string NoGridsInVirtualGarageRespond
        {
            get => _NoGridsInVirtualGarageRespond;
            set => SetValue(ref _NoGridsInVirtualGarageRespond, value);
        }

        [DisplayTab(Name = "Grids in virtual garage respond", GroupName = "Main settings", Tab = "Main settings", Order = 0, Description = "Grids in virtual garage respond")]
        public string GridsInVirtualGarageRespond
        {
            get => _GridsInVirtualGarageRespond;
            set => SetValue(ref _GridsInVirtualGarageRespond, value);
        }

        [DisplayTab(Name = "No grid in view response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "No grid in view response")]
        public string NoGridInViewResponce
        {
            get => _NoGridInViewResponce;
            set => SetValue(ref _NoGridInViewResponce, value);
        }

        [DisplayTab(Name = "Grid to far response", GroupName = "Save settings", Tab = "Save settings", Order = 0, Description = "Grid to far response")]
        public string GridToFarResponce
        {
            get => _GridToFarResponce;
            set => SetValue(ref _GridToFarResponce, value);
        }
    }
}
