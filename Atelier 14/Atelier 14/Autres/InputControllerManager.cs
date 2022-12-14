using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace AtelierXNA
{
    public class InputControllerManager : Microsoft.Xna.Framework.GameComponent
    {
        int JoueurMax { get; set; }
        bool[] ManetteActive { get; set; }
        bool Déconnection { get; set; }
        Color CouleurFond { get; set; }
        GamePadState AncienÉtatManette { get; set; }

        GamePadState ÉtatManette { get; set; }

        public InputControllerManager(Game game)
            : base(game)
        {
        }
        public override void Initialize()
        {
            Déconnection = false;
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            AncienÉtatManette = ÉtatManette;
        }
        public bool EstManetteActivée(PlayerIndex numManette)
        {
            ÉtatManette = GamePad.GetState(numManette);
            return ÉtatManette.IsConnected;
        }

        public bool EstNouvelleTouche(PlayerIndex numManette, Buttons touche)
        {
            ÉtatManette = GamePad.GetState(numManette);
            return ÉtatManette.IsButtonDown(touche) && AncienÉtatManette.IsButtonUp(touche);
        }

        public bool EstToucheEnfoncée(PlayerIndex numManette, Buttons touche)
        {
            ÉtatManette = GamePad.GetState(numManette);
            return ÉtatManette.IsButtonDown(touche);
        }
        // float DeadZOne(PlayerIndex numManette)
        //{
        //    ÉtatManette = GamePad.GetState(numManette);
        //    return ÉtatManette.ThumbSticks;
        //}
    }
}
