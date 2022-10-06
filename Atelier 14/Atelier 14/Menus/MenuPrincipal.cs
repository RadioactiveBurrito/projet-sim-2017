using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AtelierXNA
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MenuPrincipal : Microsoft.Xna.Framework.DrawableGameComponent
    {
        SpriteBatch GestionSprites { get; set; }

        RessourcesManager<SpriteFont> GestionnaireDeFonts { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }

        InputManager GestionInputClavier { get; set; }
        InputControllerManager GestionInputManette { get; set; }

        Texture2D D�marrer { get; set; }
        Texture2D Quitter { get; set; }
        Texture2D D�marrerS�lectionner { get; set; }
        Texture2D QuitterS�lectionner { get; set; }
        Arri�rePlanD�roulant Fond�cran { get; set; }

        bool D�marrerVisible { get; set; }
        bool D�marrerS�lectionnerVisible { get; set; }
        bool QuitterS�lectionnerVisible { get; set; }
        bool QuitterVisible { get; set; }
        public bool PasserMenuSuivant { get; set; }

        public MenuPrincipal(Game jeu)
            :base(jeu){ }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            Vector2 centre = new Vector2(Game.Window.ClientBounds.Width / 3, Game.Window.ClientBounds.Height / 4);

            Fond�cran = new Arri�rePlanD�roulant(this.Game, "Fond4", Atelier.INTERVALLE_MAJ_STANDARD);
            Fond�cran.Initialize();
            D�marrerVisible = false;
            D�marrerS�lectionnerVisible = true;
            QuitterS�lectionnerVisible = false;
            QuitterVisible = true;
            PasserMenuSuivant = false;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected  override void LoadContent()
        {
            GestionSprites = Game.Services.GetService(typeof(SpriteBatch)) as SpriteBatch;
            GestionInputClavier = Game.Services.GetService(typeof(InputManager)) as InputManager;
            GestionInputManette = Game.Services.GetService(typeof(InputControllerManager)) as InputControllerManager;
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            D�marrer = GestionnaireDeTextures.Find("D�marerPartie2");
            Quitter = GestionnaireDeTextures.Find("QuitterPartie2");
            D�marrerS�lectionner = GestionnaireDeTextures.Find("D�marerPartie2S�lectionner");
            QuitterS�lectionner = GestionnaireDeTextures.Find("QuitterPartie2S�lectionner");
        }

       


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Game.Exit();
            Fond�cran.Update(gameTime);
            G�rerSelectionOption();
            G�rerOptionS�lectionnner();
            base.Update(gameTime);
        }

        void G�rerSelectionOption()
        {

            if (GestionInputClavier.EstClavierActiv� || GestionInputManette.EstManetteActiv�e(PlayerIndex.One))
            {
                if (GestionInputClavier.EstNouvelleTouche(Keys.Down) || GestionInputManette.EstNouvelleTouche(PlayerIndex.One, Buttons.LeftThumbstickDown))
                {
                    D�marrerS�lectionnerVisible = false;
                    D�marrerVisible = true;
                    QuitterS�lectionnerVisible = true;
                    QuitterVisible = false;
                }

                if (GestionInputClavier.EstNouvelleTouche(Keys.Up) || GestionInputManette.EstNouvelleTouche(PlayerIndex.One, Buttons.LeftThumbstickUp))
                {
                    D�marrerS�lectionnerVisible = true;
                    D�marrerVisible = false;
                    QuitterS�lectionnerVisible = false;
                    QuitterVisible = true;
                }
            
            }
        }

        void G�rerOptionS�lectionnner()
        {
            if (GestionInputClavier.EstClavierActiv� || GestionInputManette.EstManetteActiv�e(PlayerIndex.One))
            {
                if (GestionInputClavier.EstNouvelleTouche(Keys.Enter) || GestionInputManette.EstNouvelleTouche(PlayerIndex.One, Buttons.A))
                {
                    if (D�marrerS�lectionnerVisible)
                    {
                        PasserMenuSuivant = true;
                    }
                    if (QuitterS�lectionnerVisible)
                    {
                        Game.Exit();
                    }
                }
            }
        }

        

        void G�rerVisibilit�()
        {
            if (D�marrerVisible)
            {
                GestionSprites.Draw(D�marrer, new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 4), Color.White);
            }
            if (D�marrerS�lectionnerVisible)
            {
                GestionSprites.Draw(D�marrerS�lectionner, new Vector2(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 4), Color.White);
            }
            if (QuitterVisible)
            {
                GestionSprites.Draw(Quitter, new Vector2(Game.Window.ClientBounds.Width / 2, 3*Game.Window.ClientBounds.Height / 4), Color.White);
            }
            if (QuitterS�lectionnerVisible)
            {
                GestionSprites.Draw(QuitterS�lectionner, new Vector2(Game.Window.ClientBounds.Width / 2, 3*Game.Window.ClientBounds.Height / 4), Color.White);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Fond�cran.Draw(gameTime);
            GestionSprites.Begin();
            G�rerVisibilit�();
            GestionSprites.End();
            base.Draw(gameTime);
        }


    }
}
