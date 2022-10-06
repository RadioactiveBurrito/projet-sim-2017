
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using AtelierXNA.�l�ments_Tuile;
using AtelierXNA.AI;
using AtelierXNA.Autres;
using Microsoft.Xna.Framework.Audio;


namespace AtelierXNA
{
    public abstract class Personnage : Microsoft.Xna.Framework.DrawableGameComponent, IPause
    {
        
        #region Propri�t�s, constantes et initialisation.
        protected const float LARGEUR_HITBOX = 5f;
        protected const float HAUTEUR_HITBOX = 5f;
        protected const float PROFONDEUR_HITBOX = 5f;


        //Ces constantes seront � ajouter en propri�t�s.
        protected const float VITESSE_MAX_GAUCHE_DROITE = 50f;
        const float DUR�E_BOUCLIER = 1f;
        protected const float ACC�L�RATION_SOL = 500f;
        protected const float ACC�L�RATION_AIR = 500f;
        const int DOMMAGE_ATTAQUE = 12;
        const float FORCE_COUP = 500000f;


        protected Keys[] CONTR�LES { get; private set; }
        public enum ORIENTATION { DROITE, GAUCHE };
        protected enum �TAT { COURRIR, SAUTER, ATTAQUER, LANCER, BLOQUER, MORT, IMMOBILE };

        protected �TAT �TAT_PERSO;
        protected ORIENTATION DIRECTION;


        public string TypePersonnage { get; protected set; }
        public int NbVies { get; private set; }
        public int VieEnPourcentage { get; private set; }


        protected G�n�rateur g { get; set; }
        public BoundingSphere HitBox { get; private set; }
        //public BoundingBox HitBox { get; private set; }
        public Bouclier BouclierPersonnage { get; protected set; }
        protected float RayonDuBouclier { get; set; }

        protected float FrameEntreProjectile { get; private set; }
        protected float VitesseD�placementGaucheDroite { get; set; }
        protected float VitesseMaximaleSaut { get; private set; }
        public float Masse { get; private set; }
        public float ForceCoup { get; set; }
        public int DommageAttaque { get; private set; }
        public bool EstEnAttaque { get; protected set; }
        public bool EstBouclierActif { get; protected set; }
        bool ASaut�DuneSurface { get; set; }

        //Copies de certains �l�ments de l'environnement importants pour le personnage.
        Map Carte { get; set; }
        protected List<Vector3> IntervallesSurfaces { get; set; }
        protected List<Vector3> IntervallesPossibles { get; set; }
        public Vector3 IntervalleCourante { get; private set; }



        //Donn�es propres au personnages, qui seront variables.
        public Vector3 Position { get; protected set; }
        public Vector3 GetPositionPersonnage
        {
            get { return new Vector3(Position.X, Position.Y, Position.Z); }
        }
        protected Vector3 PositionSpawn { get; set; }
        public Vector3 AnciennePosition { get; protected set; }
        Vector3 VecteurGauche { get; set; }
        protected Vector3 VecteurVitesse { get; private set; }
        protected Vector3 AncienVecteurVitesse { get; private set; }
        protected Vector3 AncienVecteurVitesseGaucheDroite { get; set; }
        protected Vector3 VecteurVitesseGaucheDroite { get; set; }
        public abstract void D�placerFrame();
        protected int CptSaut { get; set; }
        public string tag { get; private set; }

        public PlayerIndex NumManette { get; private set; }
        protected float IntervalleMAJ { get; set; }
        protected float Temps�coul�DepuisMAJ { get; set; }


        protected InputControllerManager GestionInputManette { get; set; }
        protected InputManager GestionInputClavier { get; set; }
        protected RessourcesManager<SoundEffect> GestionnaireDeSon { get; set; }

        public bool d�c�d� { get; set; }
        int FramesCrier { get; set; }
        protected int FrameCourir { get; set; }

