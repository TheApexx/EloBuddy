using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace ApexAshe
{
    class Program
    {
        public static Spell.Active Q;
        public static Spell.Skillshot W;
        public static Spell.Skillshot R;
        public static Menu AsheMenu, ComboMenu, HarassMenu, CSMenu, DrawingsMenu, SettingsMenu;
        private static Circle AARangeCircle, WRangeCircle, UltRangeCircle;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Ashe")
                return;
            Bootstrap.Init(null);
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnTick += Game_OnTick;

            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Skillshot(SpellSlot.W, 1000, SkillShotType.Linear, (int)0.25f, (int)1250, (int)20f);
            R = new Spell.Skillshot(SpellSlot.R, 8000, SkillShotType.Linear, (int)0.25f, (int)1000, (int)250f);

            // Main Menu
            AsheMenu = MainMenu.AddMenu("Apex Ashe", "ApexAshe");
            AsheMenu.AddGroupLabel("Apex Ashe");
            AsheMenu.AddSeparator();
            AsheMenu.AddLabel("Made by TheApex");
            // Combo Menu
            ComboMenu = AsheMenu.AddSubMenu("Combo", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings:");
            ComboMenu.Add("UseQ", new CheckBox("Use Q in Combo"));
            ComboMenu.Add("UseW", new CheckBox("Use W in Combo"));
            ComboMenu.Add("UseR", new CheckBox("Use ult in Combo"));
            // Harass Menu
            HarassMenu = AsheMenu.AddSubMenu("Harass", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings:");
            HarassMenu.Add("Whar", new CheckBox("Use W to harass"));
            HarassMenu.Add("Wmana", new Slider("Minimum Mana %", 40, 10, 100));
            // CS Menu
            CSMenu = AsheMenu.AddSubMenu("CS", "CS");
            CSMenu.AddGroupLabel("CS Settings:");
            CSMenu.Add("Qpush", new CheckBox("Use Q to push"));
            CSMenu.Add("Wpush", new CheckBox("Use W to push"));
            CSMenu.Add("PushMana", new Slider("Minimum Mana %", 40, 10, 100));
            // Drawings Menu
            DrawingsMenu = AsheMenu.AddSubMenu("Drawings", "Drawings");
            DrawingsMenu.AddGroupLabel("Drawings:");
            DrawingsMenu.Add("DrawAA", new CheckBox("Draw AA range"));
            DrawingsMenu.Add("DrawW", new CheckBox("Draw W range", false));
            DrawingsMenu.Add("DrawR", new CheckBox("Draw current Ult range", false));
            // Settings Menu
            SettingsMenu = AsheMenu.AddSubMenu("Settings", "Settings");
            SettingsMenu.AddGroupLabel("Settings:");
            SettingsMenu.Add("RInterrupt", new CheckBox("Interrupt with Ult"));
            SettingsMenu.Add("Rrange", new Slider("Maximum ult range", 2000, 1000, 8000));
        }
        //INTERRUPT SETTINGS
        public static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs R)
        {

        }
        //ORBWALKER MODES
        private static void Game_OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.Harass:
                    Harass();
                    break;
                case Orbwalker.ActiveModes.LaneClear:
                    CSpush();
                    break;
                case Orbwalker.ActiveModes.LastHit:
                    CS();
                    break;
                case Orbwalker.ActiveModes.JungleClear:
                    break;
            }
        }
        // COMBO
        public static void Combo()
        {
            var UltRange = SettingsMenu["Rrange"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(UltRange, DamageType.Physical);
            if (target.IsValidTarget(UltRange))
            {
                //Q USAGE
                if (Q.IsReady() && Player.HasBuff("asheqcastready") && ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(700))
                {
                    Q.Cast();
                }
                //W USAGE
                if (W.IsReady() && ComboMenu["UseW"].Cast<CheckBox>().CurrentValue && W.GetPrediction(target).HitChance >= HitChance.High && target.IsValidTarget(1000))
                {
                    W.Cast(target.ServerPosition);
                }
                //R USAGE
                if (R.IsReady() && ComboMenu["UseR"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(UltRange))
                {
                    R.Cast(target.ServerPosition);
                }
            }
        }
        // HARASS
        public static void Harass()
        {
            var targetharass = TargetSelector.GetTarget(1300, DamageType.Physical);
            var mana = HarassMenu["Wmana"].Cast<Slider>().CurrentValue;
            if (targetharass.IsValidTarget(1300) && W.IsReady() && ObjectManager.Player.ManaPercent >= mana)
            {
                W.Cast(targetharass.ServerPosition);
            }
        }
        // LANE CLEAR
        public static void CSpush()
        {
            var manapush = CSMenu["PushMana"].Cast<Slider>().CurrentValue;
            var minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, ObjectManager.Player.Position.To2D(), 1300);
            foreach (var minion in minions.Where(m => m.IsValid))
            if (W.IsReady() && CSMenu["Wpush"].Cast<CheckBox>().CurrentValue && ObjectManager.Player.ManaPercent >= manapush)
            {
                W.Cast(minion);
            }
            if (Q.IsReady() && CSMenu["Qpush"].Cast<CheckBox>().CurrentValue && Player.HasBuff("asheqcastready") && ObjectManager.Player.ManaPercent >= manapush)
            {
                Q.Cast();
            }
        }
        // LAST HITTING
        public static void CS()
        {
            var minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, ObjectManager.Player.Position.To2D(), 600);
            foreach (var minion in minions.Where(m => m.IsValid))
            {
                if (minion.Health < ObjectManager.Player.GetAutoAttackDamage(minion))
                {
                    return;
                }
            }
        }
        // DRAWINGS
        private static void Drawing_OnDraw(EventArgs args)
        {
            var UltRange = SettingsMenu["Rrange"].Cast<Slider>().CurrentValue;
            AARangeCircle = new Circle
            {
                Color = Color.Red,
                Radius = 665
            };
            if (DrawingsMenu["DrawAA"].Cast<CheckBox>().CurrentValue) 
            {
                AARangeCircle.Draw(Player.Instance.Position);
            }
            WRangeCircle = new Circle
            {
                Color = Color.Red,
                Radius = 1200
            };
            if (DrawingsMenu["DrawW"].Cast<CheckBox>().CurrentValue) 
            {
                WRangeCircle.Draw(Player.Instance.Position);
            }
            UltRangeCircle = new Circle
            {
                Color = Color.Purple,
                Radius = UltRange
            };
            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                UltRangeCircle.Draw(Player.Instance.Position);
            }
        }
    }
}
