using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;

namespace ScanWard
{
    internal class Program
    {
        private static readonly Menu Menu1 = new Menu("Scan Ward", "ScanWard", true);
        private static Font txt;
        private static Font not;

        private static Hero h_me;
        private static bool k_Scan, k_Clear;

        private static bool scan, oneturn;
        private static bool OldVisible;

        private static List<Vector3> PosList = new List<Vector3>();

        static void Main(string[] args)
        {
            #region Get local hero
            h_me = ObjectManager.LocalHero;
            if (h_me == null)
            {
                return;
            }
            #endregion

            var keySetting = new Menu("Keys", "Keys");
            keySetting.AddItem(new MenuItem("k_Scan", "Scan key").SetValue(new KeyBind('V', KeyBindType.Press)).SetTooltip("Press key then move to scan one point."));
            keySetting.AddItem(new MenuItem("k_Clear", "Clear key").SetValue(new KeyBind('B', KeyBindType.Press)).SetTooltip("Press to clear all scaned point."));
            Menu1.AddSubMenu(keySetting);
            var OptionSetting = new Menu("Options", "Options");
            OptionSetting.AddItem(new MenuItem("o_WardRange", "Range of ward").SetValue(new Slider(1700, 1000, 2000)).SetTooltip("Vision range of Observer ward."));
            OptionSetting.AddItem(new MenuItem("o_MinRange", "Minimum space beetwen 2 scan point").SetValue(new Slider(300, 100, 800)).SetTooltip("Min distance between 2 scan point."));
            Menu1.AddSubMenu(OptionSetting);

            Menu1.AddToMainMenu();

            scan = false;
            if (PosList != null)
            {
                PosList.Clear();
            }
            OldVisible = h_me.IsVisibleToEnemies;


            txt = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 11,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            not = new Font(
               Drawing.Direct3DDevice9,
               new FontDescription
               {
                   FaceName = "Tahoma",
                   Height = 12,
                   OutputPrecision = FontPrecision.Default,
                   Quality = FontQuality.Default
               });

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
            Drawing.OnEndScene += Drawing_OnEndScene;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
        }
        public static void Game_OnUpdate(EventArgs args)
        {
            h_me = ObjectManager.LocalHero;
            if (h_me == null)
            {
                return;
            }

            k_Scan = Menu1.Item("k_Scan").GetValue<KeyBind>().Active;
            k_Clear = Menu1.Item("k_Clear").GetValue<KeyBind>().Active;


            if (k_Scan)
            {
                if (!oneturn)
                {
                    OldVisible = h_me.IsVisibleToEnemies;
                    oneturn = true;
                }

                scan = true;
                if (h_me.IsVisibleToEnemies != OldVisible)
                {
                    if (PosList != null)
                    {
                        bool check = false;
                        foreach (Vector3 v in PosList)
                        {
                            if (Vector3.Distance(h_me.Position, v) < Menu1.Item("o_MinRange").GetValue<Slider>().Value)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check == false)
                        {
                            PosList.Add(h_me.Position);
                            OldVisible = h_me.IsVisibleToEnemies;
                        }
                    }
                    else
                    {
                        PosList.Add(h_me.Position);
                        OldVisible = h_me.IsVisibleToEnemies;
                    }
                }
            }
            else
            {
                oneturn = false;
            }

            if (k_Clear)
            {
                scan = false;
                if (PosList != null)
                {
                    PosList.Clear();
                }
            }
        }
        private static void Game_OnDraw(EventArgs args)
        {
            var player = ObjectManager.LocalPlayer;
            if (player == null || player.Team == Team.Observer || h_me == null)
                return;
            if (scan)
            {
                if (PosList != null)
                {
                    for (int i=0; i < PosList.Count; i++)
                    {
                        var circle = new Ensage.Common.Objects.DrawObjects.Circle(PosList[i], Menu1.Item("o_WardRange").GetValue<Slider>().Value);
                        circle.Draw(Color.Orange);
                        Drawing.DrawText(i.ToString(), Drawing.WorldToScreen(PosList[i]), Color.Orange, FontFlags.None);
                    }
                }
            }
        }
        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            txt.Dispose();
            not.Dispose();
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Drawing.Direct3DDevice9 == null || Drawing.Direct3DDevice9.IsDisposed || !Game.IsInGame)
                return;
            //txt.DrawText(null, "Scan ward ON...", 1200, 27, Color.Orange);
            //if (PosList != null)
            //{
            //    txt.DrawText(null, PosList.Count.ToString(), 1200, 37, Color.Orange);
            //}
            //else
            //{
            //    txt.DrawText(null, "Null", 1200, 37, Color.Orange);
            //}
            if (k_Scan)
            {
                txt.DrawText(null, "Ward scaning...", 1200, 67, Color.Red);
            }                                         
        }

        static void Drawing_OnPostReset(EventArgs args)
        {
            txt.OnResetDevice();
            not.OnResetDevice();
        }

        static void Drawing_OnPreReset(EventArgs args)
        {
            txt.OnLostDevice();
            not.OnLostDevice();
        }
    }
}