        public Personnage(Game game, float vitesseD�placementGaucheDroite, float vitesseMaximaleSaut, float masse, Vector3 position, float intervalleMAJ, Keys[] contr�les, PlayerIndex numManette)
            : base(game)
        {
            RayonDuBouclier = 6;
            //Propri�t�s pour le combat (� d�finir dans le constructeur)
            DommageAttaque = DOMMAGE_ATTAQUE;
            ForceCoup = FORCE_COUP;
            NumManette = numManette;


            CONTR�LES = contr�les;
            �TAT_PERSO = �TAT.IMMOBILE;
            DIRECTION = ORIENTATION.DROITE;

            IntervalleMAJ = intervalleMAJ;
            VitesseD�placementGaucheDroite = vitesseD�placementGaucheDroite;
            VitesseMaximaleSaut = vitesseMaximaleSaut;
            Masse = masse;
            Position = position;
            PositionSpawn = position;
            NbVies = 3;
            G�n�rerHitbox();
        }

        public override void Initialize()
        {
            FrameEntreProjectile = 0;
            g = Game.Services.GetService(typeof(G�n�rateur)) as G�n�rateur;
            g.ResetSeed();
            GestionnaireDeSon = Game.Services.GetService(typeof(RessourcesManager<SoundEffect>)) as RessourcesManager<SoundEffect>;
            DrawOrder = 4;
            GestionInputClavier = Game.Services.GetService(typeof(InputManager)) as InputManager;
            GestionInputManette = Game.Services.GetService(typeof(InputControllerManager)) as InputControllerManager;

            Carte = Game.Components.First(t => t is Map) as Map;
            IntervallesSurfaces = Carte.IntervallesSurfaces;

            VecteurGauche = Vector3.Normalize(Carte.VecteurGauche);
            VecteurVitesse = Vector3.Zero;

            d�c�d� = false;

            base.Initialize();
        }
        #endregion

        #region Boucle de jeu.
        public override void Update(GameTime gameTime)
        {

            Suicide();
            AncienVecteurVitesse = VecteurVitesse;
            float temps�coul� = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Temps�coul�DepuisMAJ += temps�coul�;
            if (EstMort())
            {
                --NbVies;

            }
            d�c�d� = NbVies == 0;

            VieEnPourcentage = EstMort() ? 0 : VieEnPourcentage;
            VecteurVitesse = EstMort() ? Vector3.Zero : VecteurVitesse;
            VecteurVitesseGaucheDroite = EstMort() ? Vector3.Zero : VecteurVitesseGaucheDroite;
            Position = EstMort() ? PositionSpawn : Position;
            RayonDuBouclier = EstMort() ? 6 : RayonDuBouclier;
            if (EstModePeur() && !d�c�d� && FramesCrier <= 0)
            {
                GestionnaireDeSon.Find("screaminggoat").Play();
                FramesCrier = 120;
            }
            if (Temps�coul�DepuisMAJ >= IntervalleMAJ)
            {
                FramesCrier--;
                FrameCourir--;
                RayonDuBouclier = MathHelper.Min(RayonDuBouclier + 0.02f, 6);
                FrameEntreProjectile--;
                if (VecteurVitesse.Y == 0 && VecteurVitesse.X == 0 && �TAT_PERSO != �TAT.IMMOBILE && !(this is Bot)) //Conditions ici pour g�rer l'immobilit�.
                {
                    �TAT_PERSO = �TAT.IMMOBILE;
                }
                else if (VecteurVitesse.Y != 0)
                {
                    �TAT_PERSO = �TAT.SAUTER;
                }


                AnciennePosition = new Vector3(Position.X, Position.Y, Position.Z);

                //Toutes les actions qui peuvent modifier le vecteur vitesse du personnage.
                G�rerTouchesEnfonc�es();
                G�rerAcc�l�rationGravitationnelle();
                G�rerFriction();
                AnciennePosition = new Vector3(Position.X, Position.Y, Position.Z);
                Position += (VecteurVitesse + VecteurVitesseGaucheDroite) * Temps�coul�DepuisMAJ;
                G�n�rerHitbox();

                Temps�coul�DepuisMAJ = 0;
            }
            G�rerNouvellesTouches();
        }

        void Suicide()
        {
            if (GestionInputClavier.EstNouvelleTouche(Keys.B))
            {
                d�c�d� = true;
            }
        }


