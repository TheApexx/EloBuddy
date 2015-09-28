using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;

namespace ApexUtility
{
    class Program
    {
        public static Menu ApexMenu, DrawingsMenu, TrackerMenu, ActivatorMenu;
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            Bootstrap.Init(null);
            Drawing.OnDraw += Drawing_OnDraw;

            // Main menu
            ApexMenu = MainMenu.AddMenu("Apex Utility", "ApexMenu");
            ApexMenu.AddGroupLabel("Apex Utility");
            ApexMenu.AddSeparator();
            ApexMenu.AddLabel("Made by TheApex");
            ApexMenu.AddLabel("Updated for patch 5.18");

            // Drawings Menu
            DrawingsMenu = ApexMenu.AddSubMenu("Drawings and Range:", "DrawingsMenu");
            DrawingsMenu.Add("EnemyAA", new CheckBox("Draw enemy AA range"));
            DrawingsMenu.Add("AllyAA", new CheckBox("Draw ally AA range", false));
            DrawingsMenu.Add("TowerRange", new CheckBox("Draw tower range"));
            Dra
        }
        // DRAWINGS
        private static void Drawing_OnDraw(EventArgs args)
        {
            
        }
    }
}
