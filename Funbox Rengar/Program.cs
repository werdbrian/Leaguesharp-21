using System;
using System.Linq;
using LeagueSharp;
using System.Drawing;
using LeagueSharp.Common;
namespace Rengar
{
  public class Program
    {
      private static Menu _config;
      private static Orbwalking.Orbwalker _orbwalker;
      private static Spell _q, _w, _e;
      private static SpellSlot _smite, _smitee, _smiteee;
      private static void Main(string[] args)
        {
          CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
      private static void Game_OnGameLoad(EventArgs args)
        {
          if (ObjectManager.Player.ChampionName != "Rengar")
            return;
          _q = new Spell(SpellSlot.Q, 230);
          _w = new Spell(SpellSlot.W, 350);
          _e = new Spell(SpellSlot.E, 1000);
          _e.SetSkillshot(0.25f, 70, 1500, true, SkillshotType.SkillshotLine);
          _e.MinHitChance = HitChance.Medium;
          _smite = ObjectManager.Player.GetSpellSlot("summonersmite");
          _smitee = ObjectManager.Player.GetSpellSlot("s5_summonersmiteduel");
          _smiteee = ObjectManager.Player.GetSpellSlot("s5_summonersmiteplayerganker");
          _config = new Menu("Rengar", "Rengar", true);
          _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));
          var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
          TargetSelector.AddToMenu(targetSelectorMenu);
          _config.AddSubMenu(targetSelectorMenu);
          _config.AddItem(new MenuItem("emode", "E mode").SetValue(false));
          _config.AddItem(new MenuItem("mmode", "magnet if spells ready").SetValue(false));
          _config.AddItem(new MenuItem("autoheal", "%hp autoheal").SetValue(new Slider(35, 100, 0)));
          _config.AddItem(new MenuItem("drawtar", "Active Enemy").SetValue(new Circle(true, Color.GreenYellow)));
          _config.AddToMainMenu();
          Drawing.OnDraw += Drawing_OnDraw;
          Game.OnUpdate += Game_OnUpdate;
        }

      private static void Game_OnUpdate(EventArgs args)
        {
          if (ObjectManager.Player.HasBuff("rengarqbase") || ObjectManager.Player.HasBuff("rengarqemp"))
            {
              Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);
            }
          if (ObjectManager.Player.Mana == 5)
            {
              if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 <= _config.Item("autoheal").GetValue<Slider>().Value)
                {
                  _w.Cast();
                }
            }
          foreach (var target in HeroManager.Enemies)
            {
              if (TargetSelector.GetPriority(target) == 2.5f)
                {
                  if (target.IsValidTarget(500))
                    {
                      if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                        {
                          if (_smite != SpellSlot.Unknown)
                            {
                              if (ObjectManager.Player.Spellbook.CanUseSpell(_smite) == SpellState.Ready)
                                {
                                  ObjectManager.Player.Spellbook.CastSpell(_smite, target);
                                }
                            }
                          if (_smitee != SpellSlot.Unknown)
                            {
                              if (ObjectManager.Player.Spellbook.CanUseSpell(_smitee) == SpellState.Ready)
                                {
                                  ObjectManager.Player.Spellbook.CastSpell(_smitee, target);
                                }
                            }
                          if (_smiteee != SpellSlot.Unknown)
                            {
                              if (ObjectManager.Player.Spellbook.CanUseSpell(_smiteee) == SpellState.Ready)
                                {
                                  ObjectManager.Player.Spellbook.CastSpell(_smiteee, target);
                                }
                            }
                        }
                    }
                  if (target.IsValidTarget(2000))
                    {
                      if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          TargetSelector.SetTarget(target);
                          _config.Item("ForceFocusSelected").SetValue(true);
                        }
                      if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          if (_config.Item("ForceFocusSelected").GetValue<bool>())
                            {
                              _config.Item("ForceFocusSelected").SetValue(false);
                            }
                        }
                    }
                }
            }
          if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
              var qwtarget = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
              var etarget = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
              if (_config.Item("mmode").GetValue<bool>())
                {
                  var magnet = TargetSelector.GetTarget(_q.Range, TargetSelector.DamageType.Physical);
                  if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                    {
                      if (magnet.IsValidTarget(_q.Range))
                        {
                          if (_q.IsReady() || _w.IsReady() || _e.IsReady())
                            {
                              ObjectManager.Player.IssueOrder(GameObjectOrder.AttackUnit, magnet);
                            }
                        }
                    }
                }
              if (ObjectManager.Player.Mana <= 4)
                {
                  if (_q.IsReady())
                    {
                      if (qwtarget.IsValidTarget(_q.Range))
                        {
                          _q.Cast();
                        }
                      if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                        {
                          if (etarget.IsValidTarget(1000))
                            {
                              _q.Cast();
                            }
                        }
                    }
                  if (_w.IsReady())
                    {
                      if (qwtarget.IsValidTarget(_w.Range))
                        {
                          _w.Cast();
                        }
                    }
                  if (_e.IsReady())
                    {
                      if (qwtarget.IsValidTarget(_w.Range))
                        {
                          _e.Cast(qwtarget);
                        }
                      else if (etarget.Distance(ObjectManager.Player.Position) > _w.Range)
                        {
                          if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                            {
                              if (etarget.IsValidTarget(1000))
                                {
                                  _e.Cast(etarget);
                                }
                            }
                        }
                    }
                }
              if (ObjectManager.Player.Mana == 5)
                {
                  if ((ObjectManager.Player.Health/ObjectManager.Player.MaxHealth)*100 > _config.Item("autoheal").GetValue<Slider>().Value)
                    {
                      if (!_config.Item("emode").GetValue<bool>())
                        {
                          if (_q.IsReady())
                            {
                              if (qwtarget.IsValidTarget(_q.Range))
                                {
                                  _q.Cast();
                                }
                              if (ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                {
                                  if (etarget.IsValidTarget(1000))
                                    {
                                      _q.Cast();
                                    }
                                }
                            }
                          if (_e.IsReady())
                            {
                              if (etarget.Distance(ObjectManager.Player.Position) > _q.Range)
                                {
                                  if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                    {
                                      if (etarget.IsValidTarget(1000))
                                        {
                                          _e.Cast(etarget);
                                        }
                                    }
                                }
                            }
                        }
                      if (_config.Item("emode").GetValue<bool>())
                        {
                          if (_e.IsReady())
                            {
                              if (!ObjectManager.Player.HasBuff("rengarpassivebuff"))
                                {
                                  if (etarget.IsValidTarget(1000))
                                    {
                                      _e.Cast(etarget);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
      private static void Drawing_OnDraw(EventArgs args)
        {
          var wts = Drawing.WorldToScreen(ObjectManager.Player.Position);
          var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
          if (_config.Item("drawtar").GetValue<Circle>().Active)
            {
              if (target.IsValidTarget(1200))
                {
                  Render.Circle.DrawCircle(target.Position, 115f, _config.Item("drawtar").GetValue<Circle>().Color, 1);
                }
            }
         }
     }
}