        private void G�rerFriction()
        {
            float mu_air = 0.1f;
            float mu_sol = 0.2f;
            if (VecteurVitesse.X != 0)
            {
                Vector3 AncienVecteurVitesse = VecteurVitesse;
                if (VecteurVitesse.X < 0)
                    VecteurVitesse -= VecteurGauche * mu_air * Atelier.ACC�L�RATION_GRAVITATIONNELLE_PERSONNAGE;
                else if (VecteurVitesse.X > 0)
                    VecteurVitesse += VecteurGauche * mu_air * Atelier.ACC�L�RATION_GRAVITATIONNELLE_PERSONNAGE;
                if (Math.Sign(VecteurVitesse.X) != Math.Sign(AncienVecteurVitesse.X))
                    VecteurVitesse = new Vector3(0, VecteurVitesse.Y, VecteurVitesse.Z);
            }
            if (VecteurVitesseGaucheDroite.X != 0)
            {
                Vector3 AncienVecteurVitesseGaucheDroite = VecteurVitesseGaucheDroite;
                if (VecteurVitesseGaucheDroite.X < 0)
                    VecteurVitesseGaucheDroite -= VecteurGauche * mu_sol * Atelier.ACC�L�RATION_GRAVITATIONNELLE_PERSONNAGE;
                else if (VecteurVitesseGaucheDroite.X > 0)
                    VecteurVitesseGaucheDroite += VecteurGauche * mu_sol * Atelier.ACC�L�RATION_GRAVITATIONNELLE_PERSONNAGE;
                if (Math.Sign(VecteurVitesseGaucheDroite.X) != Math.Sign(AncienVecteurVitesseGaucheDroite.X))
                    VecteurVitesseGaucheDroite = new Vector3(0, VecteurVitesseGaucheDroite.Y, VecteurVitesseGaucheDroite.Z);
            }
        }
        private void G�n�rerHitbox()
        {
            HitBox = new BoundingSphere(new Vector3(Position.X, Position.Y + 5, Position.Z), 3);
        }
        protected void G�rerAcc�l�rationGravitationnelle()
        {
            AnciennePosition = new Vector3(Position.X, Position.Y, Position.Z);
            Position += Vector3.Up * VecteurVitesse.Y * Temps�coul�DepuisMAJ;

            int redneck = 0;
            IntervallesPossibles = IntervallesSurfaces.FindAll(t => t.Z <= AnciennePosition.Y && EstDansIntervalleSurface(t, AnciennePosition)) as List<Vector3>;
            foreach (Vector3 intervalle in IntervallesPossibles)
            {
                if (redneck == 0)
                    IntervalleCourante = intervalle;
                else if (IntervalleCourante.Z <= intervalle.Z)
                    IntervalleCourante = intervalle;
                redneck++;
            }

            if (EstDansIntervalleSurface(IntervalleCourante, Position) && (IntervalleCourante.Z >= Position.Y) && (IntervalleCourante.Z <= AnciennePosition.Y) && (EstDansIntervalleSurface(IntervalleCourante, AnciennePosition)))
            {
                Position = new Vector3(AnciennePosition.X, IntervalleCourante.Z, AnciennePosition.Z);
                VecteurVitesse = new Vector3(VecteurVitesse.X, 0, VecteurVitesse.Z);
                CptSaut = 0;
            }
            else
            {
                VecteurVitesse -= Vector3.Up * Atelier.ACC�L�RATION_GRAVITATIONNELLE_PERSONNAGE * Temps�coul�DepuisMAJ;
            }
            Position -= Vector3.Up * VecteurVitesse.Y * Temps�coul�DepuisMAJ;
        }
        #region M�thodes �venementielles.
        protected void G�rerTouchesEnfonc�es()
        {
            if ((GestionInputClavier.EstEnfonc�e(CONTR�LES[0]) || GestionInputManette.EstToucheEnfonc�e(NumManette, Buttons.LeftThumbstickRight)) && ((!EstEnAttaque || VecteurVitesse.Y != 0)) && !EstBouclierActif)
            {
                Droite();
            }
            if ((GestionInputClavier.EstEnfonc�e(CONTR�LES[1]) || GestionInputManette.EstToucheEnfonc�e(NumManette, Buttons.LeftThumbstickLeft)) && ((!EstEnAttaque || VecteurVitesse.Y != 0)) && !EstBouclierActif)
            {
                Gauche();
            }

            if ((GestionInputClavier.EstEnfonc�e(CONTR�LES[2]) || GestionInputManette.EstToucheEnfonc�e(NumManette, Buttons.RightTrigger)) && VecteurVitesse.Y == 0)
            {
                Bloquer();
            }
            else if(!(this is Bot))
            {
                if (EstBouclierActif)
                {
                    RayonDuBouclier = BouclierPersonnage.Rayon;
                    Game.Components.Remove(BouclierPersonnage);
                }
                EstBouclierActif = false;
            }
        }

