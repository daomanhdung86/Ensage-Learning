﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct3D9;

namespace ScanNotVisibleEnemy
{
    class Program
    {
        #region CLASS
        public class BountyXP
        {
            public string Name
            {
                get;
                set;
            }
            public uint fBountyXP
            {
                get;
                set;
            }
            public BountyXP(string pName, uint pBountyXP)
            {
                Name = pName;
                fBountyXP = pBountyXP;
            }
        }
        public class ListBountyXP
        {
            public List<BountyXP> List
            {
                get;
                set;
            }
            public ListBountyXP()
            {
                List = new List<BountyXP>();
            }
            public uint Get_BoubtyXP(string pName)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (BountyXP xp in List)
                        {
                            if (xp.Name == pName)
                            {
                                return xp.fBountyXP;
                            }
                        }
                        uint BountyXP = 0;
                        try
                        {
                            BountyXP = (uint)Game.FindKeyValues(pName + "/BountyXP", KeyValueSource.Unit).IntValue;
                        }
                        catch (Exception e)
                        {
                        }
                        BountyXP newBountyXP = new BountyXP(pName, BountyXP);
                        List.Add(newBountyXP);
                        return BountyXP;
                    }
                    else
                    {
                        uint BountyXP = 0;
                        try
                        {
                            BountyXP = (uint)Game.FindKeyValues(pName + "/BountyXP", KeyValueSource.Unit).IntValue;
                        }
                        catch (Exception e)
                        {
                        }
                        BountyXP newBountyXP = new BountyXP(pName, BountyXP);
                        List.Add(newBountyXP );
                        return BountyXP;
                    }
                }
                else
                {
                    List = new List<BountyXP>();
                    return 0;
                }
            }
        }
        public class AlarmWarning
        {
            public Vector3 Position
            {
                get;
                set;
            }
            public uint AlarmCount
            {
                get;
                set;
            }
            public ParticleEffect ParticleEffect
            {
                get;
                set;
            }
            public float CreateTime
            {
                get;
                set;
            }
            public AlarmWarning (uint pMode, Vector3 pPosition, uint pAlarmCount)
            {
                //Console.WriteLine("                               " + pPosition.ToString() + " = " + pAlarmCount.ToString());

                Position = pPosition;
                AlarmCount = pAlarmCount;
                ParticleEffect = new ParticleEffect("particles/ui_mouseactions/drag_selected_ring.vpcf", pPosition);
                if (pMode == 0)
                {
                    ParticleEffect.SetControlPoint(1, new Vector3(Menu.Item("H_AlarmColorR").GetValue<Slider>().Value, Menu.Item("H_AlarmColorG").GetValue<Slider>().Value, Menu.Item("H_AlarmColorB").GetValue<Slider>().Value));
                }
                else
                {
                    ParticleEffect.SetControlPoint(1, new Vector3(Menu.Item("C_AlarmColorR").GetValue<Slider>().Value, Menu.Item("C_AlarmColorG").GetValue<Slider>().Value, Menu.Item("C_AlarmColorB").GetValue<Slider>().Value));
                }
                ParticleEffect.SetControlPoint(2, new Vector3(1300 * -1, 255, 0));
                CreateTime = Game.GameTime;
            }
        }
        public class ListAlarmWarning
        {
            public List<AlarmWarning> List
            {
                get;
                set;
            }
            public ListAlarmWarning()
            {
                List = new List<AlarmWarning>();
            }
            public void Add_Alarm(uint pMode, Vector3 pPosition, uint pAlarmCount)
            {
                int count = List.Count;
                for (int i = count - 1; i >= 0; i++ )
                {
                    if (List[i].Position.Distance2D(pPosition) <= Menu.Item("AlarmMinDistance").GetValue<Slider>().Value)
                    {
                        if (List[i].AlarmCount >= pAlarmCount)
                        {
                            return;
                        }
                        else
                        {
                            List.RemoveAt(i);
                        }
                    }
                }
                AlarmWarning newAlarm = new AlarmWarning(pMode, pPosition, pAlarmCount);
                List.Add(newAlarm);
            }
        }
        public class HeroExpand
        {
            #region FIELDS
            public uint HeroHandle
            {
                get;
                set;
            }
            public Hero Hero
            {
                get;
                set;
            }
            public int OldDenyCount
            {
                get;
                set;
            }
            public Ensage.LifeState OldLifeState
            {
                get;
                set;
            }
            public uint OldXP
            {
                get;
                set;
            }
            public bool IsDead
            {
                get;
                set;
            }
            public float DeadTime
            {
                get;
                set;
            }
            public float RespawnTime
            {
                get;
                set;
            }
            public float LastVisibleTime
            {
                get;
                set;
            }
            public Vector3 LasVisiblePostion
            {
                get;
                set;
            }
            public bool IsHaveTP
            {
                get;
                set;
            }
            public List<Unit> UnitsProvideXPToThisHero
            {
                get;
                set;
            }
            #endregion
            #region METHODS
            public HeroExpand(Hero x)
            {
                HeroHandle = x.Handle;
                Hero = x;
                OldLifeState = x.LifeState;
                OldXP = x.CurrentXP;
                OldDenyCount = x.Player.DenyCount;
                IsDead = true;
                DeadTime = 0;
                RespawnTime = 0;
                LastVisibleTime = Game.GameTime;
                LasVisiblePostion = x.Position;
                IsHaveTP = ((x.FindItem("item_tpscroll") != null) || (x.FindItem("item_travel_boots") != null) || (x.FindItem("item_travel_boots") != null));
                UnitsProvideXPToThisHero = new List<Unit>();
                UnitsProvideXPToThisHero.Clear();
            }
            public void Update(Hero x)
            {
                if (x.LifeState == LifeState.Dying)
                {
                    if (OldLifeState == LifeState.Alive)
                    {
                        IsDead = true;
                        DeadTime = Game.GameTime;
                        RespawnTime = DeadTime + (x.RespawnTime - x.DeathTime) - 1f;
                    }
                }
                Hero = x;
                OldXP = x.CurrentXP;
                OldDenyCount = x.Player.DenyCount;
                LastVisibleTime = Game.GameTime;
                LasVisiblePostion = x.Position;
                IsHaveTP = ((x.FindItem("item_tpscroll") != null) || (x.FindItem("item_travel_boots") != null) || (x.FindItem("item_travel_boots") != null));
                UnitsProvideXPToThisHero.Clear();
            }
            public void UpdateIsDead()
            {
                // Check just  die
                if (IsDead)
                {
                    if (Hero.LifeState == LifeState.Alive )
                    {
                        IsDead = false;
                        //Console.WriteLine(Hero.Name + ": " + (Game.GameTime - RespawnTime));
                    }
                    if (Game.GameTime > RespawnTime)
                    {
                        IsDead = false;
                        IsHaveTP = true;
                    }
                }
            }
            public bool IsAllNeutral()
            {
                if (UnitsProvideXPToThisHero != null)
                {
                    if (UnitsProvideXPToThisHero.Count > 0)
                    {
                        bool AllNeutral = true;
                        foreach (Unit u in UnitsProvideXPToThisHero)
                        {
                            if (u.Team == Team.Neutral )
                            {
                                AllNeutral = false;
                                break;
                            }
                        }
                        return AllNeutral;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            public uint Get_DifferentXP()
            {
                if (Hero.Level < 25)
                {
                    return (Hero.CurrentXP - OldXP);
                }
                else
                {
                    return GetTotalReviceXPinTheory();
                }
            }
            public uint GetTotalReviceXPinTheory()
            {
                if (UnitsProvideXPToThisHero != null)
                {
                    if (UnitsProvideXPToThisHero.Count > 0)
                    {
                        uint TotalRevice = 0;
                        foreach (Unit u in UnitsProvideXPToThisHero)
                        {
                            TotalRevice += UnitsExpand.Get_XPperHero(u.Handle);
                        }
                        return TotalRevice;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }            
            #endregion
        }
        public class ListHeroExpand
        {
            public List<HeroExpand> List = new List<HeroExpand>();
            public uint Get_OldXP(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.OldXP;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public int Get_OldDenyCount(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.OldDenyCount;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public uint Get_DifferentXP(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.Get_DifferentXP();
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public uint Get_CountDead()
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        uint count = 0;
                        foreach (HeroExpand v in List)
                        {
                            if ((v.Hero.Team == h_me.GetEnemyTeam()) && v.IsDead)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                count += 1;
                            }
                        }
                        return count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public uint Get_AliveAndVisible()
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        uint count = 0;
                        foreach (HeroExpand v in List)
                        {
                            if ((v.Hero.Team == h_me.GetEnemyTeam()) && (v.IsDead == false) && ((Game.GameTime - v.LastVisibleTime) <= TickTime))
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                count += 1;
                            }
                        }
                        return count;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public uint Get_TotalReviceXPinTheory(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.GetTotalReviceXPinTheory();
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public bool Add_UnitsProvideXPToThisHero(uint pHandle, Unit pUnit)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                v.UnitsProvideXPToThisHero.Add(pUnit);
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            public List<Unit> Get_UnitsProvideXPToThisHero(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.UnitsProvideXPToThisHero;
                            }
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            public int Get_CountUnitsProvideXPToThisHero(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.UnitsProvideXPToThisHero.Count;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public HeroExpand Get_HeroExpand(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v;
                            }
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }

            }
            public bool IsReviceXPFromAllNeutral(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.IsAllNeutral();
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            public bool Set_AlarmCount(uint pHandle, uint pAlarmCount)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                ListAlarm.Add_Alarm(0, v.Hero.Position, pAlarmCount);
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            public bool CheckHandle(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (HeroExpand v in List)
                        {
                            if (v.HeroHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            public bool UpdateHero (Hero pHero)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        if (CheckHandle(pHero.Handle))
                        {
                            Get_HeroExpand(pHero.Handle).Update(pHero);
                            return true;
                        }
                        else
                        {
                            HeroExpand newExpandHero = new HeroExpand(pHero);
                            List.Add(newExpandHero);
                            return true;
                        }
                    }
                    else
                    {
                        HeroExpand newExpandHero = new HeroExpand(pHero);
                        List.Add(newExpandHero);
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            public void Update_VisibleHeroes()
            {
                if (List == null)
                {
                    List = new List<HeroExpand>();
                }

                var AllVisibleHero = ObjectManager.GetEntities<Hero>().Where(x => !x.IsIllusion).ToList();
                if (AllVisibleHero != null)
                {
                    if (AllVisibleHero.Count > 0)
                    {
                        //Console.WriteLine("Update = " + AllVisibleHero.Count);
                        foreach (Hero v in AllVisibleHero)
                        {
                            UpdateHero(v);
                        }
                    }
                }

                if (List.Count > 0)
                {
                    foreach (HeroExpand h in List)
                    {
                        h.UpdateIsDead();
                    }
                }

            }
        }
        public class UnitExpand
        {
            public Unit Unit
            {
                get;
                set;
            }
            public uint UnitHandle
            {
                get;
                set;
            }
            public bool Check
            {
                get;
                set;
            }
            public Ensage.LifeState OldLifeState
            {
                get;
                set;
            }
            public uint BountyXP
            {
                get;
                set;
            }
            public ParticleEffect ParticleEffect
            {
                get;
                set;
            }
            public List<Hero> HerosShareXPFromThisUnit
            {
                get;
                set;
            }
            public UnitExpand(Unit x)
            {
                Unit = x;
                UnitHandle = x.Handle;
                Check = false;
                OldLifeState = x.LifeState;
                BountyXP = 0;
                HerosShareXPFromThisUnit = new List<Hero>();
                HerosShareXPFromThisUnit.Clear();
            }
            public uint Get_XPperHero()
            {
                if (HerosShareXPFromThisUnit != null)
                {
                    if (HerosShareXPFromThisUnit.Count >0)
                    {
                        return (uint)(BountyXP / HerosShareXPFromThisUnit.Count);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
        public class ListUnitExpand
        {
            public List<UnitExpand> List = new List<UnitExpand>();
            public bool Get_Check(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.Check;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            public LifeState Get_OldLifeState(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.OldLifeState;
                            }
                        }
                        return LifeState.Dying;
                    }
                    else
                    {
                        return LifeState.Dying;
                    }
                }
                else
                {
                    return LifeState.Dying;
                }
            }
            public void Set_OldLifeState(uint pHandle, LifeState pLifeState)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                v.OldLifeState = pLifeState;
                            }
                        }
                    }
                }
            }
            public uint Get_BountyXP(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.BountyXP;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public void Set_BountyXP(uint pHandle, uint pBountyXP)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                v.BountyXP = pBountyXP;
                            }
                        }
                    }
                }
            }
            public bool Add_HerosShareXPFromThisUnit(uint pHandle, Hero pHero)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                v.Check  = true; 
                                v.HerosShareXPFromThisUnit.Add(pHero);
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            public List<Hero> Get_HerosShareXPFromThisUnit(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.HerosShareXPFromThisUnit;
                            }
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            public int Get_CountHerosShareXPFromThisUnit(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.HerosShareXPFromThisUnit.Count;
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public uint Get_XPperHero(uint pHandle)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                return v.Get_XPperHero();
                            }
                        }
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            public bool Set_AlarmCount(uint pHandle, uint pAlarmCount)
            {
                if (List != null)
                {
                    if (List.Count > 0)
                    {
                        foreach (UnitExpand v in List)
                        {
                            if (v.UnitHandle == pHandle)
                            {
                                //Console.WriteLine("Get_OldXP: " + pHandle.ToString() + "-" + v.OldXP.ToString());
                                ListAlarm.Add_Alarm(1, v.Unit.Position, pAlarmCount);
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

            }
            public void Update_UnitLifeState()
            {
                List.Clear();
                var AllUnit = ObjectManager.GetEntities<Unit>().Where(x => 
                    !x.IsIllusion &&
                    x.LifeState == LifeState.Alive).ToList();
                foreach (Unit v in AllUnit)
                {
                    UnitExpand newExpandUnit = new UnitExpand(v);
                    List.Add(newExpandUnit);
                }

            }
        }
        #endregion
        #region FILEDS
        private static readonly Menu Menu = new Menu("Scan not visible enemy", "ScanNotInVisibleEnemy", true);

        private static ListBountyXP ListXP = new ListBountyXP();
        private static ListAlarmWarning ListAlarm = new ListAlarmWarning();
        private static ListHeroExpand HeroesExpand = new ListHeroExpand();
        private static ListUnitExpand UnitsExpand = new ListUnitExpand();

        private static List<Unit> AllUnits = new List<Unit>();
        private static List<Hero> AllHeros = new List<Hero>();

        private static Hero h_me;

        private static float OldTick;
        private static float TickTime = 0.06f;
        #endregion
        #region FUNCTIONS
        private static void AlarmClear()
        {
            if (ListAlarm.List != null)
            {
                if (ListAlarm.List.Count > 0)
                {
                    for (int i= ListAlarm.List.Count - 1; i >= 0; i = i - 1)
                    {
                        if (Game.GameTime - ListAlarm.List[i].CreateTime > Menu.Item("AlarmShowTime").GetValue<Slider>().Value)
                        {
                            ListAlarm.List[i].ParticleEffect.Dispose();
                            ListAlarm.List.RemoveAt(i);
                        }
                    }
                }
            }
        }
        private static float GetTimeToReach (int pMoveSpeed, Vector3 vStarPoint, Vector3 vEndPoint)
        {
            double Distance = Vector3.Distance(vStarPoint, vEndPoint) - 900;
            if (pMoveSpeed > 0)
            {
                return (float)Distance / (float)pMoveSpeed;
            }
            else
            {
                return (float)double.MaxValue;
            }
        }
        private static float GetFastTimeToGoHere(Hero pHero, Vector3 pHere)
        {
            HeroExpand v = HeroesExpand.Get_HeroExpand(pHero.Handle);
            int vHeroSpeed = pHero.MovementSpeed;
            Building ClosestBuilding = ObjectManager.GetEntities<Building>().Where( x=>
                    x.IsAlive &&
                    x.Team == pHero.Team).OrderBy(y => y.Distance2D(pHere)).FirstOrDefault();

            float tpTime = 2.5f + GetTimeToReach(vHeroSpeed, ClosestBuilding.Position, pHere);
            float MoveTime =  GetTimeToReach(vHeroSpeed, v.LasVisiblePostion, pHere);

            if (tpTime > MoveTime )
            {
                return MoveTime;
            }
            else
            {
                return tpTime;
            }
        }
        private static bool CanGetHere(HeroExpand pHero, Vector3 pHere)
        {
            Hero h = pHero.Hero;
            if (h != null)
            {
                float Time = Game.GameTime - pHero.LastVisibleTime;
                if (Time >= GetFastTimeToGoHere(h, pHere))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private static uint GetNotVisibleDangerEnemyHero(Vector3 pPosition)
        {
            if (HeroesExpand.List != null)
            {
                if (HeroesExpand.List.Count > 0)
                {
                    uint vDead = HeroesExpand.Get_CountDead();
                    uint vAliveVisible = HeroesExpand.Get_AliveAndVisible();
                    //Console.WriteLine("                                     Total = " + HeroesExpand.List.Where(x => x.Hero.Team == h_me.GetEnemyTeam()).ToList().Count);
                    //Console.WriteLine("                                     Dead = " + vDead);
                    //Console.WriteLine("                                     vAliveVisible = " + vAliveVisible);
                    //Console.WriteLine("                                     Danger = ");
                    uint vInVisibleAndDanger = 0;
                    foreach (HeroExpand he in HeroesExpand.List )
                    {
                        if ((!he.IsDead) && (he.Hero.Team == h_me.GetEnemyTeam())  && ((Game.GameTime - he.LastVisibleTime) > TickTime))
                        {
                            if (CanGetHere(he, pPosition))
                            {
                                vInVisibleAndDanger += 1;
                                //Console.WriteLine("                                              " + he.Hero.Name + " In fog = " + (Game.GameTime - he.LastVisibleTime) + " MinTime = " + GetFastTimeToGoHere(he.Hero, pPosition));
                            }
                        }
                    }
                    //Console.WriteLine("                                     NotDanger = ");
                    foreach (HeroExpand he in HeroesExpand.List)
                    {
                        if ((!he.IsDead) && (he.Hero.Team == h_me.GetEnemyTeam()) && ((Game.GameTime - he.LastVisibleTime) > TickTime))
                        {
                            if (!CanGetHere(he, pPosition))
                            {
                                //Console.WriteLine("                                              " + he.Hero.Name + " In fog = " + (Game.GameTime - he.LastVisibleTime) + " MinTime = " + GetFastTimeToGoHere(he.Hero, pPosition));
                            }
                        }
                    }

                    //Console.WriteLine("--------------------------------------------------------------");
                    return vInVisibleAndDanger;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        private static List<HeroExpand> ListDangerEnemyHero(Vector3 pPosition)
        {
            if (HeroesExpand.List != null)
            {
                if (HeroesExpand.List.Count > 0)
                {
                    return HeroesExpand.List.Where(x => !x.IsDead && x.Hero.Team == h_me.GetEnemyTeam() && ((Game.GameTime - x.LastVisibleTime) > TickTime) && CanGetHere(x, pPosition)).ToList();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static List<HeroExpand> ListDeadEnemyHero(Vector3 pPosition)
        {
            if (HeroesExpand.List != null)
            {
                if (HeroesExpand.List.Count > 0)
                {
                    return HeroesExpand.List.Where(x => x.IsDead && x.Hero.Team == h_me.GetEnemyTeam()).ToList();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static List<HeroExpand> ListVisibleEnemyHero(Vector3 pPosition)
        {
            if (HeroesExpand.List != null)
            {
                if (HeroesExpand.List.Count > 0)
                {
                    return HeroesExpand.List.Where(x => !x.IsDead && x.Hero.Team == h_me.GetEnemyTeam() && ((Game.GameTime - x.LastVisibleTime) <= TickTime)).ToList();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static List<HeroExpand> ListNotDangerEnemyHero(Vector3 pPosition)
        {
            if (HeroesExpand.List != null)
            {
                if (HeroesExpand.List.Count > 0)
                {
                    return HeroesExpand.List.Where(x => !x.IsDead && x.Hero.Team == h_me.GetEnemyTeam() && ((Game.GameTime - x.LastVisibleTime) > TickTime) && !CanGetHere(x, pPosition)).ToList();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private static List<Hero> GetShareXPHeroes (Unit u)
        {
            if (u.Team == Team.Neutral )
            {
                return (ObjectManager.GetEntities<Hero>().Where(x =>
                            x.IsAlive &&
                            !x.IsIllusion &&
                            x.Distance2D(u.Position) <= Menu.Item("XP_Range").GetValue<Slider>().Value).ToList());
            }
            else
            {
                if (u.Team == h_me.Team)
                {
                    return (ObjectManager.GetEntities<Hero>().Where(x =>
                        x.IsAlive &&
                        !x.IsIllusion &&
                        x.Team == h_me.GetEnemyTeam() &&
                        x.Distance2D(u.Position) <= Menu.Item("XP_Range").GetValue<Slider>().Value).ToList());
                }
                else
                {
                    if (u.Team == h_me.GetEnemyTeam())
                    {
                        return (ObjectManager.GetEntities<Hero>().Where(x =>
                            x.IsAlive &&
                            !x.IsIllusion &&
                            x.Team == h_me.Team &&
                            x.Distance2D(u.Position) <= Menu.Item("XP_Range").GetValue<Slider>().Value).ToList());
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        private static void Debug_1 (uint pMode, Unit pUnit)
        {
            string vpreFix = " ";
            switch(pMode)
            {
                case 1:
                    vpreFix = " ";
                    break;
                case 2:
                    vpreFix = "                               ";
                    break;
            }
            Console.WriteLine("1_" + Game.GameTime.ToString() + vpreFix + pUnit.Name + " _ " + UnitsExpand.Get_BountyXP(pUnit.Handle));
            List<Hero> list = UnitsExpand.Get_HerosShareXPFromThisUnit(pUnit.Handle );
            if (list != null)
            {
                if (list.Count > 0)
                {
                    foreach (Hero h in list)
                    {
                        Console.WriteLine("1_" + Game.GameTime.ToString() + vpreFix + "     " + h.Name + " _ " + HeroesExpand.Get_DifferentXP(h.Handle).ToString());
                    }
                }
            }

        }
        private static void Debug_2(uint pMode, Hero pHero)
        {
            string vpreFix = " ";
            switch (pMode)
            {
                case 1:
                    vpreFix = " ";
                    break;
                case 2:
                    vpreFix = "                               ";
                    break;
            }
            Console.WriteLine("2_" + Game.GameTime.ToString() + vpreFix + pHero.Name + " _ " + HeroesExpand.Get_DifferentXP(pHero.Handle) + " _ " + HeroesExpand.Get_TotalReviceXPinTheory(pHero.Handle));
            List<Unit> list = HeroesExpand.Get_UnitsProvideXPToThisHero(pHero.Handle);
            if (list != null)
            {
                if (list.Count > 0)
                {
                    foreach (Unit u in list)
                    {
                        Console.WriteLine("2_" + Game.GameTime.ToString() + vpreFix + "     " + u.Name + " _ " + UnitsExpand.Get_XPperHero(u.Handle).ToString());
                    }
                }
            }
        }
        // This function copy from VisionControl
        private static Vector2 WorldToMiniMap(Vector3 Pos, int size)
        {
            const float MapLeft = -8000;
            const float MapTop = 7350;
            const float MapRight = 7500;
            const float MapBottom = -7200;
            var MapWidth = Math.Abs(MapLeft - MapRight);
            var MapHeight = Math.Abs(MapBottom - MapTop);

            var _x = Pos.X - MapLeft;
            var _y = Pos.Y - MapBottom;

            float dx, dy, px, py;
            if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.7)
            {
                dx = 272f / 1920f * Drawing.Width;
                dy = 261f / 1080f * Drawing.Height;
                px = 11f / 1920f * Drawing.Width;
                py = 11f / 1080f * Drawing.Height;
            }
            else if (Math.Round((float)Drawing.Width / Drawing.Height, 1) >= 1.5)
            {
                dx = 267f / 1680f * Drawing.Width;
                dy = 252f / 1050f * Drawing.Height;
                px = 10f / 1680f * Drawing.Width;
                py = 11f / 1050f * Drawing.Height;
            }
            else
            {
                dx = 255f / 1280f * Drawing.Width;
                dy = 229f / 1024f * Drawing.Height;
                px = 6f / 1280f * Drawing.Width;
                py = 9f / 1024f * Drawing.Height;
            }
            var MinimapMapScaleX = dx / MapWidth;
            var MinimapMapScaleY = dy / MapHeight;

            var scaledX = Math.Min(Math.Max(_x * MinimapMapScaleX, 0), dx);
            var scaledY = Math.Min(Math.Max(_y * MinimapMapScaleY, 0), dy);

            var screenX = px + scaledX;
            var screenY = Drawing.Height - scaledY - py;

            return new Vector2((float)Math.Floor(screenX - size * 1.8), (float)Math.Floor(screenY - size * 2.7));
        }
        private static void RelationAnalysys()
        {
            if (Utils.SleepCheck("Delay time ..."))
            {

                AllHeros = ObjectManager.GetEntities<Hero>().Where(x =>
                    x.Level <= 25 &&
                    x.IsAlive &&
                    !x.IsIllusion).ToList();

                #region Check if Hero revice XP from not visible source (ex. Jungle creep die in fog)
                foreach (Hero h in AllHeros)
                {
                    uint Get_TotalReviceXPinTheory = HeroesExpand.Get_TotalReviceXPinTheory(h.Handle);
                    uint Get_DifferentXP = HeroesExpand.Get_TotalReviceXPinTheory(h.Handle);
                    if (Get_TotalReviceXPinTheory > 0)
                    {
                        if ((Get_DifferentXP - HeroesExpand.Get_TotalReviceXPinTheory(h.Handle)) > 5)
                        {
                            //Console.WriteLine("--------------------------------------------------------------");
                            //Console.WriteLine("Check for " + h.Name);
                            uint NotVisibleEnemyHeroes = GetNotVisibleDangerEnemyHero(h.Position);
                            if (NotVisibleEnemyHeroes > 0)
                            {
                                HeroesExpand.Set_AlarmCount(h.Handle, 5 + NotVisibleEnemyHeroes);
                                //Debug_2(2, h);
                            }
                        }
                        //else
                        //{
                        //    Debug_2(1, h);
                        //}
                    }
                }
                #endregion

                #region Analysys Unit
                if (AllUnits != null)
                {
                    if (AllUnits.Count > 0)
                    {
                        foreach (Unit u in AllUnits)
                        {
                            if (u.Team != h_me.GetEnemyTeam())
                            {
                                if (UnitsExpand.Get_Check(u.Handle))
                                {
                                    int countHero = UnitsExpand.Get_CountHerosShareXPFromThisUnit(u.Handle);
                                    if (countHero > 0)
                                    {
                                        foreach (Hero h in UnitsExpand.Get_HerosShareXPFromThisUnit(u.Handle))
                                        {
                                            int countUnit = HeroesExpand.Get_CountUnitsProvideXPToThisHero(h.Handle);
                                            if (countUnit > 0)
                                            {
                                                if (countUnit == 1)
                                                {
                                                    uint DifferentXP = HeroesExpand.Get_DifferentXP(h.Handle);
                                                    uint XPperHero = UnitsExpand.Get_XPperHero(u.Handle);
                                                    // Only 1 unit 
                                                    if (Math.Abs(DifferentXP - XPperHero) > 5)
                                                    {
                                                        //Console.WriteLine("--------------------------------------------------------------");
                                                        //Console.WriteLine("Check for " + u.Name);
                                                        uint NotVisibleEnemyHeroes = GetNotVisibleDangerEnemyHero(u.Position);
                                                        if (NotVisibleEnemyHeroes > 0)
                                                        {
                                                            //float test = UnitsExpand.Get_BountyXP(u.Handle) / DifgerentXP;
                                                            //Console.WriteLine("vTheoryGetXPHero = " + UnitsExpand.Get_BountyXP(u.Handle).ToString() + " / " + DifgerentXP.ToString() + " = " + test.ToString());
                                                            uint vTheoryGetXPHero = UnitsExpand.Get_BountyXP(u.Handle) / DifferentXP;
                                                            uint Get_CountHerosShareXPFromThisUnit = (uint)UnitsExpand.Get_CountHerosShareXPFromThisUnit(u.Handle);
                                                            if (NotVisibleEnemyHeroes >= vTheoryGetXPHero - Get_CountHerosShareXPFromThisUnit)
                                                            {
                                                                UnitsExpand.Set_AlarmCount(u.Handle, vTheoryGetXPHero - Get_CountHerosShareXPFromThisUnit);
                                                                //Debug_1(2, u);
                                                            }
                                                            else
                                                            {
                                                                UnitsExpand.Set_AlarmCount(u.Handle, 5 + NotVisibleEnemyHeroes);
                                                                //Debug_1(2, u);
                                                            }
                                                        }
                                                    }
                                                    //else
                                                    //{
                                                    //    Debug_1(1, u);
                                                    //}
                                                }
                                                else
                                                {
                                                    // More 1 unit
                                                    uint DifferentXP = HeroesExpand.Get_DifferentXP(h.Handle);
                                                    uint TotalXPReviceTheory = HeroesExpand.Get_TotalReviceXPinTheory(h.Handle);
                                                    if (Math.Abs(TotalXPReviceTheory - DifferentXP) > 5)
                                                    {
                                                        //Console.WriteLine("--------------------------------------------------------------");
                                                        //Console.WriteLine("Check for " + u.Name);
                                                        uint NotVisibleEnemyHeroes = GetNotVisibleDangerEnemyHero(u.Position);
                                                        if (NotVisibleEnemyHeroes > 0)
                                                        {
                                                            UnitsExpand.Set_AlarmCount(u.Handle, 5 + NotVisibleEnemyHeroes);
                                                            //Debug_1(2, u);
                                                        }
                                                    }
                                                    //else
                                                    //{
                                                    //    Debug_1(1, u);
                                                    //}
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                AllUnits.Clear();
                AllHeros.Clear();
                AllHeros.Clear();
            }
        }
        #endregion
        static void Main(string[] args)
        {
            #region Get local hero
            h_me = ObjectManager.LocalHero;
            if (h_me == null)
            {
                return;
            }
            #endregion

            Console.WriteLine("Scan Not Visible Enemy loaded...");

            var keySetting = new Menu("Keys", "Keys");
            keySetting.AddItem(new MenuItem("k_Scan", "Scan key").SetValue(new KeyBind('T', KeyBindType.Toggle)).SetTooltip("Press key to ON/OFF."));
            Menu.AddSubMenu(keySetting);
            var OptionSetting = new Menu("Options", "Options");
            OptionSetting.AddItem(new MenuItem("XP_Range", "Range revice XP").SetValue(new Slider(1300, 500, 2000)).SetTooltip(""));
            OptionSetting.AddItem(new MenuItem("AlarmShowTime", "Alarm show time").SetValue(new Slider(3, 0, 10)).SetTooltip("Time to show alarm."));
            OptionSetting.AddItem(new MenuItem("AlarmMinDistance", "Alarm min distance").SetValue(new Slider(300, 0, 1000)).SetTooltip("Min distance between 2 alarm point."));
            OptionSetting.AddItem(new MenuItem("DelayTime", "Delay time").SetValue(new Slider(0, 0, 1000)).SetTooltip("Set delay time to more FPS."));
            var HeroColorSetting = new Menu("Hero color", "HeroColor");
            HeroColorSetting.AddItem(new MenuItem("H_AlarmColorR", "Alarm color R").SetValue(new Slider(20, 0, 255)).SetTooltip("Color of alarm ring."));
            HeroColorSetting.AddItem(new MenuItem("H_AlarmColorG", "Alarm color G").SetValue(new Slider(5, 0, 255)).SetTooltip("Color of alarm ring."));
            HeroColorSetting.AddItem(new MenuItem("H_AlarmColorB", "Alarm color B").SetValue(new Slider(161, 0, 255)).SetTooltip("Color of alarm ring."));
            OptionSetting.AddSubMenu(HeroColorSetting);

            var CreepColorSetting = new Menu("Creep color", "CreepColor");
            CreepColorSetting.AddItem(new MenuItem("C_AlarmColorR", "Alarm color R").SetValue(new Slider(204, 0, 255)).SetTooltip("Color of alarm ring."));
            CreepColorSetting.AddItem(new MenuItem("C_AlarmColorG", "Alarm color G").SetValue(new Slider(0, 0, 255)).SetTooltip("Color of alarm ring."));
            CreepColorSetting.AddItem(new MenuItem("C_AlarmColorB", "Alarm color B").SetValue(new Slider(0, 0, 255)).SetTooltip("Color of alarm ring."));
            OptionSetting.AddSubMenu(CreepColorSetting);
            OptionSetting.AddItem(new MenuItem("AlarmTextSize", "Alarm text size").SetValue(new Slider(5, 0, 10)).SetTooltip("Text size alarm."));

            Menu.AddSubMenu(OptionSetting);

            Menu.AddToMainMenu();

            //ObjectManager.OnAddEntity += ObjectManager_OnAddEntity;
            //ObjectManager.OnRemoveEntity += ObjectManager_OnRemoveEntity;
            //ObjectManager.OnAddTrackingProjectile += ObjectManager_OnAddTrackingProjectile;
            //ObjectManager.OnRemoveTrackingProjectile += ObjectManager_OnRemoveTrackingProjectile;

            //Unit.OnModifierAdded += Unit_OnModifierAdded;
            //Unit.OnModifierRemoved += Unit_OnModifierRemoved;

            //Game.OnFireEvent += Game_OnFireEvents;
            //Game.OnUpdate += Game_OnUpdate;
            Game.OnIngameUpdate += Game_OnInGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
        }
        //public static void Game_OnUpdate(EventArgs args)
        //{
        //    h_me = ObjectManager.LocalHero;
        //    if (h_me == null)
        //        return;

        //    if (!Game.IsInGame || Game.GameTime < 30 || Game.IsPaused)
        //        return;
        //    Console.WriteLine((Game.GameTime) + " -------------------------------------------------- ");
        //    List<HeroExpand> d = ListDeadEnemyHero(Game.MousePosition);
        //    if (d != null)
        //    {
        //        if (d.Count > 0)
        //        {
        //            Console.WriteLine((Game.GameTime) + " Dead = " + d.Count);
        //            foreach (HeroExpand h in d)
        //            {
        //                Console.WriteLine("          " + h.Hero.Name + ": Respawn in = " + (Game.GameTime - h.RespawnTime) + " IsHaveTp = " + h.IsHaveTP);
        //            }
        //        }
        //    }
        //    List<HeroExpand> a = ListVisibleEnemyHero(Game.MousePosition);
        //    if (a != null)
        //    {
        //        if (a.Count > 0)
        //        {
        //            Console.WriteLine((Game.GameTime) + " Visible = " + d.Count);
        //            foreach (HeroExpand h in a)
        //            {
        //                Console.WriteLine("          " + h.Hero.Name + " IsHaveTp = " + h.IsHaveTP);
        //            }
        //        }
        //    }
        //    List<HeroExpand> v = ListDangerEnemyHero(Game.MousePosition);
        //    if (v != null)
        //    {
        //        if (v.Count > 0)
        //        {
        //            Console.WriteLine((Game.GameTime) + " Danger = " + v.Count);
        //            foreach (HeroExpand h in v)
        //            {
        //                Console.WriteLine("          " + h.Hero.Name + ": InFog = " + (Game.GameTime - h.LastVisibleTime) + " FastestTime = " + GetFastTimeToGoHere(h.Hero, Game.MousePosition) + " IsHaveTp = " + h.IsHaveTP);
        //            }
        //        }
        //    }
        //    List<HeroExpand> f = ListNotDangerEnemyHero(Game.MousePosition);
        //    if (f != null)
        //    {
        //        if (f.Count > 0)
        //        {
        //            Console.WriteLine((Game.GameTime) + " NOT Danger = " + f.Count);
        //            foreach (HeroExpand h in f)
        //            {
        //                Console.WriteLine("          " + h.Hero.Name + ": InFog = " + (Game.GameTime - h.LastVisibleTime) + " FastestTime = " + GetFastTimeToGoHere(h.Hero, Game.MousePosition) + " IsHaveTp = " + h.IsHaveTP);
        //            }
        //        }
        //    }

        //    //if (HeroesExpand.List != null)
        //    //{
        //    //    Console.WriteLine((Game.GameTime) + " Total = " + HeroesExpand.List.Where(x => x.Hero.Team == h_me.GetEnemyTeam()).ToList().Count);
        //    //    Console.WriteLine("     Dead: " + HeroesExpand.Get_CountDead());
        //    //    foreach (HeroExpand he in HeroesExpand.List)
        //    //    {
        //    //        if ((he.Hero.Team == h_me.GetEnemyTeam()) && he.IsDead)
        //    //        {
        //    //            Console.WriteLine("          " + he.Hero.Name + ": InFog = " + (Game.GameTime - he.LastVisibleTime) + " Respawn in = " + (he.RespawnTime - Game.GameTime) + " IsHaveTp = " + he.IsHaveTP);
        //    //        }
        //    //    }
        //    //    Console.WriteLine("     Alive and Visible: " + HeroesExpand.Get_AliveAndVisible());
        //    //    foreach (HeroExpand he in HeroesExpand.List)
        //    //    {
        //    //        if ((he.Hero.Team == h_me.GetEnemyTeam()) && !he.IsDead && ((Game.GameTime - he.LastVisibleTime) <= TickTime))
        //    //        {
        //    //            Console.WriteLine("          " + he.Hero.Name + " IsHaveTp = " + he.IsHaveTP);
        //    //        }
        //    //    }
        //    //    Console.WriteLine("    InVisible: " + (HeroesExpand.List.Where(x => x.Hero.Team == h_me.GetEnemyTeam()).ToList().Count - HeroesExpand.Get_CountDead() - HeroesExpand.Get_AliveAndVisible()));
        //    //    foreach (HeroExpand he in HeroesExpand.List)
        //    //    {
        //    //        if ((he.Hero.Team == h_me.GetEnemyTeam()) && !he.IsDead && ((Game.GameTime - he.LastVisibleTime) > TickTime))
        //    //        {
        //    //            Console.WriteLine("          " + he.Hero.Name + ": InFog = " + (Game.GameTime - he.LastVisibleTime) + " IsHaveTp = " + he.IsHaveTP);
        //    //        }
        //    //    }
        //    //}
        //    //OldTick = Game.GameTime;
        //}
        public static void Game_OnInGameUpdate(EventArgs args)
        {
            h_me = ObjectManager.LocalHero;
            if (h_me == null)
                return;

            if (!Game.IsInGame || Game.GameTime < 30 || Game.IsPaused)
                return;

            bool active = Menu.Item("k_Scan").GetValue<KeyBind>().Active;

            if (active)
            {
                int sleepTime =  Menu.Item("DelayTime").GetValue<Slider>().Value;
                if (Utils.SleepCheck("Delay time ..."))
                {
                    AllUnits = ObjectManager.GetEntities<Unit>().Where(x => 
                        x.UnitType != 1 && 
                        !x.IsIllusion &&
                        x.LifeState == LifeState.Dying &&
                        UnitsExpand.Get_OldLifeState(x.Handle) == LifeState.Alive).ToList();
                    if (AllUnits != null)
                    {
                        if (AllUnits.Count > 0)
                        {
                            foreach (Unit v in AllUnits)
                            {
                                UnitsExpand.Set_BountyXP(v.Handle, ListXP.Get_BoubtyXP(v.Name));
                                var EnemyHeroesShareEXP =  GetShareXPHeroes(v);
                                if (EnemyHeroesShareEXP != null)
                                {
                                    if (EnemyHeroesShareEXP.Count > 0)
                                    {
                                        foreach (Hero h in EnemyHeroesShareEXP)
                                        {
                                            //Console.WriteLine("     - " + h.Name);
                                            UnitsExpand.Add_HerosShareXPFromThisUnit(v.Handle, h);
                                            HeroesExpand.Add_UnitsProvideXPToThisHero(h.Handle, v);
                                        }
                                    }
                                }
                                //Console.WriteLine("-------------------------------------------");
                            }
                        }
                    }
                    RelationAnalysys();
                    Utils.Sleep(sleepTime, "Delay time ...");
                }
                AlarmClear();
                UnitsExpand.Update_UnitLifeState();
                HeroesExpand.Update_VisibleHeroes();
            }
        }
        private static void Game_OnDraw(EventArgs args)
        {
            var player = ObjectManager.LocalPlayer;
            if (player == null || player.Team == Team.Observer || h_me == null)
                return;
            int TextSize = Menu.Item("AlarmTextSize").GetValue<Slider>().Value;

            foreach (AlarmWarning v in ListAlarm.List)
            {
                if (v.AlarmCount <= 5)
                {
                    Drawing.DrawText("[" + v.AlarmCount.ToString() + "]", Drawing.WorldToScreen(v.Position), new Vector2(TextSize * 10, TextSize * 10), Color.Red, FontFlags.None);
                    Drawing.DrawText("[" + v.AlarmCount.ToString() + "]", WorldToMiniMap(v.Position, TextSize), new Vector2(TextSize * 10, TextSize * 10), Color.Red, FontFlags.None);
                }
                else
                {
                    Drawing.DrawText((v.AlarmCount - 5).ToString(), Drawing.WorldToScreen(v.Position), new Vector2(TextSize * 10, TextSize * 10), Color.Red, FontFlags.None);
                    Drawing.DrawText((v.AlarmCount - 5).ToString(), WorldToMiniMap(v.Position, TextSize), new Vector2(TextSize * 10, TextSize * 10), Color.Red, FontFlags.None);
                }
            }

            #region Debug, show all Entity

            //var Pa = ObjectManager.ParticleEffects.ToList();
            //if (Pa != null)
            //{
            //    foreach (ParticleEffect v in Pa)
            //    {
            //        Console.WriteLine("ParticleEffect = " + v.Name);
            //        Drawing.DrawText("P=" + v.Name, Drawing.WorldToScreen(v.Position), Color.DarkRed, FontFlags.None);
            //    }
            //}


            //var Pj = ObjectManager.TrackingProjectiles;
            //if (Pj != null)
            //{
            //    foreach (TrackingProjectile v in Pj)
            //    {
            //        Console.WriteLine("TrackingProjectile = " + v.Source.Name + " -> " + v.Target.Name);
            //        Drawing.DrawText(v.Source.Name + " -> " + v.Target.Name, Drawing.WorldToScreen(v.Position), Color.DarkRed, FontFlags.None);
            //    }
            //}

            //var Units = ObjectManager.GetEntities<Unit>().Where(x => x.UnitType == 1152);
            //if (Units != null)
            //{
            //    foreach (var u in Units)
            //    {
            //        var pos1 = Drawing.WorldToScreen(u.Position);
            //        var pos2 = Drawing.WorldToScreen(u.Position + new Vector3(0, -40, 0));
            //        var pos3 = Drawing.WorldToScreen(u.Position + new Vector3(0, -80, 0));
            //        var pos4 = Drawing.WorldToScreen(u.Position + new Vector3(0, -120, 0));

            //        List<Modifier> fuck = u.Modifiers.ToList();
            //        if (fuck != null)
            //        {
            //            foreach (Modifier v in fuck)
            //            {
            //                Console.WriteLine(u.Name + "=>" + v.Name);
            //                Console.WriteLine("Caster.Name = " + v.Caster.Name);
            //                Console.WriteLine("Owner.Name = " + v.Owner.Name);
            //                Console.WriteLine("Parent.Name = " + v.Parent.Name);
            //                Console.WriteLine("TextureName = " + v.TextureName);
            //                Console.WriteLine("----------------------------------------");
            //            }
            //        }


            //        Drawing.DrawText(u.Name, pos1, Color.DarkRed, FontFlags.None);
            //        Drawing.DrawText(u.UnitType.ToString(), pos2, Color.DarkRed, FontFlags.None);
            //        if (u.NetworkActivity != NetworkActivity.Attack && u.NetworkActivity != NetworkActivity.Move && u.NetworkActivity != NetworkActivity.Idle && u.NetworkActivity != NetworkActivity.Die)
            //            Console.WriteLine(u.Name + " NetworkActivity = " + u.NetworkActivity.ToString());

            //        float Distance = 150; //Change this for whatever distance you want to use

            //        float X = u.Position.X + (float)(Distance * Math.Cos(u.RotationRad));
            //        float Y = u.Position.Y + (float)(Distance * Math.Sin(u.RotationRad));
            //        Vector2 Pos5 = Drawing.WorldToScreen(new Vector3(X, Y, u.Position.Z));
            //        Drawing.DrawRect(Drawing.WorldToScreen(Pos5), new Vector2(100, 50), Drawing.GetTexture("materials/ensage_ui/items/aegis.vmat"));
            //        Vector2 Pos6 = Drawing.WorldToScreen(Prediction.InFront(u, Distance));
            //        Drawing.DrawText("x", Pos5, Color.DarkRed, FontFlags.None);
            //        switch (u.LifeState)
            //        {
            //            case LifeState.Alive:
            //                Drawing.DrawText("Alive", pos3, Color.Green, FontFlags.None);
            //                break;
            //            case LifeState.Dying:
            //                Drawing.DrawText("Dying", pos3, Color.Red, FontFlags.None);
            //                break;
            //            case LifeState.Dead:
            //                Drawing.DrawText("Dead", pos3, Color.Gray, FontFlags.None);
            //                break;
            //        }
            //        var BountyXP = Game.FindKeyValues(u.Name + "/BountyXP", KeyValueSource.Unit).IntValue;
            //        if (BountyXP != null)
            //            Drawing.DrawText("BountyXP =" + BountyXP.ToString(), pos4, Color.Green, FontFlags.None);
            //    }
            //}
            #endregion
        }
    }
}
