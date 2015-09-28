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

namespace ApexVarus
{
    class Program
    {
        public static Spell.Chargeable Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static Menu VarusMenu, ComboMenu, HarassMenu, CSMenu, DrawingsMenu;
        private static Circle QRangeCircle, UltRangeCircle;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Varus")
                return;
            Bootstrap.Init(null);
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnTick += Game_OnTick;

            Q = new Spell.Chargeable(SpellSlot.Q, 925, 1625, 4, 250, 925, 250);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 925, SkillShotType.Circular, 250, 1000, 400);
            R = new Spell.Skillshot(SpellSlot.R, 1050, SkillShotType.Linear, 150, 2000, 120);

            VarusMenu = MainMenu.AddMenu("Apex Varus", "VarusMenu");
            VarusMenu.AddGroupLabel("Apex Varus");
            VarusMenu.AddSeparator();
            VarusMenu.AddLabel("Created by TheApex");

            ComboMenu = VarusMenu.AddSubMenu("Combo:", "ComboMenu");
            ComboMenu.AddGroupLabel("Combo:");
            ComboMenu.Add("UseQ", new CheckBox("Use Q in combo"));
            ComboMenu.Add("UseE", new CheckBox("Use E in combo"));
            ComboMenu.Add("UseR", new CheckBox("Use R in combo", false));
            ComboMenu.Add("IntR", new CheckBox("Auto Ult on interruptable spell", false));
            ComboMenu.Add("CastR", new KeyBind("Cast R on key press", false, KeyBind.BindTypes.HoldActive, 'T'));

            HarassMenu = VarusMenu.AddSubMenu("Harass:", "HarassMenu");
            HarassMenu.AddGroupLabel("Harass:");
            HarassMenu.Add("HarassQ", new CheckBox("Use Q to harass"));
            HarassMenu.Add("HarassE", new CheckBox("Use E to harass"));
            HarassMenu.Add("HarassMana", new Slider("Minimum mana percent to harass", 40, 0, 100));
            HarassMenu.Add("HarassToggle", new KeyBind("Toggle harass", false, KeyBind.BindTypes.PressToggle, 'H'));

            CSMenu = VarusMenu.AddSubMenu("CS:", "CSMenu");
            CSMenu.AddGroupLabel("CS:");
            CSMenu.Add("ClearQ", new CheckBox("Use Q to waveclear"));
            CSMenu.Add("ClearE", new CheckBox("Use E to waveclear"));
            CSMenu.Add("CSMana", new Slider("Minimum mana percent to waveclear", 30, 0, 100));

            DrawingsMenu = VarusMenu.AddSubMenu("Drawings:", "DrawingsMenu");
            DrawingsMenu.AddGroupLabel("Drawings:");
            DrawingsMenu.Add("DrawQ", new CheckBox("Draw Q range", false));
            DrawingsMenu.Add("DrawR", new CheckBox("Draw R range"));
        }
        // ORBWALKER MODES
        private static void Game_OnTick(EventArgs args)
        {
            if (ComboMenu["CastR"].Cast<KeyBind>().CurrentValue)
            {
                RCast();
            }
            if (ComboMenu["HarassToggle"].Cast<KeyBind>().CurrentValue)
            {
                Harass();
            }
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

        // INTERRUPT
        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (ComboMenu["IntR"].Cast<CheckBox>().CurrentValue)
            {
                R.Cast(sender);
            }
        }
        // R CAST ON KEY
        public static void RCast()
        {
            var target = TargetSelector.GetTarget(1050, DamageType.Physical);
            if (ComboMenu["CastR"].Cast<KeyBind>().CurrentValue)
            {
                R.Cast(target.ServerPosition);
            }
        }
        // COMBO
        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1700, DamageType.Physical);
            if (target.IsValidTarget(1700))
            {
                if (Q.IsReady() && ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(1600))
                {
                    Q.Cast(target.ServerPosition);
                }
                if (E.IsReady() && ComboMenu["UseE"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(925))
                {
                    E.Cast(target.ServerPosition);
                }
                if (R.IsReady() && ComboMenu["UseR"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(1050))
                {
                    R.Cast(target.ServerPosition);
                }
            }
        }
        // HARASS
        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1700, DamageType.Physical);
            var currmana = ObjectManager.Player.ManaPercent;
            var harmana = HarassMenu["HarassMana"].Cast<Slider>().CurrentValue;
            if (target.IsValidTarget(1700) && harmana <= currmana) 
            {
                if (Q.IsReady() && HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(1600))
                {
                    Q.Cast(target.ServerPosition);
                }
                if (E.IsReady() && HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue && target.IsValidTarget(925))
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }
        // CSpush
        public static void CSpush()
        {
            var currmana = ObjectManager.Player.ManaPercent;
            var csmana = HarassMenu["CSMana"].Cast<Slider>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            foreach (var minion in minions)
            {
                if (minion.IsValidTarget(1700) && csmana <= currmana && CSMenu["ClearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    Q.Cast(minion);
                }
                if (minion.IsValidTarget(925) && csmana <= currmana && CSMenu["ClearE"].Cast<CheckBox>().CurrentValue && E.IsReady())
                {
                    E.Cast(minion);
                }
            }
        }
        // CS
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
        public static void Drawing_OnDraw(EventArgs args)
        {
            QRangeCircle = new Circle
            {
                Color = Color.Red,
                Radius = Q.MaximumRange
            };
            if (DrawingsMenu["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                QRangeCircle.Draw(Player.Instance.Position);
            }
            UltRangeCircle = new Circle
            {
                Color = Color.Red,
                Radius = R.Range
            };
            if (DrawingsMenu["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                UltRangeCircle.Draw(Player.Instance.Position);
            }
        }
    }
}