        protected virtual void Droite()
        {
            DIRECTION = ORIENTATION.DROITE;
            if (VecteurVitesse.Y == 0)
            {
                �TAT_PERSO = �TAT.COURRIR;
                if (Math.Abs(VecteurVitesseGaucheDroite.X + ACC�L�RATION_SOL * Temps�coul�DepuisMAJ) <= VITESSE_MAX_GAUCHE_DROITE)
                {
                    VecteurVitesseGaucheDroite += new Vector3(ACC�L�RATION_SOL * Temps�coul�DepuisMAJ, 0, 0);
                }
            }
            else if (Math.Abs(VecteurVitesseGaucheDroite.X + ACC�L�RATION_AIR * Temps�coul�DepuisMAJ) <= VITESSE_MAX_GAUCHE_DROITE)
            {
                VecteurVitesseGaucheDroite += new Vector3(ACC�L�RATION_AIR * Temps�coul�DepuisMAJ, 0, 0);
            }
        }

        protected virtual void Gauche()
        {
            DIRECTION = ORIENTATION.GAUCHE;
            if (VecteurVitesse.Y == 0)
            {
                �TAT_PERSO = �TAT.COURRIR;
                if (Math.Abs(VecteurVitesseGaucheDroite.X - ACC�L�RATION_SOL * Temps�coul�DepuisMAJ) <= VITESSE_MAX_GAUCHE_DROITE)
                {
                    VecteurVitesseGaucheDroite -= new Vector3(ACC�L�RATION_SOL * Temps�coul�DepuisMAJ, 0, 0);
                }
            }
            else if (Math.Abs(VecteurVitesseGaucheDroite.X - ACC�L�RATION_AIR * Temps�coul�DepuisMAJ) <= VITESSE_MAX_GAUCHE_DROITE)
            {
                VecteurVitesseGaucheDroite -= new Vector3(ACC�L�RATION_AIR * Temps�coul�DepuisMAJ, 0, 0);
            }
        }
        protected virtual void Bloquer()
        {
            EstBouclierActif = true;
            if ((BouclierPersonnage == null || !Game.Components.Contains(BouclierPersonnage)))
            {
                AjouterBouclier();
            }
            �TAT_PERSO = �TAT.BLOQUER;
        }

