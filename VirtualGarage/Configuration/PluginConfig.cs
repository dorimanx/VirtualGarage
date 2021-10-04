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


        public bool ChangeBuiltBy
        {
            get => _ChangeBuiltBy;
            set => SetValue(ref _ChangeBuiltBy, value);
        }

        public bool ChangeOwner
        {
            get => _ChangeOwner;
            set => SetValue(ref _ChangeOwner, value);
        }

        public bool ConvertToStatic
        {
            get => _ConvertToStatic;
            set => SetValue(ref _ConvertToStatic, value);
        }

        public bool ConvertToDynamic
        {
            get => _ConvertToDynamic;
            set => SetValue(ref _ConvertToDynamic, value);
        }

        public float MaxSpawnRadius
        {
            get => _MaxSpawnRadius;
            set => SetValue(ref _MaxSpawnRadius, value);
        }

        public float MaxRangeToGrid
        {
            get => _MaxRangeToGrid;
            set => SetValue(ref _MaxRangeToGrid, value);
        }

        public int OldGridDays
        {
            get => _OldGridDays;
            set => SetValue(ref _OldGridDays, value);
        }

        public int MaxPCUForGridOnSave
        {
            get => _MaxPCUForGridOnSave;
            set => SetValue(ref _MaxPCUForGridOnSave, value);
        }

        public int MaxBlocksForGridOnSave
        {
            get => _MaxBlocksForGridOnSave;
            set => SetValue(ref _MaxBlocksForGridOnSave, value);
        }

        public float MinAllowedGravityToLoad
        {
            get => _MinAllowedGravityToLoad;
            set => SetValue(ref _MinAllowedGravityToLoad, value);
        }

        public double EnemyPlayerInRange
        {
            get => _EnemyPlayerInRange;
            set => SetValue(ref _EnemyPlayerInRange, value);
        }

        public string PathToVirtualGarage
        {
            get => _PathToVirtualGarage;
            set => SetValue(ref _PathToVirtualGarage, value);
        }

        public string OnlyOwnerCanSaveResponce
        {
            get => _OnlyOwnerCanSaveResponce;
            set => SetValue(ref _OnlyOwnerCanSaveResponce, value);
        }

        public string SavingGridResponce
        {
            get => _SavingGridResponce;
            set => SetValue(ref _SavingGridResponce, value);
        }

        public string GridBlocksOverLimitResponce
        {
            get => _GridBlocksOverLimitResponce;
            set => SetValue(ref _GridBlocksOverLimitResponce, value);
        }

        public string GridPCUOverLimitResponce
        {
            get => _GridPCUOverLimitResponce;
            set => SetValue(ref _GridPCUOverLimitResponce, value);
        }

        public string GridSavedToVirtualGarageResponce
        {
            get => _GridSavedToVirtualGarageResponce;
            set => SetValue(ref _GridSavedToVirtualGarageResponce, value);
        }

        public string EnemyNearByChatRespond
        {
            get => _EnemyNearByChatRespond;
            set => SetValue(ref _EnemyNearByChatRespond, value);
        }

        public string VirtualGarageNotAllowedInGravityMoreThanResponce
        {
            get => _VirtualGarageNotAllowedInGravityMoreThanResponce;
            set => SetValue(ref _VirtualGarageNotAllowedInGravityMoreThanResponce, value);
        }

        public string GridSpawnedToWorldRespond
        {
            get => _GridSpawnedToWorldRespond;
            set => SetValue(ref _GridSpawnedToWorldRespond, value);
        }

        public string NoRoomToSpawnRespond
        {
            get => _NoRoomToSpawnRespond;
            set => SetValue(ref _NoRoomToSpawnRespond, value);
        }

        public string NoGridsInVirtualGarageRespond
        {
            get => _NoGridsInVirtualGarageRespond;
            set => SetValue(ref _NoGridsInVirtualGarageRespond, value);
        }

        public string GridsInVirtualGarageRespond
        {
            get => _GridsInVirtualGarageRespond;
            set => SetValue(ref _GridsInVirtualGarageRespond, value);
        }

        public string NoGridInViewResponce
        {
            get => _NoGridInViewResponce;
            set => SetValue(ref _NoGridInViewResponce, value);
        }

        public string GridToFarResponce
        {
            get => _GridToFarResponce;
            set => SetValue(ref _GridToFarResponce, value);
        }
    }
}
