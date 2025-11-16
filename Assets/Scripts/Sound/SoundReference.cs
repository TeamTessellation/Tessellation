namespace Sound
{
    public class SoundReference
    {
        // BGM
        // public static readonly SoundReference MainTitleBGM = new SoundReference("BGM/MainTitleBGM_V1");
        public static readonly SoundReference InGameBGM = new SoundReference("BGM/Ingame");
        
        // SFX
        public static readonly SoundReference GameOver = new SoundReference("SFX/GameOver");
        public static readonly SoundReference GoldGet = new SoundReference("SFX/GoldGet");
        public static readonly SoundReference MenuSetting = new SoundReference("SFX/MenuSetting");
        public static readonly SoundReference NumberIncrease = new SoundReference("SFX/NumberIncrease");
        public static readonly SoundReference RoundClear = new SoundReference("SFX/RoundClear");
        public static readonly SoundReference SceneTransitionIn = new SoundReference("SFX/SceneTransitionIn");
        public static readonly SoundReference SceneTransitionOut = new SoundReference("SFX/SceneTransitionOut");
        public static readonly SoundReference TileBomb = new SoundReference("SFX/TileBomb");
        public static readonly SoundReference TileClear = new SoundReference("SFX/TileClear");
        public static readonly SoundReference TileGold = new SoundReference("SFX/TileGold");
        public static readonly SoundReference TileMultiple = new SoundReference("SFX/TileMultiple");
        public static readonly SoundReference TileOptionTrigger = new SoundReference("SFX/TileOptionTrigger");
        public static readonly SoundReference TilePickup = new SoundReference("SFX/TilePickup");
        public static readonly SoundReference TileRelease = new SoundReference("SFX/TileRelease");
        public static readonly SoundReference TileRotateSelect = new SoundReference("SFX/TileRotateSelect");
        public static readonly SoundReference TileRotate = new SoundReference("SFX/TileRotate");
        public static readonly SoundReference UIClick = new SoundReference("SFX/UIClick");
        
        private readonly string path;

        public string Path => path;
        
        public SoundReference(string path)
        {
            this.path = path;
        }

        public static implicit operator string(SoundReference reference)
        {
            return reference.path;
        }
    }
}