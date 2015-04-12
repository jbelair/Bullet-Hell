using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System;

namespace ActionGame
{
    class Projectile : DynamicObject
    {
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public Vector2 PositionOrigin { get; set; }
        public Entity Parent { get; set; }
        public object Target { get; set; }
        public float Range { get; set; }
        public float Lifetime { get; set; }
        private float LifetimeMax { get; set; }
        public Vector2 ScaleEnd { get; set; }
        public bool ScaleCurved { get; set; }
        public float OpacityEnd { get; set; }
        public bool OpacityCurved { get; set; }
        public bool diesOnImpact { get; set; }
        private bool isOriented { get; set; }
        private float Angle { get; set; }
        private float Speed { get; set; }

        public StatusEffectManager effectManager;

        /// <summary>
        /// The Constructor for a projectile, this should never be used directly anywhere in code, save inside ProjectileManager
        /// </summary>
        /// <param name="key">The name of the projectile texture in the dictionary</param>
        /// <param name="range">The range in pixels of the projectile</param>
        /// <param name="speed">The speed in pixels/second of the projectile</param>
        /// <param name="angle">The direction the projectile is launched</param>
        /// <param name="isOriented">If the direction is oriented to the creator or not</param>
        /// <param name="lifetime">The lifetime in seconds the projectile stays alive for</param>
        /// <param name="parent">The creator of the projectile</param>
        /// <param name="scale">The scale of the projectile at start</param>
        /// <param name="scaleend">The scale of the projectile at end</param>
        public Projectile(string key, float range, float speed, float angle, bool isOriented, float lifetime, Vector2 scale, Vector2 scaleend, bool scalecurved, float opacity, float opacityend, bool opacitycurved, Entity parent) : base(Textures[key])
        {
            this.Range = range;
            this.Lifetime = lifetime;
            this.LifetimeMax = this.Lifetime;
            this.Parent = parent;
            this.isOriented = isOriented;
            this.Angle = angle;
            this.Speed = speed;
            this.Scale = scale;
            this.ScaleEnd = scaleend;
            this.ScaleCurved = scalecurved;
            this.Opacity = opacity;
            this.OpacityEnd = opacityend;
            this.OpacityCurved = opacitycurved;
            this.effectManager = new StatusEffectManager(new List<StatusEffect>(),this);
            if (Parent != null)
                Begin();
        }

        public Projectile(Projectile projectile) : base(projectile.Texture)
        {
            this.Angle = projectile.Angle;
            this.Colour = projectile.Colour;
            this.Depth = projectile.Depth;
            this.diesOnImpact = projectile.diesOnImpact;
            this.effectManager = projectile.effectManager;
            this.ID = projectile.ID;
            this.isOriented = projectile.isOriented;
            this.Lifetime = projectile.Lifetime;
            this.LifetimeMax = this.Lifetime;
            this.Opacity = projectile.Opacity;
            this.OpacityEnd = projectile.OpacityEnd;
            this.OpacityCurved = projectile.OpacityCurved;
            this.Origin = projectile.Origin;
            this.Parent = projectile.Parent;
            this.Position = projectile.Position;
            this.PositionOrigin = projectile.PositionOrigin;
            this.Range = projectile.Range;
            this.Rotation = projectile.Rotation;
            this.Scale = projectile.Scale;
            this.ScaleEnd = projectile.ScaleEnd;
            this.ScaleCurved = projectile.ScaleCurved;
            this.Speed = projectile.Speed;
            this.SpriteEffect = projectile.SpriteEffect;
            this.Target = projectile.Target;
            this.Texture = projectile.Texture;
            this.Velocity = projectile.Velocity;
        }

        public void Begin()
        {
            float x, y;
            if (isOriented)
            {
                Angle += Parent.Rotation;
                x = Speed * (float)Math.Cos(Angle);
                y = Speed * (float)Math.Sin(Angle);
            }
            else
            {
                x = Speed * (float)Math.Cos(Angle);
                y = Speed * (float)Math.Sin(Angle);
            }
            this.Velocity = new Vector2(x, y);
        }

        new public void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Update Logic
            float timelapse = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            Range -= Velocity.Length() * timelapse;
            Lifetime -= timelapse;
            if (ScaleCurved)
            {
                Scale = new Vector2(MathHelper.SmoothStep(ScaleEnd.X, Scale.X, Lifetime / LifetimeMax), MathHelper.SmoothStep(ScaleEnd.Y, Scale.Y, Lifetime / LifetimeMax));
            }
            else
                Scale += (ScaleEnd - Scale) / Lifetime * timelapse;
            if (OpacityCurved)
            {
                Opacity = MathHelper.SmoothStep(OpacityEnd, Opacity, Lifetime / LifetimeMax);
            }
            else
                Opacity += (OpacityEnd - Opacity) / Lifetime * timelapse;

            if (Range < 0 || Lifetime < 0)
            {
                End();
            }

            if (Target != null)
            {
                if (Target is Vector2)
                {
                    Vector2 target = (Vector2)Target;
                    Vector2 direction = target - this.Position;
                    float angle = (float)Math.Atan2(direction.Y, direction.X);
                    float x = this.Velocity.Length() * (float)Math.Cos(angle);
                    float y = this.Velocity.Length() * (float)Math.Sin(angle);
                    Velocity = new Vector2(x, y);
                }
                if (Target is DynamicObject || Target is Entity)    // Once prop is done it needs to be here too
                {
                    DynamicObject target = (DynamicObject)Target;
                    Vector2 direction = target.Position - this.Position;
                    float angle = (float)Math.Atan2(direction.Y,direction.X);
                    float x = this.Velocity.Length() * (float)Math.Cos(angle);
                    float y = this.Velocity.Length() * (float)Math.Sin(angle);
                    Velocity = new Vector2(x, y);
                }
                else
                    return;
            }
        }

        public void Collide(DynamicObject target)
        {
            Rectangle targetcollisionbox = new Rectangle((int)target.Position.X, (int)target.Position.Y, (int)(target.Texture.Bounds.Width * target.Scale.X), (int)(target.Texture.Bounds.Height * target.Scale.Y));
            Rectangle collisionbox = new Rectangle((int)this.Position.X, (int)this.Position.Y, (int)(this.Texture.Bounds.Width * this.Scale.X), (int)(this.Texture.Bounds.Height * this.Scale.Y));
            if (collisionbox.Intersects(targetcollisionbox))
            {
                if (target is Entity)
                {
                    Entity entity = (Entity)target;
                    entity.effectManager.AddEffect(this.effectManager.effects);
                }

                if (this.diesOnImpact)
                    End();
            }
        }

        new public static void LoadContent(ContentManager Content)
        {
            Projectile.Textures.Add("projrootaoe", Content.Load<Texture2D>("Textures/noisy1"));
        }

        public void End()
        {
            ProjectileManager projectileManager = ProjectileManager.Instance();
            // Dying logic goes here

            // This goes at the end
            projectileManager.RemoveProj(this);
        }
    }
}
