namespace Sound
{
    public class SoundReference
    {
        // BGM
        // public static readonly SoundReference MainTitleBGM = new SoundReference("BGM/MainTitleBGM_V1");
        public static readonly SoundReference InGameBGM = new SoundReference("BGM/Ingame_V1");
        
        // SFX
        public static readonly SoundReference GameOver = new SoundReference("SFX/GameOver_V1");
        public static readonly SoundReference GoldGet = new SoundReference("SFX/GoldGet_V1");
        public static readonly SoundReference MenuSetting = new SoundReference("SFX/MenuSetting_V1");
        public static readonly SoundReference NumberIncrease = new SoundReference("SFX/NumberIncrease_V1");
        public static readonly SoundReference RoundClear = new SoundReference("SFX/RoundClear_V1");
        public static readonly SoundReference SceneTransitionIn = new SoundReference("SFX/SceneTransitionIn_V1");
        public static readonly SoundReference SceneTransitionOut = new SoundReference("SFX/SceneTransitionOut_V1");
        public static readonly SoundReference TileBomb = new SoundReference("SFX/TileBomb_V1");
        public static readonly SoundReference TileClear = new SoundReference("SFX/TileClear_V2");
        public static readonly SoundReference TileGold = new SoundReference("SFX/TileGold_V1");
        public static readonly SoundReference TileMultiple = new SoundReference("SFX/TileMultiple_V1");
        public static readonly SoundReference TileOptionTrigger = new SoundReference("SFX/TileOptionTrigger_V1");
        public static readonly SoundReference TilePickup = new SoundReference("SFX/TilePickup_V1");
        public static readonly SoundReference TileRelease = new SoundReference("SFX/TileRelease_V1");
        public static readonly SoundReference TileRotateSelect = new SoundReference("SFX/TileRotateSelect_V1");
        public static readonly SoundReference TileRotate = new SoundReference("SFX/TileRotate_V1");
        public static readonly SoundReference UIClick = new SoundReference("SFX/UIClick_V1");
        
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