        private void G�rerNouvellesTouches()
        {
            if ((GestionInputClavier.EstNouvelleTouche(CONTR�LES[3]) || GestionInputManette.EstNouvelleTouche(NumManette, Buttons.Y)) && CptSaut < 2 && !EstEnAttaque && !EstBouclierActif)
            {
                G�rerSauts();
            }
            if ((GestionInputClavier.EstNouvelleTouche(CONTR�LES[4]) || GestionInputManette.EstNouvelleTouche(NumManette, Buttons.X)) && !EstBouclierActif && FrameEntreProjectile <= 0)
            {
                G�rerLancer();
            }
            if ((GestionInputClavier.EstNouvelleTouche(CONTR�LES[5]) || GestionInputManette.EstNouvelleTouche(NumManette, Buttons.A)) && !EstBouclierActif)
            {
                G�rerAttaque();
            }
        }
        protected void G�rerSauts()
        {
            if (CptSaut == 0)
            {
                �TAT_PERSO = �TAT.SAUTER;
                VecteurVitesse += (-VecteurVitesse.Y + VitesseMaximaleSaut) * Vector3.Up;
                ++CptSaut;
                if (CptSaut == 1)
                {
                    ASaut�DuneSurface = Position.Y == AnciennePosition.Y;
                }
            }
            else if (CptSaut == 1 && ASaut�DuneSurface)
            {
                �TAT_PERSO = �TAT.SAUTER;
                VecteurVitesse += (-VecteurVitesse.Y + VitesseMaximaleSaut) * Vector3.Up;
                ++CptSaut;
            }
        }
        protected void G�rerLancer()
        {
            if (FrameEntreProjectile <= 0)
            {
                if (this.TypePersonnage == "Ninja")
                {
                    Projectile p = new Projectile(Game, 1f, new Vector3(0, 0, -MathHelper.Pi / 2), Position, new Vector2(2, 4), "Ninja/Kunai", AtelierXNA.Atelier.INTERVALLE_MAJ_STANDARD, DIRECTION, 0.75f, true, 4, NumManette);
                    Game.Components.Add(p);
                    FrameEntreProjectile = 30;
                    �TAT_PERSO = �TAT.LANCER;
                }
                if (this.TypePersonnage == "Robot")
                {
                    Projectile p = new Projectile(Game, 1f, new Vector3(0, 0, 0), Position, new Vector2(4, 2), "Robot/laser", AtelierXNA.Atelier.INTERVALLE_MAJ_STANDARD, DIRECTION, 1f, false, 3, NumManette);
                    Game.Components.Add(p);
                    FrameEntreProjectile = 40;
                    �TAT_PERSO = �TAT.LANCER;
                }
            }
        }
        protected void G�rerAttaque()
        {
            �TAT_PERSO = �TAT.ATTAQUER;
        }
        protected abstract void AjouterBouclier();
        public void EncaisserD�g�ts(Personnage p)
        {
            GestionnaireDeSon.Find("punch").Play();
            if (p.DIRECTION == ORIENTATION.DROITE)
            {
                VecteurVitesse += p.ForceCoup * Vector3.Normalize(new Vector3(1, 0.1f, 0)) * Temps�coul�DepuisMAJ * (1 + VieEnPourcentage / 100f) / Masse;
            }
            else
            {
                VecteurVitesse += p.ForceCoup * Vector3.Normalize(new Vector3(-1, 0.1f, 0)) * Temps�coul�DepuisMAJ * (1 + VieEnPourcentage / 100f) / Masse;
            }
            VieEnPourcentage += p.DommageAttaque;
        }
        public void G�rerRecul(Personnage p)
        {
            if (VecteurVitesse.Y == 0)
            {
                VecteurVitesse += (p.VecteurVitesseGaucheDroite + p.VecteurVitesse) * p.Masse / Masse;
            }

        }

        public void EncaisserD�g�ts(Projectile p)
        {
            VieEnPourcentage += p.D�gat;
            if (p.Direction == ORIENTATION.DROITE)
            {
                VecteurVitesse += Temps�coul�DepuisMAJ * p.Force * Vector3.Right * (1 + VieEnPourcentage / 100) / Masse;
            }
            else
            {
                VecteurVitesse += Temps�coul�DepuisMAJ * p.Force * Vector3.Left * (1 + VieEnPourcentage / 100) / Masse;
            }
        }
        #endregion

        #endregion

        #region Bool�ens de la classe.
        protected bool EstMort()
        {
            return FramesCrier <= 0 && (Position.X > Carte.LIMITE_MAP.X || Position.X < Carte.LIMITE_MAP.Y || Position.Y > Carte.LIMITE_MAP.Z || Position.Y < Carte.LIMITE_MAP.W);
        }
        protected bool EstModePeur()
        {
            return (Position.X > Carte.LIMITE_MAP.X - 40 || Position.X < Carte.LIMITE_MAP.Y + 40 || Position.Y > Carte.LIMITE_MAP.Z - 30 || Position.Y < Carte.LIMITE_MAP.W + 70);
        }
        public bool EstEnCollision(Personnage p)
        {
            return p.HitBox.Intersects(HitBox);
        }
        public bool EstEnCollision(Projectile p)
        {
            return p.Sph�reDeCollision.Intersects(HitBox);
        }
        protected abstract bool EstDansIntervalleSurface(Vector3 intervalle, Vector3 position);
        #endregion

    }
}