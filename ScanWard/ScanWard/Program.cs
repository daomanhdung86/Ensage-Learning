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
        public class ScanedPoint
        {
            public Vector3 Position
            {
                get;
                set;
            }
            public bool IsSentry
            {
                get;
                set;
            }
            public ParticleEffect ParticleEffect
            {
                get;
                set;
            }
            public Ensage.Common.Objects.DrawObjects.Circle Circle
            {
                get;
                set;
            }
        }


        private static readonly Menu Menu1 = new Menu("Scan Ward", "ScanWard", true);
        private static Font txt;
        private static Font not;

        private static Hero h_me;
        private static bool k_Scan, k_Clear;

        private static bool scan, oneturn;
        private static bool OldVisible;



        private static List<ScanedPoint> ScanedList = new List<ScanedPoint>();

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
            OptionSetting.AddItem(new MenuItem("o_WardRange", "Range of Observer ward").SetValue(new Slider(1700, 500, 2000)).SetTooltip("Vision range of Observer ward."));
            OptionSetting.AddItem(new MenuItem("o_SentryRange", "Range of Sentry ward").SetValue(new Slider(850, 500, 2000)).SetTooltip("Vision range of Sentry ward."));
            OptionSetting.AddItem(new MenuItem("o_MinRange", "Minimum space beetwen 2 scan point").SetValue(new Slider(300, 100, 800)).SetTooltip("Min distance between 2 scan point."));
            Menu1.AddSubMenu(OptionSetting);

            Menu1.AddToMainMenu();

            scan = false;
            if (ScanedList != null)
            {
                foreach (ScanedPoint v in ScanedList)
                {
                    v.ParticleEffect.Dispose();
                }
                ScanedList.Clear();
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
                    if (ScanedList != null)
                    {
                        bool check = false;
                        foreach (ScanedPoint v in ScanedList)
                        {
                            if (Vector3.Distance(h_me.Position, v.Position) < Menu1.Item("o_MinRange").GetValue<Slider>().Value)
                            {
                                check = true;
                                break;
                            }
                        }
                        if (check == false)
                        {
                            if (h_me.IsInvisible())
                            {
                                var newCircle = new ScanedPoint();
                                newCircle.Position = h_me.Position;
                                newCircle.IsSentry = true;
                                newCircle.ParticleEffect = new ParticleEffect("particles/ui_mouseactions/drag_selected_ring.vpcf", h_me.Position);
                                newCircle.ParticleEffect.SetControlPoint(1, new Vector3(0, 91, 237));
                                newCircle.ParticleEffect.SetControlPoint(2, new Vector3(Menu1.Item("o_SentryRange").GetValue<Slider>().Value * -1, 255, 0));

                                ScanedList.Add(newCircle);
                                OldVisible = h_me.IsVisibleToEnemies;
                            }
                            else
                            {
                                var newCircle = new ScanedPoint();
                                newCircle.Position = h_me.Position;
                                newCircle.IsSentry = false;
                                newCircle.ParticleEffect = new ParticleEffect("particles/ui_mouseactions/drag_selected_ring.vpcf", h_me.Position);
                                newCircle.ParticleEffect.SetControlPoint(1, new Vector3(255, 221, 0));
                                newCircle.ParticleEffect.SetControlPoint(2, new Vector3(Menu1.Item("o_WardRange").GetValue<Slider>().Value * -1, 255, 0));

                                ScanedList.Add(newCircle);
                                OldVisible = h_me.IsVisibleToEnemies;
                            }
                        }
                    }
                    else
                    {
                        if (h_me.IsInvisible())
                        {
                            var newCircle = new ScanedPoint();
                            newCircle.Position = h_me.Position;
                            newCircle.IsSentry = true;
                            newCircle.ParticleEffect = new ParticleEffect("particles/ui_mouseactions/drag_selected_ring.vpcf", h_me.Position);
                            newCircle.ParticleEffect.SetControlPoint(1, new Vector3(0, 91, 237));
                            newCircle.ParticleEffect.SetControlPoint(2, new Vector3(Menu1.Item("o_SentryRange").GetValue<Slider>().Value * -1, 255, 0));

                            ScanedList.Add(newCircle);
                            OldVisible = h_me.IsVisibleToEnemies;
                        }
                        else
                        {
                            var newCircle = new ScanedPoint();
                            newCircle.Position = h_me.Position;
                            newCircle.IsSentry = false;
                            newCircle.ParticleEffect = new ParticleEffect("particles/ui_mouseactions/drag_selected_ring.vpcf", h_me.Position);
                            newCircle.ParticleEffect.SetControlPoint(1, new Vector3(255, 221, 0));
                            newCircle.ParticleEffect.SetControlPoint(2, new Vector3(Menu1.Item("o_WardRange").GetValue<Slider>().Value * -1, 255, 0));

                            ScanedList.Add(newCircle);
                            OldVisible = h_me.IsVisibleToEnemies;
                        }
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

                if (ScanedList != null)
                {
                    foreach (ScanedPoint v in ScanedList)
                    {
                        v.ParticleEffect.Dispose();
                    }
                    ScanedList.Clear();
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
                if (ScanedList != null)
                {
                    for (int i=0; i < ScanedList.Count; i++)
                    {
                        Drawing.DrawText((i + 1).ToString(), Drawing.WorldToScreen(ScanedList[i].Position), Color.Orange, FontFlags.None);
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
